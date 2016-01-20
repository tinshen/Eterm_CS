using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rams;

namespace Eterm_CS
{
    public sealed class DataAccess
    {
        public static string Path;
        public static string ClassName;

        public DataAccess()
        {
        }

        public static IDBHelp Create()
        {
            return (IDBHelp)Assembly.Load(Path).CreateInstance(ClassName);
        }
    }
}
