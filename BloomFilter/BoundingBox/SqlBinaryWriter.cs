﻿/* Tamas 2011-02-17 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlTypes;
using System.Collections;
using System.Diagnostics;

using Jhu.SqlServer.Array;

namespace GadgetLoader
{
    public class SqlBinaryWriter : BinaryWriter
    {
        
        public SqlBinaryWriter(Stream output)
            : base(output)
        {
        }

        /// <summary>
        /// TBD is this method calling itself when using this.Write iso base.Write ???
        /// </summary>
        /// <param name="b"></param>
        public void Write(SqlBinary b)
        {
            
            short l = (short) b.Value.Length;
            this.Write(l);
            base.Write(b.Value);
        }

        public void Write(SqlBytes b)
        {
            this.Write(b.Value.LongLength);
            base.Write(b.Value);
        }

        /*For max arrays*/
        public void Write(SqlRealArrayMax a)
        {
            SqlBytes b = a.ToSqlBuffer();
            this.Write(b);
        }
        public void Write(SqlBigIntArrayMax a)
        {
            SqlBytes b = a.ToSqlBuffer();
            this.Write(b);
        }

        public void Write(SqlIntArrayMax a)
        {
            SqlBytes b = a.ToSqlBuffer();
            this.Write(b);
        }

        /*For in page arrays*/
        public void Write(SqlRealArray a)
        {
            SqlBinary b = a.ToSqlBuffer();
            this.Write(b);
        }
        public void Write(SqlBigIntArray a)
        {
            SqlBinary b = a.ToSqlBuffer();
            this.Write(b);
        }

        public void Write(SqlIntArray a)
        {
            SqlBinary b = a.ToSqlBuffer();
            this.Write(b);
        }

        /* For writing a row in a snapshot file using arrays */
        public void WriteCell(Cell c)
        {
            this.Write(c.SnapNum);
            this.Write(c.PHKey);
            this.Write(c.Count);
            
            try
            {
                this.Write(SqlRealArrayMax.FromArray(c.PosToArray()));
                this.Write(SqlRealArrayMax.FromArray(c.VelToArray()));
                this.Write(SqlBigIntArrayMax.FromArray(c.IdToArray()));
                //don't write out the filter
                //this.WriteFilter(c.filter);
            }
            catch (OverflowException e)
            {
                Console.Write(e.Message);
            }
        }
        public void WriteCellSmallArrays(Cell c)
        {
            //int maxBytes = 8000; //max size of arrays is 8000 bytes
            int maxSize = 650;
            //int maxSize = 250;
            /* can pack up to ~666 3*4byte floats into an array (because the pos and vel arrays each have 3
             * components per entry, but need a 20ish byte header so drop this down to 650 (to be safe)*/
            int rowSize = c.Count;
            int numRows = 1;
            

            while (rowSize >= maxSize)
            {
                numRows++;
                rowSize = c.Count / numRows;
            }
            if ((c.Count - (numRows - 1) * rowSize) > maxSize)
            {
                numRows++;
                rowSize = c.Count / numRows;
            }


            int first = 0; //first index included
            for (int i = 0; i < numRows - 1; i++)
            {
                WriteCellRow(c, first, rowSize);
                first += rowSize;
            }

            //now do last bit
            int lastSize = c.Count - first;
            WriteCellRow(c, first, lastSize);
        }

        public void WriteCellRow(Cell c, int first, int rowSize)
        {
            int maxBytes = 8000;
            long[] idArray = c.IdToArray();
            float[] posArray = c.PosToArray();
            float[] velArray = c.VelToArray();

            long[] curIDs = new long[rowSize];
            Array.Copy(idArray, first, curIDs, 0, rowSize);
            float[] curPos = new float[rowSize];
            Array.Copy(posArray, first, curPos, 0, rowSize);
            float[] curVel = new float[rowSize];
            Array.Copy(velArray, first, curVel, 0, rowSize);

            Debug.Assert(rowSize * sizeof(float) * 3 < maxBytes && rowSize * sizeof(long) < maxBytes, "Array size is too big. It is " + rowSize);

            this.Write(c.SnapNum);
            this.Write(c.PHKey);
            this.Write(rowSize);

            try
            {
                this.Write(SqlRealArray.FromArray(curPos));
                this.Write(SqlRealArray.FromArray(curVel));
                this.Write(SqlBigIntArray.FromArray(curIDs));
                //don't write out the filter
                //this.WriteFilter(c.filter);
            }
            catch (OverflowException e)
            {
                Console.Write(e.Message);
            }
        }

        //binwriter.Write(isnap, time2, nsize, fft_re[], fft_im[]);

        public void WriteFFTModes(short snapnum, double time, int nsize, float[, ,] re, float[, ,] im)
        {
            this.Write(snapnum);
            this.Write(time);
            this.Write(nsize);
            //probably don't need these rows
            /*this.Write(re.GetLength(0));
            this.Write(re.GetLength(1));
            this.Write(re.GetLength(2));*/

            this.Write(SqlRealArrayMax.FromArray(re));
            this.Write(SqlRealArrayMax.FromArray(im));
        }

       /* public void WriteFilter(BloomFilter.Filter<long> filter)
        {
            this.Write(new SqlBinary(filter.convertToByteArray()));
            this.Write((short) filter.hashFunctionCount);
        }*/

        /* For writing FOF groups */
        //public void WriteFoFGroup(long[] idArr, short snapnum, int haloID, BloomFilter.Filter<long> filter) //don't use filter
        public void WriteFoFGroup(long[] idArr, short snapnum, int haloID)
        {
            this.Write(snapnum);
            this.Write(haloID);
            this.Write(idArr.Length);
            try
            {
                this.Write(SqlBigIntArrayMax.FromArray(idArr));
                //this.WriteFilter(filter);
            }
            catch(OverflowException e)
            {
                Console.Write(e.Message);
            }
        }
    }
}
