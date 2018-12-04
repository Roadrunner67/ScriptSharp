using ScriptSharp.ScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CSharpScriptEngine.Execute(
            //This could be code submitted from the editor
            //    @"using System;

            //public class ScriptedClass
            //{
            //    public string HelloWorld {get;set;}
            //    public ScriptedClass()
            //    {
            //        Console.WriteLine($""Initializing the {nameof(ScriptedClass)} class"");
            //        HelloWorld = ""Hello Roslyn!"";
            //    }

            //    public string GreetSoren()
            //    {
            //        return ""Hello Søren"";
            //    }
            //}"
            @"using System;

            public class CommTest
            {
                Func<string, Double> _readValFunc;

                public CommTest(Func<string, Double> readValFunc)
                {
                    _readValFunc = readValFunc;
                }
                public Double GetAValue(string identifier)
                {
                    return _readValFunc(identifier);
                }
            }
            //Console.WriteLine(new CommTest(ReadVal).GetAValue(""Test""));
            "
            );
            //And this from the REPL
            //Console.WriteLine(CSharpScriptEngine.Execute("new ScriptedClass().HelloWorld"));
            //Console.WriteLine(CSharpScriptEngine.Execute("new ScriptedClass().GreetSoren()"));
            Console.WriteLine(CSharpScriptEngine.Execute("new CommTest(ReadVal).GetAValue(\"Test\").ToString()"));

            Console.ReadKey();
        }
    }
    //Output: "Hello Roslyn!"

}
