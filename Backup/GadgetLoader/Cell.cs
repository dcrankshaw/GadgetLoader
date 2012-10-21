/* Tamas 2011-02-18 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetLoader
{
    public class Cell : List<Structs>
    {
        public int PHKey { get; set; }
        public short SnapNum { get; set; }

        public Cell(short snapnum, int phkey, int capacity)
            : base(capacity)
        {
            SnapNum = snapnum;
            PHKey = phkey;
        }
        public Cell(short snapnum) : this(snapnum,10){}

        public Cell(short snapnum, int capacity)
            : base(capacity)
        {
            SnapNum = snapnum;
            PHKey = -1;
        }

        public void Init(int _PHKey)
        {
            this.PHKey = _PHKey;
            this.Clear();

        }
        public void Sort()
        {
            this.Sort(new ParticleComparator());
        }

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
