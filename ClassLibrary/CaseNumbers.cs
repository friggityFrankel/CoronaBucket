using System;

namespace ClassLibrary
{
    public class CaseNumbers
    {
        public DateTime Date;
        public double Confirmed;
        public double Deaths;
        public double Recovered;
        public int Hospitalized;
        public int HospitalizedChange;

        public CaseNumbers(DateTime date)
        {
            Date = date;
            Confirmed = 0;
            Deaths = 0;
            Recovered = 0;
        }

        public double Unresolved
        {
            get
            {
                return Confirmed - Deaths - Recovered;
            }
        }
    }
}
