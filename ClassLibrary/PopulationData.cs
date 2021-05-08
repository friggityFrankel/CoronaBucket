using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary
{
    class PopulationData
    {

        public class Rootobject
        {
            public Datum[] data { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string title { get; set; }
            public string desc { get; set; }
            public string doi { get; set; }
            public string date { get; set; }
            public string popyear { get; set; }
            public string citation { get; set; }
            public string data_file { get; set; }
            public string archive { get; set; }
            public string _public { get; set; }
            public string source { get; set; }
            public string data_format { get; set; }
            public string author_email { get; set; }
            public string author_name { get; set; }
            public string maintainer_name { get; set; }
            public string maintainer_email { get; set; }
            public string project { get; set; }
            public string category { get; set; }
            public string gtype { get; set; }
            public string continent { get; set; }
            public string country { get; set; }
            public string iso3 { get; set; }
            public string[] files { get; set; }
            public string url_img { get; set; }
            public string organisation { get; set; }
            public string license { get; set; }
            public string url_summary { get; set; }
        }

    }
}
