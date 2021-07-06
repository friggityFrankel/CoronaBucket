using System;

namespace Classes
{
    public class JsonDailyData
    {
        // pasted from https://covid19.mathdro.id/api/daily/MM-dd-yyyy/
        public DateTime date { get; set; }
        public string fips { get; set; }
        public string admin2 { get; set; }
        public string provinceState { get; set; }
        public string countryRegion { get; set; }
        public string lastUpdate { get; set; }
        public string lat { get; set; }
        public string _long { get; set; }
        public string confirmed { get; set; }
        public string deaths { get; set; }
        public string recovered { get; set; }
        public string active { get; set; }
        public string combinedKey { get; set; }
        public string incidentRate { get; set; }
        public string caseFatalityRatio { get; set; }
    }
}