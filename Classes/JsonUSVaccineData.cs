namespace Classes
{
    public class JsonUSVaccineData
    {
        // from https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/vaccinations/us_state_vaccinations.csv
        public string date { get; set; }
        public string location { get; set; }
        public string total_vaccinations { get; set; }
        public string total_distributed { get; set; }
        public string people_vaccinated { get; set; }
        public string people_fully_vaccinated_per_hundred { get; set; }
        public string total_vaccinations_per_hundred { get; set; }
        public string people_fully_vaccinated { get; set; }
        public string people_vaccinated_per_hundred { get; set; }
        public string distributed_per_hundred { get; set; }
        public string daily_vaccinations_raw { get; set; }
        public string daily_vaccinations { get; set; }
        public string daily_vaccinations_per_million { get; set; }
        public string share_doses_used { get; set; }
    }
}