﻿using System.Collections.Generic;

namespace ClassLibrary
{
    public class StateData
    {
        // pasted from StateList.json
        public List<StateInfo> states { get; set; }
    }

    public class StateInfo
    {
        public string State { get; set; }
        public string Abbrev { get; set; }
        public string Code { get; set; }
        public int Population { get; set; }
    }

}