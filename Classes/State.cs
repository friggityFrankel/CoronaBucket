using System.Collections.Generic;
using System.Linq;

namespace Classes
{
    public class State
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public int Population { get; set; }
        public List<DailyNumbers> DailyNumbers { get; set; }

        public DailyNumbers Change
        {
            get
            {
                var list = DailyNumbers.OrderByDescending(d => d.Date);
                var change = new DailyNumbers();
                var c1 = list.First();
                var c2 = list.Skip(1).First();

                if (c1 != null && c2 != null)
                {
                    change.Date = c1.Date;
                    change.Confirmed = c1.Confirmed - c2.Confirmed;
                    change.Deaths = c1.Deaths - c2.Deaths;
                    change.Resolved = c1.Resolved - c2.Resolved;
                    change.TotalDoses = c1.TotalDoses - c2.TotalDoses;
                    change.FirstDose = c1.FirstDose - c2.FirstDose;
                    change.FullyDosed = c1.FullyDosed - c2.FullyDosed;
                }

                return change;
            }
        }

        public DailyNumbers Percent
        {
            get
            {
                var percent = new DailyNumbers();
                var p1 = DailyNumbers.OrderByDescending(d => d.Date).First();

                if (Population > 0 && p1 != null)
                {
                    percent.Date = p1.Date;
                    percent.Confirmed = p1.Confirmed * 100.0 / Population;
                    percent.Deaths = p1.Deaths * 100.0 / Population;
                    percent.Resolved = p1.Resolved * 100.0 / Population;
                    percent.TotalDoses = p1.TotalDoses * 100.0 / Population;
                    percent.FirstDose = p1.FirstDose * 100.0 / Population;
                    percent.FullyDosed = p1.FullyDosed * 100.0 / Population;
                }

                return percent;
            }
        }

        public DailyNumbers Trend
        {
            get
            {
                var count = (DailyNumbers.Count >= 7) ? 7 : DailyNumbers.Count;
                var numbs = DailyNumbers.OrderByDescending(d => d.Date).Take(count);
                var trend = new DailyNumbers()
                {
                    Date = numbs.First().Date,
                    Confirmed = numbs.Average(n => n.Confirmed),
                    Deaths = numbs.Average(n => n.Deaths),
                    Resolved = numbs.Average(n => n.Resolved),
                    TotalDoses = numbs.Average(n => n.TotalDoses),
                    FirstDose = numbs.Average(n => n.FirstDose),
                    FullyDosed = numbs.Average(n => n.FullyDosed)
                };

                return trend;
            }
        }
    }
}
