using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes
{
    public class Country
    {
        public string Name { get; set; }
        public double Population { get { return Numbers.OrderByDescending(c => c.Date).First().Population; } }
        public List<CountryNumbers> Numbers { get; set; }

        public Country()
        {
            Numbers = new List<CountryNumbers>();
        }

        public double DoseTotal { get { return Numbers.OrderByDescending(c => c.Date).First().DoseTotal; } }
        public double DoseTotalNew
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    return Numbers.OrderByDescending(c => c.Date).First().DoseTotal - Numbers.OrderByDescending(c => c.Date).Skip(1).First().DoseTotal;
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseTotal;
                }
            }
        }
        public double DoseTotalSmooth
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    var smooth = Numbers.OrderByDescending(c => c.Date).Take(7).ToList();
                    var changes = new List<double>();
                    for (var i = 0; i < smooth.Count - 1; i++)
                    {
                        var change = smooth[i].DoseTotal - smooth[i + 1].DoseTotal;
                        changes.Add(change);
                    }
                    return changes.Average(c => c);
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseTotal;
                }
            }
        }
        public double DoseFirst { get { return Numbers.OrderByDescending(c => c.Date).First().DoseFirst; } }
        public double DoseFirstPercent { get { return Numbers.OrderByDescending(c => c.Date).First().DoseFirst / Population * 100.0; } }
        public double DoseFirstNew
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    return Numbers.OrderByDescending(c => c.Date).First().DoseFirst - Numbers.OrderByDescending(c => c.Date).Skip(1).First().DoseFirst;
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseFirst;
                }
            }
        }
        public double DoseFirstSmooth
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    var smooth = Numbers.OrderByDescending(c => c.Date).Take(7).ToList();
                    var changes = new List<double>();
                    for (var i = 0; i < smooth.Count - 1; i++)
                    {
                        var change = smooth[i].DoseFirst - smooth[i + 1].DoseFirst;
                        changes.Add(change);
                    }
                    return changes.Average(c => c);
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseFirst;
                }
            }
        }
        public double DoseFully { get { return Numbers.OrderByDescending(c => c.Date).First().DoseFully; } }
        public double DoseFullyPercent { get { return Numbers.OrderByDescending(c => c.Date).First().DoseFully / Population * 100.0; } }
        public double DoseFullyNew
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    return Numbers.OrderByDescending(c => c.Date).First().DoseFully - Numbers.OrderByDescending(c => c.Date).Skip(1).First().DoseFully;
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseFully;
                }
            }
        }
        public double DoseFullySmooth
        {
            get
            {

                if (Numbers.Count > 1)
                {
                    var smooth = Numbers.OrderByDescending(c => c.Date).Take(7).ToList();
                    var changes = new List<double>();
                    for (var i = 0; i < smooth.Count - 1; i++)
                    {
                        var change = smooth[i].DoseFully - smooth[i + 1].DoseFully;
                        changes.Add(change);
                    }
                    return changes.Average(c => c);
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseFully;
                }
            }
        }
        public double DoseBooster { get { return Numbers.OrderByDescending(c => c.Date).First().DoseBooster; } }
        public double DoseBoosterPercent { get { return Numbers.OrderByDescending(c => c.Date).First().DoseBooster / Population * 100.0; } }
        public double DoseBoosterNew
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    return Numbers.OrderByDescending(c => c.Date).First().DoseBooster - Numbers.OrderByDescending(c => c.Date).Skip(1).First().DoseBooster;
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseBooster;
                }
            }
        }
        public double DoseBoosterSmooth
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    var smooth = Numbers.OrderByDescending(c => c.Date).Take(7).ToList();
                    var changes = new List<double>();
                    for (var i = 0; i < smooth.Count - 1; i++)
                    {
                        var change = smooth[i].DoseBooster - smooth[i + 1].DoseBooster;
                        changes.Add(change);
                    }
                    return changes.Average(c => c);
                }
                else
                {
                    return Numbers.FirstOrDefault().DoseBooster;
                }
            }
        }
        public double Confirmed { get { return Numbers.OrderByDescending(c => c.Date).First().Confirmed; } }
        public double ConfirmedPercent { get { return Numbers.OrderByDescending(c => c.Date).First().Confirmed / Population * 100.0; } }
        public double ConfirmedNew
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    return Numbers.OrderByDescending(c => c.Date).First().Confirmed - Numbers.OrderByDescending(c => c.Date).Skip(1).First().Confirmed;
                }
                else
                {
                    return Numbers.FirstOrDefault().Confirmed;
                }

            }
        }
        public double ConfirmedSmooth
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    var smooth = Numbers.OrderByDescending(c => c.Date).Take(7).ToList();
                    var changes = new List<double>();
                    for (var i = 0; i < smooth.Count - 1; i++)
                    {
                        var change = smooth[i].Confirmed - smooth[i + 1].Confirmed;
                        changes.Add(change);
                    }
                    return changes.Average(c => c);
                }
                else
                {
                    return Numbers.FirstOrDefault().Confirmed;
                }

            }
        }
        public double Deaths { get { return Numbers.OrderByDescending(c => c.Date).First().Deaths; } }
        public double DeathsNew
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    return Numbers.OrderByDescending(c => c.Date).First().Deaths - Numbers.OrderByDescending(c => c.Date).Skip(1).First().Deaths;
                }
                else
                {
                    return Numbers.FirstOrDefault().Deaths;
                }
            }
        }
        public double DeathsSmooth
        {
            get
            {
                if (Numbers.Count > 1)
                {
                    var smooth = Numbers.OrderByDescending(c => c.Date).Take(7).ToList();
                    var changes = new List<double>();
                    for (var i = 0; i < smooth.Count - 1; i++)
                    {
                        var change = smooth[i].Deaths - smooth[i + 1].Deaths;
                        changes.Add(change);
                    }
                    return changes.Average(c => c);
                }
                else
                {
                    return Numbers.FirstOrDefault().Deaths;
                }
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
                    lines.Add("/[Total s[Percent]s (Change | 7-Day Average)]/");
                    lines.Add($"*[n[Total Doses Administered]n]*: {DoseTotal.ToString("N0", CultureInfo.CurrentCulture)} (+{DoseTotalNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DoseTotalSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"*[b{{Fully Vaccinated}}b]*: {DoseFully.ToString("N0", CultureInfo.CurrentCulture)} s[{DoseFullyPercent.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{DoseFullyNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DoseFullySmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"*[g{{Boosters Administered}}g]*: {DoseBooster.ToString("N0", CultureInfo.CurrentCulture)} s[{DoseBoosterPercent.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{DoseBoosterNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DoseBoosterSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add("-----");
                    lines.Add($"y{{Cases}}y: {Confirmed.ToString("N0", CultureInfo.CurrentCulture)} s[{ConfirmedPercent.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{ConfirmedNew.ToString("N0", CultureInfo.CurrentCulture)} | +{ConfirmedSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"r{{Deaths}}r: {Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{DeathsNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DeathsSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    //lines.Add($"g{{Recovered}}g: {Current.Recovered.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.Recovered.ToString("N0", CultureInfo.CurrentCulture)} | +{SevenDay.Recovered.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    //lines.Add($"p[Unresolved]p: {Current.Unresolved.ToString("N0", CultureInfo.CurrentCulture)} (+{Change.Unresolved.ToString("N0", CultureInfo.CurrentCulture)} | +{SevenDay.Unresolved.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add("");
                }
                else
                {
                    if (Name == "United States")
                    {
                        lines.Add("r{U}rb[S]bb{A}b s[pop. " + Population.ToString("N0", CultureInfo.CurrentCulture) + "]s");
                    }
                    else
                    {
                        lines.Add($"b[{Name}]b s[pop. {Population.ToString("N0", CultureInfo.CurrentCulture)}]s");
                    }
                    lines.Add($"• Total Doses Administered: {DoseTotal.ToString("N0", CultureInfo.CurrentCulture)} (+{DoseTotalNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DoseTotalSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"• At Least 1st Dose: {DoseFirst.ToString("N0", CultureInfo.CurrentCulture)} s[{DoseFirstPercent.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{DoseFirstNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DoseFirstSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"• Fully Vaccinated: {DoseFully.ToString("N0", CultureInfo.CurrentCulture)} s[{DoseFullyPercent.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{DoseFullyNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DoseFullySmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    if (DoseBooster > 0)
                    {
                        lines.Add($"• Boosters Administered: {DoseBooster.ToString("N0", CultureInfo.CurrentCulture)} s[{DoseBoosterPercent.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{DoseBoosterNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DoseBoosterSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    }

                    lines.Add("---");
                    lines.Add($"• Cases: {Confirmed.ToString("N0", CultureInfo.CurrentCulture)} s[{ConfirmedPercent.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{ConfirmedNew.ToString("N0", CultureInfo.CurrentCulture)} | +{ConfirmedSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add($"• Deaths: {Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{DeathsNew.ToString("N0", CultureInfo.CurrentCulture)} | +{DeathsSmooth.ToString("N0", CultureInfo.CurrentCulture)})".Replace("+-", "-"));
                    lines.Add("");
                }
                return lines;
            }
        }
    }
}
