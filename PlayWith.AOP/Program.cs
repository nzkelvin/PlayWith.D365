using MethodTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayWith.AOP
{
    class Program
    {
        [Time]
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
        }
    }
}
