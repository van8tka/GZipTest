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
            if (args.Length == 3)
            {
                action = args[0];
                input = args[1];
                output = args[2];
                if (CheckAction(action) && CheckInputFile(input) && CheckOutputFile(output))
                    return true;
                else
                {
                    Console.ReadLine();
                    return false;
                }
                    
            }          
            Console.WriteLine("Ошибка входных параметров.");
            Console.ReadLine();
            return false;
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
                Console.WriteLine("Ошибка в указании пути к выходному файлу." + e);
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
                    Console.WriteLine("Входной файл отсутствует или путь к файлу указан не верно.");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка в указании пути к входному файлу." + e);
                return false;
            }
        }


        private static bool CheckAction(string action)
        {
            if (action.Equals(Constants.COMPRESS, StringComparison.OrdinalIgnoreCase) || action.Equals(Constants.DECOMPRESS, StringComparison.OrdinalIgnoreCase))
                return true;
            else
                Console.WriteLine("Ошибка указания параметра действия архиватора([compress\\decompress])");
            return false;
        }

        private static bool IsValidPath(string path)
        {
            try
            {
                Regex driveRegex = new Regex(@"^[a-zA-Z]:\\$");
                if (!driveRegex.IsMatch(path.Substring(0, 3)))
                {
                    Console.WriteLine("Указанного диска не существует или путь к файлу указан не верно.");
                    return false;
                }
                string invalidChars = new string(Path.GetInvalidPathChars()) + @":/?*" + "\"";
                Regex invalidRegex = new Regex("[" + Regex.Escape(invalidChars) + "]");
                if (invalidRegex.IsMatch(path.Substring(3, path.Length - 3)))
                {
                    Console.WriteLine("При  указании пути к файлу используются недопустимые символы.");
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
