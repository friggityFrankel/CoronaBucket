using System;
using System.Text.Json.Serialization;

namespace ClassLibrary
{
    public class ResultData
    {
        // pasted from https://covid19.mathdro.id/api/daily/MM-dd-yyyy/

        public DateTime date { get; set; }
        public string fips { get; set; }
        public string admin2 { get; set; }
        public string provinceState { get; set; }
        public string countryRegion { get; set; }
        public string lastUpdate { get; set; }

        [JsonPropertyName("lat")]
        public string latitude { get; set; }

        [JsonPropertyName("long")]
        public string longitude { get; set; }
        public string confirmed { get; set; }
        public string deaths { get; set; }
        public string recovered { get; set; }
        public string active { get; set; }
        public string combinedKey { get; set; }
    }
}
