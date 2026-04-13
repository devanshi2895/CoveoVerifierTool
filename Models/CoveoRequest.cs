using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace CoveoVerifierTool.Models
{
    public class CoveoRequest
    {
        public string aq { get; set; }
        public List<string> fieldsToInclude { get; set; }
        public List<string> fieldsToExclude { get; set; }
        public int numberOfResults { get; set; } = 999;

        public CoveoRequest()
        {
            fieldsToInclude = new List<string>();
            fieldsToExclude = new List<string>();
        }

        public string ToJson()
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }
    }
}
