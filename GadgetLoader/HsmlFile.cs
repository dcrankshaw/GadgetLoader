/* Can be deleted??


using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    class HsmlFile
    {
        public UInt32 numFile; // number of data points in this file
        public UInt32 numPrevious; // number of data points before this file
        public UInt64 numTotal; // total number of points
        public UInt32 numFiles; // number of files

        public float[] hsml; // arrays of length numFile to contain actual values
        public float[] density;
        public float[] velDisp;

        public HsmlFile(string filename)
        {
            // no need for records/bytesRead, not in fortran format
            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                numFile = reader.ReadUInt32();
                numPrevious = reader.ReadUInt32();
                numTotal = reader.ReadUInt64();
                numFiles = reader.ReadUInt32();

                hsml = new float[numFile];
                density = new float[numFile];
                velDisp = new float[numFile];

                for (int i = 0; i < numFile; i++)
                    hsml[i] = reader.ReadSingle();

                for (int i = 0; i < numFile; i++)
                    density[i] = reader.ReadSingle();

                for (int i = 0; i < numFile; i++)
                    velDisp[i] = reader.ReadSingle();
            }
        }
    }
}


*/