using System;
using System.Collections.Generic;
using System.Linq;

namespace Classes
{
    public class Country
    {
        public string Name { get; set; }
        public string Iso2 { get; set; }
        public string Iso3 { get; set; }
        public List<State> States { get; set; }

        public DailyNumbers GetDailyNumbers(DateTime date)
        {
            var dailyNumbers = new DailyNumbers(date);
            return dailyNumbers;
        }
    }
}