using System;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    class DebugOut
    {
        static string debug = "";
        static string sqlCommands = "";

        public static void PrintLine(string str)
        {
            debug += str + "\r\n";
        }

        public static string GetDebug()
        {
            return debug;
        }

        public static void ClearCommands()
        {
            sqlCommands = "";
        }

        public static void AddCommand(string command)
        {
            sqlCommands += (command + "\r\n");
        }
        public static void AddCommand(string filename, string table)
        {
            AddCommand(filename, table, true);
        }
        public static void AddCommand(string filename, string table, bool native)
        {
            sqlCommands += "bulk insert " + table + " from '" + filename
                + "' with("+(native?"datafiletype='native'":"fieldterminator=','")+", tablock)\r\n";
        }

        public static void SaveCommands(string filename)
        {
            TextWriter tw = new StreamWriter(filename);
            tw.Write(sqlCommands);
            tw.Close();
        }
    }
}
