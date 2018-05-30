using System;
using System.Text;
using EnvDTE;
using NUnit.Framework;
using System.Windows.Forms;
using EnvDTE80;

//using Microsoft.Extensions.Options;
//using Ploeh.AutoFixture;
//using Microsoft.AspNetCore.Mvc;

namespace TypeScriptDefinitionGenerator.Tests
{
    [TestFixture]
    public class AuthenticateServiceTest : BaseTestController
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
        public void HowToUseCodeModelSpike()
        {
            // get the DTE reference...
            DTE2 dte2 = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.15.0");

            // get the solution
            var worker = new SolutionWorker();
            worker.ExamineSolution(dte2.Solution);
        }

        public void OpenExample(EnvDTE80.DTE2 dte)
        {
            // Create the full pathname to NewSolution.sln.  
            string tempPath = System.IO.Path.GetTempPath();
            string solnName = "NewSolution";
            string solnPath = tempPath + solnName + ".sln";

            // Try to open NewSolution.sln.  
            try
            {
                dte.Solution.Open(solnPath);
            }
            catch (Exception ex)
            {
                if (MessageBox.Show("Solution " + solnPath +
                                    " doesn't exist. " + "Create it?", "",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Create and save NewSolution.sln.  
                    dte.Solution.Create(tempPath, solnName);
                    dte.Solution.SaveAs(solnPath);
                }
            }
        }

        [Test]
        public void _ShouldWorkProperly()
        {
            //Arrange
            ProjectItem item = null;

            //Solution sln;
            //sln = new SolutionClass();
            //sln.Open(System.IO.Path.GetPathRoot("./") + "TypeScriptDefinitionGenerator.sln");

            //var proj = EnvDTE.Solution.FindProjectItem(knownFile).ContainingProject;




            // get the DTE reference...
            DTE2 dte2 = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.15.0");

            var worker = new SolutionWorker();
            worker.ExamineSolution(dte2.Solution);

            item = worker.GetProjectItem(dte2.Solution, "Class1.cs");

            //Act
            //string dts = GenerationService.ConvertToTypeScript(item);
            //var res = Encoding.UTF8.GetBytes(dts);

            //string dts = GenerationService.ConvertToTypeScript(item);
            Options.SetOptionsOverrides(new OptionsOverride()
            {
                DefaultModuleName = "Server.Dtos",
                WebEssentials2015 = true,
                CamelCaseTypeNames = false,
            });
            var list = IntellisenseParser.ProcessFile(item);


            //Assert
            //Assert.IsNotNull(userId);
            //Assert.AreEqual(registerDto.Login, userNew.Login);
        }
    
        [Test]
        public void e_ShouldWorkProperly()
        {
            //Arrange
            ProjectItem item = null;


            //Act
            //var list = IntellisenseParser.ProcessFile(item, null);
            //var res = IntellisenseWriter.WriteTypeScript(list);

            //Assert
            //Assert.IsNotNull(userId);
            //Assert.AreEqual(registerDto.Login, userNew.Login);
        }
    }
}