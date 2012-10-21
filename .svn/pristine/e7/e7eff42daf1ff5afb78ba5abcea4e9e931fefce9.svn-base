using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;

namespace BoundingBox
{
    class Tester
    {
        public string server;
        public string database;
        private short[] timesteps;
        public float[] paddings;
        public Dictionary<int, int> maxGroupIds;
        public Dictionary<int, List<int>> sampleGroups; //<timestep, fofIDs>
        private double percentToSample;
        public Dictionary<short, Dictionary<float, int>> missingParticles; //<padLevel, number of missing particles>
        public Dictionary<short, Dictionary<float, double>> timeToRun; // <fofTS, <padLevel, time>>

        public Tester(string svr, string db, short[] ts, float[] pads, double per)
        {
            server = svr;
            database = db;
            timesteps = ts;
            paddings = pads;
            percentToSample = per;
            maxGroupIds = new Dictionary<int, int>();
            sampleGroups = new Dictionary<int, List<int>>();
            timeToRun = new Dictionary<short, Dictionary<float, double>>();
            missingParticles = new Dictionary<short, Dictionary<float, int>>();
            maxGroupIds.Add(30, 48277);
            maxGroupIds.Add(40, 241907);
            maxGroupIds.Add(50, 366047);
            maxGroupIds.Add(60, 414207);
            foreach (short t in timesteps)
            {
                GenerateSample(t, percentToSample);
            }
        }

        private void GenerateSample(int ts, double percent)
        {
            int maxId = maxGroupIds[ts];
            //int sampleSize = (int) Math.Round(percent * maxId);
            int sampleSize = 10;
            Random rand = new Random();
            List<int> groupIds = new List<int>();
            for (int i = 0; i < sampleSize; i++)
            {
                groupIds.Add(rand.Next(maxId));
            }
            sampleGroups.Add(ts, groupIds);
        }

        public void collectStats()
        {
            foreach (short ts in timesteps)
            {
                collectStats(ts);
            }
        }

        private void collectStats(short fofTS)
        {
            Dictionary<float, double> runningTimes = new Dictionary<float,double>();
            Dictionary<float, int> missedParts = new Dictionary<float, int>();
            SqlConnection conn = new SqlConnection(BoundingBox.createConnString(database, server));
            conn.Open();
            string getStats = "Exec SimDBdev.dbo.collectBBoxStats @padlevel, @partTS, @fofTS, @fof";
            SqlCommand statsCommand = new SqlCommand(getStats, conn);
            statsCommand.CommandTimeout = 0;
            SqlParameter padParam = new SqlParameter("@padlevel", System.Data.SqlDbType.Real);
            SqlParameter partTSParam = new SqlParameter("@partTS", System.Data.SqlDbType.SmallInt);
            SqlParameter fofTSParam = new SqlParameter("@fofTS", System.Data.SqlDbType.SmallInt);
            SqlParameter retVal = new SqlParameter("retval", System.Data.SqlDbType.Int);
            retVal.Direction = System.Data.ParameterDirection.ReturnValue;
            fofTSParam.Value = fofTS;
            SqlParameter fofIDParam = new SqlParameter("@fof", System.Data.SqlDbType.Int);
            foreach (float p in paddings)
            {
                padParam.Value = p;
                int totalMissed = 0;
                //Start Timing
                DateTime start = DateTime.Now;
                for (short partTS = 30; partTS <= 60; partTS += 15)
                {
                    partTSParam.Value = partTS;
                    int numGroupsProcessed = 0;
                    foreach (int fofID in sampleGroups[fofTS])
                    {
                        
                        fofIDParam.Value = fofID;
                        statsCommand.Parameters.Add(fofIDParam);
                        statsCommand.Parameters.Add(partTSParam);
                        statsCommand.Parameters.Add(padParam);
                        statsCommand.Parameters.Add(fofTSParam);
                        statsCommand.Parameters.Add(retVal);

                        //Run Accuracy Test
                        statsCommand.ExecuteScalar();
                        int numMissed = (Int32)retVal.Value;
                        totalMissed += numMissed;
                        statsCommand.Parameters.Clear();
                        numGroupsProcessed++;
                        if (numGroupsProcessed % 5 == 0)
                        {
                            Console.Out.WriteLine("PartTS: {0}, PadLevel: {1}, FoFTS: {2}, Groups Processed: {3}", partTS, p, fofTS, numGroupsProcessed);
                        }
                    }
                }
                //Stop Timing
                DateTime end = DateTime.Now;
                TimeSpan runningTime = end - start;

                runningTimes.Add(p, runningTime.TotalMilliseconds);
                missedParts.Add(p, totalMissed);
            }
            timeToRun.Add(fofTS, runningTimes);
            missingParticles.Add(fofTS, missedParts);
            conn.Close();
        }

