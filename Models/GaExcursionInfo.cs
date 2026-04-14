using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace CoveoVerifierTool.Models
{
  public class GaExcursionInfo
  {
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string Description { get; set; }
    public string ContractId { get; set; }
    public string Reference { get; set; }
    public object Tags { get; set; }
    public string Location { get; set; }
    public string SpecialNotes { get; set; }
    public object EventDiscount { get; set; }
    public object TitleLogos { get; set; }
    public object Images { get; set; }
    public string AdditionalEssentialInformation { get; set; }
    public string EssentialInformation { get; set; }
    public string CTA { get; set; }
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string SriggleId { get; set; }
    public string SriggleCode { get; set; }
    public string SriggleName { get; set; }
    public object SriggleActive { get; set; }

    public static readonly string[] ExpectedKeys = new[]
    {
      "Name", "ShortDescription", "Description", "ContractId", "Reference",
      "Tags", "Location", "SpecialNotes", "EventDiscount", "TitleLogos",
      "Images", "AdditionalEssentialInformation", "EssentialInformation",
      "CTA", "ItemId", "ItemName", "SriggleId", "SriggleCode",
      "SriggleName", "SriggleActive"
    };

    /// <summary>
    /// Parses the fgaez120xcursioninfo50416 Coveo field (a JSON array of {name, value} pairs).
    /// Returns the populated GaExcursionInfo and the raw key/value lookup dictionary.
    /// </summary>
    public static Tuple<GaExcursionInfo, Dictionary<string, object>> DeserializeGaExcursionInfo(string json)
    {
      var info = new GaExcursionInfo();
      var dict = new Dictionary<string, object>();

      if (string.IsNullOrWhiteSpace(json))
        return Tuple.Create(info, dict);

      var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
      List<Dictionary<string, object>> entries;
      try { entries = serializer.Deserialize<List<Dictionary<string, object>>>(json); }
      catch { return Tuple.Create(info, dict); }

      if (entries == null) return Tuple.Create(info, dict);

      foreach (var entry in entries)
      {
        object nameObj;
        if (entry.TryGetValue("name", out nameObj) && nameObj != null)
        {
          string key = nameObj.ToString();
          object val;
          entry.TryGetValue("value", out val);
          dict[key] = val;
        }
      }

      info.Name                           = Str(dict, "Name");
      info.ShortDescription               = Str(dict, "ShortDescription");
      info.Description                    = Str(dict, "Description");
      info.ContractId                     = Str(dict, "ContractId");
      info.Reference                      = Str(dict, "Reference");
      info.Tags                           = Val(dict, "Tags");
      info.Location                       = Str(dict, "Location");
      info.SpecialNotes                   = Str(dict, "SpecialNotes");
      info.EventDiscount                  = Val(dict, "EventDiscount");
      info.TitleLogos                     = Val(dict, "TitleLogos");
      info.Images                         = Val(dict, "Images");
      info.AdditionalEssentialInformation = Str(dict, "AdditionalEssentialInformation");
      info.EssentialInformation           = Str(dict, "EssentialInformation");
      info.CTA                            = Str(dict, "CTA");
      info.ItemId                         = Str(dict, "ItemId");
      info.ItemName                       = Str(dict, "ItemName");
      info.SriggleId                      = Str(dict, "SriggleId");
      info.SriggleCode                    = Str(dict, "SriggleCode");
      info.SriggleName                    = Str(dict, "SriggleName");
      info.SriggleActive                  = Val(dict, "SriggleActive");

      return Tuple.Create(info, dict);
    }

    public static List<string> GetMissingKeys(Dictionary<string, object> dict)
    {
      var missing = new List<string>();
      foreach (string key in ExpectedKeys)
      {
        object val;
        if (!dict.TryGetValue(key, out val) || val == null || (val is string && (string)val == ""))
          missing.Add(key);
      }
      return missing;
    }

    private static string Str(Dictionary<string, object> dict, string key)
    {
      object val;
      if (!dict.TryGetValue(key, out val) || val == null) return null;
      return val.ToString();
    }

    private static object Val(Dictionary<string, object> dict, string key)
    {
      object val;
      dict.TryGetValue(key, out val);
      return val;
    }
  }
}
