﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary
{
    public class Region
    {
        public string Name;
        public string Iso2;
        public List<State> States;
        public CaseNumbers CurrentCases;

        public Region(string name)
        {
            Name = name;
            States = new List<State>();
            CurrentCases = new CaseNumbers(DateTime.Today);
        }

        public CaseNumbers Cases(DateTime date)
        {
            CaseNumbers caseNumbers = new CaseNumbers(date);

            foreach (var state in States)
            {
                var stateCases = state.Cases.SingleOrDefault(c => c.Date == date);
                if (stateCases != null)
                {
                    caseNumbers.Confirmed += stateCases.Confirmed;
                    caseNumbers.Deaths += stateCases.Deaths;
                    caseNumbers.Recovered += stateCases.Recovered;
                }
            }

            return caseNumbers;
        }

        public CaseNumbers Change(DateTime date)
        {
            CaseNumbers change = new CaseNumbers(date);
            CaseNumbers c1 = Cases(date);
            CaseNumbers c2 = Cases(date.AddDays(-1));

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
            CaseNumbers previousCases = Cases(date);

            if (CurrentCases != null && previousCases != null)
            {
                change.Confirmed = CurrentCases.Confirmed - previousCases.Confirmed;
                change.Deaths = CurrentCases.Deaths - previousCases.Deaths;
                change.Recovered = CurrentCases.Recovered - previousCases.Recovered;
            }

            return change;
        }
    }
}