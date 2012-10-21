/* Tamas 2011-02-17 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlTypes;

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
            this.Write(b.Value.LongLength);
            base.Write(b.Value);
        }

        public void Write(SqlBytes b)
        {
            this.Write(b.Value.LongLength);
            base.Write(b.Value);
        }
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

        public void Write(Cell c)
        {
            this.Write(c.SnapNum);
            this.Write(c.PHKey);
            this.Write(c.Count);
            try
            {
                this.Write(SqlRealArrayMax.FromArray(c.PosToArray()));
                this.Write(SqlRealArrayMax.FromArray(c.VelToArray()));
                this.Write(SqlBigIntArrayMax.FromArray(c.IdToArray()));
            }
            catch (OverflowException e)
            {
                Console.Write(e.Message);
            }
        }

        public void Write(int[] idArr, short snapnum, int haloID)
        {
            this.Write(snapnum);
            this.Write(haloID);
            this.Write(idArr.Length);
            try
            {
                this.Write(SqlIntArrayMax.FromArray(idArr));
            }
            catch(OverflowException e)
            {
                Console.Write(e.Message);
            }
        }
    }
}
