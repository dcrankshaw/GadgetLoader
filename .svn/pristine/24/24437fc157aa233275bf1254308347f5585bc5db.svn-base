/*Can be deleted????


using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    class GalaxiesFile
    {
        public GalaxyInfo[] galaxies;
        public GalaxiesFile(string galaxiesFile, int nummag, bool hasMainLeafId)
        {
            using (BinaryReader galaxyReader = new BinaryReader(new FileStream(galaxiesFile, FileMode.Open)))
            {
                int numgalaxies = galaxyReader.ReadInt32();
                galaxies = new GalaxyInfo[numgalaxies];
                for (int i = 0; i < numgalaxies; i++)
                {
                    galaxies[i] = new GalaxyInfo();
                    galaxies[i].GalID = galaxyReader.ReadInt64();
                    galaxies[i].HaloID = galaxyReader.ReadInt64();

                    galaxies[i].FirstProgGal = galaxyReader.ReadInt64();
                    galaxies[i].NextProgGal = galaxyReader.ReadInt64();
                    galaxies[i].LastProgGal = galaxyReader.ReadInt64();
                    galaxies[i].FileTreeNr = galaxyReader.ReadInt64(); // check
                    galaxies[i].DescendantGal = galaxyReader.ReadInt64();
////                    galaxies[i].SubID = galaxyReader.ReadInt64();
////                    galaxies[i].MMSubID = galaxyReader.ReadInt64();
                    galaxies[i].PeanoKey = galaxyReader.ReadInt32();
                    galaxies[i].Redshift = galaxyReader.ReadSingle();
                    galaxies[i].Type = galaxyReader.ReadInt32();
                    galaxies[i].SnapNum = galaxyReader.ReadInt32();
                    galaxies[i].CentralMvir = galaxyReader.ReadSingle();
                    galaxies[i].x = galaxyReader.ReadSingle();
                    galaxies[i].y = galaxyReader.ReadSingle();
                    galaxies[i].z = galaxyReader.ReadSingle();
                    galaxies[i].vx = galaxyReader.ReadSingle();
                    galaxies[i].vy = galaxyReader.ReadSingle();
                    galaxies[i].vz = galaxyReader.ReadSingle();
//                    galaxies[i].Spin = galaxyReader.ReadSingle();
                    galaxies[i].Len = galaxyReader.ReadInt32();
                    galaxies[i].Mvir = galaxyReader.ReadSingle();
                    galaxies[i].Rvir = galaxyReader.ReadSingle();
                    galaxies[i].Vvir = galaxyReader.ReadSingle();
                    galaxies[i].Vmax = galaxyReader.ReadSingle();
                    galaxies[i].InfallRadius = galaxyReader.ReadSingle();
                    galaxies[i].Infallmass = galaxyReader.ReadSingle();
                    galaxies[i].Orimergetime = galaxyReader.ReadSingle();
//                    galaxies[i].Hotradius = galaxyReader.ReadSingle();
//                    galaxies[i].retainfrac = galaxyReader.ReadSingle();
galaxies[i].MergeSat = galaxyReader.ReadSingle();
                    galaxies[i].ColdGas = galaxyReader.ReadSingle();
//                    galaxies[i].ColdAvailable = galaxyReader.ReadSingle();
                    galaxies[i].StellarMass = galaxyReader.ReadSingle();
                    galaxies[i].BulgeMass = galaxyReader.ReadSingle();
                    galaxies[i].HotGas = galaxyReader.ReadSingle();
                    galaxies[i].EjectedMass = galaxyReader.ReadSingle();
                    galaxies[i].BlackHoleMass = galaxyReader.ReadSingle();
//                    galaxies[i].major = galaxyReader.ReadSingle();
//                    galaxies[i].minor = galaxyReader.ReadSingle();
//                    galaxies[i].majorsnap = galaxyReader.ReadSingle();
                    galaxies[i].MetalsColdGas = galaxyReader.ReadSingle();
                    galaxies[i].MetalsStellarMass = galaxyReader.ReadSingle();
                    galaxies[i].MetalsBulgeMass = galaxyReader.ReadSingle();
                    galaxies[i].MetalsHotGas = galaxyReader.ReadSingle();
                    galaxies[i].MetalsEjectedMass = galaxyReader.ReadSingle();
                    galaxies[i].Sfr = galaxyReader.ReadSingle();
                    galaxies[i].SfrBulge = galaxyReader.ReadSingle();
                    galaxies[i].MergeMass = galaxyReader.ReadSingle();
////                    galaxies[i].reheatedMass = galaxyReader.ReadSingle();
////                    galaxies[i].reheatedMassfake = galaxyReader.ReadSingle();
////                    galaxies[i].dEjectedmass = galaxyReader.ReadSingle();
//                    galaxies[i].StB = galaxyReader.ReadSingle();
                    galaxies[i].XrayLum = galaxyReader.ReadSingle();
                    galaxies[i].BulgeSize = galaxyReader.ReadSingle();
                    galaxies[i].Stellardisk = galaxyReader.ReadSingle();
                    galaxies[i].GasDiskRadius = galaxyReader.ReadSingle();
                    galaxies[i].DisruptionOn = galaxyReader.ReadInt32();
                    galaxies[i].MergeOn = galaxyReader.ReadInt32();
                    galaxies[i].CoolingRadius = galaxyReader.ReadSingle();

galaxies[i].ICM = galaxyReader.ReadSingle();
galaxies[i].MetalsICM = galaxyReader.ReadSingle();


                    galaxies[i].Mag = new float[nummag];
                    for(int im = 0; im < nummag; im++)
                        galaxies[i].Mag[im] = galaxyReader.ReadSingle();
                    galaxies[i].MagBulge = new float[nummag];
                    for (int im = 0; im < nummag; im++)
                        galaxies[i].MagBulge[im] = galaxyReader.ReadSingle();
                    galaxies[i].MagDust = new float[nummag];
                    for (int im = 0; im < nummag; im++)
                        galaxies[i].MagDust[im] = galaxyReader.ReadSingle();
                    galaxies[i].MassWeightAge = galaxyReader.ReadSingle();
                    galaxyReader.ReadBytes(4); // dummy



                    if (hasMainLeafId)
                        galaxies[i].mainLeafId = galaxyReader.ReadInt64();
                    else
                        galaxies[i].mainLeafId = -1L;

                }
            }
        }

    }
}


*/