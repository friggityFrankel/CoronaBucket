using System;

namespace ClassLibrary
{
    public class VaccinationData
    {
        // from https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/vaccinations/vaccinations.csv
        public string location;
        public string iso_code;
        public DateTime date;
        public int total_vaccinations;
        public int total_vaccinations_per_hundred;
        public int daily_vaccinations_raw;
        public int daily_vaccinations;
        public int daily_vaccinations_per_million;
        public int people_vaccinated;
        public int people_vaccinated_per_hundred;
        public int people_fully_vaccinated;
        public int people_fully_vaccinated_per_hundred;
    }
}