﻿using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.RepresentationModel;

namespace EVEMon.SDEExternalsToSql
{
    internal static class Util
    {
        private static string s_text;
        private static int s_counter;
        private static int s_percentOld;

        /// <summary>
        /// Updates the percent done.
        /// </summary>
        /// <param name="total">The total.</param>
        internal static void UpdatePercentDone(int total)
        {
            s_counter++;
            int percent = (s_counter * 100 / total);

            if (s_counter != 1 && s_percentOld >= percent)
                return;

            if (!String.IsNullOrEmpty(s_text))
                Console.SetCursorPosition(Console.CursorLeft - s_text.Length, Console.CursorTop);

            s_text = String.Format("{0}%", percent);
            Console.Write(s_text);
            s_percentOld = percent;
        }

        /// <summary>
        /// Displays the end time.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        internal static void DisplayEndTime(DateTime startTime)
        {
            Console.WriteLine(@" in {0}", DateTime.Now.Subtract(startTime).ToString("g"));
        }

        /// <summary>
        /// Resets the counters.
        /// </summary>
        internal static void ResetCounters()
        {
            s_counter = 0;
            s_percentOld = 0;
            s_text = String.Empty;
        }

        /// <summary>
        /// Gets the script for the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        internal static string GetScriptFor(string tableName)
        {
            var resourceName = String.Format(@"EVEMon.SDEExternalsToSql.Scripts.{0}.table.sql", tableName);

            string result = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(result))
                return result;

            throw new SettingsPropertyNotFoundException(String.Format("{0}.table.sql resource file does not exists!", tableName));
        }

        /// <summary>
        /// Checks the yaml file exists.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        internal static string CheckYamlFileExists(string filename)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                String.Format(@"YamlFiles{0}{1}", Path.DirectorySeparatorChar, filename));

            if (File.Exists(filePath))
                return filePath;

            Console.WriteLine(@"{0} file does not exists!", filename);
            return String.Empty;
        }

        /// <summary>
        /// Parses the yaml file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        internal static YamlMappingNode ParseYamlFile(string filePath)
        {
            YamlMappingNode rNode;
            using (TextReader tReader = new StreamReader(filePath))
            {
                YamlStream yStream = new YamlStream();
                yStream.Load(tReader);
                rNode = yStream.Documents.First().RootNode as YamlMappingNode;
            }
            return rNode;
        }

        /// <summary>
        /// Gets the value or default string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        internal static string GetValueOrDefaultString<T>(this T? obj) where T : struct
        {
            return obj.HasValue
                ? obj is Boolean
                    ? Convert.ToByte(obj.GetValueOrDefault()).ToString(CultureInfo.InvariantCulture)
                    : obj.Value.ToString()
                : Database.Null;
        }

        /// <summary>
        /// Gets the text or default string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        internal static string GetTextOrDefaultString(this string text)
        {
            return String.IsNullOrWhiteSpace(text)
                ? Database.Null
                : String.Format("'{0}'", text.Replace("'", Database.StringEmpty));
        }

        internal static void HandleException(IDbCommand command, Exception e)
        {
            Console.WriteLine();
            Console.WriteLine(@"Unable to execute SQL command: {0}", command.CommandText);
            Console.WriteLine(e.Message);
            Console.ReadLine();
            Environment.Exit(-1);

        }
    }
}