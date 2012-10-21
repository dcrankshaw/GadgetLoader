using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;

using Jhu.SqlServer.Array;

    public partial class FilterUDFs
    {
        //can convert this to take in a table
        [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "AddToFilterFillRow",
        TableDefinition="snap SMALLINT, phkey INT, filter VARBINARY(8000), hashFunctions INT, expectedCap INT")]
        public static IEnumerable AddFilterToSnap(SqlBytes arrayBinary, short snap, int phkey) //, out long outId, out SqlBinary finalFilter, out int hashFunctions, out int expectedCap)
        {

            List<object[]> results = new List<object[]>();

            SqlBigIntArrayMax myIds = new SqlBigIntArrayMax(arrayBinary);
            
            const int expectedFilterCapacity = 1000;
            float errorRate = BloomFilter.Filter<long>.bestErrorRate(expectedFilterCapacity);
            int hashFunctions = BloomFilter.Filter<long>.bestK(expectedFilterCapacity, errorRate);
            BloomFilter.Filter<long> filter = new BloomFilter.Filter<long>(expectedFilterCapacity, errorRate, hashFunctions);

            long[] idsArray = myIds.ToArray();
            for (int i = 0; i < idsArray.Length; i++)
            {
                filter.Add(idsArray[i]);
            }
            object[] item = { snap, phkey, filter.convertToByteArray(), hashFunctions, expectedFilterCapacity };
            results.Add(item);
            return results;
            //finalFilter = filter.convertToByteArray();
        }

        public static void AddToFilterFillRow(Object obj, out SqlInt16 snap, out SqlInt32 outPHKey, out SqlBinary finalFilter, out SqlInt32 hashFunctions, out SqlInt32 expectedCap)
        {
            object[] row = (object[])obj;
            finalFilter = new SqlBinary((byte[])row[2]);
            snap = new SqlInt16((short) row[0]);// (SqlInt16)row[10];
            outPHKey = new SqlInt32((int) row[1]);
            
            hashFunctions = new SqlInt32((int)row[3]);
            expectedCap = new SqlInt32((int)row[4]);

        }

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlInt32 checkFilterForId(SqlBinary filterAsBinary, SqlInt64 id, SqlInt32 hashFunctions)
        {
            BloomFilter.Filter<long> filter = new BloomFilter.Filter<long>((byte[])filterAsBinary, (int)hashFunctions);
            return filter.Contains((long)id) ? 1 : 0;
        }

        /**Compares the contents of two bloom filters to see if they contain any overlapping elements
         * Comparison is done by taking bitwise AND of the two filters,
         * if this operation is non zero there is some overlap
         * Returns true if bitwise AND is nonzero, false otherwise
         * */
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlInt32 compareFilters(SqlBinary filter1, SqlBinary filter2, SqlInt32 numHashFunctions)
        {
            byte[] f1 = (byte[])filter1;
            byte[] f2 = (byte[])filter2;
            //int[] setBitsLookup = {0, 1, 1, 2, 1, 2, 6, 3, 1,};

            //int result = 0;
            //int index = 0;
            int max = (f1.Length < f2.Length) ? f1.Length : f2.Length;
            int collisions = 0;
            for (int i = 0; i < f1.Length; i++)
            {
                int andResult = f1[i] & f2[i];
                //int count;
                byte mask;
                int count;

                for (count = 0, mask = 0x80; mask != 0; mask >>=1)
                {
                    if ((andResult & mask) != 0) {
                        count++;
                    }
                }
                collisions += count;
            }

            //collisions = (collisions < (int)numHashFunctions) ? 0 : 1;
            return (SqlInt32) collisions;

/*
            while (result == 0 && index < max)
            {
                result = (f1[index] & f2[index]);
                index++;
            }

            return (result == 0) ? 0 : 1;*/
        }



    };
