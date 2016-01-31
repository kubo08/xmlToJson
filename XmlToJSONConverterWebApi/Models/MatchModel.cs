using System.Collections.Generic;

namespace XmlToJSONConverterWebApi.Models
{
    public class MatchModel
    {
        public long fixtureId { get; set; }
        public List<object> Events { get; set; }
    }
}