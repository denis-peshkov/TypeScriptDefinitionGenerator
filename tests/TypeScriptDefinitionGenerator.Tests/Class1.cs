using ClassLibrary1;

namespace TypeScriptDefinitionGenerator.Tests
{
    public class Class1
    {
        public string StrField { get; set; }

        public IntEnum IntEnumField { get; set; }

        public SomeClass SomeClassField { get; set; }

        public SomeEnum SomeEnumField { get; set; }

        public SomeSomeClass SomeSomeClassField { get; set; }

    }

    public enum IntEnum
    {
        First,
        Next
    }
}
