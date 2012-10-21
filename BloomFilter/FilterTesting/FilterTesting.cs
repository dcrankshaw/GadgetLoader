using System;
using System.Collections.Generic;
using System.Text;


/*
 * Create 5 filters, for each measurement write out value for each filter as well as mean value
 * 
 */
namespace BloomFilter
{
    class FilterTesting
    {
        static void Main(string[] args)
        {
            int expectedSize = 1000;
            int numHashFunctions = 10;
            Random rand = new Random();
            int maxID = (int) Math.Pow(512, 3);
            Dictionary<int, Filter<long>> randFilterList = new Dictionary<int, Filter<long>>();
            Dictionary<int, Filter<long>> runsFilterMap = new Dictionary<int, Filter<long>>();
            
            for(int i = 0; i < 5; i++)
            {
                randFilterList.Add(i, new Filter<long>(expectedSize));
                int ID = rand.Next(maxID - 2000); //subtract 2000 so we don't exceed maxID when inserting 1000 new values in
                runsFilterMap.Add(ID, new Filter<long>(expectedSize));
            }
            if (randFilterList[0].hashFunctionCount != numHashFunctions)
            {
                Console.Out.WriteLine("Wrong number of hash functions");
                return;
            }

            Console.Out.WriteLine("Filter size: " + randFilterList[0].getTotalSize());

            int incSize = 100;

            //first get results for inserting random numbers from the domain into the list
            for (int numEl = 0; numEl < expectedSize; numEl += incSize)
            {
                for (int i = 0; i < incSize; i++)
                {
                    foreach(Filter<long> f in randFilterList.Values)
                    {
                        f.Add((long) rand.Next(maxID));
                    }
                    foreach (int ID in runsFilterMap.Keys)
                    {
                        runsFilterMap[ID].Add(ID + i + numEl);
                    }
                }
                Console.Out.WriteLine("\n\nTesting with " + numEl + " elements inserted");
                Console.Out.WriteLine("\nRandom Filters");
                generateStats(randFilterList, numEl);
                Console.Out.WriteLine("\nRuns of sequential numbers");
                generateStats(runsFilterMap, numEl);
            }
            //Console.In.Read();

        }

        //calculate mean, median, stdev, number of entries of each filter
        //also calculate average of each measure
        public static void generateStats(Dictionary<int, Filter<long>> filterList, int numEl)
        {
            double avgMean = 0;
            double avgMedian = 0;
            double avgStdDev = 0;
            foreach (int ID in filterList.Keys)
            {
                Filter<long> f = filterList[ID];
                List<int> setBits = f.getSetPositions();
                double sum = 0;
                foreach (int i in setBits)
                {
                    sum += i;
                }
                double mean = sum / (double) setBits.Count;
                setBits.Sort();
                int median = setBits[setBits.Count / 2];
                double sumDiff = 0;
                foreach (int i in setBits)
                {
                    double diff = i - mean;
                    sumDiff += Math.Pow(diff, 2);
                }
                double stdDev = Math.Sqrt(sumDiff / setBits.Count);
                Console.Out.WriteLine("ID: " + ID + "\tSet Bits: " + setBits.Count + "\tMean: " + mean + "\tMedian: " + median + "\tStdDev: " + stdDev);
                avgMean += mean;
                avgMedian += median;
                avgStdDev += stdDev;
            }
            avgMean = avgMean / filterList.Count;
            avgMedian = avgMedian / filterList.Count;
            avgStdDev = avgStdDev / filterList.Count;
            Console.Out.WriteLine("Averages\t\t\tMean: " + avgMean + "\tMedian: " + avgMedian + "\tStdDev: " + avgStdDev);
        }

    }
}
