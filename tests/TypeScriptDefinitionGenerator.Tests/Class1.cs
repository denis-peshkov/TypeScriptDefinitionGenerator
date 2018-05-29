using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary1;

namespace TypeScriptDefinitionGenerator.Tests
{
    public class Class1
    {
        public string Temp { get; set; }

        public Mas Mas { get; set; }

        public Some Some { get; set; }
    }

    public enum Mas
    {
        First,
        Next
    }
}
