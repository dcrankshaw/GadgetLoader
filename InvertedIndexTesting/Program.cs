using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;

namespace InvertedIndexTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            //individualHalosTest();
            //haloRangesTest();
            singleHaloAllTimestepsTest();
            

            
        }

        public static void haloRangesTest()
        {
            using (SqlConnection connection = new SqlConnection("server=gw18;database=particleDB;Trusted_Connection=true;Asynchronous Processing = true"))
            {
                connection.Open();
                Dictionary<int, TimeSpan> noSlotsTiming = new Dictionary<int, TimeSpan>();
                Dictionary<int, TimeSpan> slotsTiming = new Dictionary<int, TimeSpan>();


                runNoSlotsCommand(0, connection);
                for (int numHalos = 1; numHalos <= 10; ++numHalos)
                {
                    noSlotsTiming.Add(numHalos, runNoSlotsRangeCommand(1, numHalos, connection));
                    Debug.WriteLine("NOSLOTS: finished " + numHalos);
                }
                Debug.WriteLine("");

                for (int numHalos = 1; numHalos <= 10; ++numHalos)
                {
                    slotsTiming.Add(numHalos, runSlotsRangeCommand(1, numHalos, connection));
                    Debug.WriteLine("SLOTS: finished " + numHalos);
                }

                using (StreamWriter writer = new StreamWriter("H:\\reverse_index\\results\\index_lookup_range_results", false))
                {
                    foreach (int range in noSlotsTiming.Keys)
                    {
                        writer.WriteLine(range);
                        writer.WriteLine(slotsTiming[range].TotalMilliseconds + ", " + noSlotsTiming[range].TotalMilliseconds);
                    }
                }

            }
        }

        public static void individualHalosTest()
        {
            using (SqlConnection connection = new SqlConnection("server=gw18;database=particleDB;Trusted_Connection=true;Asynchronous Processing = true"))
            {
                connection.Open();
                int[] fofIDs100 = { 2531, 2532, 2533, 2534, 2535, 2536, 2537, 2538, 2539, 2540 };
                int[] fofIDs200 = { 1089, 1090, 1091, 1092, 1093, 1094, 1095, 15019, 15020, 15021 };
                int[] fofIDs500 = { 269, 270, 271, 272, 273, 274, 275, 276, 277, 278 };
                int[] fofIDs1000 = { 82, 14110, 14111, 27195, 27196, 27197, 27198, 27199, 40326, 52843 };
                int[] fofIDs5000 = { 0, 14060, 27136, 40257, 40258, 90431, 104073, 129159, 142428, 167765 };
                Dictionary<int, int[]> fofIDsDict = new Dictionary<int, int[]>();
                fofIDsDict.Add(100, fofIDs100);
                fofIDsDict.Add(200, fofIDs200);
                fofIDsDict.Add(500, fofIDs500);
                fofIDsDict.Add(1000, fofIDs1000);
                fofIDsDict.Add(5000, fofIDs5000);
                
                Dictionary<int, List<TimeSpan>> noSlotsTiming = new Dictionary<int, List<TimeSpan>>();
                Dictionary<int, List<TimeSpan>> slotsTiming = new Dictionary<int, List<TimeSpan>>();




                // first do all slots timing
                foreach (int groupSize in fofIDsDict.Keys)
                {
                    List<TimeSpan> currentTimes = new List<TimeSpan>();
                    foreach (int group in fofIDsDict[groupSize])
                    {
                        currentTimes.Add(runSlotsCommand(group, connection));
                    }
                    slotsTiming.Add(groupSize, currentTimes);
                    Debug.WriteLine("SLOTS: Finished group size " + groupSize);
                }

                // now do all noSlots timing
                foreach (int groupSize in fofIDsDict.Keys)
                {
                    List<TimeSpan> currentTimes = new List<TimeSpan>();
                    foreach (int group in fofIDsDict[groupSize])
                    {
                        currentTimes.Add(runNoSlotsCommand(group, connection));
                    }
                    noSlotsTiming.Add(groupSize, currentTimes);
                    Debug.WriteLine("NOSLOTS: Finished group size " + groupSize);
                }

                using (StreamWriter writer = new StreamWriter("H:\\reverse_index\\results\\index_lookup_results", false))
                {
                    foreach (int groupSize in fofIDsDict.Keys)
                    {
                        writer.WriteLine(groupSize);
                        List<TimeSpan> noSlots = noSlotsTiming[groupSize];
                        List<TimeSpan> slots = slotsTiming[groupSize];
                        for (int i = 0; i < noSlots.Count; ++i)
                        {
                            writer.WriteLine(slots[i].TotalMilliseconds + ", " + noSlots[i].TotalMilliseconds);
                        }
                        writer.WriteLine();
                    }
                }

            }
        }

        public static void singleHaloAllTimestepsTest()
        {
            using (SqlConnection connection = new SqlConnection("server=gw18;database=particleDB;Trusted_Connection=true;Asynchronous Processing = true"))
            {
                connection.Open();
                int[] fofIDs100 = { 2531, 2532, 2533, 2534, 2535, 2536, 2537, 2538, 2539, 2540 };
                int[] fofIDs200 = { 1089, 1090, 1091, 1092, 1093, 1094, 1095, 15019, 15020, 15021 };
                int[] fofIDs500 = { 269, 270, 271, 272, 273, 274, 275, 276, 277, 278 };
                int[] fofIDs1000 = { 82, 14110, 14111, 27195, 27196, 27197, 27198, 27199, 40326, 52843 };
                int[] fofIDs5000 = { 0, 14060, 27136, 40257, 40258, 90431, 104073, 129159, 142428, 167765 };
                Dictionary<int, int[]> fofIDsDict = new Dictionary<int, int[]>();
                fofIDsDict.Add(100, fofIDs100);
                fofIDsDict.Add(200, fofIDs200);
                fofIDsDict.Add(500, fofIDs500);
                fofIDsDict.Add(1000, fofIDs1000);
                fofIDsDict.Add(5000, fofIDs5000);

                Dictionary<int, List<TimeSpan>> slotsTiming = new Dictionary<int, List<TimeSpan>>();




                // first do all slots timing
                foreach (int groupSize in fofIDsDict.Keys)
                {
                    List<TimeSpan> currentTimes = new List<TimeSpan>();
                    foreach (int group in fofIDsDict[groupSize])
                    {
                        currentTimes.Add(runSlotsAllTimestepsCommand(group, connection));
                    }
                    slotsTiming.Add(groupSize, currentTimes);
                    Debug.WriteLine("SLOTS: Finished group size " + groupSize);
                }

                // write the results to file
                using (StreamWriter writer = new StreamWriter("H:\\reverse_index\\results\\index_lookup_results_all_snaps", false))
                {
                    foreach (int groupSize in fofIDsDict.Keys)
                    {
                        writer.WriteLine(groupSize);
                        
                        List<TimeSpan> slots = slotsTiming[groupSize];
                        for (int i = 0; i < slots.Count; ++i)
                        {
                            writer.WriteLine(slots[i].TotalMilliseconds);
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        public static TimeSpan runSlotsCommand(int group, SqlConnection connection)
        {
            

            clearCaches(connection);
            string slots = "exec particleDB.dbo.spIndexLookup 63, " + group + ", 0";
            SqlCommand slotsCommand = new SqlCommand(slots, connection);
            DateTime slotsStart = DateTime.Now;
            slotsCommand.ExecuteNonQuery();
            TimeSpan slotsDuration = DateTime.Now - slotsStart;
            return slotsDuration;
        }

        public static TimeSpan runSlotsAllTimestepsCommand(int group, SqlConnection connection)
        {


            clearCaches(connection);
            string slots = "exec particleDB.dbo.spIndexLookupAllTimesteps 63, " + group;
            SqlCommand slotsCommand = new SqlCommand(slots, connection);
            slotsCommand.CommandTimeout = 0;
            DateTime slotsStart = DateTime.Now;
            slotsCommand.ExecuteNonQuery();
            TimeSpan slotsDuration = DateTime.Now - slotsStart;
            return slotsDuration;
        }

        public static TimeSpan runNoSlotsCommand(int group, SqlConnection connection)
        {
            clearCaches(connection);
            string noSlots = "exec particleDB.dbo.spIndexLookupNoSlots 63, " + group + ", 0";
            SqlCommand noSlotsCommand = new SqlCommand(noSlots, connection);
            DateTime noSlotsStart = DateTime.Now;
            noSlotsCommand.ExecuteNonQuery();
            TimeSpan noSlotsDuration = DateTime.Now - noSlotsStart;
            return noSlotsDuration;
        }

        public static TimeSpan runSlotsRangeCommand(int firstGroup, int lastGroup, SqlConnection connection)
        {


            clearCaches(connection);
            string slots = "exec particleDB.dbo.spIndexLookupRange 63, " + firstGroup + ", " + lastGroup + ", 0";
            SqlCommand slotsCommand = new SqlCommand(slots, connection);
            DateTime slotsStart = DateTime.Now;
            slotsCommand.CommandTimeout = 0;
            slotsCommand.ExecuteNonQuery();
            TimeSpan slotsDuration = DateTime.Now - slotsStart;
            return slotsDuration;
        }

        public static TimeSpan runNoSlotsRangeCommand(int firstGroup, int lastGroup, SqlConnection connection)
        {
            clearCaches(connection);
            string noSlots = "exec particleDB.dbo.spIndexLookupNoSlotsRange 63, " + firstGroup + ", " + lastGroup + ", 0";
            SqlCommand noSlotsCommand = new SqlCommand(noSlots, connection);
            DateTime noSlotsStart = DateTime.Now;
            noSlotsCommand.CommandTimeout = 0;
            noSlotsCommand.ExecuteNonQuery();
            TimeSpan noSlotsDuration = DateTime.Now - noSlotsStart;
            return noSlotsDuration;
        }

        public static void clearCaches(SqlConnection connection)
        {
            // DBCC DROPCLEANBUFFERS
                // DBCC FREEPROCCACHE

                string dropCleanBuffersString = "DBCC DROPCLEANBUFFERS";
                string freeProcCacheString = "DBCC FREEPROCCACHE";

                SqlCommand buffersCommand = new SqlCommand(dropCleanBuffersString, connection);
                buffersCommand.ExecuteNonQuery();
                SqlCommand procCommand = new SqlCommand(freeProcCacheString, connection);
                procCommand.ExecuteNonQuery();
        }
    }
}

