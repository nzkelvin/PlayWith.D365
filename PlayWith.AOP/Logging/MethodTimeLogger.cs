using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlayWith.AOP.Logging
{
    internal static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, long milliseconds)
        {
            Console.WriteLine($"{methodBase.Name} Method Time elapsed: {milliseconds}ms");
        }
    }
}
