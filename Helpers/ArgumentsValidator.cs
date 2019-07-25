using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GZipTest
{
    internal class ArgumentsValidator
    {
        /// <summary>
        /// метод проверки входных данных
        /// </summary>
        /// <param name="args">список аргументов проверки</param>
        /// <returns>true-валидные данные, false-невалидные</returns>
        internal static bool Validate(string[] args)
        {
            string action;
            string input;
            string output;
            bool success = false;
            if (args.Length == 3)
            {
                action = args[0];
                input = args[1];
                output = args[2];
                if (CheckAction(action) && CheckInputFile(input) && CheckOutputFile(output))
                    success = true;
            }
            if (!success)
            {
                Console.WriteLine("Error input parameters.");
                Console.ReadLine();
            }
            return success;
        }

        private static bool CheckOutputFile(string output)
        {
            try
            {
                if (!IsValidPath(output))
                    return false;
                var path = Path.GetFullPath(output);
                if (File.Exists(path))
                    return true;
                else
                {
                    File.Create(output).Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Path to output file is not correct." + e);
                return false;
            }
        }

        private static bool CheckInputFile(string input)
        {
            try
            {
                if (!IsValidPath(input))
                    return false;
                var path = Path.GetFullPath(input);
                if (File.Exists(path))
                    return true;
                else
                    Console.WriteLine("The input file is missing or path to file is not correct.");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Path to input file is not correct." + e);
                return false;
            }
        }


        private static bool CheckAction(string action)
        {
            if (action.Equals(Constants.COMPRESS, StringComparison.OrdinalIgnoreCase) || action.Equals(Constants.DECOMPRESS, StringComparison.OrdinalIgnoreCase))
                return true;
            else
                Console.WriteLine("Is not correct action parameter([compress\\decompress]).");
            return false;
        }

        private static bool IsValidPath(string path)
        {
            try
            {
                Regex driveRegex = new Regex(@"^[a-zA-Z]:\\$");
                if (!driveRegex.IsMatch(path.Substring(0, 3)))
                {
                    Console.WriteLine("The disk is not exist or the file path is not correct.");
                    return false;
                }
                string invalidChars = new string(Path.GetInvalidPathChars()) + @":/?*" + "\"";
                Regex invalidRegex = new Regex("[" + Regex.Escape(invalidChars) + "]");
                if (invalidRegex.IsMatch(path.Substring(3, path.Length - 3)))
                {
                    Console.WriteLine("Invalid symbols in the file path.");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
