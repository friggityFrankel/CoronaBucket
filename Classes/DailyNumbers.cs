using System;

namespace Classes
{
    public class DailyNumbers
    {
        public DateTime Date { get; set; }
        public double Confirmed { get; set; }
        public double Deaths { get; set; }
        public double Resolved { get; set; }
        public double Hospitalized { get; set; }
        public double Unresolved { get { return Confirmed - Deaths - Resolved; } }
        public double TotalDoses { get; set; }
        public double FirstDose { get; set; }
        public double FullyDosed { get; set; }

        public DailyNumbers()
        {
        }

        public DailyNumbers(DateTime date)
        {
            Date = date;
        }
    }
}