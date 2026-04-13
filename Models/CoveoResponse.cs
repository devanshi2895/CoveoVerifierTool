using System.Collections.Generic;

namespace CoveoVerifierTool.Models
{
  public class CoveoResponse
  {
    public int totalCount { get; set; }
    public int totalCountFiltered { get; set; }
    public List<CoveoResult> results { get; set; }

    public CoveoResponse()
    {
      results = new List<CoveoResult>();
    }
  }

  public class CoveoResult
  {
    public string title { get; set; }
    public string uri { get; set; }
    public string printableUri { get; set; }
    public Dictionary<string, object> raw { get; set; }

    public CoveoResult()
    {
      raw = new Dictionary<string, object>();
    }
  }
}
