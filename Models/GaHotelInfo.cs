using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace CoveoVerifierTool.Models
{
  public class GaHotelInfo
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Rating { get; set; }
    public string Distance { get; set; }
    public string Brand { get; set; }
    public string Location { get; set; }
    public string SpecialHotelInclusion { get; set; }
    public object PremiumVacationTags { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string SpecialNotes { get; set; }
    public string YasExpress { get; set; }
    public string YasExpressLink { get; set; }
    public object BreakfastSavings { get; set; }
    public object ExcludeHotelFromDiscount { get; set; }
    public object UseDefaultDiscount { get; set; }
    public object PropertyType { get; set; }
    public string LocationId { get; set; }
    public object HotelLocation { get; set; }
    public string HotelLocationId { get; set; }
    public string PropertyTypeId { get; set; }
    public object Tags { get; set; }
    public object Facilities { get; set; }
    public object YasNeighbourhoodBenefits { get; set; }
    public object YasNeighbourhoodProgram { get; set; }
    public object Images { get; set; }
    public object DigitalPass { get; set; }
    public string HotelUrl { get; set; }
    public object HotelUSPs { get; set; }
    public object HotelAmenities { get; set; }
    public object PremiumFreeInclusions { get; set; }
    public string FreeInclusionSectionTitle { get; set; }
    public object FreeInclusions { get; set; }
    public string DefaultSectionTitle { get; set; }
    public string DefaultSection { get; set; }
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string SriggleId { get; set; }
    public string SriggleCode { get; set; }
    public string SriggleName { get; set; }
    public object SriggleActive { get; set; }

    public static readonly string[] ExpectedKeys = new[]
    {
      "Name", "Description", "Rating", "Distance", "Brand", "Location",
      "SpecialHotelInclusion", "PremiumVacationTags", "Latitude", "Longitude",
      "SpecialNotes", "YasExpress", "YasExpressLink", "BreakfastSavings",
      "ExcludeHotelFromDiscount", "UseDefaultDiscount", "PropertyType", "LocationId",
      "HotelLocation", "HotelLocationId", "PropertyTypeId", "Tags", "Facilities",
      "YasNeighbourhoodBenefits", "YasNeighbourhoodProgram", "Images", "DigitalPass",
      "HotelUrl", "HotelUSPs", "HotelAmenities", "PremiumFreeInclusions",
      "FreeInclusionSectionTitle", "FreeInclusions", "DefaultSectionTitle",
      "DefaultSection", "ItemId", "ItemName", "SriggleId", "SriggleCode",
      "SriggleName", "SriggleActive"
    };

    /// <summary>
    /// Parses the fgahotelinfo50416 Coveo field (a JSON array of {name, value} pairs).
    /// Returns the populated GaHotelInfo and the raw key/value lookup dictionary.
    /// </summary>
    public static Tuple<GaHotelInfo, Dictionary<string, object>> DeserializeGaHotelInfo(string json)
    {
      var info = new GaHotelInfo();
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

      info.Name                      = Str(dict, "Name");
      info.Description               = Str(dict, "Description");
      info.Rating                    = Str(dict, "Rating");
      info.Distance                  = Str(dict, "Distance");
      info.Brand                     = Str(dict, "Brand");
      info.Location                  = Str(dict, "Location");
      info.SpecialHotelInclusion     = Str(dict, "SpecialHotelInclusion");
      info.PremiumVacationTags       = Val(dict, "PremiumVacationTags");
      info.Latitude                  = Str(dict, "Latitude");
      info.Longitude                 = Str(dict, "Longitude");
      info.SpecialNotes              = Str(dict, "SpecialNotes");
      info.YasExpress                = Str(dict, "YasExpress");
      info.YasExpressLink            = Str(dict, "YasExpressLink");
      info.BreakfastSavings          = Val(dict, "BreakfastSavings");
      info.ExcludeHotelFromDiscount  = Val(dict, "ExcludeHotelFromDiscount");
      info.UseDefaultDiscount        = Val(dict, "UseDefaultDiscount");
      info.PropertyType              = Val(dict, "PropertyType");
      info.LocationId                = Str(dict, "LocationId");
      info.HotelLocation             = Val(dict, "HotelLocation");
      info.HotelLocationId           = Str(dict, "HotelLocationId");
      info.PropertyTypeId            = Str(dict, "PropertyTypeId");
      info.Tags                      = Val(dict, "Tags");
      info.Facilities                = Val(dict, "Facilities");
      info.YasNeighbourhoodBenefits  = Val(dict, "YasNeighbourhoodBenefits");
      info.YasNeighbourhoodProgram   = Val(dict, "YasNeighbourhoodProgram");
      info.Images                    = Val(dict, "Images");
      info.DigitalPass               = Val(dict, "DigitalPass");
      info.HotelUrl                  = Str(dict, "HotelUrl");
      info.HotelUSPs                 = Val(dict, "HotelUSPs");
      info.HotelAmenities            = Val(dict, "HotelAmenities");
      info.PremiumFreeInclusions     = Val(dict, "PremiumFreeInclusions");
      info.FreeInclusionSectionTitle = Str(dict, "FreeInclusionSectionTitle");
      info.FreeInclusions            = Val(dict, "FreeInclusions");
      info.DefaultSectionTitle       = Str(dict, "DefaultSectionTitle");
      info.DefaultSection            = Str(dict, "DefaultSection");
      info.ItemId                    = Str(dict, "ItemId");
      info.ItemName                  = Str(dict, "ItemName");
      info.SriggleId                 = Str(dict, "SriggleId");
      info.SriggleCode               = Str(dict, "SriggleCode");
      info.SriggleName               = Str(dict, "SriggleName");
      info.SriggleActive             = Val(dict, "SriggleActive");

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
