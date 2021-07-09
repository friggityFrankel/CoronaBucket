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
        public State world;
        public List<JsonDailyData> casesList;
        public List<JsonVaccineData> vaccinesList;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FilePathTextbox.Text = @"D:\temp\";
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            StartDatePicker.DisplayDateEnd = DateTime.Today.AddDays(-2);
        }

        private void Refresh()
        {
            DebugLabel.Content = "Retrieving...";

            casesList = GetDailyData();
            vaccinesList = GetVaccines();
            world = BuildWorld();

            CountriesList.ItemsSource = BuildCountryList();
            StatesList.ItemsSource = BuildUnitedStatesList();
            
            WorldResultsGrid.DataContext = world;

            DebugLabel.Content = "Refresh Completed";
            WriteButton.IsEnabled = true;
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
            foreach (var vac in vaccinesList)
            {
                var country = countryList.SingleOrDefault(c => c.Abbreviation == vac.iso_code);
                if (country != null)
                {
                    var previousNumbers = country.DailyNumbers.FirstOrDefault();

                    DateTime.TryParse(vac.date, out DateTime date);
                    double.TryParse(vac.total_vaccinations, out double total);
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
                // Build state with population
                var newState = new State()
                {
                    Name = stateInfo.State,
                    Abbreviation = stateInfo.Abbrev,
                    Population = stateInfo.Population
                };

                // Get Vaccines
                var vacList = vaccineList.Where(v => v.location == newState.Name).OrderByDescending(v => v.date);
                foreach (var vac in vacList)
                {
                    DateTime.TryParse(vac.date, out DateTime date);
                    double.TryParse(vac.total_vaccinations, out double total);
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

                // Get Cases
                var caseList = casesList.Where(c => c.provinceState == newState.Name);
                foreach (var cases in caseList)
                {
                    double.TryParse(cases.confirmed, out double confirmed);
                    double.TryParse(cases.deaths, out double deaths);
                    double.TryParse(cases.recovered, out double recovered);

                    var dailyNumbers = newState.DailyNumbers.FirstOrDefault(d => d.Date == cases.date);
                    if (dailyNumbers != null)
                    {
                        dailyNumbers.Confirmed += confirmed;
                        dailyNumbers.Deaths += deaths;
                        dailyNumbers.Recovered += recovered;
                    }
                    else
                    {
                        dailyNumbers = new DailyNumbers();
                        dailyNumbers.Confirmed = confirmed;
                        dailyNumbers.Deaths = deaths;
                        dailyNumbers.Recovered = recovered;

                        var previous = newState.DailyNumbers.FirstOrDefault(d => d.Date == cases.date.AddDays(-1));
                        if (previous != null)
                        {
                            dailyNumbers.DosesTotal = previous.DosesTotal;
                            dailyNumbers.DosesFirst = previous.DosesFirst;
                            dailyNumbers.DosesFully = previous.DosesFully;
                        }

                        newState.DailyNumbers.Add(dailyNumbers);

                        newState.DailyNumbers = newState.DailyNumbers.OrderByDescending(d => d.Date).ToList();
                    }
                }

                stateList.Add(newState);
            }

            return stateList;
        }

        private State BuildWorld()
        {
            var startDate = StartDatePicker.SelectedDate.Value;
            var world = new State()
            {
                Name = "World",
                Population = 7742277000.0
            };

            for (DateTime i = startDate; i < DateTime.Today; i = i.AddDays(1))
            {
                var dailyNumbers = new DailyNumbers(i);
                var vac = vaccinesList.Where(v => v.date == i.ToString("yyyy-MM-dd") && v.location == "World").FirstOrDefault();

                if (vac != null)
                {
                    double.TryParse(vac.total_vaccinations, out double total);
                    double.TryParse(vac.people_vaccinated, out double partial);
                    double.TryParse(vac.people_fully_vaccinated, out double fully);

                    dailyNumbers.DosesTotal = total;
                    dailyNumbers.DosesFirst = partial;
                    dailyNumbers.DosesFully = fully;
                }

                foreach (var cases in casesList.Where(c => c.date == i))
                {
                    double.TryParse(cases.confirmed, out double confirmed);
                    double.TryParse(cases.deaths, out double deaths);
                    double.TryParse(cases.recovered, out double recovered);

                    dailyNumbers.Confirmed += confirmed;
                    dailyNumbers.Deaths += deaths;
                    dailyNumbers.Recovered += recovered;
                }

                world.DailyNumbers.Add(dailyNumbers);
            }

            return world;
        }

        private List<JsonDailyData> GetDailyData()
        {
            var startDate = StartDatePicker.SelectedDate.Value;
            var filePath = FilePathTextbox.Text;
            var resultsList = new List<JsonDailyData>();
            for (var i = startDate; i < DateTime.Now; i = i.AddDays(1))
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
                    DebugLabel.Content = ex.Message;
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

            DebugLabel.Content = "Write Completed";

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
            var filePath = FilePathTextbox.Text;
            var countdown = File.ReadAllLines(filePath + "Countdown.txt")[0].Split(';');
            var countdownDay = DateTime.Parse(countdown[0]);
            var countdownEvent = countdown[1];

            var previousId = File.ReadAllLines(filePath + "Previously.txt")[0].Split(' ')[1];
            var previousURL = "https://www.shacknews.com/chatty?id=" + previousId;

            var txtFile = $"{DateTime.Now.ToString("MMdd")}_World-NEW.txt";
            var lines = new List<string>();

            var us = ((List<State>)CountriesList.ItemsSource).FirstOrDefault(c => c.Name == "US");

            lines.Add($"*[b{{Corona Bucket}}b]*: {(countdownDay - DateTime.Today).Days} Days Till {countdownEvent}");
            lines.Add("");
            lines.AddRange(world.ToPost);
            lines.AddRange(us.ToPost);
            lines.Add("JHU’s Daily COVID-19 Data in Motion");
            lines.Add("https://coronavirus.jhu.edu/covid-19-daily-video");
            lines.Add("");
            lines.Add("Johns Hopkins University COVID-19 Dashboard");
            lines.Add("https://gisanddata.maps.arcgis.com/apps/opsdashboard/index.html#/bda7594740fd40299423467b48e9ecf6");
            lines.Add("");
            lines.Add("NYTimes US Hotspot Tracker");
            lines.Add("https://www.nytimes.com/interactive/2020/us/coronavirus-us-cases.html?#hotspots");
            lines.Add("");
            lines.Add("Folding @Home");
            lines.Add("https://foldingathome.org/");
            lines.Add("Join the Shacknews Folding@Home team to help fight COVID-19 https://apps.foldingathome.org/teamstats/team50784.html");
            lines.Add("");
            lines.Add($"#COVID19{DateTime.Today.ToString("yyyyMMdd")}");
            lines.Add($"Previously in the bucket: {previousURL}");

            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }
        private void WriteCountries()
        {
            var filePath = FilePathTextbox.Text;
            var topCountries = ((List<State>)CountriesList.ItemsSource).Where(c => c.Name != "US").OrderByDescending(c => c.Percent.DosesFully).Take(15);
            var txtFile = $"{DateTime.Today.ToString("MMdd")}_TopCountries-NEW.txt";
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
            var filePath = FilePathTextbox.Text;
            var topStates = ((List<State>)StatesList.ItemsSource).OrderByDescending(s => s.Percent.DosesFully);
            var txtFile = $"{DateTime.Today.ToString("MMdd")}_TopStates-NEW.txt";
            var lines = new List<string>();
            lines.Add("e[Top 15 US States by Population Fully Vaccinated]e");
            lines.Add("");
            foreach (var state in topStates.Take(15))
            {
                lines.AddRange(state.ToPost);
            }
            lines.Add("\n\n\n\n");
            lines.Add("e[Remaining US States Alphabetically]e");
            lines.Add("");
            var i = 0;
            foreach (var state in topStates.Skip(15).OrderBy(s => s.Name))
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

        private void Refresh_Click(object sender, RoutedEventArgs e) { Refresh(); }
        private void Write_Click(object sender, RoutedEventArgs e) { WriteAll(); }

        private void StateSelection_Changed(object sender, SelectionChangedEventArgs e)
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