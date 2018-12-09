using System;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace ScriptSharp.ScriptEngine
{
    public class Global
    {
        public int X;
        public Func<string, Double> ReadVal;
    }
    public class CSharpScriptEngine
    {
        public static Double MyReader(string Id)
        {
            switch (Id)
            {
                case "Test":
                    return 100.0;
                case "Test2":
                    return 37.5;
                default:
                    return 1.0;
            }
        }

        private static ScriptState<object> scriptState = null;
        public static object Execute(string code)
        {
            Global global = new Global { X = 3, ReadVal = MyReader };
            ScriptOptions scriptOptions = ScriptOptions.Default.WithImports("System", "System.Collections.Generic");
            scriptState = scriptState == null ? CSharpScript.RunAsync(code: code, options: scriptOptions, globals: global).Result : scriptState.ContinueWithAsync(code: code).Result;
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
                return scriptState.ReturnValue;
            return null;
        }
    }
}
