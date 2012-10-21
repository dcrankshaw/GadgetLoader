/* Tamas 2011-02-17 */
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
    /// <summary>
    /// This class provides methods to write various data types - including
    /// SqlArray - to the SQL Server native binary format for faster bulk loading.
    /// </summary>
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

        /*For MAX arrays*/

        /// <summary>
        /// Writes SqlRealArrayMax datatype (for 4 byte floats)
        /// </summary>
        /// <param name="a">The data to write</param>
        public void Write(SqlRealArrayMax a)
        {
            SqlBytes b = a.ToSqlBuffer();
            this.Write(b);
        }

        /// <summary>
        /// Writes SqlBigIntArrayMax datatype (for 8 byte ints)
        /// </summary>
        /// <param name="a">the data to write</param>
        public void Write(SqlBigIntArrayMax a)
        {
            SqlBytes b = a.ToSqlBuffer();
            this.Write(b);
        }

        /// <summary>
        /// Writes SqlIntArrayMax datatype (for 4 byte ints)
        /// </summary>
        /// <param name="a">the data to write</param>
        public void Write(SqlIntArrayMax a)
        {
            SqlBytes b = a.ToSqlBuffer();
            this.Write(b);
        }

        /// <summary>
        /// Writes SqlSmallIntArrayMax datatype (for 2 byte ints)
        /// </summary>
        /// <param name="a">the data to write</param>
        public void Write(SqlSmallIntArrayMax a)
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

        /// <summary>
        /// Writes a cell to a single row in a table by 
        /// putting all the ids, all the positions, and all velocities into
        /// SqlArrays
        /// </summary>
        /// <seealso cref="Cell"/>
        /// <param name="c">the cell to write</param>
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
                // TODO: Log this in the summary file
                Console.Write(e.Message);
            }
        }

        /// <summary>
        /// Writes a cell to a single row in a table, similarly to WriteCell(Cell),
        /// but this method compresses the position and velocity arrays to take up half
        /// the space (each element is 2 bytes instead of 4). It also writes the
        /// data necessary to decompress the arrays (x0, y0, z0, the inverse scale
        /// factor, the Peano-Hilbert level, and the box length).
        /// </summary>
        /// <param name="c">the cell to write out</param>
        /// <param name="phbits"> the Peano-Hilbert indexing level</param>
        /// <param name="box">the length of the box (in Mpc)</param>
        /// <seealso cref="Cell"/>
        public void WriteCompressedCell(Cell c, int phbits, double box)
        {
            this.Write(c.SnapNum);
            this.Write(c.PHKey);
            Int16[] posArray = c.PosToCompressedArray(phbits, box);
            Int16[] velArray = c.VelToCompressedArray();
            this.Write(c.x0);
            this.Write(c.y0);
            this.Write(c.z0);
            this.Write(c.Count);
            this.Write(c.scaleFactorInv);
            try
            {
                this.Write(SqlSmallIntArrayMax.FromArray(posArray));
                this.Write(SqlSmallIntArrayMax.FromArray(velArray));
                this.Write(SqlBigIntArrayMax.FromArray(c.IdToArray()));
                //don't write out the filter
                //this.WriteFilter(c.filter);
            }
            catch (OverflowException e)
            {
                // TODO: log this in the summary file
                Console.Write(e.Message);
            }
        }

        public void WriteUncompressedCell(Cell c, int phbits, double box)
        {
            this.Write(c.SnapNum);
            this.Write(c.PHKey);
            this.Write(c.x0);
            this.Write(c.y0);
            this.Write(c.z0);
            this.Write(c.Count);
            this.Write(c.scaleFactorInv);
            try
            {
                this.Write(SqlRealArrayMax.FromArray(c.PosToArray()));
                this.Write(SqlRealArrayMax.FromArray(c.VelToArray()));
                this.Write(SqlBigIntArrayMax.FromArray(c.IdToArray()));
            }
            catch (OverflowException e)
            {
                // TODO: log this in the summary file
                Console.Write(e.Message);
            }
        }

        /// <summary>
        /// Write the cell to as few as possible rows in a table as possible, but
        /// use short SqlArrays instead of max SqlArrays. Short SqlArrays must be
        /// less than 8000B long, so if necessary we split the cell across multiple
        /// rows
        /// </summary>
        /// <param name="c">The cell to write</param>
        /// <seealso cref="Cell"/>
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

        /// <summary>
        /// Writes a portion of a cell to a single row of a table. This is a private
        /// method used by WriteCellSmallArrays(Cell) to split a Cell that is too
        /// big into multiple rows4
        /// </summary>
        /// <param name="c">The Cell to write</param>
        /// <param name="first">The index of the first particle in the cell to write to this row</param>
        /// <param name="rowSize">The number of particles to write to this Cell</param>
        private void WriteCellRow(Cell c, int first, int rowSize)
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
            }
            catch (OverflowException e)
            {
                // TODO: log this to the summary file
                Console.Write(e.Message);
            }
        }

        /// <summary>
        /// Write the FFT modes of a simulation as two arrays, one containing the real components
        /// and the other containing the corresponding imaginary components at matching indices
        /// </summary>
        /// <param name="snapnum">The timestep this FFTMode is for??</param>
        /// <param name="time">Not sure????</param>
        /// <param name="nsize">???</param>
        /// <param name="re">The real components</param>
        /// <param name="im">The imaginary components</param>
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

        /// <summary>
        /// Writes a BloomFilter as a byte array as well as the number of hash functions used to construct this BloomFilter
        /// </summary>
        /// <param name="filter">the BloomFilter to write</param>
        /// <seealso cref="BloomFilter.Filter"/>
        public void WriteFilter(BloomFilter.Filter<long> filter)
        {
            this.Write(new SqlBinary(filter.convertToByteArray()));
            this.Write((short) filter.hashFunctionCount);
        }

        
        //public void WriteFoFGroup(long[] idArr, short snapnum, int haloID, BloomFilter.Filter<long> filter) //don't use filter

        /// <summary>
        /// Writes a Friends of Friends group - a set of particle IDs
        /// representing a spatial clustering - to a single row in a table using SqlArrays
        /// </summary>
        /// <param name="idArr">The array of IDs that make up the FoF group</param>
        /// <param name="snapnum">The simulation timestep of the FoFGroup</param>
        /// <param name="haloID">The ID of the FoF Group. The ID is unique within a timestep,
        /// but not within the simulation (e.g. timestep 40 will have a FoF group with
        /// ID 1 and so will timestep 42, but they don't necessarilyrefer to the
        /// same group of particles). </param>
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
