using System;

namespace Classes
{
    public class JsonStateData
    {
        // pasted from https://api.covidtracking.com/v1/states/{state}/current.json
        public int date { get; set; }
        public string state { get; set; }
        public int positive { get; set; }
        public object probableCases { get; set; }
        public int negative { get; set; }
        public object pending { get; set; }
        public string totalTestResultsSource { get; set; }
        public int totalTestResults { get; set; }
        public int hospitalizedCurrently { get; set; }
        public object hospitalizedCumulative { get; set; }
        public int inIcuCurrently { get; set; }
        public object inIcuCumulative { get; set; }
        public object onVentilatorCurrently { get; set; }
        public object onVentilatorCumulative { get; set; }
        public int recovered { get; set; }
        public string dataQualityGrade { get; set; }
        public string lastUpdateEt { get; set; }
        public DateTime dateModified { get; set; }
        public string checkTimeEt { get; set; }
        public int death { get; set; }
        public object hospitalized { get; set; }
        public DateTime dateChecked { get; set; }
        public int totalTestsViral { get; set; }
        public object positiveTestsViral { get; set; }
        public object negativeTestsViral { get; set; }
        public int positiveCasesViral { get; set; }
        public object deathConfirmed { get; set; }
        public object deathProbable { get; set; }
        public object totalTestEncountersViral { get; set; }
        public object totalTestsPeopleViral { get; set; }
        public object totalTestsAntibody { get; set; }
        public object positiveTestsAntibody { get; set; }
        public object negativeTestsAntibody { get; set; }
        public object totalTestsPeopleAntibody { get; set; }
        public object positiveTestsPeopleAntibody { get; set; }
        public object negativeTestsPeopleAntibody { get; set; }
        public object totalTestsPeopleAntigen { get; set; }
        public object positiveTestsPeopleAntigen { get; set; }
        public object totalTestsAntigen { get; set; }
        public object positiveTestsAntigen { get; set; }
        public string fips { get; set; }
        public int positiveIncrease { get; set; }
        public int negativeIncrease { get; set; }
        public int total { get; set; }
        public int totalTestResultsIncrease { get; set; }
        public int posNeg { get; set; }
        public int deathIncrease { get; set; }
        public int hospitalizedIncrease { get; set; }
        public string hash { get; set; }
        public int commercialScore { get; set; }
        public int negativeRegularScore { get; set; }
        public int negativeScore { get; set; }
        public int positiveScore { get; set; }
        public int score { get; set; }
        public string grade { get; set; }
    }
}