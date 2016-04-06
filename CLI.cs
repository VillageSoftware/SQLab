using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLab
{
    public class CLI
    {
        private string _connectionString;
        private const int _defaultNumberOfColumns = 255;
        private int? _limitNumberOfColumns = null;

        public CLI() { }

        public int Run(string[] args)
        {
            // Usage
            // $ sqlab script1.sql script2.sql

            const string ConnectionStringName = "default";
            _connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

            if (args.Length < 2)
            {
                Output("Not enough args", ConsoleColor.Red);
                Output("Usage:");
                Output(" > sqlab File1.sql File2.sql");
                return 0xA0; //Bad args
            }

            _limitNumberOfColumns = ParseArgs(args);

            //Load SQL files from args

            string sql1 = LoadSql(args[0]);
            if (sql1 == null)
            {
                return 0x2; //File not found
            }

            string sql2 = LoadSql(args[1]);
            if (sql2 == null)
            {
                return 0x2; //File not found
            }

            //Run SQL files

            List<string> results1;
            List<string> results2;

            try
            {
                results1 = RunSql(sql1);
                results2 = RunSql(sql2);
            }
            catch (ApplicationException)
            {
                return 0x6; //Invalid handle (SQL failure)
            }
            catch (ArgumentException)
            {
                return 0xA0; //Bad args
            }

            //Compare results

            int resultCode = 0;

            int max = Math.Min(results1.Count, results2.Count);
            //int diff = Math.Abs(results1.Count - results2.Count);

            int i;
            for (i = 0; i < max; i++)
            {
                if (results1[i] != results2[i])
                {
                    Output("Fail: row number {0} has differences", ConsoleColor.Red, i);
                    Output("A ='{0}'", results1[i]);
                    Output("B ='{0}'", results2[i]);
                    resultCode = 0x1;
                }
            }

            if (results1.Count != results2.Count)
            {
                Output("Fail: different number of records", ConsoleColor.Red);
                Output("A has {0} records", results1.Count);
                Output("B has {0} records", results2.Count);
                resultCode = 0x1;
            }

            if (_limitNumberOfColumns.HasValue)
            {
                Output("Warning: Only the first {0} columns were checked, because of the --only option", ConsoleColor.Yellow, _limitNumberOfColumns);
            }

            if (i == 0)
            {
                Output("Warning: There were no results to compare. Check your SQL files.", ConsoleColor.Yellow);
            }

            if (resultCode == 0)
            {
                Output("Success: The results are the same.", ConsoleColor.Green);
            }

            Output("Rows processed: {0}", i);
            Output("Done");
            return resultCode;

        }

        private int? ParseArgs(string[] args)
        {
            if (args.Length > 2 && args[2] == "--only")
            {
                if (args.Length == 3)
                {
                    Output("Warning: User specified --only flag but without a number of fields", ConsoleColor.Yellow);
                    return null;
                }

                int validInt;
                if (Int32.TryParse(args[3], out validInt))
                {
                    return validInt;
                }
            }

            return null;
        }

        private List<string> RunSql(string sql)
        {
            var rows = new List<string>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var command = new SqlCommand(sql, conn))
                {
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            do
                            {
                                //Default number of columns
                                int width = _limitNumberOfColumns ?? _defaultNumberOfColumns;
                                bool firstRun = true;

                                foreach (IDataRecord dataRow in reader)
                                {
                                    var values = new object[width];

                                    //Get the values and correct the number of columns
                                    width = dataRow.GetValues(values);

                                    if (_limitNumberOfColumns.HasValue)
                                    {
                                        width = _limitNumberOfColumns.Value;
                                    }

                                    if (firstRun)
                                    {
                                        values = values.Take(width).ToArray();
                                    }

                                    try
                                    {
                                        string row = String.Join(",", values.Select(v => v.ToString()));
                                        rows.Add(row);
                                    }
                                    catch (NullReferenceException)
                                    {
                                        Output("Error: Some results have fewer columns than specified in the --only option. Please remove or reduce the --only option and try again.", ConsoleColor.Red);
                                        throw new ArgumentException();
                                    }

                                    firstRun = false;
                                }

                                //Read multiple batches using NextResult
                            } while (reader.NextResult());
                        }
                    }
                    catch (Exception ex)
                    {
                        Output("Error: ({0}) {1}", ConsoleColor.Red, ex.GetType().ToString(), ex.Message);
                        throw new ApplicationException();
                    }
                }
            }

            return rows;
        }

        private void Output(string format, ConsoleColor colour, params object[] args)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(format, (object[])args);
            Console.ResetColor();
        }

        private void Output(string format, params object[] args)
        {
            Console.WriteLine(format, (object[])args);
        }

        private void Output(string text)
        {
            Console.WriteLine(text);
        }

        private void Output(string text, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private string LoadSql(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + filePath;
            }

            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                Output("Can't load SQL file '{0}'", filePath);
                return null;
            }
        }

    }
}
