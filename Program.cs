// Created by:  Ian Catapano
// Date:		3/14/19
// Purpose:		File comes to us with names in all upper case, but Meditech Expanse requires them in proper case for import.
//				This program will take that file from a directory change those names to proper case and return the file to a different directory and delete the old file.

using System;
using System.IO;
using System.Globalization;

namespace Vanguard_Names_ToProper
{
    class Program
    {
        static void Main(string[] args)
        {
            string envSource = @"%USERPROFILE%\Documents\Source\";
            string envTarget = @"%USERPROFILE%\Documents\Target\";
            var dirSource = Environment.ExpandEnvironmentVariables(envSource);
            var dirTarget = Environment.ExpandEnvironmentVariables(envTarget);

            string[] files = Directory.GetFiles(dirSource);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string deletePath = dirSource + fileName;
                string finalPath = dirTarget + fileName;

                foreach (string line in File.ReadLines(deletePath))
                {
                    if (line[0] == '0' && line[1] == '5')
                    {
                        string line1 = line.ToUpper(new CultureInfo("en-US", false));
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                        string newLine = textInfo.ToTitleCase(line1.ToLower());
                        File.AppendAllText(finalPath, newLine + Environment.NewLine);
                    }
                    else
                        File.AppendAllText(finalPath, line + Environment.NewLine);
                }
                File.Delete(deletePath);
            }
        }
    }
}