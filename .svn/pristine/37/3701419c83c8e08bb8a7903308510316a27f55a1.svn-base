using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloomFilter;

namespace CollisionTesting
{
    class DataPoint
    {

        public int hashFunctions { get; set; }
        public int filterSize { get; set; }
        public long sum;
        public double numIntersects;

        public List<Filter<long>> haloFilters;

        public DataPoint(int h, int f, List<List<long>> halos)
        {
            hashFunctions = h;
            filterSize = f;
            sum = 0;
            haloFilters = new List<Filter<long>>();
            numIntersects = 0;
            foreach (List<long> halo in halos)
            {
                Filter<long> currentBuild = new Filter<long>(2, 0.5f, null, filterSize, hashFunctions);
                foreach (long id in halo)
                {
                    currentBuild.Add(id);
                }
                haloFilters.Add(currentBuild);
            }
        }

        public void testFilter(Filter<long> cellFilter)
        {
            foreach (Filter<long> currentHalo in haloFilters)
            {
                int result = currentHalo.intersectWith(cellFilter);
                if (result >= 0)
                {
                    sum += result;
                    numIntersects++;
                }
                else
                {
                    Console.Out.WriteLine("Error, intersect result less than 0");
                }
            }
        }

        public double getMean()
        {
            return sum / numIntersects;
        }

        

    }
}
