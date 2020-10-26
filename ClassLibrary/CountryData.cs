using System.Collections.Generic;

namespace ClassLibrary
{
    public class CountryData
    {
        // pasted from https://covid19.mathdro.id/api/countries/
        public List<CountryInfo> countries { get; set; }
    }

    public class CountryInfo
    {
        public string name { get; set; }
        public string iso2 { get; set; }
        public string iso3 { get; set; }
    }
}
