using System;

namespace ClassLibrary
{
    public class CaseNumbers
    {
        public DateTime Date;
        public int Confirmed;
        public int Deaths;
        public int Recovered;
        public int Hospitalized;
        public int HospitalizedChange;

        public CaseNumbers(DateTime date)
        {
            Date = date;
            Confirmed = 0;
            Deaths = 0;
            Recovered = 0;
        }

        public int Unresolved
        {
            get
            {
                return Confirmed - Deaths - Recovered;
            }
        }
    }
}
