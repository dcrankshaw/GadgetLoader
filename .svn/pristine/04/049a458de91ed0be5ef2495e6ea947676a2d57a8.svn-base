using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Jhu.SqlServer.Array;
using System.Data.SqlTypes;
using System.Diagnostics;
using GadgetLoader;

namespace BoundingBox
{
    class Program
    {
        //Command line args of the form:
            // BoundingBox localhost SimDBdev 0 64 0 1000
        // BoundingBox {SERVER} {DATABASE} {TIMESTEPSTART} {TIMESTEPEND} {MINFOF} {MAXFOF}
        static void Main(string[] args)
        {
            
            string server = args[0];
            string database = args[1];
            //rest of args are hardcoded right now
            /*
            short timestepInitial = Int16.Parse(args[2]);
            short timestepFinal = Int16.Parse(args[3]);
            int minFofID = Int32.Parse(args[4]);
            int maxFofID = Int32.Parse(args[5]);
            */

            createBoxes(server, database);

            //runTests(server, database);
            
        }

        public static void runTests(string server, string database)
        {
            //short[] timesteps = {30, 40, 50, 60};
            short[] timesteps = { 40, 50, 60 };
            float[] pads = {15, 20, 25, 30};
            double percent = .15;
            Tester myTester = new Tester(server, database, timesteps, pads, percent);
            myTester.collectStats();
            string dir = "D:\\bounding_box_testing";
            myTester.writeResults(dir);

        }

        public static void createBoxes(string server, string database)
        {
            DateTime start = DateTime.Now;
            float[] pads = {0, 40, 45};
            foreach (float pad in pads)
            {
                BoxCreator current = new BoxCreator(server, database, 30, 60, 0, -1, pad);
                current.createBoxes();
                Console.Out.WriteLine("Pad level {0} completed", pad);
                DateTime curTime = DateTime.Now;
                TimeSpan elapsedTime = curTime - start;
                try
                {
                    Console.Out.WriteLine("Elapsed time: {0,c}" + elapsedTime);
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("Time elapsed formatting error. Can ignore.");
                    Console.Out.WriteLine(e.Message);
                }
            }
        }
    }

    
}
