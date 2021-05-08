using System;

namespace ClassLibrary
{
    public class VaccinationNumbers
    {
        public DateTime Date;
        public double Total;
        public double Daily;
        public double Partial;
        public double Fully;

        public VaccinationNumbers(DateTime date)
        {
            Date = date;
            Total = 0;
            Daily = 0;
            Partial = 0;
            Fully = 0;
        }
    }
}