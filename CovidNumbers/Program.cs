using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ClassLibrary;
using Newtonsoft.Json;
using System.Globalization;

namespace CovidNumbers
{
    class Program
    {
        //static DateTime startDate = new DateTime(2020, 1, 1);
        static DateTime startDate = DateTime.Today.AddDays(-3);
        static string filePath = @"D:\temp\";

        static void Main(string[] args)
        {
            var results = GetResults();
            var sortedResults = SortResults(results);

            WriteWorldResults(sortedResults.SingleOrDefault(r => r.Name == "US"));
            WriteCountryResults(sortedResults);
            WriteUSResults(sortedResults.SingleOrDefault(r => r.Name == "US"));

            // For making charts
            //WriteCsvResults(results);

            // For making state specific charts
            //WriteStateResults(sortedResults.SingleOrDefault(r => r.Name == "US"), "SD");
        }

        static string GetJsonString(string jsonQuery)
        {
            string jsonString = "";
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
            return jsonString;
        }

        static List<ResultData> GetResults()
        {
            var resultsList = new List<ResultData>();
            for (var i = startDate; i < DateTime.Now; i = i.AddDays(1))
            {
                Console.WriteLine($"Retrieving: {i}...");
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
                        var results = JsonConvert.DeserializeObject<List<ResultData>>(jsonString);
                        foreach (var item in results)
                        {
                            item.date = i;
                            if (item.confirmed == "")
                                item.confirmed = "0";
                            if (item.deaths == "")
                                item.deaths = "0";
                            if (item.recovered == "")
                                item.recovered = "0";
                        }
                        resultsList.AddRange(results);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
            return resultsList;
        }

        static CaseNumbers GetDailyResults(DateTime date)
        {
            var cases = new CaseNumbers(date);
            var jsonString = GetJsonString($"https://covid19.mathdro.id/api/daily/{date.ToString("MM-dd-yyyy")}");

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                try
                {
                    var jsonData = JsonConvert.DeserializeObject<List<ResultData>>(jsonString);
                    foreach (var item in jsonData)
                    {
                        if (!string.IsNullOrWhiteSpace(item.confirmed))
                        {
                            double.TryParse(item.confirmed, out double confirmed);
                            cases.Confirmed += confirmed;
                        }
                        if (!string.IsNullOrWhiteSpace(item.deaths))
                        {
                            double.TryParse(item.deaths, out double deaths);
                            cases.Deaths += deaths;
                        }
                        if (!string.IsNullOrWhiteSpace(item.recovered))
                        {
                            double.TryParse(item.recovered, out double recovered);
                            cases.Recovered += recovered;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return cases;
        }

        static CaseNumbers GetCurrentWorld()
        {
            Console.WriteLine("Retrieving World numbers");
            var cases = new CaseNumbers(DateTime.Today);
            var jsonString = GetJsonString("https://covid19.mathdro.id/api");

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                try
                {
                    var jsonData = JsonConvert.DeserializeObject<WorldResultData>(jsonString);
                    cases.Confirmed = (jsonData.confirmed != null) ? jsonData.confirmed.value : 0;
                    cases.Deaths = (jsonData.deaths != null) ? jsonData.deaths.value : 0;
                    cases.Recovered = (jsonData.recovered != null) ? jsonData.recovered.value : 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return cases;
        }

        static CaseNumbers GetCurrentCountry(string country)
        {
            Console.WriteLine($"Retrieving {country}");
            var cases = new CaseNumbers(DateTime.Today);
            var jsonString = GetJsonString($"https://covid19.mathdro.id/api/countries/{country}");

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                try
                {
                    var jsonData = JsonConvert.DeserializeObject<CountryResultData>(jsonString);
                    cases.Confirmed = (jsonData.confirmed != null) ? jsonData.confirmed.value : 0;
                    cases.Deaths = (jsonData.deaths != null) ? jsonData.deaths.value : 0;
                    cases.Recovered = (jsonData.recovered != null) ? jsonData.recovered.value : 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return cases;
        }

        static CaseNumbers GetCurrentState(string state)
        {
            Console.WriteLine($"Retrieving {state}");
            var cases = new CaseNumbers(DateTime.Today);
            var jsonString = GetJsonString($"https://api.covidtracking.com/v1/states/{state}/current.json");

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var jsonData = JsonConvert.DeserializeObject<StateResultData>(jsonString, settings);
                    if (jsonData != null)
                    {
                        cases.Confirmed = jsonData.positive;
                        cases.Deaths = jsonData.death;
                        cases.Hospitalized = jsonData.hospitalizedCurrently;
                        cases.HospitalizedChange = jsonData.hospitalizedIncrease;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return cases;
        }

        static CountryData GetCountryList()
        {
            var jsonString = GetJsonString("https://covid19.mathdro.id/api/countries/");
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                var results = JsonConvert.DeserializeObject<CountryData>(jsonString);
                return results;
            }
            return new CountryData();
        }

        static StateData GetStateList()
        {
            var fileName = "StateList.json";
            var jsonString = File.ReadAllText(Path.Combine(filePath + @"json\", fileName));
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                var results = JsonConvert.DeserializeObject<StateData>(jsonString);
                return results;
            }
            return new StateData();
        }

        private static List<Region> SortResults(List<ResultData> resultData)
        {
            var regions = new List<Region>();
            var countyList = GetCountryList();
            var stateList = GetStateList();
            for (var i = startDate; i < DateTime.Today; i = i.AddDays(1))
            {
                Console.WriteLine($"Sorting {i}");
                var dayList = resultData.Where(d => d.date == i && d.confirmed != "");

                foreach (var item in dayList)
                {
                    var caseNumbers = new CaseNumbers(i);

                    double.TryParse(item.confirmed, out double confirmed);
                    double.TryParse(item.deaths, out double deaths);
                    double.TryParse(item.recovered, out double recovered);

                    caseNumbers.Confirmed = confirmed;
                    caseNumbers.Deaths = deaths;
                    caseNumbers.Recovered = recovered;

                    var region = regions.SingleOrDefault(r => r.Name == item.countryRegion);
                    if (region == null)
                    {
                        region = new Region(item.countryRegion);

                        region.Iso2 = countyList.countries.SingleOrDefault(c => c.name == region.Name)?.iso2;
                        if (region.Iso2 != null)
                        {
                            region.CurrentCases = GetCurrentCountry(region.Iso2);
                        }

                        regions.Add(region);
                    }

                    var stateName = item.provinceState;
                    if (string.IsNullOrWhiteSpace(stateName))
                    {
                        stateName = item.countryRegion;
                    }

                    var state = region.States.SingleOrDefault(s => s.Name == stateName);
                    if (state == null)
                    {
                        state = new State(stateName);
                        if (region.Iso2 == "US")
                        {
                            var stateInfo = stateList.states.SingleOrDefault(s => s.State == state.Name);
                            if (stateInfo != null)
                            {
                                state.Abbr = stateList.states.SingleOrDefault(s => s.State == state.Name).Code;
                                state.Population = stateList.states.SingleOrDefault(s => s.State == state.Name).Population;
                                if (!string.IsNullOrWhiteSpace(state.Abbr))
                                {
                                    state.CurrentCases = GetCurrentState(state.Abbr);
                                }
                            }
                        }
                        state.Cases.Add(caseNumbers);
                        region.States.Add(state);
                    }
                    else
                    {
                        var cases = state.Cases.SingleOrDefault(c => c.Date == caseNumbers.Date);
                        if (cases == null)
                        {
                            state.Cases.Add(caseNumbers);
                        }
                        else
                        {
                            cases.Confirmed += caseNumbers.Confirmed;
                            cases.Deaths += caseNumbers.Deaths;
                            cases.Recovered += caseNumbers.Recovered;
                        }
                    }
                }
            }

            return regions;
        }

        private static void WriteCsvResults(List<ResultData> resultData)
        {
            var txtFile = $"{DateTime.Now.ToString("MMdd")}_csv.txt";
            var txtLines = new List<string>();
            txtLines.Add("Date Cases(World) Deaths(World) Recovered(World) Cases(US) Deaths(US) Recovered(US)");

            for (var i = startDate; i < DateTime.Today; i = i.AddDays(1))
            {
                var dayList = resultData.Where(d => d.date == i && d.confirmed != "");
                var worldCases = new CaseNumbers(i);
                var usCases = new CaseNumbers(i);

                foreach (var item in dayList)
                {
                    double.TryParse(item.confirmed, out double confirmed);
                    double.TryParse(item.deaths, out double deaths);
                    double.TryParse(item.recovered, out double recovered);

                    worldCases.Confirmed += confirmed;
                    worldCases.Deaths += deaths;
                    worldCases.Recovered += recovered;

                    if (item.countryRegion == "US")
                    {
                        usCases.Confirmed += confirmed;
                        usCases.Deaths += deaths;
                        usCases.Recovered += recovered;
                    }
                }

                var line = $"{i:yyyy-MM-dd} {worldCases.Confirmed} {worldCases.Deaths} {worldCases.Recovered} {usCases.Confirmed} {usCases.Deaths} {usCases.Recovered}";
                txtLines.Add(line);
            }

            File.WriteAllLines(Path.Combine(filePath, txtFile), txtLines);
            Console.WriteLine("Write to world file successful");
        }

        private static void WriteWorldResults(Region usRegion)
        {
            var eventTitle = "We Have a Real President with a Plan";
            var eventDate = new DateTime(2021, 1, 20);
            var previousId = File.ReadAllLines(filePath + "Previously.txt")[0].Split(' ')[1];
            var previousURL = "https://www.shacknews.com/chatty?id=" + previousId;

            var checkDate = DateTime.Today.AddDays(-2);
            var txtFile = $"{DateTime.Now.ToString("MMdd")}_World.txt";
            var lines = new List<string>();
            var current = GetCurrentWorld();
            var change = GetDailyResults(checkDate);
            var usChange = usRegion.Change(DateTime.Today.AddDays(-1));

            lines.Add($"*[b{{Corona Bucket}}b]*: {(eventDate - DateTime.Today).Days} Days Till {eventTitle}");
            lines.Add("");
            lines.Add("b{World Wide}b totals:");
            lines.Add($"y{{Cases}}y: {current.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} (+{(current.Confirmed - change.Confirmed).ToString("N0", CultureInfo.CurrentCulture)})");
            lines.Add($"r{{Deaths}}r: {current.Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{(current.Deaths - change.Deaths).ToString("N0", CultureInfo.CurrentCulture)})");
            lines.Add($"g{{Recovered}}g: {current.Recovered.ToString("N0", CultureInfo.CurrentCulture)} (+{(current.Recovered - change.Recovered).ToString("N0", CultureInfo.CurrentCulture)})");
            lines.Add($"p[Unresolved]p: {current.Unresolved.ToString("N0", CultureInfo.CurrentCulture)} (+{(current.Unresolved - change.Unresolved).ToString("N0", CultureInfo.CurrentCulture)})");
            lines.Add("");
            lines.Add("r{U}rb[S]bb{A}b totals:");
            lines.Add($"Cases: {usRegion.CurrentCases.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} (+{usChange.Confirmed.ToString("N0", CultureInfo.CurrentCulture)})");
            lines.Add($"Deaths: {usRegion.CurrentCases.Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{usChange.Deaths.ToString("N0", CultureInfo.CurrentCulture)})");
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
            lines.Add($"#COVID19{DateTime.Now.ToString("yyyyMMdd")}");
            lines.Add($"Previously in the bucket: {previousURL}");

            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }

        private static void WriteCountryResults(List<Region> regions)
        {
            var checkDate = DateTime.Today.AddDays(-1);
            var sortedRegions = regions.Where(r => r.Name != "US").OrderByDescending(r => r.Change(checkDate).Confirmed);
            var txtFile = $"{DateTime.Now.ToString("MMdd")}_TopCountries.txt";
            var lines = new List<string>();
            lines.Add("e[Top 20 Countries]e");
            foreach (var region in sortedRegions.Take(21))
            {
                var confirmed = region.CurrentCases.Confirmed;
                var deaths = region.CurrentCases.Deaths;
                if (confirmed == 0 && deaths == 0)
                {
                    confirmed = region.Cases(checkDate).Confirmed;
                    deaths = region.Cases(checkDate).Deaths;
                }

                var change = region.Change(checkDate);

                lines.Add("");
                lines.Add($"b[{region.Name}]b");
                lines.Add($"Cases: {confirmed.ToString("N0", CultureInfo.CurrentCulture)} (+{change.Confirmed.ToString("N0", CultureInfo.CurrentCulture)})");
                lines.Add($"Deaths: {deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{change.Deaths.ToString("N0", CultureInfo.CurrentCulture)})");
            }

            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }

        private static void WriteUSResults(Region states)
        {
            var checkDate = DateTime.Today.AddDays(-1);
            var sortedStates = states.States.OrderByDescending(s => s.Change(checkDate).Confirmed);
            var txtFile = $"{DateTime.Now.ToString("MMdd")}_TopStates.txt";
            var lines = new List<string>();
            lines.Add("e[Top 15 US States by Gains]e");
            foreach (var state in sortedStates.Take(15))
            {
                if (state.CurrentCases.Confirmed > 0)
                {
                    var cases = state.Cases.SingleOrDefault(c => c.Date == checkDate);
                    if (cases != null)
                    {
                        var change = state.Change(checkDate);

                        var stateName = $"b[{state.Name}]b s[pop. {state.Population.ToString("N0", CultureInfo.CurrentCulture)}]s ";
                        var stateCases = $"Cases: {state.CurrentCases.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} s[{state.CurrentPercentage.Confirmed.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{change.Confirmed.ToString("N0", CultureInfo.CurrentCulture)})";
                        var stateDeaths = $"Deaths: {state.CurrentCases.Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{change.Deaths.ToString("N0", CultureInfo.CurrentCulture)})";
                        var stateHospitalized = $"Hospitalized: {state.CurrentCases.Hospitalized.ToString("N0", CultureInfo.CurrentCulture)} (+{state.CurrentCases.HospitalizedChange.ToString("N0", CultureInfo.CurrentCulture)})";
                        
                        lines.Add("");
                        lines.Add(stateName);
                        lines.Add(stateCases);
                        lines.Add(stateDeaths);
                        lines.Add(stateHospitalized);
                    }
                }
            }
            lines.Add("\n\n\n\n");
            lines.Add("e[Remaining US States by Gains]e");
            foreach (var state in sortedStates.Skip(15))
            {
                if (state.CurrentCases.Confirmed > 0)
                {
                    var cases = state.Cases.SingleOrDefault(c => c.Date == checkDate);
                    if (cases != null)
                    {
                        var change = state.Change(checkDate);

                        var stateName = $"b[{state.Name}]b s[pop. {state.Population.ToString("N0", CultureInfo.CurrentCulture)}]s";
                        var stateCases = $"Cases: {state.CurrentCases.Confirmed.ToString("N0", CultureInfo.CurrentCulture)} s[{state.CurrentPercentage.Confirmed.ToString("N2", CultureInfo.CurrentCulture)}%]s (+{change.Confirmed.ToString("N0", CultureInfo.CurrentCulture)})";
                        var stateDeaths = $"Deaths: {state.CurrentCases.Deaths.ToString("N0", CultureInfo.CurrentCulture)} (+{change.Deaths.ToString("N0", CultureInfo.CurrentCulture)})";
                        var stateHospitalized = $"Hospitalized: {state.CurrentCases.Hospitalized.ToString("N0", CultureInfo.CurrentCulture)} (+{state.CurrentCases.HospitalizedChange.ToString("N0", CultureInfo.CurrentCulture)})";

                        lines.Add("");
                        lines.Add(stateName);
                        lines.Add(stateCases);
                        lines.Add(stateDeaths);
                        lines.Add(stateHospitalized);
                    }
                }
            }

            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }

        private static void WriteStateResults(Region region, string stateAbbr)
        {
            var state = region.States.SingleOrDefault(s => s.Abbr == stateAbbr);
            var txtFile = $"{DateTime.Now.ToString("MMdd")}_{stateAbbr}_csv.txt";
            var lines = new List<string>();
            foreach (var item in state.Cases)
            {
                lines.Add($"{item.Date:yyyy-MM-dd} {item.Confirmed} {item.Deaths}");
            }

            File.WriteAllLines(Path.Combine(filePath, txtFile), lines);
        }
    }
}