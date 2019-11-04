using System;
using EnvDTE;
using NUnit.Framework;

namespace TypeScriptDefinitionGenerator.Tests
{
    [TestFixture]
    public class BaseTestController
    {
        /// <summary>
        /// "VisualStudio.DTE.15.0" for VS2017
        /// "VisualStudio.DTE.16.0" for VS2019
        /// </summary>
        internal const string VisualStudioProgId = "VisualStudio.DTE.16.0";

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
        }

        [SetUp]
        public virtual void SetUp()
        {

        }

        [TearDown]
        public virtual void TearDown()
        {

        }
    }
}
