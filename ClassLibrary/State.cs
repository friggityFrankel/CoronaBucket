﻿using System;
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
        public List<VaccinationNumbers> Vaccinations;

        public State(string name)
        {
            Name = name;
            CurrentCases = new CaseNumbers(DateTime.Today);
            Cases = new List<CaseNumbers>();
            Vaccinations = new List<VaccinationNumbers>();
        }

        public CaseNumbers TrendingCases()
        {

            var trend = new CaseNumbers(DateTime.Today);
            var count = (Cases.Count >= 7) ? 7 : Cases.Count;
            var cases = Cases.OrderByDescending(x => x.Date).Take(count);

            trend.Confirmed = cases.Average(x => x.Confirmed);
            trend.Deaths = cases.Average(x => x.Deaths);
            trend.Recovered = cases.Average(x => x.Recovered);

            return trend;
        }

        public VaccinationNumbers TrendingVaccines()
        {
            var trend = new VaccinationNumbers(DateTime.Today);
            var count = (Vaccinations.Count >= 7) ? 7 : Vaccinations.Count;
            var vaccs = Vaccinations.OrderByDescending(v => v.Date).Take(count);

            trend.Total = vaccs.Average(x => x.Total);
            trend.Daily = vaccs.Average(x => x.Daily);
            trend.Fully = vaccs.Average(x => x.Fully);

            return trend;
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
                    percentages.Confirmed = CurrentCases.Confirmed * 100.0 / Population;
                    percentages.Deaths = CurrentCases.Deaths * 100.0 / Population;
                    percentages.Recovered = CurrentCases.Recovered * 100.0 / Population;
                }
                return percentages;
            }
        }

        public VaccinationNumbers VaccineChange()
        {
            var c1 = Vaccinations.OrderByDescending(v => v.Date).FirstOrDefault();
            if (c1 == null)
            {
                return new VaccinationNumbers(DateTime.Now);
            }
            var change = new VaccinationNumbers(c1.Date);

            if (c1 != null)
            {
                change.Total = c1.Total;
                change.Partial = c1.Partial;
                change.Fully = c1.Fully;
            }

            var c2 = Vaccinations.OrderByDescending(v => v.Date).Skip(1).FirstOrDefault();

            if (c2 != null)
            {
                change.Total -= c2.Total;
                change.Partial -= c2.Partial;
                change.Fully -= c2.Fully;
            }

            return change;
        }

        public VaccinationNumbers VaccinePercentage()
        {
            var percentages = new VaccinationNumbers(DateTime.Today);

            if (Population > 0)
            {
                var vacs = Vaccinations.OrderByDescending(v => v.Date).FirstOrDefault();
                if (vacs != null)
                {
                    percentages.Total = vacs.Total * 100.0 / Population;
                    percentages.Partial = vacs.Partial * 100.0 / Population;
                    percentages.Fully = vacs.Fully * 100.0 / Population;
                }
            }

            return percentages;
        }
    }
}