using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    class GroupTabFile
    {
        public bool wasWritten = false;
        public int ifile;

        public int previousLastGroupIndex; // global index of first group in this file

        public int lastGroupIndex
        {
            get { return previousLastGroupIndex + numGroups; }
        }
        public bool ContainsGroup(int index)
        {
            return numGroups > 0 && index > previousLastGroupIndex && index <= lastGroupIndex;
        }

        public int numGroups; // groups in this file
        public int totalGroups; // total number of groups
        public int numIDs; // IDs in this index (but in other file)
        public Int64 totalIDs; // total IDs (contained in halos)
        public int numFiles; // number of split files

        public GroupTabInfo[] groups;
        public GroupTabInfo GetGroupByGlobalIndex(int igroup)
        {
            return (numGroups > 0 ? groups[igroup - previousLastGroupIndex - 1] : null);
        }

                /// <summary>
        /// 
        /// </summary>
        /// <param name="_process"></param>
        /// <param name="filename"></param>
        /// <param name="_snapnum"></param>
        /// <param name="_ifile"></param>
        public GroupTabFile(string filename, GroupTabFile previous)
        //string filename, bool _hasVelDisp, short snapnum, float redshift)
        {
            this.ifile = (previous == null ? 0 : previous.ifile + 1);

            // no need for records/bytesRead, not in fortran format
            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                numGroups = reader.ReadInt32();
                totalGroups = reader.ReadInt32();
                numIDs = reader.ReadInt32();
                totalIDs = reader.ReadInt64();
                numFiles = reader.ReadInt32();
                previousLastGroupIndex = (previous == null ? -1 : previous.lastGroupIndex);
                groups = new GroupTabInfo[numGroups];
                for (int i = 0; i < numGroups; i++)
                    groups[i] = new GroupTabInfo();
                // Len+offset+Mass
                reader.ReadBytes(4*3*numGroups);

                for (int i = 0; i < numGroups; i++)
                {
                    groups[i].cmx = reader.ReadSingle();
                    groups[i].cmy = reader.ReadSingle();
                    groups[i].cmz = reader.ReadSingle();
                }
                for (int i = 0; i < numGroups; i++)
                {
                    groups[i].cvx = reader.ReadSingle();
                    groups[i].cvy = reader.ReadSingle();
                    groups[i].cvz = reader.ReadSingle();
                }
            }
        }
    }

    
    class GroupFile
    {
        public bool wasWritten = false;
        private GroupProcess process; // for accessing useful config data
        public short snapnum;
        public int ifile;
        public bool hasVelDisp = false;

        public int previousLastGroupIndex; // global index of first group in this file
        public int previousLastSubhaloIndex; // global index of first subhalo in this file

        public int lastGroupIndex
        {
            get { return previousLastGroupIndex + numGroups; }
        }
        public int lastSubhaloIndex
        {
            get { return previousLastSubhaloIndex + numSubgroups; }
        }
        public int SubhaloRank(int isubhalo)
        {
            return isubhalo - previousLastSubhaloIndex - 1; 
        }

        public bool ContainsGroup(int index)
        {
            return numGroups > 0 && index > previousLastGroupIndex && index <= lastGroupIndex;
        }
        public int GroupIndexInFile(int index)
        {
            return index - previousLastGroupIndex -1;
        }
        public bool ContainsSubhalo(int index)
        {
            return numSubgroups > 0 && index > previousLastSubhaloIndex && index <= lastSubhaloIndex;
        }

        public int numGroups; // groups in this file
        public int totalGroups; // total number of groups
        public int numIDs; // IDs in this index (but in other file)
        public Int64 totalIDs; // total IDs (contained in halos)
        public int numFiles; // number of split files
        public int numSubgroups; // number of subgroups in this file
        public int totalSubgroups; // and total number of subgroups

        public GroupInfo[] groups;
        public GroupInfo GetGroupByGlobalIndex(int igroup)
        {
            return (numGroups > 0 ? groups[igroup - previousLastGroupIndex-1] : null);
        }
        public SubhaloInfo[] subhalos;
        public SubhaloInfo GetSubhaloByGlobalIndex(int isubhalo)
        {
            return (numSubgroups > 0 ? subhalos[isubhalo - previousLastSubhaloIndex -1] : null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_process"></param>
        /// <param name="filename"></param>
        /// <param name="_snapnum"></param>
        /// <param name="_ifile"></param>
        public GroupFile(GroupProcess _process, string filename, short _snapnum, GroupFile previous)
            //string filename, bool _hasVelDisp, short snapnum, float redshift)
        {
            this.process = _process;
            this.snapnum = _snapnum;
            this.ifile = (previous == null?0:previous.ifile+1);

            float redshift = process.globals.redshifts[snapnum];
            bool hasVelDisp = process.hasVelDisp;
            // no need for records/bytesRead, not in fortran format
            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                numGroups = reader.ReadInt32();
                totalGroups = reader.ReadInt32();
                numIDs = reader.ReadInt32();
                totalIDs = reader.ReadInt64();
                numFiles = reader.ReadInt32();
                numSubgroups = reader.ReadInt32();
                totalSubgroups = reader.ReadInt32();


                previousLastGroupIndex = (previous == null ? -1 : previous.lastGroupIndex);
                previousLastSubhaloIndex = (previous == null ? -1 : previous.lastSubhaloIndex);



                groups = new GroupInfo[numGroups];
                for (int i = 0; i < numGroups; i++)
                {
                    groups[i] = new GroupInfo();
                    groups[i].snapnum = snapnum;
                    groups[i].redshift = redshift;
                }

                subhalos = new SubhaloInfo[numSubgroups];
                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i] = new SubhaloInfo();
                    subhalos[i].snapnum = snapnum;
                    subhalos[i].redshift = redshift;
                }


                for (int i = 0; i < numGroups; i++)
                    groups[i].Length = reader.ReadInt32();

                for (int i = 0; i < numGroups; i++)
                    groups[i].offset = reader.ReadInt32();

                for (int i = 0; i < numGroups; i++)
                    groups[i].Mass = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                {
                    groups[i].x = reader.ReadSingle();
                    groups[i].y = reader.ReadSingle();
                    groups[i].z = reader.ReadSingle();
                    groups[i].ix = process.GetZone(groups[i].x);
                    groups[i].iy = process.GetZone(groups[i].y);
                    groups[i].iz = process.GetZone(groups[i].z);
                    groups[i].phkey = process.GetPHKey(groups[i].x, groups[i].y, groups[i].z);
                }

                for (int i = 0; i < numGroups; i++)
                   groups[i].M_Mean200 = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                    groups[i].R_Mean200 = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                    groups[i].M_Crit200 = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                    groups[i].R_Crit200 = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                    groups[i].M_TopHat200 = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                    groups[i].R_TopHat200 = reader.ReadSingle();

                if (hasVelDisp)
                {
                    for (int i = 0; i < numGroups; i++)
                        groups[i].VelDisp_Mean200 = reader.ReadSingle();

                    for (int i = 0; i < numGroups; i++)
                        groups[i].VelDisp_Crit200 = reader.ReadSingle();

                    for (int i = 0; i < numGroups; i++)
                        groups[i].VelDisp_TopHat200 = reader.ReadSingle();
                }
                for (int i = 0; i < numGroups; i++)
                    reader.ReadInt32();
//                    groups[i].contamCount = reader.ReadInt32();

                for (int i = 0; i < numGroups; i++)
                    reader.ReadSingle();
//                    groups[i].contamMass = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                    groups[i].numSubs = reader.ReadInt32();

                for (int i = 0; i < numGroups; i++)
                    groups[i].firstSubIndex = reader.ReadInt32();

                // -------------------------------------
                // now we're at the subhalo table start
                // -------------------------------------
                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].length = reader.ReadInt32();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].offset = reader.ReadInt32();

                // skip offsets and parents
                reader.ReadBytes(4 * numSubgroups);

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].mass = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].x = reader.ReadSingle();
                    subhalos[i].y = reader.ReadSingle();
                    subhalos[i].z = reader.ReadSingle();
                    subhalos[i].ix = process.GetZone(subhalos[i].x);
                    subhalos[i].iy = process.GetZone(subhalos[i].y);
                    subhalos[i].iz = process.GetZone(subhalos[i].z);
                    subhalos[i].phkey = process.GetPHKey(subhalos[i].x, subhalos[i].y, subhalos[i].z);
                }

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].vx = reader.ReadSingle();
                    subhalos[i].vy = reader.ReadSingle();
                    subhalos[i].vz = reader.ReadSingle();
                }

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].cmx = reader.ReadSingle();
                    subhalos[i].cmy = reader.ReadSingle();
                    subhalos[i].cmz = reader.ReadSingle();
                }

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].sx = reader.ReadSingle();
                    subhalos[i].sy = reader.ReadSingle();
                    subhalos[i].sz = reader.ReadSingle();
                }

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].veldisp = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].velMax = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].velMaxRad = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].hmradius = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].mostBoundId = reader.ReadInt64();

                // skip GrNr
                reader.ReadBytes(4 * numSubgroups);

            }
        }
        public override string ToString()
        {
            return ifile + "," + numGroups+","+ previousLastGroupIndex + "," + lastGroupIndex + "," 
                + numSubgroups + "," + previousLastSubhaloIndex + "," + lastSubhaloIndex;
        }

    }

    /// <summary>
    /// All subhalos in same file as their FOF group.
    /// Thereofore simpler processing.
    /// </summary>
    class XXLGroupFile
    {
        public bool wasWritten = false;
        private XXLGroupProcess process; // for accessing useful config data
        public short snapnum;
        public int ifile;
        public bool hasVelDisp = false;

        public int previousLastGroupIndex; // global index of first group in this file
        public int previousLastSubhaloIndex; // global index of first subhalo in this file

        public int lastGroupIndex
        {
            get { return previousLastGroupIndex + numGroups; }
        }
        public int lastSubhaloIndex
        {
            get { return previousLastSubhaloIndex + numSubgroups; }
        }
        public int SubhaloRank(int isubhalo)
        {
            return isubhalo - previousLastSubhaloIndex - 1;
        }

        public int numGroups; // groups in this file
        public long totalGroups; // total number of groups
        public int numIDs; // IDs in this index (but in other file)
        public Int64 totalIDs; // total IDs (contained in halos)
        public int numFiles; // number of split files
        public int numSubgroups; // number of subgroups in this file
        public long totalSubgroups; // and total number of subgroups

        public GroupInfo_MXXL[] groups;
        public SubhaloInfo_MXXL[] subhalos;

        public const long SNAP_PREFIX = (long)1e12;
        public const int FOF_PREFIX = (int)1e5;
        public long firstFofId = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_process"></param>
        /// <param name="filename"></param>
        /// <param name="_snapnum"></param>
        public XXLGroupFile(XXLGroupProcess _process, string _pathname, int _ifile, short _snapnum, bool _hasVeldisp)
        {

            this.process = _process;
            this.snapnum = _snapnum;
            this.ifile = _ifile;
            string filename = _process.GetSubTab(_pathname, _snapnum, ifile);
            this.hasVelDisp = _hasVeldisp;

            float redshift = process.globals.redshifts[snapnum];
            bool hasVelDisp = process.hasVelDisp;
            // no need for records/bytesRead, not in fortran format
            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                numGroups = reader.ReadInt32();
                totalGroups = reader.ReadInt64();
                numIDs = reader.ReadInt32();
                totalIDs = reader.ReadInt64();
                numFiles = reader.ReadInt32();
                numSubgroups = reader.ReadInt32();
                totalSubgroups = reader.ReadInt64();

                groups = new GroupInfo_MXXL[numGroups];
                for (int i = 0; i < numGroups; i++)
                {
                    groups[i] = new GroupInfo_MXXL();
                    groups[i].snapnum = snapnum;
                    groups[i].redshift = redshift;
                }

                subhalos = new SubhaloInfo_MXXL[numSubgroups];
                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i] = new SubhaloInfo_MXXL();
                    subhalos[i].snapnum = snapnum;
                    subhalos[i].redshift = redshift;
                }
                for (int i = 0; i < numGroups; i++)
                    groups[i].Length = reader.ReadInt32();

                for (int i = 0; i < numGroups; i++)
                    groups[i].offset = reader.ReadInt32();

                for (int i = 0; i < numGroups; i++)
                    groups[i].GroupNr = reader.ReadInt64();

                for (int i = 0; i < numGroups; i++)
                {
                    groups[i].cmx = reader.ReadSingle();
                    groups[i].cmy = reader.ReadSingle();
                    groups[i].cmz = reader.ReadSingle();
                }
                for (int i = 0; i < numGroups; i++)
                {
                    groups[i].vx = reader.ReadSingle();
                    groups[i].vy = reader.ReadSingle();
                    groups[i].vz = reader.ReadSingle();
                }
                for (int i = 0; i < numGroups; i++)
                {
                    groups[i].x = reader.ReadSingle();
                    groups[i].y = reader.ReadSingle();
                    groups[i].z = reader.ReadSingle();
                    groups[i].ix = process.GetZone(groups[i].x);
                    groups[i].iy = process.GetZone(groups[i].y);
                    groups[i].iz = process.GetZone(groups[i].z);
                    groups[i].phkey = process.GetPHKey(groups[i].x, groups[i].y, groups[i].z);
                }
                for (int i = 0; i < numGroups; i++)
                    groups[i].M_Mean200 = reader.ReadSingle();

                for (int i = 0; i < numGroups; i++)
                    groups[i].M_Crit200 = reader.ReadSingle();
                for (int i = 0; i < numGroups; i++)
                    groups[i].M_TopHat200 = reader.ReadSingle();
                for (int i = 0; i < numGroups; i++)
                    groups[i].VelDisp = reader.ReadSingle();
                if (hasVelDisp)
                {
                    for (int i = 0; i < numGroups; i++)
                        groups[i].VelDisp_Mean200 = reader.ReadSingle();

                    for (int i = 0; i < numGroups; i++)
                        groups[i].VelDisp_Crit200 = reader.ReadSingle();

                    for (int i = 0; i < numGroups; i++)
                        groups[i].VelDisp_TopHat200 = reader.ReadSingle();
                }
                for (int i = 0; i < numGroups; i++)
                    groups[i].numSubs = reader.ReadInt32();

                for (int i = 0; i < numGroups; i++)
                    groups[i].firstSubIndex = reader.ReadInt32();

                // -------------------------------------
                // now we're at the subhalo table start
                // -------------------------------------
                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].length = reader.ReadInt32();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].offset = reader.ReadInt32();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].GrNr = reader.ReadInt64();
                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].SubNr = reader.ReadInt64();

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].x = reader.ReadSingle();
                    subhalos[i].y = reader.ReadSingle();
                    subhalos[i].z = reader.ReadSingle();
                    subhalos[i].ix = process.GetZone(subhalos[i].x);
                    subhalos[i].iy = process.GetZone(subhalos[i].y);
                    subhalos[i].iz = process.GetZone(subhalos[i].z);
                    subhalos[i].phkey = process.GetPHKey(subhalos[i].x, subhalos[i].y, subhalos[i].z);
                }

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].vx = reader.ReadSingle();
                    subhalos[i].vy = reader.ReadSingle();
                    subhalos[i].vz = reader.ReadSingle();
                }

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].cmx = reader.ReadSingle();
                    subhalos[i].cmy = reader.ReadSingle();
                    subhalos[i].cmz = reader.ReadSingle();
                }

                for (int i = 0; i < numSubgroups; i++)
                {
                    subhalos[i].sx = reader.ReadSingle();
                    subhalos[i].sy = reader.ReadSingle();
                    subhalos[i].sz = reader.ReadSingle();
                }

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].veldisp = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].velMax = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].velMaxRad = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].hmradius = reader.ReadSingle();

                for (int i = 0; i < numSubgroups; i++)
                    for(int k = 0; k < 6; k++)
                        subhalos[i].shape[k] = reader.ReadSingle();
                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].bindingEnergy = reader.ReadSingle();
                for (int i = 0; i < numSubgroups; i++)
                    subhalos[i].potentialEnergy = reader.ReadSingle();
                for (int i = 0; i < numSubgroups; i++)
                    for (int k = 0; k < 9; k++)
                        subhalos[i].profile[k] = reader.ReadSingle();
            }
        }
        public void WriteGroupFile(BinaryWriter groupwriter, BinaryWriter subhalowriter)
        {
            long globalFofId = snapnum * SNAP_PREFIX + firstFofId;
            int subhalorank = 0;

            // prepare indices
            for (int igroup = 0; igroup < numGroups; igroup++)
            {
                GroupInfo_MXXL currentGroup = groups[igroup];
                currentGroup.fofId = globalFofId++;

                if (currentGroup.numSubs > 0)
                {
                    long subhaloid = currentGroup.fofId * FOF_PREFIX;
                    for (int isubhalo = currentGroup.firstSubIndex; isubhalo <= currentGroup.lastSubIndex; isubhalo++)
                    {
                        SubhaloInfo_MXXL currentSubhalo = subhalos[isubhalo];

                        // do more stuff
                        currentSubhalo.subhaloFileId = MakeSubhaloFileID(snapnum, ifile, subhalorank++);
                        currentSubhalo.fofId = currentGroup.fofId;
                        currentSubhalo.subhaloFOFId = subhaloid++;
                        if (isubhalo == currentGroup.firstSubIndex)
                            currentGroup.firstSubID = currentSubhalo.subhaloFOFId;
                    }
                }
            }

            // a potential sort would go here, this must be done AFTER the groups's subhaloid-s have been set !
            if (groupwriter != null)
            {
                for (int igroup = 0; igroup < numGroups; igroup++)
                    groups[igroup].WriteBinary(groupwriter, hasVelDisp);
            }
            if (subhalowriter != null)
            {
                for (int isubhalo = 0; isubhalo < numSubgroups; isubhalo++)
                    subhalos[isubhalo].WriteBinary(subhalowriter);
            }
            wasWritten = true;

        }
        public long MakeSubhaloFileID(int snap, int file, int index)
        {
            return (snap * 10000L + file) * 1000000L + index;
        }
    }

    
    
    class Queue
    {
        LinkedList<GroupFile> list;
        public Queue(GroupFile first)
        {
            list = new LinkedList<GroupFile>();
        }
        public void Add(GroupFile f)
        {
            list.AddLast(f);
        }
        public void Remove(GroupFile f)
        {
            list.Remove(f);
        }
    }

}