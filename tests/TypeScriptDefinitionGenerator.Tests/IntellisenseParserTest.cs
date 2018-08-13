using System.Linq;
using EnvDTE;
using NUnit.Framework;
using EnvDTE80;

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
        }

    }
}