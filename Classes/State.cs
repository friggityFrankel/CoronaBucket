using System.Collections.Generic;
using System.Globalization;
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
                var c1 = Current;
                var c2 = new DailyNumbers();
                if (DailyNumbers.Count > 1)
                {
                    var cases = DailyNumbers.OrderByDescending(d => d.Date).Skip(1).Where(d => d.Confirmed > 0).FirstOrDefault();
                    var vacs = DailyNumbers.OrderByDescending(d => d.Date).Skip(1).Where(d => d.DosesTotal > 0).FirstOrDefault();
                    if (cases != null)
                    {
                        c2.Confirmed = cases.Confirmed;
                        c2.Deaths = cases.Deaths;
                        c2.Recovered = cases.Recovered;
                    }
                    if (vacs != null)
                    {
                        c2.DosesTotal = vacs.DosesTotal;
                        c2.DosesFirst = vacs.DosesFirst;
                        c2.DosesFully = vacs.DosesFully;
                    }
                }
                else
                {
                    c2 = c1;
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

        public List<string> ToPost
        {
            get
            {
                var lines = new List<string>();
                if (Name == "World")
                {
                    lines.Add($"b{{World Wide}}b totals s[pop. {Population.ToString("N0", CultureInfo.CurrentCulture)}]s:");
                    lines.Add("/[Total s[Percent]s (Change | Trend)]/");
                    lines.Add($"*[n[Total Doses Administered]n]*: {Current.DosesTotal.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.DosesTotal.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.DosesTotal.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"*[b{{Fully Vaccinated}}b]*: {Current.DosesFully.ToString("N0", CultureInfo.CurrentCulture)} s[{Percent.DosesFully.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{Change.DosesFully.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.DosesFully.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add("-----");
                    lines.Add($"y{{Cases}}y: {Current.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} s[{Percent.Confirmed.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{Change.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.Confirmed.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"r{{Deaths}}r: {Current.Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.Deaths.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.Deaths.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"g{{Recovered}}g: {Current.Recovered.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.Recovered.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.Recovered.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"p[Unresolved]p: {Current.Unresolved.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.Unresolved.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.Unresolved.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add("");
                }
                else
                {
                    if (Name == "US")
                    {
                        lines.Add("r{U}rb[S]bb{A}b s[pop. " + Population.ToString("N0", CultureInfo.CurrentCulture) + "]s");
                    }
                    else
                    {
                        lines.Add($"b[{Name}]b s[pop. {Population.ToString("N0", CultureInfo.CurrentCulture)}]s");
                    }
                    lines.Add($"• Total Doses Administered: {Current.DosesTotal.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.DosesTotal.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.DosesTotal.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"• At Least 1st Dose: {Current.DosesFirst.ToString("N0", CultureInfo.CurrentCulture)} s[{Percent.DosesFirst.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{Change.DosesFirst.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.DosesFirst.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"• Fully Vaccinated: {Current.DosesFully.ToString("N0", CultureInfo.CurrentCulture)} s[{Percent.DosesFully.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{Change.DosesFully.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.DosesFully.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add("---");
                    lines.Add($"• Cases: {Current.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} s[{Percent.Confirmed.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{Change.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.Confirmed.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"• Deaths: {Current.Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.Deaths.ToString("N0", CultureInfo.CurrentCulture)} | +{Trend.Deaths.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add("");
                }
                return lines;
            }
        }
    }
}
