using System;

namespace ClassLibrary
{
    public class CountryResultData
    {
        // pasted from https://covid19.mathdro.id/api/countries/{country}

        public ResultNumbers confirmed { get; set; }
        public ResultNumbers recovered { get; set; }
        public ResultNumbers deaths { get; set; }
        public DateTime lastUpdate { get; set; }
    }
}