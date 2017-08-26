using MethodTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayWith.AOP
{
    /// <summary>
    /// Inspired By https://github.com/jlattimer/D365FodyLogging/
    /// </summary>
    public class Program
    {
        [Time]
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
        }
    }
}
