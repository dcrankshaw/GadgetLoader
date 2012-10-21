using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    /// <summary>
    /// Following describes a Millennium/MillenniumII snapshot file.
    /// NB Aquarius has different details (doubles for positions, integer*4 for id-s)
    /// </summary>
    class SnapFile
    {
        public UInt32[] numParts = new UInt32[6]; // number of particles for each type, in this file
        public double[] massParts = new double[6]; // mass of each particle per type
        public double time; // scale factor a
        public double redshift; // current redshift
        public UInt32 flag_sfr;
        public UInt32 flag_feedback;
        public UInt32[] numTotals = new UInt32[6]; // total particles per type
        public UInt32 flag_cooling;
        public UInt32 numSubfiles; // duh
        public double boxSize; // size of containing box
        public double omega0; // matter density at z = 0
        public double omegaLambda; // in units of critical density
        public double hubbleParam; // is 'h' in units of 100 km s^-1 Mpc^-1
        public UInt32 flag_age; // unused
        public UInt32 flag_metals; // unused
        public UInt32[] nLargeSims = new UInt32[6]; // holds MSB if there are a buncha particles
        public UInt32 flag_entr_ics; // initial conditions contain other stuff?

        public uint numSample;
        public float[,] pos; // described above
        public float[,] vel;
        public float[] hsml;
        public UInt64[] id;

        public SnapFile(String filename)
        {

            int record, bytesRead;
            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                record = reader.ReadInt32(); // unformatted fortran record start
                bytesRead = 0;
                for (int i = 0; i < 6; i++) // 24
                {
                    numParts[i] = reader.ReadUInt32();
                    bytesRead += 4;
                }
                for (int i = 0; i < 6; i++) // 48
                {
                    massParts[i] = reader.ReadDouble();
                    bytesRead += 8;
                }
                time = reader.ReadDouble(); // time, 8
                bytesRead += 8;
                redshift = reader.ReadDouble(); //redshift, 8
                bytesRead += 8;

                flag_sfr = reader.ReadUInt32(); //flag_sfr, 4
                bytesRead += 4;
                flag_feedback = reader.ReadUInt32(); //flag_feedback, 4
                bytesRead += 4;
                for (int i = 0; i < 6; i++)
                {
                    numTotals[i] = reader.ReadUInt32(); //npartall, 24
                    bytesRead += 4;
                }
                flag_cooling = reader.ReadUInt32(); // flag_cooling, 4
                bytesRead += 4;
                numSubfiles = reader.ReadUInt32(); //Nsubfiles, 4
                bytesRead += 4;
                boxSize = reader.ReadDouble(); //boxsize, 8
                bytesRead += 8;
                omega0 = reader.ReadDouble();
                bytesRead += 8;
                omegaLambda = reader.ReadDouble();
                bytesRead += 8;
                hubbleParam = reader.ReadDouble();
                bytesRead += 8;
                flag_age = reader.ReadUInt32();
                bytesRead += 4;
                flag_metals = reader.ReadUInt32();
                bytesRead += 4;

                for (int i = 0; i < 6; i++)
                {
                    nLargeSims[i] = reader.ReadUInt32();
                    bytesRead += 4;
                }

                flag_entr_ics = reader.ReadUInt32();
                bytesRead += 4;
                
                reader.ReadBytes(record-bytesRead);
                int record2 = reader.ReadInt32();
                if (record != record2)
                    throw new Exception("Record end not equal to record begin!"); // unformatted fortran record end

                uint np = numParts[1];

                numSample = 0;
                
                numSample = np;

                pos = new float[numSample, 3];
                vel = new float[numSample, 3];
                id = new UInt64[numSample];
                ReadData(reader);
            }
        }

        

        private void ReadData(BinaryReader reader)
        {
            int bytesRead = 0;
            int record = reader.ReadInt32();
            for (int i = 0; i < numSample; i++)
            {
                pos[i, 0] = reader.ReadSingle();
                pos[i, 1] = reader.ReadSingle();
                pos[i, 2] = reader.ReadSingle();

                bytesRead += 12;
            }
            // skip to end of record
            reader.ReadBytes(record - bytesRead);
            int record2 = reader.ReadInt32();
            if (record != record2)
                throw new Exception("Record end not equal to record begin!"); // unformatted fortran record end

            bytesRead = 0;
            record = reader.ReadInt32();
            for (int i = 0; i < numSample; i++)
            {
                vel[i, 0] = reader.ReadSingle();
                vel[i, 1] = reader.ReadSingle();
                vel[i, 2] = reader.ReadSingle();
                bytesRead += 12;
            }
            // skip to end of record
            reader.ReadBytes(record - bytesRead);
            record2 = reader.ReadInt32();
            if (record != record2)
                throw new Exception("Record end not equal to record begin!"); // unformatted fortran record end

            bytesRead = 0;
            record = reader.ReadInt32();
            for (int i = 0; i < numSample; i++)
            {
                id[i] = reader.ReadUInt64();
                bytesRead += 8;
//                id[i] = reader.ReadUInt32();
//                bytesRead += 4;
            }
            record2 = reader.ReadInt32();
            if (record != record2)
                throw new Exception("Record end not equal to record begin!"); // unformatted fortran record end
        }
    }


}



/*
 This is the header for an interleaved snapshot file. 
struct HeaderI
{
    uint64_t numTotal; // total number of IDs
    double mass; // mass of each particle
    double time; // all these are just taken from snaps
    double redshift; 
    double boxSize;
    double omega0;
    double omegaLambda;
    double hubbleParam;
};
*/