/* Tamas 2011-02-18 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetLoader
{
    /// <summary>
    /// Contains a list of particles in the same Peano-Hilbert cell.
    /// Has methods to facilitate converting the cell to SqlArrays
    /// for storage in SQL Server.
    /// </summary>
    public class Cell : List<Structs>
    {
        /// <summary>
        /// The Peano-Hilbert index of the cell (all particles in the cell have the same PH index)
        /// </summary>
        public int PHKey { get; set; }
        /// <summary>
        /// The x coordinate of the lower left hand corner of the cell
        /// (assuming that (0,0,0) is the lower left hand corner of the simulation box).
        /// </summary>
        public float x0 { get; set; }
        /// <summary>
        /// The y coordinate of the lower left hand corner of the cell
        /// (assuming that (0,0,0) is the lower left hand corner of the simulation box).
        /// </summary>
        public float y0 { get; set; }
        /// <summary>
        /// The z coordinate of the lower left hand corner of the cell
        /// (assuming that (0,0,0) is the lower left hand corner of the simulation box).
        /// </summary>
        public float z0 { get; set; }
        /// <summary>
        /// The simulation timestep these particle positions and velocities are from.
        /// </summary>
        public short SnapNum { get; set; }
        /// <summary>
        /// A scaling factor used to decompress the particle positions when compressed.
        /// <seealso cref="PosToCompressedArray(int, double)"/>
        /// </summary>
        public float scaleFactorInv { get; set; }
        //public BloomFilter.Filter<long> filter { get; set; }

        public Cell(short snapnum, int phkey, int capacity)
            : base(capacity)
        {
            SnapNum = snapnum;
            PHKey = phkey;
            x0 = -1;
            y0 = -1;
            z0 = -1;
            scaleFactorInv = 0;
        }
        public Cell(short snapnum) : this(snapnum,10){}

        public Cell(short snapnum, int capacity)
            : base(capacity)
        {
            SnapNum = snapnum;
            PHKey = -1;
            x0 = -1;
            y0 = -1;
            z0 = -1;
        }

        /// <summary>
        /// Add a particle to the cell
        /// </summary>
        /// <param name="s"> The particle struct to add</param>
        public void AddToCell(Structs s)
        {
            this.Add(s);
            /* Add ID to the Cell's bloomfilter as well */
            //this.filter.Add((long)s.id);
        }

        /// <summary>
        /// Reset the Cell to to a new PH cell
        /// (so we don't have to allocate memory for a new cell and wait for this one to be garbage collected).
        /// </summary>
        /// <param name="_PHKey">The Peano-Hilbert index of the new cell</param>
        public void Init(int _PHKey)
        {
            this.PHKey = _PHKey;
            this.Clear();
            this.x0 = -1;
            this.y0 = -1;
            this.z0 = -1;

            //this.filter = new BloomFilter.Filter<long>(LoaderParamSingleton.getInstance().expectedSize)
        }
        /// <summary>
        /// Sort the cell particles by snapnum, then phkey, then particle id.
        /// <seealso cref="ParticleComparator"/>
        /// </summary>
        public void Sort()
        {
            this.Sort(new ParticleComparator());
        }

        /// <summary>
        /// Creates a 1D array of all of the particle's positions in order.
        /// The ith particle's position components are:
        /// x = 3*i + 0,
        /// y = 3*i + 1,
        /// z = 3*i + 2.
        /// </summary>
        /// <returns>The array of positions</returns>
        public float[] PosToArray()
        {
            float[] v = new float[3 * Count];
            for (int i = 0; i < Count; i++)
            {
                v[3 * i + 0] = this[i].x;
                v[3 * i + 1] = this[i].y;
                v[3 * i + 2] = this[i].z;
            }
            return v;
        }

        /// <summary>
        /// Compresses the particles positions to two-byte integer values.
        /// The compression finds the x, y, and z offsets from x0, y0, and z0
        /// respectively, then multiplies the offsets by a scaling factor so that
        /// the position values take up the full range of values from 0 to 2^15 - 1.
        /// We do this to preserve as many significant figures of the 4byte float as
        /// possible. The function then puts the compressed positions into a 1D array
        /// in the same order as the uncompressed PosToArray() function.
        /// </summary>
        /// <seealso cref="PostToArray()"/>
        /// <param name="phbits">The number of levels the PH ordering recurses down.
        /// There are 8^k cells for a k phbit level.</param>
        /// <param name="box">The length of a side of the simulation box in Mpc
        /// (Indra simulation box size is 1000 Mpc = 1 Gpc).</param>
        /// <returns>The compressed position array.</returns>
        public Int16[] PosToCompressedArray(int phbits, double box)
        {
            PeanoHilbertID getPosObj = new PeanoHilbertID();
            int cx, cy, cz;
            getPosObj.GetPosition(phbits, PHKey, out cx, out cy, out cz);
            float cellLength = (float)box / (float)(1 << phbits);
            x0 = cellLength * cx;
            y0 = cellLength * cy;
            z0 = cellLength * cz;
            float scaleFactor =  (float) (Math.Pow(2.0, 15.0) - 1) / cellLength;
            scaleFactorInv = 1.0f / scaleFactor;
            Int16[] v = new Int16[3 * Count];
            for (int i = 0; i < Count; i++)
            {
                v[3 * i + 0] = (Int16) ((this[i].x - x0) * scaleFactor);
                v[3 * i + 1] = (Int16)((this[i].y - y0) * scaleFactor);
                v[3 * i + 2] = (Int16)((this[i].z - x0) * scaleFactor);
            }
            return v;
        }

        /// <summary>
        /// Compresses the velocities to 2 byte integers by rounding.
        /// Velocity components are returned in a 1D array in the same
        /// order as the position components.
        /// </summary>
        /// <seealso cref="PostToArray()"/>
        /// <returns>The array of velocities</returns>
        public Int16[] VelToCompressedArray()
        {
            Int16[] v = new Int16[3 * Count];
            for (int i = 0; i < Count; i++)
            {
                v[3 * i + 0] = (Int16)Math.Round(this[i].vx);
                v[3 * i + 1] = (Int16)Math.Round(this[i].vy);
                v[3 * i + 2] = (Int16)Math.Round(this[i].vz);
            }
            return v;
        }

        /// <summary>
        /// Puts velocity components into a 1D array in the same
        /// order as the position components.
        /// </summary>
        /// <seealso cref="PostToArray()"/>
        /// <returns>The array of velocities</returns>
        public float[] VelToArray()
        {
            float[] v = new float[3 * Count];
            for (int i = 0; i < Count; i++)
            {
                v[3 * i + 0] = this[i].vx;
                v[3 * i + 1] = this[i].vy;
                v[3 * i + 2] = this[i].vz;
            }
            return v;
        }

        /// <summary>
        /// Puts IDs into a an array such that the position and velocity components
        /// of the particle at index i are at
        /// x(resp. vx) = 3*i + 0,
        /// y(resp. vy) = 3*i + 1,
        /// z(resp. vz) = 3*i + 2.
        /// </summary>
        /// <returns>The array of ids.</returns>
        public long[] IdToArray()
        {
            long[] v = new long[Count];
            for (int i = 0; i < Count; i++)
            {
                v[i] = (long)this[i].id;
            }
            return v;
        }

    }

    
}
