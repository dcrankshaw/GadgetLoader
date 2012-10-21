using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    class IDFile
    {
        public UInt32 numGroups; // number of groups present in this file
        public UInt32 totalGroups; // total #
        public UInt32 numIDs; // IDs in this file
        public UInt64 totalIDs; // total number of points
        public UInt32 numFiles; // number of files
        public UInt32 offset; // probably offset of first particle?

        public UInt32[] IDs; // particle IDs

        public IDFile(string filename)
        {
            // no need for records/bytesRead, not in fortran format
            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                numGroups = reader.ReadUInt32();
                totalGroups = reader.ReadUInt32();
                numIDs = reader.ReadUInt32();
                totalIDs = reader.ReadUInt64();
                numFiles = reader.ReadUInt32();
                offset = reader.ReadUInt32();

                IDs = new UInt32[numIDs];

                for (int i = 0; i < numIDs; i++)
                    IDs[i] = reader.ReadUInt32();
            }
        }
    }
}