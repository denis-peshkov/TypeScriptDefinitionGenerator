using System;
using System.Text;
using EnvDTE;
using NUnit.Framework;
using System.Windows.Forms;

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
        public void _ShouldWorkProperly()
        {
            //Arrange
            ProjectItem item = null;
             

            var proj = EnvDTE.Solution.FindProjectItem(knownFile).ContainingProject;


            //Act
            string dts = GenerationService.ConvertToTypeScript(item);
            var res = Encoding.UTF8.GetBytes(dts);

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