using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    class TreedataFile
    {
        HaloTreeProcess process;
        private BinaryReader treeReader = null;
        private BinaryReader treeIdsReader = null;
        public int ntrees;
        public int totnhalos;
        public int[] treenhalos;
        public long numtot;
        public int current = 0;
        public TreedataFile(HaloTreeProcess _process, string treeFile, string treeIdsFile)
        {
            this.process = _process;
            treeReader = new BinaryReader(new FileStream(treeFile, FileMode.Open));
            treeIdsReader = new BinaryReader(new FileStream(treeIdsFile, FileMode.Open));
            ntrees = treeReader.ReadInt32();
            totnhalos = treeReader.ReadInt32();
            treenhalos = new int[ntrees];
            numtot = 0;
            for(int i =0; i < ntrees; i++)
            {
                treenhalos[i] = treeReader.ReadInt32();
                numtot += treenhalos[i];
            }
            if (numtot != totnhalos)
                DebugOut.PrintLine("numtot != totnhalos: "+numtot+" vs. "+totnhalos);
        }
        public int next(TreedataInfo[] trees)
        {
//            trees = new TreedataInfo[num];
            int num=Math.Min(totnhalos-current,trees.Length);
            for (int i = 0; i < num; i++)
            {
                    trees[i] = new TreedataInfo();
                    // skip descendant, firstProg, nextprog, firstfof, nextfof
                    treeReader.ReadBytes(20);
                    trees[i].length = treeReader.ReadInt32();
                    trees[i].M_Mean200 = treeReader.ReadSingle();
                    trees[i].M_Crit200 = treeReader.ReadSingle();
                    trees[i].M_TopHat = treeReader.ReadSingle();
                    trees[i].x = treeReader.ReadSingle();
                    trees[i].y = treeReader.ReadSingle();
                    trees[i].z = treeReader.ReadSingle();
                    trees[i].vx = treeReader.ReadSingle();
                    trees[i].vy = treeReader.ReadSingle();
                    trees[i].vz = treeReader.ReadSingle();
                    trees[i].VelDisp = treeReader.ReadSingle();
                    trees[i].Vmax = treeReader.ReadSingle();
                    trees[i].sx = treeReader.ReadSingle();
                    trees[i].sy = treeReader.ReadSingle();
                    trees[i].sz = treeReader.ReadSingle();
                    trees[i].mostboundid = treeReader.ReadInt64();
                    trees[i].snapnum = treeReader.ReadInt32();
                    
                    trees[i].fileNr = treeReader.ReadInt32();
                    trees[i].subhaloindex = treeReader.ReadInt32();
                    trees[i].subhalfmass = treeReader.ReadSingle();

                    // read DB-IDs
                    trees[i].haloId = treeIdsReader.ReadInt64();
                    trees[i].treeid = treeIdsReader.ReadInt64();
                    trees[i].firstProgenitorId = treeIdsReader.ReadInt64();
                    trees[i].lastProgenitorId = treeIdsReader.ReadInt64();
                    trees[i].nextProgenitorId = treeIdsReader.ReadInt64();
                    trees[i].descendantId = treeIdsReader.ReadInt64();
                    trees[i].firstHaloInFOFGroupId = treeIdsReader.ReadInt64();
                    trees[i].nextHaloInFOFGroupId = treeIdsReader.ReadInt64();
                    if (process.hasMainLeafId)
                        trees[i].mainLeafId = treeIdsReader.ReadInt64();
                    else
                        trees[i].mainLeafId = -1L;
                    trees[i].redshift = (float)treeIdsReader.ReadDouble();
                    trees[i].phkey = treeIdsReader.ReadInt32(); // dummy ???
                    trees[i].dummy2 = treeIdsReader.ReadInt32(); // dummy ???

                    trees[i].fofId = -1L;
                    trees[i].subhaloFOFId = -1L;
                    trees[i].vMaxRad = 0;

                                    
                    trees[i].ix = process.GetZone(trees[i].x);
                    trees[i].iy = process.GetZone(trees[i].y);
                    trees[i].iz = process.GetZone(trees[i].z);
                    // could use dummy1 for this
//                    int phkey = process.GetPHKey(trees[i].x, trees[i].y, trees[i].z);
//                    if (phkey != trees[i].phkey)
//                        DebugOut.PrintLine("PhKey incompatibility in halo " + i+", "+phkey+" vs. "+trees[i].phkey);
                    trees[i].phkey = process.GetPHKey(trees[i].x, trees[i].y, trees[i].z); ;
                    trees[i].RandomInt = process.globals.random.Next(process.globals.maxRandom);

                    trees[i].subhaloFileId = process.MakeSubhaloFileID(trees[i].snapnum, trees[i].fileNr, trees[i].subhaloindex);


                current++;
            }
            return num;
        }
        public void Close()
        {
            treeReader.Close();
            treeIdsReader.Close();
        }

    }
}