        public void writeResults(string dir)
        {
            //write sample groups
            foreach(int group in sampleGroups.Keys)
            {
                List<int> fofIDs = sampleGroups[group];
                string filename = dir + "\\ts_" + group;
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    foreach (int id in fofIDs)
                    {
                        writer.WriteLine(id);
                    }
                }
            }

            string fname = dir + "\\stats";
            using(StreamWriter writer = new StreamWriter(fname))
            {
                foreach(short ts in timesteps)
                {
                    writer.WriteLine("FoF Timestep: {0}\t\tNum FoF Groups: {1}", ts, sampleGroups[ts].Count);
                    foreach (float pad in missingParticles[ts].Keys)
                    {
                        writer.WriteLine("{0}\t\t{1}\t\t{2}\n", pad, missingParticles[ts][pad], timeToRun[ts][pad]);
                    }
                }
            }
        }

        //Collect sample group stats down here with a seperate query
        public void collectSampleStats()
        {
            Console.Out.WriteLine("Unimplemented");
        }


        /***********************************************************

         * fofTS = current foFTS (from timesteps array)
         * List<Int16> fofGroupSample = generateRandomSample(percent=.1, max=maxGroupID); //for that timestep
         * startTiming();
         * int accuracy_counter = 0;
         * long totalPartsLookedUp = 0;
         * for(partTS = 0; partTS <= 60; partTS += 15)
         * {
         *      foreach(groupnum in fofGroups)
         *      {
         *          getPartPositions(fofID=groupnum, fofTS, partTS, out boolean accurate, out int group_size)
         *          if(!accurate)) //means failed accuracy test
         *          {
         *              accuracy_counter++;
         *          }
         *          totalPartsLookedUp += groupSize;
         *          ///How can I find stdev? Do I have to record all groupsizes? Will that be too big?
         *          //Maybe the best way to do this would be to seperate out accuracy and statistics testing from
         *              performance testing. This way I wouldn't have to worry about memory overhead and things
         *              besides the database operations taking up too much time. I can even run the accuracy tests
         *              on the same random samples as the performance tests. <<<<< DO THIS??
         *          
         *      }
         * }
         * stopTiming();
        


        ************************************************************/
        /*
         * What am I actually testing?
         * Select a random sample of 10% of the FoFGroups in each timestep we have a BBox for
         *                          ----(use same random sample for each padding level)
         * For each of those FoFGroups, find its particles positions in timesteps 0, 15, 30, 45, 60
         *  ----switch order of for loops, so that we are accessing a different fof group each time to decrease caching effects
         *  
         * when finding positions
         * ----make sure that we have a position for every particle - ||this tests accuracy||
         *          if we don't, increment the failed accuracy test counter
         * ----Time the amount of time it takes to look up a FoFTimeSteps worth of positions
         *     also make sure that we record the number of groups in that sample and the average (and stdev) of the size of the groups.
         *          can later compare the average size of the groups in the samples to those in the timesteps
         *     
         *  
         * 
         */
    }
}
