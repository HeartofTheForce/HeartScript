using System;
using System.IO;
using HeartScript.Compiling.Emit;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Cli
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                string source = File.ReadAllText(args[0]);

                var parser = ParsingHelper.BuildPatternParser("./src/demo.hg");
                var pattern = parser.Patterns["root"].Trim();
                var node = parser.MatchComplete(pattern, source);

                int arrayLength = 10;
                int[] scriptArray = RandomArray(arrayLength, 0, 100);
                int[] referenceArray = new int[arrayLength];
                Array.Copy(scriptArray, referenceArray, arrayLength);

                object[]? parameters = new object[]
                {
                    scriptArray,
                };
                var compiledMethodInfo = EmitCompiler.CompileFunction(node);
                compiledMethodInfo.Invoke(null, parameters);

                Console.WriteLine("Input");
                Console.WriteLine(string.Join(", ", referenceArray));
                Console.WriteLine("Output");
                Console.WriteLine(string.Join(", ", scriptArray));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static readonly Random s_random = new Random();
        private static int[] RandomArray(int length, int min, int max)
        {
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = s_random.Next(min, max);
            }

            return result;
        }
    }
}
