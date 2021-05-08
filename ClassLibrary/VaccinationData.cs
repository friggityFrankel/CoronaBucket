using System;

namespace ClassLibrary
{
    public class VaccinationData
    {
        // from https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/vaccinations/vaccinations.csv
        public string location;
        public string iso_code;
        public DateTime date;
        public double total_vaccinations;
        public double total_vaccinations_per_hundred;
        public double daily_vaccinations_raw;
        public double daily_vaccinations;
        public double daily_vaccinations_per_million;
        public double people_vaccinated;
        public double people_vaccinated_per_hundred;
        public double people_fully_vaccinated;
        public double people_fully_vaccinated_per_hundred;
    }
}