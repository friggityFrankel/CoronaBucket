using System.Collections.Generic;
using System.Linq;

namespace Classes
{
    public class State
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public double Population { get; set; }
        public List<DailyNumbers> DailyNumbers { get; set; }

        public State()
        {
            DailyNumbers = new List<DailyNumbers>();
        }

        public DailyNumbers Current
        {
            get
            {
                var current = new DailyNumbers();
                if (DailyNumbers.Count > 0)
                {
                    current = DailyNumbers.OrderByDescending(d => d.Date).First();
                }
                return current;
            }
        }

        public DailyNumbers Change
        {
            get
            {
                var c1 = new DailyNumbers();
                var c2 = new DailyNumbers();
                if (DailyNumbers.Count > 0)
                {
                    c1 = DailyNumbers.OrderByDescending(d => d.Date).First();
                    if (DailyNumbers.Count > 1)
                    {
                        c2 = DailyNumbers.OrderByDescending(d => d.Date).Skip(1).First();
                    }
                    else
                    {
                        c2 = c1;
                    }
                }
                return ChangeOf(c1, c2);
            }
        }

        public DailyNumbers ChangeOf(DailyNumbers c1, DailyNumbers c2)
        {
            return new DailyNumbers()
            {
                Date = c1.Date,
                Confirmed = c1.Confirmed - c2.Confirmed,
                Deaths = c1.Deaths - c2.Deaths,
                Recovered = c1.Recovered - c2.Recovered,
                DosesTotal = c1.DosesTotal - c2.DosesTotal,
                DosesFirst = c1.DosesFirst - c2.DosesFirst,
                DosesFully = c1.DosesFully - c2.DosesFully
            };
        }

        public DailyNumbers Percent
        {
            get
            {
                var percent = new DailyNumbers();
                if (DailyNumbers.Count > 0 && Population > 0)
                {
                    var p1 = DailyNumbers.OrderByDescending(d => d.Date).First();
                    percent.Date = p1.Date;
                    percent.Confirmed = p1.Confirmed * 100.0 / Population;
                    percent.Deaths = p1.Deaths * 100.0 / Population;
                    percent.Recovered = p1.Recovered * 100.0 / Population;
                    percent.DosesTotal = p1.DosesTotal * 100.0 / Population;
                    percent.DosesFirst = p1.DosesFirst * 100.0 / Population;
                    percent.DosesFully = p1.DosesFully * 100.0 / Population;
                }

                return percent;
            }
        }

        public DailyNumbers Trend
        {
            get
            {
                var trend = new DailyNumbers();
                if (DailyNumbers.Count > 0)
                {
                    var count = (DailyNumbers.Count >= 7) ? 7 : DailyNumbers.Count;
                    var numbs = DailyNumbers.OrderByDescending(d => d.Date).Take(count).ToList();
                    var changes = new List<DailyNumbers>();
                    for (int i = 0; i < count - 1; i++)
                    {
                        var c1 = numbs[i];
                        var c2 = numbs[i + 1];
                        changes.Add(ChangeOf(c1, c2));
                    }

                    trend.Date = changes.First().Date;
                    trend.Confirmed = changes.Average(n => n.Confirmed);
                    trend.Deaths = changes.Average(n => n.Deaths);
                    trend.Recovered = changes.Average(n => n.Recovered);
                    trend.DosesTotal = changes.Average(n => n.DosesTotal);
                    trend.DosesFirst = changes.Average(n => n.DosesFirst);
                    trend.DosesFully = changes.Average(n => n.DosesFully);
                }

                return trend;
            }
        }
    }
}
