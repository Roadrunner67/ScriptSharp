using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEnvironment
{
    public delegate void LogOutput(string str);

    public static class Env
    {
        public static event LogOutput OnLogoutput;

        public static void WriteLine(string output)
        {
            OnLogoutput(output + "\n");
        }
    }
}
