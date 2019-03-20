using System.Linq;
using EnvDTE;
using EnvDTE80;
using NUnit.Framework;

//using Microsoft.Extensions.Options;
//using Ploeh.AutoFixture;
//using Microsoft.AspNetCore.Mvc;

namespace TypeScriptDefinitionGenerator.Tests
{
    [TestFixture]
    public class IntellisenseParserTest : BaseTestController
    {
        public override void SetUp()
        {
            base.SetUp();
        }

        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        [Explicit("Can't be run on build server, some problems on build server")]
        public void HowToUseCodeModelSpike()
        {
            // get the DTE reference...
            DTE2 dte2 = (DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.15.0");

            // get the solution
            var worker = new SolutionWorker();
            worker.ExamineSolution(dte2.Solution);
        }

        [Test]
        [Explicit("Can't be run on build server, some problems on build server")]
        public void _ShouldWorkProperly()
        {
            //Arrange

            // get the DTE reference...
            DTE2 dte2 = (DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.15.0");


            var worker = new SolutionWorker();
            worker.ExamineSolution(dte2.Solution);

            ProjectItem item = worker.GetProjectItem(dte2.Solution, "Class1.cs");

            //Act
            Options.SetOptionsOverrides(new OptionsOverride()
            {
                CamelCaseEnumerationValues = false,
                CamelCasePropertyNames = true,
                CamelCaseTypeNames = false,

                WebEssentials2015 = false,

                ClassInsteadOfInterface = false,
                DefaultModuleName = "Server.Dtos",
                UseNamespace = true,
                DeclareModule = true,
                IgnoreIntellisense = true,
            });
            var list = IntellisenseParser.ProcessFile(item).ToList();
            var sourceItemPath = item.Properties.Item("FullPath").Value as string;
            var tsFile = IntellisenseWriter.WriteTypeScript(list, sourceItemPath);

            //Assert
            Assert.IsNotNull(list);
            Assert.NotZero(list.Count);
            Assert.AreEqual("TypeScriptDefinitionGenerator.Tests.Class1", list[0].FullName);
            Assert.AreEqual("Class1", list[0].Name);
            Assert.AreEqual(5, list[0].Properties.Count);

            Assert.AreEqual("IntEnumField", list[0].Properties[1].Name);
            Assert.AreEqual("IntEnumField", list[0].Properties[1].NameWithOption);
            Assert.AreEqual(null, list[0].Properties[1].InitExpression);
            Assert.AreEqual(null, list[0].Properties[1].Summary);
            Assert.AreEqual("TypeScriptDefinitionGenerator.Tests.IntEnum", list[0].Properties[1].Type.CodeName);
            Assert.AreNotEqual("any", list[0].Properties[1].Type.TypeScriptName);
            Assert.AreEqual("TypeScriptDefinitionGenerator.Tests.IntEnum", list[0].Properties[1].Type.TypeScriptName);
            Assert.AreNotEqual(null, list[0].Properties[1].Type.ClientSideReferenceName);
            Assert.AreEqual("TypeScriptDefinitionGenerator.Tests.IntEnum", list[0].Properties[1].Type.ClientSideReferenceName);
            Assert.AreEqual(false, list[0].Properties[1].Type.IsArray);
            Assert.AreEqual(false, list[0].Properties[1].Type.IsDictionary);
            Assert.AreEqual(true, list[0].Properties[1].Type.IsKnownType);
            Assert.AreEqual(false, list[0].Properties[1].Type.IsOptional);


            Assert.AreEqual("SomeEnumField", list[0].Properties[3].Name);
            Assert.AreEqual("SomeEnumField", list[0].Properties[3].NameWithOption);
            Assert.AreEqual(null, list[0].Properties[3].InitExpression);
            Assert.AreEqual(null, list[0].Properties[3].Summary);
            Assert.AreEqual("ClassLibrary1.SomeEnum", list[0].Properties[3].Type.CodeName);
            Assert.AreNotEqual("any", list[0].Properties[3].Type.TypeScriptName);
            Assert.AreEqual("ClassLibrary1.SomeEnum", list[0].Properties[3].Type.TypeScriptName);
            Assert.AreNotEqual(null, list[0].Properties[3].Type.ClientSideReferenceName);
            Assert.AreEqual("ClassLibrary1.SomeEnum", list[0].Properties[3].Type.ClientSideReferenceName);
            Assert.AreEqual(false, list[0].Properties[3].Type.IsArray);
            Assert.AreEqual(false, list[0].Properties[3].Type.IsDictionary);
            Assert.AreEqual(true, list[0].Properties[3].Type.IsKnownType);
            Assert.AreEqual(false, list[0].Properties[3].Type.IsOptional);


            Assert.AreEqual("SomeSomeClassField", list[0].Properties[4].Name);
            Assert.AreEqual("SomeSomeClassField", list[0].Properties[4].NameWithOption);
            Assert.AreEqual(null, list[0].Properties[4].InitExpression);
            Assert.AreEqual(null, list[0].Properties[4].Summary);
            Assert.AreEqual("ClassLibrary1.SomeSomeClass", list[0].Properties[4].Type.CodeName);
            Assert.AreNotEqual("any", list[0].Properties[4].Type.TypeScriptName);
            Assert.AreEqual("ClassLibrary1.SomeSomeClass", list[0].Properties[4].Type.TypeScriptName);
            Assert.AreNotEqual(null, list[0].Properties[4].Type.ClientSideReferenceName);
            Assert.AreEqual("ClassLibrary1.SomeSomeClass", list[0].Properties[4].Type.ClientSideReferenceName);
            Assert.AreEqual(false, list[0].Properties[4].Type.IsArray);
            Assert.AreEqual(false, list[0].Properties[4].Type.IsDictionary);
            Assert.AreEqual(true, list[0].Properties[4].Type.IsKnownType);
            Assert.AreEqual(false, list[0].Properties[4].Type.IsOptional);
            Assert.AreEqual(13, list[0].Properties[4].Type.Shape.Count());
            // Inc6
            Assert.AreEqual("System.Guid", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc6").Type.CodeName);
            Assert.AreNotEqual("any", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc6").Type.TypeScriptName);
            Assert.AreEqual("string", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc6").Type.TypeScriptName);
            // Inc13
            Assert.AreEqual("System.Collections.Generic.Dictionary<string, int>", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc13").Type.CodeName);
            Assert.AreEqual("System.Collections.Generic.Dictionary", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc13").Type.ClientSideReferenceName);
            Assert.AreEqual(true, list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc13").Type.IsDictionary);
            Assert.AreEqual("{ [index: string]: number }", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc13").Type.TypeScriptName);
            // Inc14
            Assert.AreEqual("System.Collections.Generic.IDictionary<string, int>", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc14").Type.CodeName);
            Assert.AreEqual("System.Collections.Generic.IDictionary", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc14").Type.ClientSideReferenceName);
            Assert.AreEqual(true, list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc14").Type.IsDictionary);
            Assert.AreEqual("{ [index: string]: number }", list[0].Properties[4].Type.Shape.First(o => o.Name == "Inc14").Type.TypeScriptName);
        }

        [Test]
        [Explicit("Can't be run on build server, some problems on build server")]
        public void _ShouldWorkProperly2()
        {
            //Arrange

            // get the DTE reference...
            DTE2 dte2 = (DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.15.0");


            var worker = new SolutionWorker();
            worker.ExamineSolution(dte2.Solution);

            ProjectItem item = worker.GetProjectItem(dte2.Solution, "SomeClass.cs");

            var expectedFile =
                "// ------------------------------------------------------------------------------\n" +
                "// <auto-generated>\n" +
                "//     This file was generated by TypeScript Definition Generator v2.2.0.0\n" +
                "// </auto-generated>\n" +
                "// ------------------------------------------------------------------------------\n" +
                "import { SomeEnum } from \"./SomeEnum.generated\";\n" +
                "export interface SomeClass {\n" +
                "  inc1: number;\n" +
                "  some: SomeEnum;\n" +
                "}\n";

            //Act
            Options.SetOptionsOverrides(new OptionsOverride()
            {
                CamelCaseEnumerationValues = false,
                CamelCasePropertyNames = true,
                CamelCaseTypeNames = false,

                WebEssentials2015 = false,

                ClassInsteadOfInterface = false,
                DeclareModule = false,
                DefaultModuleName = "Server.Dtos",
                EOLType = EOLType.LF,
                IgnoreIntellisense = true,
                IndentTab = false,
                IndentTabSize = 2,
                UseNamespace = true,
            });
            var list = IntellisenseParser.ProcessFile(item).ToList();
            var sourceItemPath = item.Properties.Item("FullPath").Value as string;
            var tsFile = IntellisenseWriter.WriteTypeScript(list, sourceItemPath);

            //Assert
            Assert.IsNotNull(list);
            Assert.NotZero(list.Count);
            Assert.AreEqual("ClassLibrary1.SomeClass", list[0].FullName);
            Assert.AreEqual("SomeClass", list[0].Name);
            Assert.AreEqual(2, list[0].Properties.Count);

            Assert.AreEqual("Inc1", list[0].Properties[0].Name);
            Assert.AreEqual("Inc1", list[0].Properties[0].NameWithOption);
            Assert.AreEqual(null, list[0].Properties[0].InitExpression);
            Assert.AreEqual(null, list[0].Properties[0].Summary);
            Assert.AreEqual("int", list[0].Properties[0].Type.CodeName);
            Assert.AreEqual("number", list[0].Properties[0].Type.TypeScriptName);
            Assert.AreEqual("number", list[0].Properties[0].Type.TypeScriptName);
            Assert.AreEqual(null, list[0].Properties[0].Type.ClientSideReferenceName);
            Assert.AreEqual(false, list[0].Properties[0].Type.IsArray);
            Assert.AreEqual(false, list[0].Properties[0].Type.IsDictionary);
            Assert.AreEqual(true, list[0].Properties[0].Type.IsKnownType);
            Assert.AreEqual(false, list[0].Properties[0].Type.IsOptional);

            Assert.AreEqual("Some", list[0].Properties[1].Name);
            Assert.AreEqual("Some", list[0].Properties[1].NameWithOption);
            Assert.AreEqual(null, list[0].Properties[1].InitExpression);
            Assert.AreEqual(null, list[0].Properties[1].Summary);
            Assert.AreEqual("ClassLibrary1.SomeEnum", list[0].Properties[1].Type.CodeName);
            Assert.AreEqual("SomeEnum", list[0].Properties[1].Type.TypeScriptName);
            Assert.AreEqual("SomeEnum", list[0].Properties[1].Type.TypeScriptName);
            Assert.AreEqual("SomeEnum", list[0].Properties[1].Type.ClientSideReferenceName);
            Assert.AreEqual(false, list[0].Properties[1].Type.IsArray);
            Assert.AreEqual(false, list[0].Properties[1].Type.IsDictionary);
            Assert.AreEqual(true, list[0].Properties[1].Type.IsKnownType);
            Assert.AreEqual(false, list[0].Properties[1].Type.IsOptional);

            Assert.AreEqual(expectedFile, tsFile);

        }

        [Test]
        [Explicit("Can't be run on build server, some problems on build server")]
        public void CheckGenerateTypeScriptOutput()
        {
            // get the DTE reference...
            DTE2 dte2 = (DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.15.0");

            var worker = new SolutionWorker();
            worker.ExamineSolution(dte2.Solution);

            var testFiles = new System.Collections.Generic.Dictionary<string, string>
            {
                {
                    "SomeEnum.cs",
                    "// ------------------------------------------------------------------------------\n" +
                    "// <auto-generated>\n" +
                    "//     This file was generated by TypeScript Definition Generator v2.2.0.0\n" +
                    "// </auto-generated>\n" +
                    "// ------------------------------------------------------------------------------\n" +
                    "export enum SomeEnum {\n" +
                    "  This,\n" +
                    "  That,\n" +
                    "}\n"
                },
                {
                    "SecondClass.cs",
                    "// ------------------------------------------------------------------------------\n" +
                    "// <auto-generated>\n" +
                    "//     This file was generated by TypeScript Definition Generator v2.2.0.0\n" +
                    "// </auto-generated>\n" +
                    "// ------------------------------------------------------------------------------\n" +
                    "import { SomeClass } from \"./SomeClass.generated\";\n" +
                    "import { SomeSomeClass } from \"./SomeSomeClass.generated\";\n" +
                    "export interface SecondClass {\n" +
                    "  myProperty: number;\n" +
                    "  complex1: SomeClass;\n" +
                    "  complex2: SomeSomeClass;\n" +
                    "}\n"
                },
                {
                    "SomeClass.cs",
                    "// ------------------------------------------------------------------------------\n" +
                    "// <auto-generated>\n" +
                    "//     This file was generated by TypeScript Definition Generator v2.2.0.0\n" +
                    "// </auto-generated>\n" +
                    "// ------------------------------------------------------------------------------\n" +
                    "import { SomeEnum } from \"./SomeEnum.generated\";\n" +
                    "export interface SomeClass {\n" +
                    "  inc1: number;\n" +
                    "  some: SomeEnum;\n" +
                    "}\n"
                },
                {
                    "SomeSomeClass.cs",
                    "// ------------------------------------------------------------------------------\n" +
                    "// <auto-generated>\n" +
                    "//     This file was generated by TypeScript Definition Generator v2.2.0.0\n" +
                    "// </auto-generated>\n" +
                    "// ------------------------------------------------------------------------------\n" +
                    "import { SomeClass } from \"./SomeClass.generated\";\n" +
                    "export interface SomeSomeClass extends SomeClass {\n" +
                    "  inc2: number;\n" +
                    "  inc3: number;\n" +
                    "  inc4: number;\n" +
                    "  inc5: number;\n" +
                    "  inc6: string;\n" +
                    "  inc7: any;\n" +
                    "  inc8: number[];\n" +
                    "  inc9: number[];\n" +
                    "  inc10: number[];\n" +
                    "  inc11: number[];\n" +
                    "  inc12: boolean;\n" +
                    "  inc13: { [index: string]: number };\n" +
                    "  inc14: { [index: string]: number };\n" +
                    "}\n"
                },
                {
                    "ThirdClass.cs",
                    "// ------------------------------------------------------------------------------\n" +
                    "// <auto-generated>\n" +
                    "//     This file was generated by TypeScript Definition Generator v2.2.0.0\n" +
                    "// </auto-generated>\n" +
                    "// ------------------------------------------------------------------------------\n" +
                    "import { SomeClass } from \"../SomeClass.generated\";\n" +
                    "import { SomeSomeClass } from \"../SomeSomeClass.generated\";\n" +
                    "export interface ThirdClass {\n" +
                    "  myProperty: number;\n" +
                    "  complex1: SomeClass;\n" +
                    "  complex2: SomeSomeClass;\n" +
                    "}\n"
                }
            };

            foreach (var testFile in testFiles)
            {
                var testFileName = testFile.Key;
                var expectedFile = testFile.Value;
                ProjectItem item = worker.GetProjectItem(dte2.Solution, testFileName);
                
                Assert.NotNull(item, $"Could not find {testFileName}");

                Options.SetOptionsOverrides(new OptionsOverride()
                {
                    CamelCaseEnumerationValues = false,
                    CamelCasePropertyNames = true,
                    CamelCaseTypeNames = false,
                    WebEssentials2015 = false,
                    ClassInsteadOfInterface = false,
                    DeclareModule = false,
                    DefaultModuleName = "Server.Dtos",
                    EOLType = EOLType.LF,
                    IgnoreIntellisense = true,
                    IndentTab = false,
                    IndentTabSize = 2,
                    UseNamespace = true,
                });
                var list = IntellisenseParser.ProcessFile(item).ToList();
                var sourceItemPath = item.Properties.Item("FullPath").Value as string;
                var tsFile = IntellisenseWriter.WriteTypeScript(list, sourceItemPath);

                Assert.AreEqual(expectedFile, tsFile);
            }
        }
    }
}