using System;

namespace Classes
{
    public class DailyNumbers
    {
        public DateTime Date { get; set; }
        public double Confirmed { get; set; }
        public double Deaths { get; set; }
        public double Recovered { get; set; }
        public double Hospitalized { get; set; }
        public double Unresolved { get { return Confirmed - Deaths - Recovered; } }
        public double DosesTotal { get; set; }
        public double DosesFirst { get; set; }
        public double DosesFully { get; set; }
        public double DosesBooster { get; set; }

        public DailyNumbers()
        {
        }

        public DailyNumbers(DateTime date)
        {
            Date = date;
        }
    }
}