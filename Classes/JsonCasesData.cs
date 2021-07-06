using System;

namespace Classes
{
    public class JsonCasesData
    {
        public ResultNumbers confirmed { get; set; }
        public ResultNumbers recovered { get; set; }
        public ResultNumbers deaths { get; set; }
        public string dailySummary { get; set; }
        public ResultDetail dailyTimeSeries { get; set; }
        public string image { get; set; }
        public string source { get; set; }
        public string countries { get; set; }
        public ResultDetail countryDetail { get; set; }
        public DateTime lastUpdate { get; set; }
    }

    public class ResultNumbers
    {
        public int value { get; set; }
        public string detail { get; set; }
    }

    public class ResultDetail
    {
        public string pattern { get; set; }
        public string example { get; set; }
    }
}