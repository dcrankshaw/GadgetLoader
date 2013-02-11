using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data.SqlTypes;

using GadgetLoader;

namespace PeanoHilbertNeighbors
{
    class Program
    {
        private struct NeighborCode
        {
            public int neighbor_phkey;
            public byte encoded_number;
            public byte encoded_offsets;

            // create encodings in this struct's constructor
            public NeighborCode(int key, int x, int y, int z)
            {
                neighbor_phkey = key;
                int xp = (x < 0 ? 3 : x);
                xp = xp << 4;
                int yp = (y < 0 ? 3 : y);
                yp = yp << 2;
                int zp = (z < 0 ? 3 : z);
                encoded_offsets = (byte) (xp | yp | zp);
                encoded_number = (byte)(9 * (x < 0 ? 2 : x) + 3 * (y < 0 ? 2 : y) + (z < 0 ? 2 : z));
            }

            public override string ToString()
            {
                string s = "(" + neighbor_phkey + "," + encoded_number + "," + Convert.ToString(encoded_offsets, 2) + ")";
                return s;
            }
        }


        static int phbits = 6;
        static double box = 1000;
        static double phboxinv = ((double)(1 << phbits)) / box;
        static int cellsPerSide = 64;
        static double cellLength = box / cellsPerSide;
        static double totalPhkeys = Math.Pow(8, phbits);

        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;
            int[,,] phkeys = new int[64,64,64];
            int[, ,] phkeysRev = new int[64, 64, 64];

            for (int x = 0; x < cellsPerSide; ++x)
            {
                for (int y = 0; y < cellsPerSide; ++y)
                {
                    for (int z = 0; z < cellsPerSide; ++z)
                    {
                        phkeys[x, y, z] = GetPHKey(x * cellLength, y * cellLength, z * cellLength);
                    }
                }
            }

            for (int i = 0; i < 64*64*64; ++i)
            {
                int x, y, z;
                PeanoHilbertID idtest = new PeanoHilbertID();
                idtest.GetPosition(6, i, out x, out y, out z);
                phkeysRev[x, y, z] = i;

            }

            Debug.WriteLine(mod(-1, 1000));
            Debug.WriteLine(mod(1000, 1000));

//           for (int x = 0; x < cellsPerSide; ++x)
//           {
//               for (int y = 0; y < cellsPerSide; ++y)
//               {
//                   for (int z = 0; z < cellsPerSide; ++z)
//                   {
//                       if (phkeys[x, y, z] != phkeysRev[x, y, z])
//                       {
//                           Debug.WriteLine("orig: " + phkeys[x, y, z] + ", rev: " + phkeysRev[x, y, z] + "(" + x + "," + y + "," + z + ")");
//                       }
//                       else if (x % 10 == 0 && y % 10 == 0 && z % 10 == 0)
//                       {
//                           Debug.WriteLine("okay " + x);
//                       }
//                   }
//               }
//           }
//           Debug.WriteLine("done!!!!!!");

           
            Debug.WriteLine("Generated phkey array");
            bool done = false;
            Dictionary<int, List<NeighborCode>> neighborLists = new Dictionary<int, List<NeighborCode>>();
            for (int x = 0; x < cellsPerSide; ++x)
            {
                for (int y = 0; y < cellsPerSide; ++y)
                {
                    for (int z = 0; z < cellsPerSide; ++z)
                    {
                        List<NeighborCode> neighbors = new List<NeighborCode>();
 //                       int[] xs = { mod(x - 1, (int)cellsPerSide), x, mod(x + 1, (int)cellsPerSide) };
 //                       int[] ys = { mod(y - 1, (int)cellsPerSide), y, mod(y + 1, (int)cellsPerSide) };
 //                       int[] zs = { mod(z - 1, (int)cellsPerSide), z, mod(z + 1, (int)cellsPerSide) };
 //                       for (int i = 0; i < 3; ++i)
 //                       {
 //                           for (int j = 0; j < 3; ++j)
 //                           {
 //                               for (int k = 0; k < 3; ++k)
 //                               {
 //                                   if ((i == 1) && (j == 1) && (k == 1))
 //                                   {
 //                                       //TODO could also put in an offset that we change from 0 to -1 here so that we don't skip 14
 //                                       //continue;
 //                                       neighbors.Add(new Tuple<Byte, Byte, int>((Byte)0, (phkeys[xs[1], ys[1], zs[k]])));
 //                                       continue;
 //                                   }
 //                                   neighbors.Add(new Tuple<Byte, Byte, int>((Byte) (9*i + 3*j + k + 1), (phkeys[xs[i], ys[j], zs[k]])));
 //                               }
 //                           }
 //                       }
                        int maxxxx = 0;
                        for (int xx = -1; xx <= 1; ++xx)
                        {
                            int modx = mod(xx + x, (int)cellsPerSide);
                            if (modx > maxxxx) { maxxxx = modx; }
                            for (int yy = -1; yy <= 1; ++yy)
                            {
                                int mody = mod(yy + y, (int)cellsPerSide);
                                for (int zz = -1; zz <= 1; ++zz)
                                {
                                    int modz = mod(zz + z, (int)cellsPerSide);
                                    NeighborCode current = new NeighborCode(phkeys[modx, mody, modz], xx, yy, zz);
                                    neighbors.Add(current);
                                }
                            }
                        }
                        Debug.WriteLineIf(!done, maxxxx);
                        done = true;
                        neighborLists.Add(phkeys[x,y,z], neighbors);
                    }
                }
            }

            Debug.WriteLine("Writing File...");

            using (BinaryWriter binWriter = new BinaryWriter(new FileStream("H:\\phkey_neighbor_lists_binary", FileMode.Create)))
            {
                foreach (int phkey in neighborLists.Keys)
                {
                    foreach (NeighborCode neighbor in neighborLists[phkey])
                    {
                        
                        binWriter.Write(phkey);
                        // neighbor code
                        binWriter.Write(neighbor.encoded_number);
                        // byte 00xxyyzz where x=0:00, x=1:01, x=-1:11
                        binWriter.Write(neighbor.encoded_offsets);
                        // neighbor phkey
                        binWriter.Write(neighbor.neighbor_phkey);
                    }
                }
            }
            
            
            using (StreamWriter writer = new StreamWriter("H:\\phkey_neighbor_lists", false))
            {
                foreach (int phkey in neighborLists.Keys)
                {
                    StringBuilder line = new StringBuilder(phkey + ":\t");
                    foreach (NeighborCode neighbor in neighborLists[phkey])
                    {
                        line.Append(neighbor.ToString() + " ");
                    }
                    writer.WriteLine(line.ToString());
                }
            }
            

            TimeSpan runtime = DateTime.Now - start;
            Debug.WriteLine("runtime: " + runtime.TotalSeconds + "." + runtime.Milliseconds);

        }

        // from http://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
        public static int mod(int i, int m)
        {
            return (i%m + m)%m;
        }

        public static int GetPHKey(double x, double y, double z)
        {
            int phbits = 6;
            double box = 1000;
            double phboxinv = ((double)(1 << phbits)) / box;
            int ix = (int)Math.Floor(x * phboxinv);
            int iy = (int)Math.Floor(y * phboxinv);
            int iz = (int)Math.Floor(z * phboxinv);
            return PeanoHilbertID.GetPeanoHilbertID(phbits, ix, iy, iz);
        }
    }
}
