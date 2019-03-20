using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator.Helpers
{
    /// <summary>
    /// Exception which contains message which can be shown to user.
    /// </summary>
    class ExceptionForUser : Exception
    {
        public ExceptionForUser(string message) : base(message) { }
        public ExceptionForUser(string message, Exception inner) : base(message, inner) { }
    }
}
