using System;
using EnvDTE;
using NUnit.Framework;

namespace TypeScriptDefinitionGenerator.Tests
{
    [TestFixture]
    public class BaseTestController
    {

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
