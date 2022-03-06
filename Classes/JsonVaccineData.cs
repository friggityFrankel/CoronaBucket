namespace Classes
{
    public class JsonVaccineData
    {
        // from https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/vaccinations/vaccinations.csv
        public string location { get; set; }
        public string iso_code { get; set; }
        public string date { get; set; }
        public string total_vaccinations { get; set; }
        public string people_vaccinated { get; set; }
        public string people_fully_vaccinated { get; set; }
        public string total_boosters { get; set; }
        public string daily_vaccinations_raw { get; set; }
        public string daily_vaccinations { get; set; }
        public string total_vaccinations_per_hundred { get; set; }
        public string people_vaccinated_per_hundred { get; set; }
        public string people_fully_vaccinated_per_hundred { get; set; }
        public string total_boosters_per_hundred { get; set; }
        public string daily_vaccinations_per_million { get; set; }
        public string daily_people_vaccinated { get; set; }
        public string daily_people_vaccinated_per_hundred { get; set; }
    }
}