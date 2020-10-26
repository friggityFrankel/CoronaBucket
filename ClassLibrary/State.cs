using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary
{
    public class State
    {
        public string Name;
        public string Abbr;
        public int Population;
        public CaseNumbers CurrentCases;
        public List<CaseNumbers> Cases;

        public State(string name)
        {
            Name = name;
            CurrentCases = new CaseNumbers(DateTime.Today);
            Cases = new List<CaseNumbers>();
        }

        public CaseNumbers Change(DateTime date)
        {
            CaseNumbers change = new CaseNumbers(date);
            var c1 = Cases.SingleOrDefault(c => c.Date == date);
            var c2 = Cases.SingleOrDefault(c => c.Date == date.AddDays(-1));

            if (c1 != null && c2 != null)
            {
                change.Confirmed = c1.Confirmed - c2.Confirmed;
                change.Deaths = c1.Deaths - c2.Deaths;
                change.Recovered = c1.Recovered - c2.Recovered;
            }

            return change;
        }
        public CaseNumbers ChangeSince(DateTime date)
        {
            CaseNumbers change = new CaseNumbers(DateTime.Today);
            CaseNumbers previousCases = Cases.SingleOrDefault(c => c.Date == date);

            if (CurrentCases != null && previousCases != null)
            {
                change.Confirmed = CurrentCases.Confirmed - previousCases.Confirmed;
                change.Deaths = CurrentCases.Deaths - previousCases.Deaths;
                change.Recovered = CurrentCases.Recovered - previousCases.Recovered;
            }

            return change;
        }

        public CaseNumbers CurrentPercentage
        {
            get
            {
                CaseNumbers percentages = new CaseNumbers(DateTime.Today);
                if (Population > 0)
                {
                    percentages.Confirmed = CurrentCases.Confirmed * 100 / Population;
                    percentages.Deaths = CurrentCases.Deaths * 100 / Population;
                    percentages.Recovered = CurrentCases.Recovered * 100 / Population;
                }
                return percentages;
            }
        }
    }
}