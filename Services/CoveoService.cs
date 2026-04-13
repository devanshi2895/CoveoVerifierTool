using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CoveoVerifierTool.Models;

namespace CoveoVerifierTool.Services
{
  public class VerifyResult
  {
    public string Label { get; set; }
    public bool Exists { get; set; }
    public int Count { get; set; }
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
        fieldsToInclude = new List<string> { "fhotelinfo50416" },
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
        fieldsToInclude = new List<string> { "fez120xcursioninfo50416" },
        fieldsToExclude = new List<string>(CommonExclusions),
        numberOfResults = 999
      };
    }

    public async Task<VerifyResult> VerifyAsync(CoveoRequest request, string label)
    {
      Console.WriteLine();
      Console.WriteLine(new string('-', 60));
      Console.WriteLine("Label : " + label);
      Console.WriteLine("AQ    : " + request.aq);
      Console.WriteLine(new string('-', 60));

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
        return new VerifyResult { Label = label, Exists = false, Count = 0 };
      }

      string responseBody = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(string.Format("[ERROR] HTTP {0} - {1}", (int)response.StatusCode, response.ReasonPhrase));
        Console.WriteLine("Response: " + responseBody);
        Console.ResetColor();
        return new VerifyResult { Label = label, Exists = false, Count = 0 };
      }

      var serializer = new JavaScriptSerializer();
      serializer.MaxJsonLength = int.MaxValue;
      var coveoResponse = serializer.Deserialize<CoveoResponse>(responseBody);

      if (coveoResponse.totalCount > 0)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(string.Format("[{0}] Data EXISTS — {1} record(s) found.", label, coveoResponse.totalCount));
        Console.ResetColor();

        if (coveoResponse.results != null && coveoResponse.results.Count > 0)
        {
          var first = coveoResponse.results[0];
          Console.WriteLine("  First result title: " + first.title);
          Console.WriteLine("  First result title: " + first.uri);
          Console.WriteLine("  First result title: " + first.printableUri);
          //if (first.raw != null && first.raw.Count > 0)
          //{
          //    Console.WriteLine("  Raw fields:");
          //    foreach (var kvp in first.raw)
          //    {
          //        Console.WriteLine(string.Format("    {0} = {1}", kvp.Key, kvp.Value));
          //    }
          //}
        }

        return new VerifyResult { Label = label, Exists = true, Count = coveoResponse.totalCount };
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(string.Format("[{0}] Data NOT found — 0 results returned.", label));
        Console.ResetColor();
        return new VerifyResult { Label = label, Exists = false, Count = 0 };
      }
    }
  }
}
