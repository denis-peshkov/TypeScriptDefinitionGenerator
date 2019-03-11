using System;
using System.Collections.Generic;

namespace ClassLibrary1
{
    public class SomeSomeClass: SomeClass
    {
        public int Inc2 { get; set; }
        public long Inc3 { get; set; }
        public float Inc4 { get; set; }
        public decimal Inc5 { get; set; }
        public Guid Inc6 { get; set; }
        public char Inc7 { get; set; }
        public List<int> Inc8 { get; set; }
        public List<int> Inc9 { get; set; }
        public IEnumerable<int> Inc10 { get; set; }
        public int[] Inc11 { get; set; }
        public bool Inc12 { get; set; }
        public Dictionary<string, int> Inc13 { get; set; }
        public IDictionary<string, int> Inc14 { get; set; }
    }
}