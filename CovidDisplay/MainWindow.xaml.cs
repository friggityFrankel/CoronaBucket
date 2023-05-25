using Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace CovidDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Country world;
        public List<JsonDailyData> casesList;
        public List<JsonVaccineData> vaccinesList;
        public List<JsonCountryData> countriesList;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FilePathTextbox.Text = @"C:\Users\nukem\OneDrive\Documents\CoronaBucket\";
            RangePicker.Value = 7;
            GetDatePicker.SelectedDate = DateTime.Today.AddDays(-1);
        }

        private void Refresh()
        {
            countriesList = GetCountryData();
            casesList = GetDailyData();
            vaccinesList = GetVaccines();


            CountriesList.ItemsSource = BuildCountries();
            StatesList.ItemsSource = BuildUnitedStatesList();

            world = ((List<Country>)CountriesList.ItemsSource).FirstOrDefault(c => c.Name == "World"); ;

            WorldResultsGrid.DataContext = world;

            WriteButton.IsEnabled = true;
        }

        private string GetJsonString(string jsonQuery)
        {
            var jsonString = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(jsonQuery);
                    using (var response = client.GetAsync("").Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var contents = response.Content.ReadAsStringAsync();
                            jsonString = contents.Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return jsonString;
        }

        private List<State> BuildCountryList()
        {
            var countryList = new List<State>();
            var endDate = GetDatePicker.SelectedDate.Value;
            var range = (int)RangePicker.Value * -1;

            // Build Countries with populations
            var countryInfoList = GetCountryInfo();
            var populationList = GetPopulations();
            foreach (var countryInfo in countryInfoList.countries!)
            {
                var newCountry = new State(endDate.AddDays(range), endDate)
                {
                    Name = countryInfo.name,
                    Abbreviation = countryInfo.iso3
                };

                var pop = populationList.SingleOrDefault(c => c.Iso3 == newCountry.Abbreviation);
                if (pop != null)
                {
                    newCountry.Population = pop.Population;
                }

                countryList.Add(newCountry);
            }

            // Get Vaccines
            foreach (var vac in vaccinesList)
            {
                var country = countryList.SingleOrDefault(c => c.Abbreviation == vac.iso_code);
                if (country != null)
                {
                    DateTime.TryParse(vac.date, out DateTime date);
                    double.TryParse(vac.total_vaccinations, out double total);
                    double.TryParse(vac.people_vaccinated, out double partial);
                    double.TryParse(vac.people_fully_vaccinated, out double fully);

                    var dailyNumbers = country.DailyNumbers.SingleOrDefault(d => d.Date == date);
                    if (dailyNumbers != null)
                    {
                        dailyNumbers.DosesTotal = total;
                        dailyNumbers.DosesFirst = partial;
                        dailyNumbers.DosesFully = fully;
                    }
                }
            }

            // Get Cases
            foreach (var cases in casesList)
            {
                var country = countryList.SingleOrDefault(c => c.Name == cases.countryRegion);
                if (country != null)
                {
                    double.TryParse(cases.confirmed, out double confirmed);
                    double.TryParse(cases.deaths, out double deaths);
                    double.TryParse(cases.recovered, out double recovered);

                    var dailyNumbers = country.DailyNumbers.SingleOrDefault(d => d.Date == cases.date);
                    if (dailyNumbers != null)
                    {
                        dailyNumbers.Confirmed += confirmed;
                        dailyNumbers.Deaths += deaths;
                        dailyNumbers.Recovered += recovered;
                    }
                }
            }

            foreach (var country in countryList)
            {
                for (var i = country.DailyNumbers.Count - 2; i >= 0; i--)
                {
                    var dailyNumbers = country.DailyNumbers[i];
                    var previous = country.DailyNumbers[i + 1];
                    if (dailyNumbers.DosesTotal == 0)
                    {
                        dailyNumbers.DosesTotal = previous.DosesTotal;
                        dailyNumbers.DosesFirst = previous.DosesFirst;
                        dailyNumbers.DosesFully = previous.DosesFully;
                    }

                    if (dailyNumbers.Confirmed == 0)
                    {
                        dailyNumbers.Confirmed = previous.Confirmed;
                        dailyNumbers.Deaths = previous.Deaths;
                        dailyNumbers.Recovered = previous.Recovered;
                    }
                }
            }

            return countryList;
        }

        private List<State> BuildUnitedStatesList()
        {
            var stateList = new List<State>();
            var endDate = GetDatePicker.SelectedDate.Value;
            var range = (int)RangePicker.Value * -1;
            var stateInfoList = GetStateInfo();
            var vaccineList = GetUSVaccines();
            var caseList = casesList.Where(c => c.countryRegion == "US");

            foreach (var stateInfo in stateInfoList.states)
            {
                // Build state with population
                var newState = new State(endDate.AddDays(range), endDate)
                {
                    Name = stateInfo.State,
                    Abbreviation = stateInfo.Abbrev,
                    Population = stateInfo.Population
                };

                stateList.Add(newState);
            }

            // Get Vaccines
            foreach (var vac in vaccineList)
            {
                var state = stateList.SingleOrDefault(s => s.Name == vac.location.Replace(" State", ""));
                if (state != null)
                {
                    DateTime.TryParse(vac.date, out DateTime date);
                    double.TryParse(vac.total_vaccinations, out double total);
                    double.TryParse(vac.people_vaccinated, out double partial);
                    double.TryParse(vac.people_fully_vaccinated, out double fully);

                    var dailyNumbers = state.DailyNumbers.SingleOrDefault(d => d.Date == date);
                    if (dailyNumbers != null)
                    {
                        dailyNumbers.DosesTotal = total;
                        dailyNumbers.DosesFirst = partial;
                        dailyNumbers.DosesFully = fully;
                    }
                }
            }

            // Get Cases
            foreach (var cases in caseList)
            {
                var state = stateList.SingleOrDefault(s => s.Name == cases.provinceState);
                if (state != null)
                {
                    double.TryParse(cases.confirmed, out double confirmed);
                    double.TryParse(cases.deaths, out double deaths);
                    double.TryParse(cases.recovered, out double recovered);

                    var dailyNumbers = state.DailyNumbers.SingleOrDefault(d => d.Date == cases.date);
                    if (dailyNumbers != null)
                    {
                        dailyNumbers.Confirmed += confirmed;
                        dailyNumbers.Deaths += deaths;
                        dailyNumbers.Recovered += recovered;
                    }
                }
            }

            foreach (var state in stateList)
            {
                for (int i = state.DailyNumbers.Count - 2; i >= 0; i--)
                {
                    var dailyNumbers = state.DailyNumbers[i];
                    var previous = state.DailyNumbers[i + 1];
                    if (dailyNumbers.DosesTotal == 0)
                    {
                        dailyNumbers.DosesTotal = previous.DosesTotal;
                        dailyNumbers.DosesFirst = previous.DosesFirst;
                        dailyNumbers.DosesFully = previous.DosesFully;
                    }

                    if (dailyNumbers.Confirmed == 0)
                    {
                        dailyNumbers.Confirmed = previous.Confirmed;
                        dailyNumbers.Deaths = previous.Deaths;
                        dailyNumbers.Recovered = previous.Recovered;
                    }
                }
            }

            return stateList;
        }

        private State BuildWorld()
        {
            var endDate = GetDatePicker.SelectedDate.Value;
            var range = (int)RangePicker.Value * -1;
            var world = new State(endDate.AddDays(range), endDate)
            {
                Name = "World",
                Population = 7742277000.0
            };

            foreach (var dailyNumbers in world.DailyNumbers)
            {
                var vac = vaccinesList.SingleOrDefault(v => v.location == "World" && v.date == dailyNumbers.Date.ToString("yyyy-MM-dd"));
                if (vac != null)
                {
                    double.TryParse(vac.total_vaccinations, out var total);
                    double.TryParse(vac.people_vaccinated, out var partial);
                    double.TryParse(vac.people_fully_vaccinated, out var fully);
                    double.TryParse(vac.total_boosters, out var booster);

                    dailyNumbers.DosesTotal = total;
                    dailyNumbers.DosesFirst = partial;
                    dailyNumbers.DosesFully = fully;
                    dailyNumbers.DosesBooster = booster;
                }

                foreach (var cases in casesList.Where(c => c.date == dailyNumbers.Date))
                {
                    double.TryParse(cases.confirmed, out double confirmed);
                    double.TryParse(cases.deaths, out double deaths);
                    double.TryParse(cases.recovered, out double recovered);

                    dailyNumbers.Confirmed += confirmed;
                    dailyNumbers.Deaths += deaths;
                    dailyNumbers.Recovered += recovered;
                }
            }

            for (var i = world.DailyNumbers.Count - 2; i >= 0; i--)
            {
                var dailyNumbers = world.DailyNumbers[i];
                var previous = world.DailyNumbers[i + 1];
                if (dailyNumbers.DosesTotal == 0)
                {
                    dailyNumbers.DosesTotal = previous.DosesTotal;
                    dailyNumbers.DosesFirst = previous.DosesFirst;
                    dailyNumbers.DosesFully = previous.DosesFully;
                    dailyNumbers.DosesBooster = previous.DosesBooster;
                }

                if (dailyNumbers.Confirmed == 0)
                {
                    dailyNumbers.Confirmed = previous.Confirmed;
                    dailyNumbers.Deaths = previous.Deaths;
                    dailyNumbers.Recovered = previous.Recovered;
                }
            }

            return world;
        }

        private List<Country> BuildCountries()
        {
            var resultsList = new List<Country>();
            try
            {
                foreach (var item in countriesList)
                {
                    Country country;
                    CountryNumbers numbers = new CountryNumbers();

                    if (resultsList.Find(c => c.Name == item.location) != null)
                    {
                        country = resultsList.First(c => c.Name == item.location);
                    }
                    else
                    {
                        //country = new Country() { Name = item.location };
                        resultsList.Add(country = new Country() { Name = item.location });
                    }

                    double.TryParse(item.population, out var population);

                    double.TryParse(item.total_vaccinations, out var vacTotal);
                    double.TryParse(item.people_vaccinated, out var vacFirst);
                    double.TryParse(item.people_fully_vaccinated, out var vacFully);
                    double.TryParse(item.total_boosters, out var vacBoost);
                    double.TryParse(item.total_cases, out var casesTotal);
                    double.TryParse(item.total_deaths, out var deathsTotal);

                    double.TryParse(item.new_vaccinations, out var vacTotalNew);
                    double.TryParse(item.new_cases, out var casesNew);
                    double.TryParse(item.new_deaths, out var deathsNew);

                    double.TryParse(item.new_vaccinations_smoothed, out var vacNewSmooth);
                    double.TryParse(item.new_people_vaccinated_smoothed, out var vacFirstSmooth);
                    double.TryParse(item.new_cases_smoothed, out var casesSmooth);
                    double.TryParse(item.new_deaths_smoothed, out var deathsSmooth);

                    numbers.Population = population;
                    numbers.DoseTotal = vacTotal;
                    numbers.DoseFirst = vacFirst;
                    numbers.DoseFully = vacFully;
                    numbers.DoseBooster = vacBoost;
                    numbers.Confirmed = casesTotal;
                    numbers.Deaths = deathsTotal;

                    country.Numbers.Add(numbers);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            return resultsList;
        }

        private List<JsonCountryData> GetCountryData()
        {
            var filePath = FilePathTextbox.Text;
            var fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            var resultsList = new List<JsonCountryData>();
            try
            {
                var jsonString = GetJsonString("https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/latest/owid-covid-latest.csv");
                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    File.WriteAllText(Path.Combine(filePath + @"json\", fileName), jsonString);

                    for (var i = 0; i < 7; i++)
                    {
                        fileName = DateTime.Now.AddDays(-i).ToString("yyyyMMdd") + ".txt";
                        jsonString = File.ReadAllText(Path.Combine(filePath + @"json\", fileName));
                        var dataList = jsonString.Split("\n");
                        for (var j = 1; j < dataList.Length - 1; j++)
                        {
                            var dataItem = dataList[j].Split(',');
                            var countryItem = new JsonCountryData()
                            {
                                iso_code = dataItem[0],
                                continent = dataItem[1],
                                location = dataItem[2],
                                last_updated_date = dataItem[3],
                                total_cases = dataItem[4],
                                new_cases = dataItem[5],
                                new_cases_smoothed = dataItem[6],
                                total_deaths = dataItem[7],
                                new_deaths = dataItem[8],
                                new_deaths_smoothed = dataItem[9],
                                total_cases_per_million = dataItem[10],
                                new_cases_per_million = dataItem[11],
                                new_cases_smoothed_per_million = dataItem[12],
                                total_deaths_per_million = dataItem[13],
                                new_deaths_per_million = dataItem[14],
                                new_deaths_smoothed_per_million = dataItem[15],
                                reproduction_rate = dataItem[16],
                                icu_patients = dataItem[17],
                                icu_patients_per_million = dataItem[18],
                                hosp_patients = dataItem[19],
                                hosp_patients_per_million = dataItem[20],
                                weekly_icu_admissions = dataItem[21],
                                weekly_icu_admissions_per_million = dataItem[22],
                                weekly_hosp_admissions = dataItem[23],
                                weekly_hosp_admissions_per_million = dataItem[24],
                                total_tests = dataItem[25],
                                new_tests = dataItem[26],
                                total_tests_per_thousand = dataItem[27],
                                new_tests_per_thousand = dataItem[28],
                                new_tests_smoothed = dataItem[29],
                                new_tests_smoothed_per_thousand = dataItem[30],
                                positive_rate = dataItem[31],
                                tests_per_case = dataItem[32],
                                tests_units = dataItem[33],
                                total_vaccinations = dataItem[34],
                                people_vaccinated = dataItem[35],
                                people_fully_vaccinated = dataItem[36],
                                total_boosters = dataItem[37],
                                new_vaccinations = dataItem[38],
                                new_vaccinations_smoothed = dataItem[39],
                                total_vaccinations_per_hundred = dataItem[40],
                                people_vaccinated_per_hundred = dataItem[41],
                                people_fully_vaccinated_per_hundred = dataItem[42],
                                total_boosters_per_hundred = dataItem[43],
                                new_vaccinations_smoothed_per_million = dataItem[44],
                                new_people_vaccinated_smoothed = dataItem[45],
                                new_people_vaccinated_smoothed_per_hundred = dataItem[46],
                                stringency_index = dataItem[47],
                                population_density = dataItem[48],
                                median_age = dataItem[49],
                                aged_65_older = dataItem[50],
                                aged_70_older = dataItem[51],
                                gdp_per_capita = dataItem[52],
                                extreme_poverty = dataItem[53],
                                cardiovasc_death_rate = dataItem[54],
                                diabetes_prevalence = dataItem[55],
                                female_smokers = dataItem[56],
                                male_smokers = dataItem[57],
                                handwashing_facilities = dataItem[58],
                                hospital_beds_per_thousand = dataItem[59],
                                life_expectancy = dataItem[60],
                                human_development_index = dataItem[61],
                                population = dataItem[62],
                                excess_mortality_cumulative_absolute = dataItem[63],
                                excess_mortality_cumulative = dataItem[64],
                                excess_mortality = dataItem[65],
                                excess_mortality_cumulative_per_million = dataItem[66]
                            };
                            resultsList.Add(countryItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return resultsList;
        }

        private List<JsonDailyData> GetDailyData()
        {
            var endDate = GetDatePicker.SelectedDate.Value;
            var range = (int)RangePicker.Value * -1;
            var filePath = FilePathTextbox.Text;
            var resultsList = new List<JsonDailyData>();
            for (var i = endDate.AddDays(range); i <= endDate; i = i.AddDays(1))
            {
                try
                {
                    var fileName = $"{i.ToString("yyyyMMdd")}.txt";
                    var jsonString = "";

                    if (i < DateTime.Now.AddDays(-3) && File.Exists(Path.Combine(filePath + @"json\", fileName)))
                    {
                        jsonString = File.ReadAllText(Path.Combine(filePath + @"json\", fileName));
                    }
                    else
                    {
                        jsonString = GetJsonString($"https://covid19.mathdro.id/api/daily/{i.ToString("MM-dd-yyyy")}");
                        if (!string.IsNullOrWhiteSpace(jsonString))
                        {
                            File.WriteAllText(Path.Combine(filePath + @"json\", fileName), jsonString);
                        }
                    }

                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        var results = JsonConvert.DeserializeObject<List<JsonDailyData>>(jsonString);
                        foreach (var item in results)
                        {
                            item.date = i;
                        }
                        resultsList.AddRange(results);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return resultsList.OrderByDescending(d => d.date).ToList();
        }

        private List<JsonVaccineData> GetVaccines()
        {
            var vacList = new List<JsonVaccineData>();
            var jsonString = GetJsonString(@"https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/vaccinations/vaccinations.csv");

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                var dataList = jsonString.Split("\n");
                for (int i = 1; i < dataList.Length - 1; i++)
                {
                    var dataItem = dataList[i].Split(',');
                    var vacItem = new JsonVaccineData()
                    {
                        location = dataItem[0],
                        iso_code = dataItem[1],
                        date = dataItem[2],
                        total_vaccinations = dataItem[3],
                        people_vaccinated = dataItem[4],
                        people_fully_vaccinated = dataItem[5],
                        total_boosters = dataItem[6],
                        daily_vaccinations_raw = dataItem[7],
                        daily_vaccinations = dataItem[8],
                        total_vaccinations_per_hundred = dataItem[9],
                        people_vaccinated_per_hundred = dataItem[10],
                        people_fully_vaccinated_per_hundred = dataItem[11],
                        total_boosters_per_hundred = dataItem[12],
                        daily_vaccinations_per_million = dataItem[13],
                        daily_people_vaccinated = dataItem[14],
                        daily_people_vaccinated_per_hundred = dataItem[15],
                    };
                    vacList.Add(vacItem);
                }
            }
            return vacList;
        }

        private List<JsonUSVaccineData> GetUSVaccines()
        {
            var usVacList = new List<JsonUSVaccineData>();
            var jsonString = GetJsonString(@"https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/vaccinations/us_state_vaccinations.csv");

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                var dataList = jsonString.Split("\n");
                for (int i = 1; i < dataList.Length - 1; i++)
                {
                    var dataItem = dataList[i].Split(',');
                    var vacItem = new JsonUSVaccineData()
                    {
                        date = dataItem[0],
                        location = dataItem[1],
                        total_vaccinations = dataItem[2],
                        total_distributed = dataItem[3],
                        people_vaccinated = dataItem[4],
                        people_fully_vaccinated_per_hundred = dataItem[5],
                        total_vaccinations_per_hundred = dataItem[6],
                        people_fully_vaccinated = dataItem[7],
                        people_vaccinated_per_hundred = dataItem[8],
                        distributed_per_hundred = dataItem[9],
                        daily_vaccinations_raw = dataItem[10],
                        daily_vaccinations = dataItem[11],
                        daily_vaccinations_per_million = dataItem[12],
                        share_doses_used = dataItem[13]
                    };
                    usVacList.Add(vacItem);
                }
            }
            return usVacList;
        }

        private JsonCountries GetCountryInfo()
        {
            var jsonString = GetJsonString("https://covid19.mathdro.id/api/countries/");
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                var results = JsonConvert.DeserializeObject<JsonCountries>(jsonString);
                return results;
            }
            return new JsonCountries();
        }

        private JsonStateInfo GetStateInfo()
        {
            var filePath = FilePathTextbox.Text;
            var fileName = "StateList.json";
            var jsonString = File.ReadAllText(Path.Combine(filePath + @"json\", fileName));
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                var results = JsonConvert.DeserializeObject<JsonStateInfo>(jsonString);
                return results;
            }
            return new JsonStateInfo();
        }

        private List<JsonPopulationData> GetPopulations()
        {
            var filePath = FilePathTextbox.Text;
            var pops = new List<JsonPopulationData>();
            var populations = File.ReadAllLines(filePath + "Populations.txt");
            foreach (var item in populations)
            {
                var info = item.Split(' ');
                var newPop = new JsonPopulationData
                {
                    Iso3 = info[0],
                    Population = double.Parse(info[1])
                };
                pops.Add(newPop);
            }

            return pops;
        }

        private void WriteAll()
        {
            WriteWorld();
            WriteCountries();
            WriteStates();

            var filePath = FilePathTextbox.Text;
            if (Directory.Exists(filePath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    Arguments = filePath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
        }

        private void WriteWorld()
        {
            var writeDate = GetDatePicker.SelectedDate.Value.AddDays(1);
            var filePath = FilePathTextbox.Text;

            var countdown = File.ReadAllLines(filePath + "Countdown.txt")[0].Split(';');
            var countdownText = "";
            if (countdown.Length > 1)
            {
                var countdownDay = DateTime.Parse(countdown[0]);
                var countdownEvent = countdown[1];
                countdownText = $": { (countdownDay - writeDate).Days} Days Till { countdownEvent}";
            }

            var previousList = File.ReadAllLines(filePath + "Previously.txt").ToList();
            var previousId = previousList.First().Split(';')[1];
            var lastyearId = previousList.SingleOrDefault(d => d.Contains(writeDate.AddYears(-1).ToString("yyyy-MM-dd"))).Split(';')[1];

            var txtFile = $"{writeDate.ToString("MMdd")}_World-NEW.txt";
            var lines = new List<string>();

            var us = ((List<Country>)CountriesList.ItemsSource).FirstOrDefault(c => c.Name == "United States");

            lines.Add($"*[b{{Corona Bucket}}b]*{countdownText}");
            lines.Add("");
            lines.AddRange(world.ToPost);
            lines.AddRange(us.ToPost);
            lines.Add("JHU’s Daily COVID-19 Data in Motion");
            lines.Add("https://coronavirus.jhu.edu/covid-19-daily-video");
            lines.Add("");
            lines.Add("Johns Hopkins University COVID-19 Dashboard");
            lines.Add("https://gisanddata.maps.arcgis.com/apps/opsdashboard/index.html#/bda7594740fd40299423467b48e9ecf6");
            lines.Add("");
            lines.Add("NYTimes US Covid-19 Tracker");
            lines.Add("https://www.nytimes.com/interactive/2023/us/covid-cases.html");
            lines.Add("");
            lines.Add("Folding @Home");
            lines.Add("https://foldingathome.org/");
            lines.Add("Join the Shacknews Folding@Home team to help fight COVID-19 https://apps.foldingathome.org/teamstats/team50784.html");
            lines.Add("");
            lines.Add("COVID-19 Therapeutics Locator");
            lines.Add("https://covid-19-therapeutics-locator-dhhs.hub.arcgis.com/");
            lines.Add("");
            lines.Add($"#COVID19{writeDate.ToString("yyyyMMdd")}");
            lines.Add($"Previously in the bucket: https://www.shacknews.com/chatty?id={previousId}");
            lines.Add($"This time last year: https://www.shacknews.com/chatty?id={lastyearId}");

            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }

        private void WriteCountries()
        {
            var writeDate = GetDatePicker.SelectedDate.Value.AddDays(1);
            var filePath = FilePathTextbox.Text;
            var topCountries = ((List<Country>)CountriesList.ItemsSource).Where(c => c.Name != "United States" && c.Name != "World" && c.Population > 1000000).OrderByDescending(c => c.DoseFullyPercent).Take(15);
            var txtFile = $"{writeDate.ToString("MMdd")}_TopCountries-NEW.txt";
            var lines = new List<string>();
            lines.Add("e[Top 15 Countries by Population Fully Vaccinated]e");
            lines.Add("");
            foreach (var country in topCountries)
            {
                lines.AddRange(country.ToPost);
            }
            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }
        private void WriteStates()
        {
            var writeDate = GetDatePicker.SelectedDate.Value.AddDays(1);
            var filePath = FilePathTextbox.Text;
            var topStates = ((List<State>)StatesList.ItemsSource).OrderByDescending(s => s.Percent.DosesFully);
            var txtFile = $"{writeDate.ToString("MMdd")}_TopStates-NEW.txt";
            var lines = new List<string>();
            lines.Add("e[Top 15 US States by Population Fully Vaccinated]e");
            lines.Add("");
            foreach (var state in topStates.Take(15))
            {
                lines.AddRange(state.ToPost);
            }
            lines.Add("\n\n\n\n");
            lines.Add("e[Dumbest 15 US States by Population Unvaccinated]e");
            lines.Add("");
            foreach (var state in topStates.TakeLast(15).OrderBy(s => s.Percent.DosesFirst))
            {
                lines.AddRange(state.ToPost);
            }
            lines.Add("\n\n\n\n");
            lines.Add("e[Remaining US States Alphabetically]e");
            lines.Add("");
            var i = 0;
            foreach (var state in topStates.Skip(15).SkipLast(15).OrderBy(s => s.Name))
            {
                lines.AddRange(state.ToPost);
                i++;
                if (i % 15 == 0)
                {
                    lines.Add("\n\n\n\n");
                }
            }
            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }
        private void CopyCountry()
        {
            if (CountriesList.SelectedItem is State state)
            {
                var post = "";
                foreach (var item in state.ToPost)
                {
                    post += item + "\n";
                }
                Clipboard.SetText(post.Trim());
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) { Refresh(); }
        private void Copy_Click(object sender, RoutedEventArgs e) { CopyCountry(); }
        private void Write_Click(object sender, RoutedEventArgs e) { WriteAll(); }

        private void StateSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Country state)
            {
                WorldResultsGrid.DataContext = state;
            }
            else
            {
                WorldResultsGrid.DataContext = world;

            }
        }
    }
}