using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLab
{
    class Program
    {
        static int Main(string[] args)
        {

            var cli = new CLI();
            return cli.Run(args);

        }

    }
}
