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
            FilePathTextbox.Text = @"C:\Users\nukem\OneDrive\Documents\CoronaBucket\";
            RangePicker.Value = 7;
            GetDatePicker.SelectedDate = DateTime.Today.AddDays(-1);
        }

        private void Refresh()
        {
            casesList = GetDailyData();
            vaccinesList = GetVaccines();
            world = BuildWorld();

            CountriesList.ItemsSource = BuildCountryList();
            StatesList.ItemsSource = BuildUnitedStatesList();

            WorldResultsGrid.DataContext = world;

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
            foreach (var countryInfo in countryInfoList.countries)
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
                for (int i = country.DailyNumbers.Count - 2; i >= 0; i--)
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
                    double.TryParse(vac.total_vaccinations, out double total);
                    double.TryParse(vac.people_vaccinated, out double partial);
                    double.TryParse(vac.people_fully_vaccinated, out double fully);
                    double.TryParse(vac.total_boosters, out double booster);

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

            for (int i = world.DailyNumbers.Count - 2; i >= 0; i--)
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
            var previousId = previousList.SingleOrDefault(d => d.Contains(writeDate.AddDays(-1).ToString("yyyy-MM-dd"))).Split(';')[1];
            var lastyearId = previousList.SingleOrDefault(d => d.Contains(writeDate.AddYears(-1).ToString("yyyy-MM-dd"))).Split(';')[1];

            var txtFile = $"{writeDate.ToString("MMdd")}_World-NEW.txt";
            var lines = new List<string>();

            var us = ((List<State>)CountriesList.ItemsSource).FirstOrDefault(c => c.Name == "US");

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
            lines.Add("NYTimes US Hotspot Tracker");
            lines.Add("https://www.nytimes.com/interactive/2020/us/coronavirus-us-cases.html?#hotspots");
            lines.Add("");
            lines.Add("Folding @Home");
            lines.Add("https://foldingathome.org/");
            lines.Add("Join the Shacknews Folding@Home team to help fight COVID-19 https://apps.foldingathome.org/teamstats/team50784.html");
            lines.Add("");
            lines.Add("Free At Home COVID-19 Tests (US Only)");
            lines.Add("https://www.covidtests.gov/");
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
            var topCountries = ((List<State>)CountriesList.ItemsSource).Where(c => c.Name != "US").OrderByDescending(c => c.Percent.DosesFully).Take(15);
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