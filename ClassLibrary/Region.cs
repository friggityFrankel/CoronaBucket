using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary
{
    public class Region
    {
        public string Name;
        public string Iso2;
        public string Iso3;
        public List<State> States;
        public List<VaccinationNumbers> Vaccinations;
        public CaseNumbers CurrentCases;
        public double Population;

        public Region(string name)
        {
            Name = name;
            States = new List<State>();
            Vaccinations = new List<VaccinationNumbers>();
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

        public VaccinationNumbers VaccineChange()
        {
            var c1 = Vaccinations.OrderByDescending(v => v.Date).FirstOrDefault();
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