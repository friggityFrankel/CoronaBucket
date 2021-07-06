using Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        //public DateTime startDate = new DateTime(2020, 01, 01);
        public DateTime startDate = DateTime.Today.AddDays(-7);
        public string filePath = @"D:\temp\";
        public State world;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void Refresh()
        {
            world = BuildWorld();
            CountriesList.ItemsSource = BuildCountryList(); //this is also where World gets its vaccines from
            StatesList.ItemsSource = BuildUnitedStatesList();
            WorldResultsGrid.DataContext = world;
            DebugLabel.Content = "Completed";
        }

        private string GetJsonString(string jsonQuery)
        {
            string jsonString = "";
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
                DebugLabel.Content = ex.Message;
            }
            return jsonString;
        }

        private List<State> BuildCountryList()
        {
            var countryList = new List<State>();

            // Build Countries
            var countryInfoList = GetCountryInfo();
            foreach (var countryInfo in countryInfoList.countries)
            {
                var newCountry = new State()
                {
                    Name = countryInfo.name,
                    Abbreviation = countryInfo.iso3
                };
                countryList.Add(newCountry);
            }

            // Get Populations
            var populationList = GetPopulations();
            foreach (var pop in populationList)
            {
                var country = countryList.SingleOrDefault(c => c.Abbreviation == pop.Iso3);
                if (country != null)
                {
                    country.Population = pop.Population;
                }
            }

            // Get Vaccines
            var vaccineList = GetVaccines();
            foreach (var vac in vaccineList)
            {
                var country = countryList.SingleOrDefault(c => c.Abbreviation == vac.iso_code);
                if (country != null)
                {
                    var previousNumbers = country.DailyNumbers.FirstOrDefault();

                    DateTime.TryParse(vac.date, out DateTime date);
                    double.TryParse(vac.daily_vaccinations_raw, out double total);
                    double.TryParse(vac.people_vaccinated, out double partial);
                    double.TryParse(vac.people_fully_vaccinated, out double fully);

                    if (total == 0 && previousNumbers != null)
                    {
                        total = previousNumbers.DosesTotal;
                    }

                    if (partial == 0 && previousNumbers != null)
                    {
                        partial = previousNumbers.DosesFirst;
                    }

                    if (fully == 0 && previousNumbers != null)
                    {
                        fully = previousNumbers.DosesFully;
                    }

                    var dailyNumbers = new DailyNumbers(date)
                    {
                        DosesTotal = total,
                        DosesFirst = partial,
                        DosesFully = fully
                    };

                    country.DailyNumbers.Insert(0, dailyNumbers);
                }
                else if (vac.location == "World")
                {
                    DateTime.TryParse(vac.date, out DateTime date);
                    double.TryParse(vac.daily_vaccinations_raw, out double total);
                    double.TryParse(vac.people_vaccinated, out double partial);
                    double.TryParse(vac.people_fully_vaccinated, out double fully);

                    var dailyNumbers = world.DailyNumbers.SingleOrDefault(d => d.Date == date);
                    if (dailyNumbers == null)
                    {
                        dailyNumbers = new DailyNumbers();
                        world.DailyNumbers.Add(dailyNumbers);
                    }
                    dailyNumbers.DosesTotal = total;
                    dailyNumbers.DosesFirst = partial;
                    dailyNumbers.DosesFully = fully;
                }
            }

            // Get Cases
            var casesList = GetDailyData();
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
                    else
                    {
                        dailyNumbers = new DailyNumbers(cases.date);
                        dailyNumbers.Confirmed = confirmed;
                        dailyNumbers.Deaths = deaths;
                        dailyNumbers.Recovered = recovered;

                        var previous = country.DailyNumbers.FirstOrDefault(d => d.Date == cases.date.AddDays(-1));
                        if (previous != null)
                        {
                            dailyNumbers.DosesTotal = previous.DosesTotal;
                            dailyNumbers.DosesFirst = previous.DosesFirst;
                            dailyNumbers.DosesFully = previous.DosesFully;
                        }

                        country.DailyNumbers.Add(dailyNumbers);

                        country.DailyNumbers = country.DailyNumbers.OrderByDescending(d => d.Date).ToList();
                    }
                }
            }

            return countryList;
        }

        private List<State> BuildUnitedStatesList()
        {
            var stateList = new List<State>();
            var stateInfoList = GetStateInfo();
            var vaccineList = GetUSVaccines();
            foreach (var stateInfo in stateInfoList.states)
            {
                var newState = new State()
                {
                    Name = stateInfo.State,
                    Abbreviation = stateInfo.Abbrev,
                    Population = stateInfo.Population
                };

                var vacList = vaccineList.Where(v => v.location == newState.Name);
                foreach (var vac in vacList)
                {
                    DateTime.TryParse(vac.date, out DateTime date);
                    double.TryParse(vac.daily_vaccinations_raw, out double total);
                    double.TryParse(vac.people_vaccinated, out double partial);
                    double.TryParse(vac.people_fully_vaccinated, out double fully);

                    var dailyNumbers = new DailyNumbers(date)
                    {
                        DosesTotal = total,
                        DosesFirst = partial,
                        DosesFully = fully
                    };

                    newState.DailyNumbers.Add(dailyNumbers);
                }

                stateList.Add(newState);
            }

            return stateList;
        }

        private State BuildWorld()
        {
            Console.WriteLine("Get Current World...");
            var world = new State()
            {
                Name = "World",
                Population = 7742277000.0
            };

            for (DateTime i = startDate; i < DateTime.Today; i = i.AddDays(1))
            {
                var dailyNumber = new DailyNumbers(i);
                var fileName = $"{i.ToString("yyyyMMdd")}.txt";
                var jsonString = "";
                if (i < DateTime.Now.AddDays(-3) && File.Exists(Path.Combine(filePath + @"json\", fileName)))
                {
                    jsonString = File.ReadAllText(Path.Combine(filePath + @"json\", fileName));
                }
                else
                {
                    jsonString = GetJsonString($"https://covid19.mathdro.id/api/daily/{i.ToString("MM-dd-yyyy")}");
                }

                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    try
                    {

                        var jsonData = JsonConvert.DeserializeObject<List<JsonDailyData>>(jsonString);
                        foreach (var item in jsonData)
                        {
                            if (!string.IsNullOrWhiteSpace(item.confirmed))
                            {
                                double.TryParse(item.confirmed, out double confirmed);
                                dailyNumber.Confirmed += confirmed;
                            }
                            if (!string.IsNullOrWhiteSpace(item.deaths))
                            {
                                double.TryParse(item.deaths, out double deaths);
                                dailyNumber.Deaths += deaths;
                            }
                            if (!string.IsNullOrWhiteSpace(item.recovered))
                            {
                                double.TryParse(item.recovered, out double recovered);
                                dailyNumber.Recovered += recovered;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                world.DailyNumbers.Add(dailyNumber);
            }

            return world;
        }

        private List<JsonDailyData> GetDailyData()
        {
            var resultsList = new List<JsonDailyData>();
            for (var i = startDate; i < DateTime.Now; i = i.AddDays(1))
            {
                Console.WriteLine($"Retrieving: {i}...");
                DebugLabel.Content = $"Retrieving: {i}...";
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
                    DebugLabel.Content = ex.Message;
                }
            }
            return resultsList.OrderByDescending(d => d.date).ToList();
        }

        private List<JsonVaccineData> GetVaccines()
        {
            Console.WriteLine("Get Vaccines...");
            DebugLabel.Content = "Get Vaccines...";
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
                        daily_vaccinations_raw = dataItem[6],
                        daily_vaccinations = dataItem[7],
                        total_vaccinations_per_hundred = dataItem[8],
                        people_vaccinated_per_hundred = dataItem[9],
                        people_fully_vaccinated_per_hundred = dataItem[10],
                        daily_vaccinations_per_million = dataItem[11]
                    };
                    vacList.Add(vacItem);
                }
            }
            return vacList;
        }

        private List<JsonUSVaccineData> GetUSVaccines()
        {
            Console.WriteLine("Get US Vaccines...");
            DebugLabel.Content = "Get US Vaccines...";
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
            Console.WriteLine("Get Country List");
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

        private void Refresh_Click(object sender, RoutedEventArgs e) { Refresh(); }

        private void StateSelection_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is State state)
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