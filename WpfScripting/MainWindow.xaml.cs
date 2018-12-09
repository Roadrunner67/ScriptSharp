using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeEnvironment;

namespace WpfScripting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            code.Text = @"
                using System;
                using Calculator;
                using CodeEnvironment;

                namespace First
                {
                    public class Program
                    {
                        public static void Main()
                        {
                            Env.WriteLine(""Hello, world!"");
                            Calc calc = new Calc();
                            Env.WriteLine(calc.Add(7,13).ToString());
                        }
                    }
                }
            ";
        }

        public void Log(string logOutput)
        {
            output.Text += logOutput + "\n";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            // Reference to System.Drawing library
            parameters.ReferencedAssemblies.Add("Calculator.dll");
            parameters.ReferencedAssemblies.Add("CodeEnvironment.dll");
            // True - memory generation, false - external file generation
            parameters.GenerateInMemory = true;
            // True - exe file generation, false - dll file generation
            parameters.GenerateExecutable = true;
            Env.OnLogoutput += Log;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code.Text);

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException(sb.ToString());
            }

            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("First.Program");
            MethodInfo main = program.GetMethod("Main");

            main.Invoke(null, null);

        }
    }
}
