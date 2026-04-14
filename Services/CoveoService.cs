using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CoveoVerifierTool.Models;

namespace CoveoVerifierTool.Services
{
  public class VerifyResult
  {
    public string SriggLeId { get; set; }
    public string Label { get; set; }
    public string Title { get; set; }
    public bool Exists { get; set; }
    public int Count { get; set; }
    public string ItemId { get; set; }
    public string ItemPath { get; set; }
    public List<string> MissingHotelKeys { get; set; }
    public List<string> MissingExcursionKeys { get; set; }
    public GaHotelInfo HotelInfo { get; set; }
    public GaExcursionInfo ExcursionInfo { get; set; }

    public VerifyResult()
    {
      MissingHotelKeys = new List<string>();
      MissingExcursionKeys = new List<string>();
    }
  }

  public class CoveoService
  {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public static readonly List<string> CommonExclusions = new List<string>
        {
            "duration50416",
            "groupByResults50416",
            "indexDuration50416",
            "requestDuration50416",
            "searchUid50416",
            "totalCount50416",
            "totalCountFiltered50416",
            "clickUri50416",
            "excerpt50416",
            "excerptHighlights50416",
            "percentScore50416",
            "printableUri50416",
            "printableUriHighlights50416",
            "titleHighlights50416",
            "uniqueId50416",
            "uri50416"
        };

    public CoveoService()
    {
      _baseUrl = ConfigurationManager.AppSettings["Coveo:BaseUrl"];
      string authToken = ConfigurationManager.AppSettings["Coveo:AuthToken"];

      if (string.IsNullOrWhiteSpace(_baseUrl))
        throw new InvalidOperationException("Coveo:BaseUrl is missing from App.config.");
      if (string.IsNullOrWhiteSpace(authToken))
        throw new InvalidOperationException("Coveo:AuthToken is missing from App.config.");

      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", authToken);
      _httpClient.DefaultRequestHeaders.Accept
          .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public CoveoRequest BuildHotelRequest(string language = "en", string sriggLeId = "29")
    {
      return new CoveoRequest
      {
        aq = string.Format(
              "((@flanguage50416=={0}) AND (@ffullpath50416 *= /sitecore/content/YasConnect/GlobalContent/SharedContent/Platform/*) AND ((@ftemplateid50416==d801ccec-bb25-4621-a1db-ea4eaf32120e AND @fsriggleid50416=\"{1}\")))",
              language, sriggLeId),
        fieldsToInclude = new List<string> { "fhotelinfo50416", "fgahotelinfo50416" },
        fieldsToExclude = new List<string>(CommonExclusions),
        numberOfResults = 999
      };
    }

    public CoveoRequest BuildExcursionRequest(string language = "en", string sriggLeId = "11650")
    {
      return new CoveoRequest
      {
        aq = string.Format(
              "((@flanguage50416=={0}) AND (@ffullpath50416 *= /sitecore/content/YasConnect/GlobalContent/SharedContent/Platform/*) AND ((@ftemplateid50416==44002bda-145b-49a9-a8f4-36b568362c76 AND @fsriggleid50416=\"{1}\")))",
              language, sriggLeId),
        fieldsToInclude = new List<string> { "fexcursioninfo50416", "fgaez120xcursioninfo50416" },
        fieldsToExclude = new List<string>(CommonExclusions),
        numberOfResults = 999
      };
    }

    public async Task<VerifyResult> VerifyAsync(CoveoRequest request, string label, string sriggLeId)
    {
      Console.WriteLine();

      string json = request.ToJson();
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      HttpResponseMessage response;
      try
      {
        response = await _httpClient.PostAsync(_baseUrl, content);
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[ERROR] Request failed: " + ex.Message);
        Console.ResetColor();
        return new VerifyResult { SriggLeId = sriggLeId, Label = label, Exists = false, Count = 0, ItemId = string.Empty, ItemPath = string.Empty };
      }

      string responseBody = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(string.Format("[ERROR] HTTP {0} - {1}", (int)response.StatusCode, response.ReasonPhrase));
        Console.WriteLine("Response: " + responseBody);
        Console.ResetColor();
        return new VerifyResult { SriggLeId = sriggLeId, Label = label, Exists = false, Count = 0, ItemId = string.Empty, ItemPath = string.Empty };
      }

      var serializer = new JavaScriptSerializer();
      serializer.MaxJsonLength = int.MaxValue;
      var coveoResponse = serializer.Deserialize<CoveoResponse>(responseBody);

      if (coveoResponse.totalCount > 0)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.ResetColor();

        string itemId = string.Empty;
        string itemPath = string.Empty;
        string title = string.Empty;
        GaHotelInfo hotelInfo = null;
        GaExcursionInfo excursionInfo = null;
        List<string> missingHotelKeys = new List<string>();
        List<string> missingExcursionKeys = new List<string>();

        if (coveoResponse.results != null && coveoResponse.results.Count > 0)
        {
          var first = coveoResponse.results[0];
          title  = first.title ?? string.Empty;
          itemId = ExtractItemId(first.uri);

          if (label == "HOTEL")
          {
            object rawField;
            if (first.raw.TryGetValue("fgahotelinfo50416", out rawField) && rawField != null)
            {
              var parsed = GaHotelInfo.DeserializeGaHotelInfo(rawField.ToString());
              hotelInfo = parsed.Item1;
              missingHotelKeys = GaHotelInfo.GetMissingKeys(parsed.Item2);
            }
            else
            {
              missingHotelKeys = new List<string>(GaHotelInfo.ExpectedKeys);
            }
          }
          else if (label == "EXCURSION")
          {
            object rawField;
            if (first.raw.TryGetValue("fgaez120xcursioninfo50416", out rawField) && rawField != null)
            {
              var parsed = GaExcursionInfo.DeserializeGaExcursionInfo(rawField.ToString());
              excursionInfo = parsed.Item1;
              missingExcursionKeys = GaExcursionInfo.GetMissingKeys(parsed.Item2);
            }
            else
            {
              missingExcursionKeys = new List<string>(GaExcursionInfo.ExpectedKeys);
            }
          }
        }

        return new VerifyResult
        {
          SriggLeId = sriggLeId, Title = title, Label = label, Exists = true,
          Count = coveoResponse.totalCount, ItemId = itemId, ItemPath = itemPath,
          HotelInfo = hotelInfo, ExcursionInfo = excursionInfo,
          MissingHotelKeys = missingHotelKeys, MissingExcursionKeys = missingExcursionKeys
        };
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.ResetColor();
        return new VerifyResult { SriggLeId = sriggLeId, Title = string.Empty, Label = label, Exists = false, Count = 0, ItemId = string.Empty, ItemPath = string.Empty };
      }
    }

    // Extract bare GUID from e.g. sitecore://database/web/ItemId/67C2B893-.../Language/en/Version/1
    private static string ExtractItemId(string uri)
    {
      if (string.IsNullOrEmpty(uri)) return string.Empty;
      var match = Regex.Match(uri,
          @"[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}",
          RegexOptions.IgnoreCase);
      return match.Success ? match.Value.ToUpperInvariant() : uri;
    }


  }
}
