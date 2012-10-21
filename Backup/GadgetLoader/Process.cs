using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace GadgetLoader
{
    class Runner
    {
        public bool isProcessing ;
        public string SQLCommandsFile;
        private List<Process> processes = new List<Process>();
        public Runner(string _SQLCommandsFile)
        {
            this.isProcessing = false;
            this.SQLCommandsFile = _SQLCommandsFile;
        }

        public void Add(Process process)
        {
            if(process == null)
                return;
            processes.Add(process);
            process.runner = this;
        }
        public void Run()
        {
            try
            {
                isProcessing = true;
                foreach (Process process in processes)
                {
                    if (isProcessing)
                        process.Run();
                    else
                        break;
                }
                DebugOut.PrintLine("Processing complete.");
                DebugOut.PrintLine("SQL bulk insert commands saved to " + SQLCommandsFile);
                if (SQLCommandsFile != null)
                    DebugOut.SaveCommands(SQLCommandsFile);

            }
            catch (Exception e)
            {
                DebugOut.PrintLine("====== ERROR "+e.Message);
            }
            isProcessing = false;

        }
    }



    class SnapshotsProcess : Process
    {
        public String snapshotFilePrefix;
        public int firstSnapshotFile = 0; 
        public int lastSnapshotFile = -1; // last< first implies all
        public float samplingRate = 1.0F;
        public string snapshotTable = "Snapshot";
        public short firstSnap = 0;
        public short lastSnap = 0;
        public bool useHsml = false;
        public bool writeArrays = true;

        public SnapshotsProcess(GlobalParameters globals) : base(globals)
        {}

        public override void Run()
        {
            for(short i = firstSnap; i <= lastSnap; i++)
            {
                if(isProcessing)
                {
                    if (writeArrays)
                        ProcessSnapshotWithArrays(i);
                    else
                        ProcessSnapshot(i);
                }
                else
                {
                    DebugOut.PrintLine("Processing snapshots Interrupted at snapshot "+i);
                    break;
                }
            }
        }
        private void ProcessSnapshot(short snap)
        {
            DebugOut.PrintLine("PROCESSING SNAPSHOT " + snap);
            // using this only if needed
            HsmlReader hsmlReader = new HsmlReader();

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(GetSnapDir(outPath,snap));
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            int curFile = firstSnapshotFile; // index of current read/write file
            int numFile = 0;
            if (useHsml)
                hsmlReader = new HsmlReader(GetHsmlPath(inPath, snap));

            if (lastSnapshotFile < firstSnapshotFile)
            {
                lastSnapshotFile = firstSnapshotFile;
                numFile = -1;
            }
            while (curFile <= lastSnapshotFile)
            {
                DebugOut.PrintLine("..file " + curFile + "/" + lastSnapshotFile);

                string filename = "";
                try
                {
                    filename = GetSnapFile(inPath, snap,snapshotFilePrefix, curFile);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
                // now load the file
                SnapFile curSnap = new SnapFile(filename, samplingRate);
                // and open the stream for writing
                using (BinaryWriter binwriter = new BinaryWriter(new FileStream(GetSnapDefault(outPath, snap, snapshotFilePrefix, curFile), FileMode.Create)))
                {
                    Structs[] parts = new Structs[curSnap.numSample];
                    // now write each particle into the array
                    for (int i = 0; i < curSnap.numSample; i++)
                    {
                        parts[i].x = curSnap.pos[i, 0];
                        parts[i].y = curSnap.pos[i, 1];
                        parts[i].z = curSnap.pos[i, 2];
                        parts[i].vx = curSnap.vel[i, 0];
                        parts[i].vy = curSnap.vel[i, 1];
                        parts[i].vz = curSnap.vel[i, 2];
                        parts[i].snapnum = snap;
                        parts[i].id = curSnap.id[i];
                        // add in highest-order bit
                        parts[i].id |= ((UInt64)curSnap.nLargeSims[1]) << 32;
                        // make ph-key
                        parts[i].phkey = GetPHKey(parts[i].x, parts[i].y, parts[i].z);
                        // read hsml, if desired
                        if (useHsml) // TODO fix this part for random sampling
                        {
                            parts[i].hsml = hsmlReader.GetHsml();
                            parts[i].veldisp = hsmlReader.GetVelDisp();
                            parts[i].density = hsmlReader.GetDensity();
                            // and advance read pointer
                            hsmlReader.Next();
                        }
                    }
                    // now sort before writing files
                    Array.Sort<Structs>(parts, new ParticleComparator());
                    // and then write output
                    if (useHsml)
                        for (int i = 0; i < curSnap.numSample; i++)
                            parts[i].WriteBinary(binwriter);
                    else
                        for (int i = 0; i < curSnap.numSample; i++)
                            parts[i].WriteBinaryNoHsml(binwriter);

                    // and add a bulk insert
                    DebugOut.AddCommand(GetSnapDefault(outPath, snap, snapshotFilePrefix,curFile), snapshotTable);

                    DebugOut.PrintLine("..wrote " + curSnap.numSample + "/" + curSnap.numTotals[1] + " points");
                }

                // and set numFiles
                if (numFile == -1)
                {
                    numFile = (int)curSnap.numSubfiles-firstSnapshotFile+1;
                    lastSnapshotFile = numFile-1;
                }

                curFile++;
                // avoid outofmemory errors
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }
        }
        private void ProcessSnapshotWithArrays(short snap)
        {
            DebugOut.PrintLine("PROCESSING SNAPSHOT WITH ARRAYS" + snap);
            // using this only if needed
            HsmlReader hsmlReader = new HsmlReader();

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(GetSnapDir(outPath, snap));
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            int curFile = firstSnapshotFile; // index of current read/write file
            int numFile = 0;
            if (useHsml)
                hsmlReader = new HsmlReader(GetHsmlPath(inPath, snap));

            if (lastSnapshotFile < firstSnapshotFile)
            {
                lastSnapshotFile = firstSnapshotFile;
                numFile = -1;
            }
            while (curFile <= lastSnapshotFile)
            {
                DebugOut.PrintLine("..file " + curFile + "/" + lastSnapshotFile);

                string filename = "";
                try
                {
                    filename = GetSnapFile(inPath, snap, snapshotFilePrefix, curFile);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
                // now load the file
                SnapFile curSnap = new SnapFile(filename, samplingRate);
                // and open the stream for writing
                using (SqlBinaryWriter binwriter = new SqlBinaryWriter(new FileStream(GetSnapDefault(outPath, snap, snapshotFilePrefix, curFile), FileMode.Create)))
                {
                    Structs[] parts = new Structs[curSnap.numSample];
                    // now write each particle into the array
                    for (int i = 0; i < curSnap.numSample; i++)
                    {
                        parts[i].x = curSnap.pos[i, 0];
                        parts[i].y = curSnap.pos[i, 1];
                        parts[i].z = curSnap.pos[i, 2];
                        parts[i].vx = curSnap.vel[i, 0];
                        parts[i].vy = curSnap.vel[i, 1];
                        parts[i].vz = curSnap.vel[i, 2];
                        parts[i].snapnum = snap;
                        parts[i].id = curSnap.id[i];
                        // add in highest-order bit
                        parts[i].id |= ((UInt64)curSnap.nLargeSims[1]) << 32;
                        // make ph-key
                        parts[i].phkey = GetPHKey(parts[i].x, parts[i].y, parts[i].z);
                        // read hsml, if desired
                        if (useHsml) // TODO fix this part for random sampling
                        {
                            parts[i].hsml = hsmlReader.GetHsml();
                            parts[i].veldisp = hsmlReader.GetVelDisp();
                            parts[i].density = hsmlReader.GetDensity();
                            // and advance read pointer
                            hsmlReader.Next();
                        }
                    }
                    // now sort before writing files
                    // TBD this may not be necessary if the particles 
                    // are sorted in the files already.
                    Array.Sort<Structs>(parts, new ParticleComparator());

                    Cell cell = new Cell(snap);
                    int currentPHkey = -1;
                    for (int i = 0; i < curSnap.numSample; i++)
                    {
                        if (parts[i].phkey != currentPHkey)
                        {
                            if(cell.Count > 0)
                                binwriter.Write(cell);
                            currentPHkey = parts[i].phkey;
                            cell.Init(currentPHkey);
                        }
                        cell.Add(parts[i]);
                    }
                    if (cell.Count > 0)
                        binwriter.Write(cell);


                    // and add a bulk insert
                    DebugOut.AddCommand(GetSnapDefault(outPath, snap, snapshotFilePrefix, curFile), snapshotTable);

                    DebugOut.PrintLine("..wrote " + curSnap.numSample + "/" + curSnap.numTotals[1] + " points");
                }

                // and set numFiles
                if (numFile == -1)
                {
                    numFile = (int)curSnap.numSubfiles - firstSnapshotFile + 1;
                    lastSnapshotFile = numFile - 1;
                }

                curFile++;
                // avoid outofmemory errors
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }
        }

    }

    //START NEW CODE


    class SimDBFOFProcess : Process
    {

        public string inPath = "";
        public string outPath = "";
        public string groupIDFilePrefix = "group_ids_";
        public string groupTabFilePrefix = "group_tab_";
        public int firstSnapshotFile = 0;
        public int lastSnapshotFile = 0;
        public Int16 firstSnap = 0;
        public Int16 lastSnap = 0;





        public SimDBFOFProcess(GlobalParameters globals)
            : base(globals)
        { }
        public override void Run()
        {
            for (int i = firstSnap; i <= lastSnap; i++)
            {
                if (isProcessing)
                    ProcessSimDBFofs(i);
                else
                {
                    DebugOut.PrintLine("Processing SimDB FOF groups interrupted at snapshot " + i);
                    break;
                }
            }
        }
        public void ProcessSimDBFofs(int isnap)
        {
            DebugOut.PrintLine("PROCESSING FOF Groups from Snapshot " + isnap);

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(GetSnapDir(this.outPath, isnap));
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            int skip = 0;
            //long skip_sub = 0;
            int filenumber = 0;
            string groupTabFile = "";
            string groupIDFile = "";
            int[] GroupLen = new int[0];
            int[] GroupOffset = new int[0];
            int[] IDs = new int[0];
            bool done = false;
            int Ntask = 0;
            int nGroups = 0;
            int totNGroups = 0;
            int NIds = 0;
            while(!done)
            {
                groupTabFile = GetSimDBFOFFile(this.inPath, isnap, this.groupTabFilePrefix, filenumber);
                

                using (BinaryReader reader = new BinaryReader(new FileStream(groupTabFile, FileMode.Open, FileAccess.Read)))
                {
                    
                    nGroups = reader.ReadInt32();
                    NIds = reader.ReadInt32();
                    totNGroups = reader.ReadInt32();
                    Ntask = reader.ReadInt32();
                    if(filenumber == 0)
                    {
                        GroupLen = new int[totNGroups];
                        GroupOffset = new int[totNGroups];
                    }
                    if (nGroups > 0)
                    {
                        for (int i = skip; i < skip + nGroups; i++)
                        {
                            GroupLen[i] = reader.ReadInt32();
                        }
                        for (int i = skip; i < skip + nGroups; i++)
                        {
                            GroupOffset[i] = reader.ReadInt32();
                        }
                        skip += nGroups;
                    }
                }
                //DebugOut.PrintLine("Ngroups: " + nGroups + " totNGroups: " + totNGroups + "  NIds: " + NIds + "  Ntask: " + Ntask);
                filenumber++;
                /*if (filenumber > lastSnapshotFile)
                {
                    done = true;
                }*/
                if (filenumber == Ntask)
                {
                    done = true;
                }

            }


            
            nGroups = 0;
            totNGroups = 0;
            NIds = 0;
            Ntask = 0;
            done = false;
            filenumber = 0;
            skip = 0;
            
            while (!done)
            {
                groupIDFile = GetSimDBFOFFile(this.inPath, isnap, this.groupIDFilePrefix, filenumber);


                using (BinaryReader reader = new BinaryReader(new FileStream(groupIDFile, FileMode.Open, FileAccess.Read)))
                {

                    nGroups = reader.ReadInt32();
                    NIds = reader.ReadInt32();
                    totNGroups = reader.ReadInt32();
                    Ntask = reader.ReadInt32();
                    //Will I get integer overflow here???
                    int totNIds = 0;
                    for (int i = 0; i < GroupLen.Length; i++)
                    {
                        totNIds += GroupLen[i];
                    }

                    if (filenumber == 0)
                    {
                        IDs = new int[totNIds];
                        

                    }
                    if (NIds > 0)
                    {
                        for (int i = skip; i < skip + NIds; i++)
                        {
                            IDs[i] = reader.ReadInt32();
                        }
                       
                        skip += NIds;
                    }
                }
                //DebugOut.PrintLine("Ngroups: " + nGroups + " totNGroups: " + totNGroups + "  NIds: " + NIds + "  Ntask: " + Ntask);
                filenumber++;
                /*if (filenumber > lastSnapshotFile)
                {
                    done = true;
                }*/
                if (filenumber == Ntask)
                {
                    done = true;
                }
            }
       

            
            string outfilepath = this.outPath + "//groups_snap" + isnap;

            using (SqlBinaryWriter binwriter = new SqlBinaryWriter(
                new FileStream(outfilepath, FileMode.Create)))
            {
                
                for (int i = 0; i < totNGroups; i++)
                {
                    
                    if (GroupLen[i] > 0)
                    {
                        int[] curGroupIds = new int[GroupLen[i]];
                        Array.Copy(IDs, GroupOffset[i], curGroupIds, 0, curGroupIds.Length);
                        binwriter.Write(curGroupIds, (short)isnap, i);
                    }
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            DebugOut.PrintLine("Completed writing snapshot: " + isnap);
        }

    }




    //END NEW CODE














    class FOFOrderedSnapshotsProcess : Process
    {
        public String snapshotFilePrefix;
        public String snapshotTable;
        public int snapshotFile = -1; // implies all
        public float samplingRate = 1.0F;
        public short firstSnap = 0;
        public short lastSnap = 0;

        // TODO something about the next ...
        long multiplier = 1000000; // sufficient for minimilii



        public FOFOrderedSnapshotsProcess(GlobalParameters globals)
            : base(globals)
        {}
        public override void Run()
        {
            for(short i = firstSnap; i <= lastSnap; i++)
            {
                if(isProcessing)
                    ProcessFOFOrderedSnapshot(i);
                else
                {
                    DebugOut.PrintLine("Processing FOF ordered snapshots Interrupted at snapshot "+i);
                    break;
                }
            }
        }
                
        private void ProcessFOFOrderedSnapshot(short snap)
        {
            DebugOut.PrintLine("PROCESSING FOF ordered SNAPSHOT " + snap);

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(GetSnapDir(outPath, snap));
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            long nonFofIdPrefix = GetNonFOFIdPrefix(snap); // the prefix of particles not belonging to any group

            int curFile = 0; // index of current read/write file
            int numFiles = 1; // total number of files
            string dir = GetSnapDir(inPath, snap);

            LittleFOF[] fofs = ReadLittleFOFs(dir);
            ReadLittleSubHalos(dir, fofs);

            int countFOF = 0; // number of particles in FOF groups
            foreach (LittleFOF f in fofs)
                countFOF += f.np;


            using (StreamWriter subhaloOut = new StreamWriter(new FileStream(GetSubHaloOutFile(outPath, snap), FileMode.Create)))
            {
                foreach (LittleFOF lfof in fofs)
                {
                    if (lfof.subhalos != null)
                    {
                        foreach (LittleSubHalo lsubhalo in lfof.subhalos)
                            subhaloOut.WriteLine(lsubhalo.ToString());
                    }
                }
            }
            DebugOut.AddCommand("bulk insert SubhaloParticle from '"+GetSubHaloOutFile(outPath, snap)+ "' with(fieldterminator=',',tablock,order(subhaloId))");

            int iFOF = 0;
            int ipFof = 0;
            long countNonGrouped = -1;
            // load first file
            LittleFOF fof = null;
            bool nofofs = false;
            if (fofs.Length > 0)
                fof = fofs[0];
            else
                nofofs = true;
            long fofidPrefix = GetFOFIdPrefix(fof);

            while(curFile < numFiles)
            {
                string filename = GetSnapDefault(inPath, snap, snapshotFilePrefix, curFile);
                FOFOrderedSnapFile currentSnapshot = new FOFOrderedSnapFile(this, filename);
                numFiles = (int)currentSnapshot.numSubfiles;
                countNonGrouped = currentSnapshot.numTotals[1] - countFOF;
                if (fofs.Length == 0)
                    fofidPrefix = nonFofIdPrefix - countNonGrouped;
                DebugOut.PrintLine("PROCESSING FOF ordered SNAPSHOT " + snap + " file " + curFile);
                for (int iparticle = 0; iparticle < currentSnapshot.numParts[1]; iparticle++)
                {
                    long fofId = fofidPrefix + ipFof++;
                    currentSnapshot.particles[iparticle].id = fofId;
                    if (fof != null && ipFof >= fof.np)
                    {
                        ipFof = 0;
                        iFOF++;
                        if (iFOF < fofs.Length)
                        {
                            fof = fofs[iFOF];
                            fofidPrefix = GetFOFIdPrefix(fof);
                        }
                        else
                        {
                            fof = null;
                            fofidPrefix = nonFofIdPrefix - countNonGrouped;
                        }
                    }
                }
                
                using (BinaryWriter binwriter = new BinaryWriter(new FileStream(GetSnapDefault(outPath, snap, snapshotFilePrefix, curFile), FileMode.Create)))
                {
                    foreach (FOFOrderedParticle particle in currentSnapshot.particles)
                        particle.WriteBinary(binwriter, snap);

                    // and add a bulk insert
                    //DebugOut.AddCommand(GetSnapDefault(outPath, snap, snapshotFilePrefix, curFile), snapshotTable);
                    DebugOut.AddCommand("bulk insert " + snapshotTable + " from '" + GetSnapDefault(outPath, snap, snapshotFilePrefix, curFile) + "' with(datafiletype='native',tablock, order(id))");

                    DebugOut.PrintLine("..wrote " + currentSnapshot.numParts[1] + "/" + currentSnapshot.numTotals[1] + " points");
                }

                curFile++;
                /*
                if (curFile < numFiles)
                {
                    sign = 1;
                    filename = GetSnapDefault(inPath, snap, snapshotFilePrefix, curFile);
                    currentSnapshot = new FOFOrderedSnapFile(this, filename);
                }
                else
                    break;
                 */
            }
        }

        private string GetSubHaloOutFile(string outpath, int snap)
        {
            return outpath + "\\subhalos_" + FormatSnap(snap) + ".csv";
        }
        private long GetFOFIdPrefix(LittleFOF fof)
        {
            if (fof == null)
                return 0;
            else 
                return fof.fofId * multiplier;
        }
        private long GetNonFOFIdPrefix(int snap)
        {
            return (snap+1)*10000000000L * multiplier ;
        }

         /// <summary>
        /// Read and write snapshot files as ordered by Mik Boylan-Kolchin.
        /// Write IDs
        /// </summary>
        /// <param name="snap"></param>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        LittleFOF[] ReadLittleFOFs(String file)
        {
            List<String[]> rows = ReadGAVO_CSV(file+"\\fof.csv");
            LittleFOF[] fofs = new LittleFOF[rows.Count - 1];
            for (int i = 1; i < rows.Count; i++)
            {
                String[] words = rows[i];
                LittleFOF fof = new LittleFOF();
                fof.fofId = long.Parse(words[0]);
                fof.np = Int32.Parse(words[1]);
                fof.numSubs = Int32.Parse(words[2]);
                if(fof.numSubs > 0)
                    fof.subhalos = new LittleSubHalo[fof.numSubs];
                fofs[i - 1] = fof;
            }
            return fofs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        void ReadLittleSubHalos(String file, LittleFOF[] fofs)
        {
            if (fofs.Length == 0)
                return;
            List<String[]> rows = ReadGAVO_CSV(file + "\\subhalo.csv");
            LittleFOF currentFOF = fofs[0];
            int subhaloCount = 0;
            int ifof = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                String[] words = rows[i];
                LittleSubHalo sh = new LittleSubHalo();
                sh.subhaloId = long.Parse(words[0]);
                sh.np = Int32.Parse(words[1]);
                if (sh.fofId != currentFOF.fofId)
                {
                    while (true)
                    {
                        ifof++;
                        currentFOF = fofs[ifof];
                        if (currentFOF.fofId == sh.fofId)
                            break;
                        if (ifof >= fofs.Length)
                        {
                            throw new Exception("ReadLittleSubHalos: Invalid state, found subhalo without fof group");
                        }
                    }
                    subhaloCount = 0;
                }
                try
                {
                    currentFOF.subhalos[subhaloCount++] = sh;
                }
                catch (IndexOutOfRangeException ioe)
                {
                    DebugOut.PrintLine("Help");
                    throw ioe;
                }
            }
            // set first particle ids
            foreach (LittleFOF fof in fofs)
            {
                if (fof.numSubs == 0)
                    continue;
                long particleId = GetFOFIdPrefix(fof);
                foreach (LittleSubHalo subhalo in fof.subhalos)
                {
                    subhalo.firstParticleFofId = particleId;
                    particleId += subhalo.np;
                }

            }
        }

        /// <summary>
        /// Read a csv file and return list of string[].
        /// First array contains file names.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        List<String[]> ReadGAVO_CSV(string file)
        {
            List<String[]> rows = new List<String[]>();
            using (System.IO.StreamReader sr = System.IO.File.OpenText(file))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        string[] words = line.Split(new char[] { ',' });
                        rows.Add(words);
                    }
                }
            }
            return rows;
        }
        


    }
    class GroupProcess : Process
    {
        public short firstSnap = 0;
        public short lastSnap = 0;
        public bool writeFOFs = false;
        public bool writeSubHalos = false;
        public bool hasVelDisp = false;
        public bool writeSubhaloIDs = false;

        public string groupIdTable = "FOFId";
        public string fofTable = "FOF";
        public string subhaloTable = "Subhalo";
        public GroupProcess(GlobalParameters globals)
            : base(globals)
        { }
        public override void  Run()
        {
            for (short i = firstSnap; i <= lastSnap; i++)
            {
                if (isProcessing)
                    ProcessGroups(i);
                else
                {
                    DebugOut.PrintLine("Processing Groups interrupted at snapshot " + i);
                    break;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="previousFile"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        GroupTabFile NextGroupTabFile(GroupTabFile previousFile, short snap)
        {
            int ifile = (previousFile == null?0: previousFile.ifile+1);
            GroupTabFile groupFile = new GroupTabFile(GetGroupTab(inPath, snap, ifile), previousFile);
            return groupFile;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="previousFile"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        GroupFile NextGroupFile(List<GroupFile> files, GroupFile previousFile, short snap)
        {
            int ifile = (previousFile == null ? 0 : previousFile.ifile + 1);
            GroupFile groupFile = null;
            if (ifile < files.Count)
            {
                groupFile = files[ifile];
                if (groupFile == null)
                    DebugOut.PrintLine("ERROR Group file " + ifile + " was already removed");
            }
            if (groupFile == null)
            {
                groupFile = new GroupFile(this, GetSubTab(inPath, snap, ifile), snap, previousFile);
                files.Add(groupFile);
            }
            return groupFile;
        }

        /// <summary>
        /// Do not read all group files at once, as that seems to give memory problems.
        /// </summary>
        /// <param name="snap"></param>
        public void ProcessGroups(short snap)
        {
            DebugOut.PrintLine("PROCESSING GROUP " + snap +", optimised for low memory usage.");

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(GetGroupDir(outPath, snap));
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            List<GroupFile> openGroupFiles = new List<GroupFile>();

            GroupTabFile currentGroupTabFile = NextGroupTabFile(null, snap);
            GroupFile currentGroupFile = NextGroupFile(openGroupFiles, null, snap);
            GroupFile currentSubhaloFile = currentGroupFile;

            int numFiles = currentGroupFile.numFiles;
            int totNumGroups = currentGroupFile.totalGroups;
            int totNumSubgroups = currentGroupFile.totalSubgroups;
           
            GroupInfo currentGroup;
            SubhaloInfo currentSubhalo;
            
            int igroup = 0;
            int groupIndexInFile = 0;
            int nextSubhalo = 0;
            BinaryWriter groupwriter = null;
            if(writeFOFs)
                groupwriter = new BinaryWriter(new FileStream(GetGroupFile(outPath, snap), FileMode.Create));
            BinaryWriter subhalowriter = null;
            if(writeSubHalos)
                subhalowriter = new BinaryWriter(new FileStream(GetSubhaloFile(outPath, snap), FileMode.Create));
                
            for (igroup = 0; igroup < totNumGroups; igroup++)
            {
                while (!currentGroupFile.ContainsGroup(igroup))
                {
                    GroupFile previousGroupFile = currentGroupFile;
                    currentGroupFile = NextGroupFile(openGroupFiles, previousGroupFile, snap);
                    if (previousGroupFile.ifile < currentSubhaloFile.ifile) // previousGroupFile.lastSubhaloIndex < nextSubhalo)
                    {
                        WriteGroupFile(previousGroupFile, groupwriter, subhalowriter);
                        openGroupFiles[previousGroupFile.ifile] = null;
                    }
                }
                currentGroup = currentGroupFile.GetGroupByGlobalIndex(igroup);
                groupIndexInFile = currentGroupFile.GroupIndexInFile(igroup);
                currentGroup.fofId = MakeGroupID(snap, currentGroupFile.ifile, groupIndexInFile);
                while (!currentGroupTabFile.ContainsGroup(igroup))
                {
                    GroupTabFile previousGroupTabFile = currentGroupTabFile;
                    currentGroupTabFile = NextGroupTabFile(previousGroupTabFile, snap);
                }
                currentGroup.centerOfMassData = currentGroupTabFile.GetGroupByGlobalIndex(igroup);

                if (currentGroup.numSubs > 0)
                {
                    int rank = 0;
                    for (int isubhalo = currentGroup.firstSubIndex; isubhalo <= currentGroup.lastSubIndex; isubhalo++)
                    {
                        while (!currentSubhaloFile.ContainsSubhalo(isubhalo))
                        {
                            GroupFile previousSubhaloFile = currentSubhaloFile;
                            currentSubhaloFile = NextGroupFile(openGroupFiles, previousSubhaloFile, snap);
                            if (previousSubhaloFile.ifile < currentGroupFile.ifile)//previousSubhaloFile.lastGroupIndex < igroup)
                            {
                                WriteGroupFile(previousSubhaloFile, groupwriter, subhalowriter);
                                openGroupFiles[previousSubhaloFile.ifile] = null;
                            }                            
                        }
                        currentSubhalo = currentSubhaloFile.GetSubhaloByGlobalIndex(isubhalo);
                        // do more stuff
                        currentSubhalo.subhaloFileId = MakeSubhaloFileID(snap, currentSubhaloFile.ifile, currentSubhaloFile.SubhaloRank(isubhalo));
                        currentSubhalo.fofId = currentGroup.fofId;
                        currentSubhalo.subhaloFOFId = MakeSubhaloFOFID(currentGroup.fofId, rank);
                        if (rank == 0)
                            currentGroup.firstSubID = currentSubhalo.subhaloFOFId;
                        rank++;
                        nextSubhalo++;
                    }
                }
            }
            foreach (GroupFile f in openGroupFiles)
                if (f != null)
                    WriteGroupFile(f, groupwriter, subhalowriter);
            if (groupwriter != null)
            {
                groupwriter.Flush();
                groupwriter.Close();
                DebugOut.AddCommand(GetGroupFile(outPath, snap), fofTable);
                DebugOut.PrintLine("..wrote all " + totNumGroups + " FOF groups");
            }
            if (subhalowriter != null)
            {
                subhalowriter.Flush();
                subhalowriter.Close();
                DebugOut.AddCommand(GetSubhaloFile(outPath, snap), subhaloTable);
                DebugOut.PrintLine("..wrote all " + totNumSubgroups + " subhalo infos");
            }
        }

        void WriteGroupFile(GroupFile groupFile,BinaryWriter groupwriter,BinaryWriter subhalowriter )
        {
            if (groupFile.wasWritten)
            {
                DebugOut.PrintLine("ERROR " + groupFile.ifile + " was alrady written");
                return;
            }

            // a potential sort would go here, this must be done AFTER the groups's subhaloid-s have been set !
            if (writeFOFs)
            {
                for (int igroup = 0; igroup < groupFile.numGroups; igroup++)
                    groupFile.groups[igroup].WriteBinary(groupwriter, groupFile.hasVelDisp);
            }
            if (writeSubHalos)
            {
                for (int isubhalo = 0; isubhalo < groupFile.numSubgroups; isubhalo++)
                    groupFile.subhalos[isubhalo].WriteBinary(subhalowriter);
            }
            groupFile.wasWritten = true;

        }
    }

    class XXLGroupProcess : GroupProcess
    {
        public XXLGroupProcess(GlobalParameters globals)
            : base(globals)
        { }
        public override void Run()
        {
            for (short i = firstSnap; i <= lastSnap; i++)
            {
                if (isProcessing)
                    ProcessGroups(i);
                else
                {
                    DebugOut.PrintLine("Processing XXL Groups interrupted at snapshot " + i);
                    break;
                }
            }

        }

        /// <summary>
        /// Do not read all group files at once, as that seems to give memory problems.
        /// </summary>
        /// <param name="snap"></param>
        public new void ProcessGroups(short snap)
        {
            DebugOut.PrintLine("PROCESSING XXL GROUP " + snap + ", optimised for low memory usage.");

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(GetGroupDir(outPath, snap));
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            BinaryWriter groupwriter = null;
            if (writeFOFs)
                groupwriter = new BinaryWriter(new FileStream(GetGroupFile(outPath, snap), FileMode.Create));
            BinaryWriter subhalowriter = null;
            if (writeSubHalos)
                subhalowriter = new BinaryWriter(new FileStream(GetSubhaloFile(outPath, snap), FileMode.Create));
            int ifile = 0;
            int numFiles = 1;
            long totNumGroups = 0, totNumSubgroups = 0;
            long firstFofId = 0;
            while(ifile < numFiles)
            {
                XXLGroupFile currentGroupFile = new XXLGroupFile(this, inPath, ifile, snap, hasVelDisp);
                currentGroupFile.firstFofId = firstFofId;
                firstFofId += currentGroupFile.numGroups;
                numFiles = currentGroupFile.numFiles;

                currentGroupFile.WriteGroupFile(groupwriter, subhalowriter);
                ifile++;
                totNumGroups += currentGroupFile.numGroups;
                totNumSubgroups += currentGroupFile.numSubgroups;
                if (groupwriter != null)
                    DebugOut.PrintLine("..wrote " + currentGroupFile.numGroups + " FOF groups from file " + ifile);
                if (subhalowriter != null)
                    DebugOut.PrintLine("..wrote " + currentGroupFile.numSubgroups + " SubHalos from file " + ifile);
            }
            if (groupwriter != null)
            {
                groupwriter.Flush();
                groupwriter.Close();
                DebugOut.AddCommand(GetGroupFile(outPath, snap), fofTable);
                DebugOut.PrintLine("..wrote all " + totNumGroups + " FOF groups");
            }
            if (subhalowriter != null)
            {
                subhalowriter.Flush();
                subhalowriter.Close();
                DebugOut.AddCommand(GetSubhaloFile(outPath, snap), subhaloTable);
                DebugOut.PrintLine("..wrote all " + totNumSubgroups + " subhalo infos");
            }
        }


    }
    class HaloTreeProcess : Process
    {
        public String treesPrefix;
        public String treeIdsPrefix;
        public bool hasMainLeafId = false;
        public string haloTreeTable = "HaloTree";
        public int firstVolume = 0;
        public int lastVolume = 0;

        public HaloTreeProcess(GlobalParameters globals)
            : base(globals)
        {}
        public override void  Run()
        {
            for (int i = firstVolume; i <= lastVolume; i++)
            {
                if (isProcessing)
                    ProcessTreedata(i);
                else
                {
                    DebugOut.PrintLine("Processing HaloTrees interrupted at snapshot " + i);
                    break;
                }
            }
        }
        
        protected string GetTreesOutFile(string path, int vol)
        {
            return path + "\\halotrees."+ vol;
        }
        
        /// <summary>
        /// TODO update so that each tree is written immediately after the tree is read
        /// </summary>
        /// <param name="ivol"></param>
        public void  ProcessTreedata(int ivol)
        {
            DebugOut.PrintLine("PROCESSING TREEDATA " + ivol);

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(outPath);
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            // 
            string treesFile = inPath+"\\"+treesPrefix+ivol;
            string treeIdsFile = inPath+"\\"+treeIdsPrefix+ivol;
            string treesOutFile = GetTreesOutFile(outPath, ivol);

            TreedataFile tdf = new TreedataFile(this, treesFile, treeIdsFile);
            DebugOut.PrintLine("Opened " + treesFile);
            DebugOut.PrintLine(" numtot =  "+ tdf.numtot );
            DebugOut.PrintLine(" ntrees =  " + tdf.ntrees);
            DebugOut.PrintLine(" totnhalos =  " + tdf.totnhalos);

            // post-process
            int count = 0;
            int step = 1000000;
            int totnhalos = tdf.totnhalos;
            TreedataInfo[] trees = new TreedataInfo[step];

            using (BinaryWriter binwriter = new BinaryWriter(
                new FileStream(GetTreesOutFile(outPath, ivol), FileMode.Create)))
            {
                while (true)
                {
                    int numRead = tdf.next(trees);
                    count += numRead;
                    for (int i = 0; i < numRead; i++)
                        trees[i].WriteBinary(binwriter);
                    DebugOut.PrintLine("Read and wrote " + numRead + " tree halos");
                    if (numRead < step)
                        break;
                }

                DebugOut.PrintLine("Read and wrote total " + count + " tree halos (expected " + totnhalos+")");
            }
            tdf.Close();
            // and add a bulk insert
            DebugOut.AddCommand(GetTreesOutFile(outPath, ivol), haloTreeTable);
        }


    }

    class GalaxyTreeProcess : Process
    {
        public String galaxyFilePrefix = "SA_galtree_";
        public bool hasMainLeafId = false;
        public int firstVolume = 0;
        public int lastVolume = 0;
        public string galaxyTable = "Galaxy";
        public int nummag = 5;
        public GalaxyTreeProcess(GlobalParameters globals)
            : base(globals)
        {}
        public override void  Run()
        {
            for (int i = firstVolume; i <= lastVolume; i++)
            {
                if (isProcessing)
                    ProcessGalaxies(i);
                else
                {
                    DebugOut.PrintLine("Processing Galaxies interrupted at volume " + i);
                    break;
                }
            }
        }
        public void ProcessGalaxies(int ivol)
        {
            DebugOut.PrintLine("PROCESSING GALAXIES " + ivol);

            // and make the directory, just to be safe
            try
            {
                Directory.CreateDirectory(GetGalaxiesDir(outPath));
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

            // 
            string galaxiesFile = GetGalaxiesDir(inPath) + galaxyFilePrefix + ivol;
            string galaxiesOutFile = GetGalaxiesDir(outPath) + galaxyFilePrefix + ivol;

            GalaxiesFile galFile = new GalaxiesFile(galaxiesFile, nummag, hasMainLeafId);
            // post-process
            for (int i = 0; i < galFile.galaxies.Length; i++)
            {
                galFile.galaxies[i].ix = GetZone(galFile.galaxies[i].x);
                galFile.galaxies[i].iy = GetZone(galFile.galaxies[i].y);
                galFile.galaxies[i].iz = GetZone(galFile.galaxies[i].z);
                galFile.galaxies[i].PeanoKey = GetPHKey(galFile.galaxies[i].x, galFile.galaxies[i].y, galFile.galaxies[i].z);
                galFile.galaxies[i].RandomInt = globals.random.Next(globals.maxRandom);
            }
            // possible sort and calculation of mainleafid
            // ...
            // first sort
            Array.Sort(galFile.galaxies, new GalaxyComparator());

            // then walk tree to find mainLeafId
            if (!hasMainLeafId && false) // not yet
            {
                long next = 0;
                while (next < galFile.galaxies.Length)
                {
                    GalaxyInfo tree = galFile.galaxies[next];
                    CalculateMainLeafId(tree, tree, new GalaxyTrees(galFile.galaxies));
                    next += tree.LastProgenitorId - tree.GalID + 1;
                }
            }


            using (BinaryWriter binwriter = new BinaryWriter(
                new FileStream(GetGalaxiesOutFile(outPath, ivol), FileMode.Create)))
            {

                for (int i = 0; i < galFile.galaxies.Length; i++)
                    galFile.galaxies[i].WriteBinary(binwriter);
                // and add a bulk insert
                DebugOut.AddCommand(GetGalaxiesOutFile(outPath, ivol), galaxyTable);
            }
        }

    }


    
    class GlobalParameters
    {
        // redshifts
        public List<float> redshifts;

        public int phbits = 8;
        public int numzones = 20;
        public double box = 100;
        public double phboxinv;
        public double zoneinv;
        public int maxRandom;
        public Random random;


        /*public GlobalParameters(int _phbits, int _numZones, float _box, int _maxRandom
          , string redshiftFile, string sqlCommandsFile)*/

        public GlobalParameters(
            int _phbits, int _numZones, float _box, int _maxRandom
            , string sqlCommansdsFile)
        {
            phbits = _phbits;
            numzones = _numZones;
            box = _box;
            phboxinv = ((double)(1 << phbits)) / box;
            zoneinv = ((double)numzones) / box;

            maxRandom = _maxRandom;
            random = new Random();
            /*initRedshifts(redshiftFile);*/
            
        }
        /// <summary>
        /// Read redshifts as function of snapnum form file.
        /// </summary>
        /// <param name="file"></param>
        private void initRedshifts(string file)
        {
            redshifts = new List<float>();
            using (System.IO.StreamReader sr = System.IO.File.OpenText(file))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        string[] words = Regex.Split(line,"[\t ]+");
                        if (words.Length > 1)
                        {
                            float z = float.Parse(words[2].Trim());
                            redshifts.Add(z);
                        }
                    }
                }
            }
        }



    
    }
    abstract class Process 
    {
        public GlobalParameters globals;
        public Runner runner;
        // Runtime parameters
        public bool isProcessing
        {
            get { return runner.isProcessing; }
        }
        // Parameters
        private String InPath;
        private  String OutPath;

        public string inPath
        {
            get { return InPath; }
            set
            {
                InPath = value.TrimEnd('\\');
            }
        }

        public string outPath
        {
            get { return OutPath; }
            set
            {
                OutPath = value.TrimEnd('\\');
            }
        }

        public Process(GlobalParameters _globals)
        {
            globals = _globals;
        }
        public abstract void Run();
       
        public long CalculateMainLeafId(TreeInfo current, TreeInfo root, Trees trees)
        {
            if (current.LastProgenitorId == current.Id)
                current.MainLeafId = current.Id;
            else
            {
                TreeInfo next = trees.get(current.Id - root.Id + 1);
                current.MainLeafId = CalculateMainLeafId(next, root, trees);
                  
                while(next.LastProgenitorId < current.LastProgenitorId)
                {
                    next = trees.get(next.LastProgenitorId-root.Id+1);
                    CalculateMainLeafId(next, root, trees);
                }
            }
            return current.MainLeafId;
        }



        public long MakeSubhaloFileID(int snap, int file, int index)
        {
            return (snap * 10000L + file) * 1000000L + index;
        }
        public long MakeGroupID(int snap, int file, int index)
        {
            if (index > 1000000L)
                Console.Write("index too large!");
            return (snap * 10000L + file) * 1000000L + index;
        }
        public long MakeSubhaloFOFID(long fofId, int rank)
        {
            return (fofId * 1000000 + rank);
        }

        public int GetPHKey(double x, double y, double z)
        {
            int ix = (int)Math.Floor(x * globals.phboxinv);
            int iy = (int)Math.Floor(y * globals.phboxinv);
            int iz = (int)Math.Floor(z * globals.phboxinv);
            return PeanoHilbertID.GetPeanoHilbertID(globals.phbits, ix, iy, iz);
        }

        protected String FormatSnap(int snap)
        {
            if (snap < 10)
                return "00" + Convert.ToString(snap);
            if (snap < 100)
                return "0" + Convert.ToString(snap);
            return Convert.ToString(snap);
        }

        protected string GetSnapDir(string path, int snap)
        {
            return path + "\\snapdir_" + FormatSnap(snap);
        }

        protected string GetGroupDir(string path, int snap)
        {
            return path + "\\groups_" + FormatSnap(snap);
        }

        protected string GetGalaxiesDir(string path)
        {
            return path + "\\galaxies\\";
        }
        protected string GetGalaxiesOutFile(string path, int vol)
        {
            return GetGalaxiesDir(path) + "\\galaxies." + vol;
        }
        protected string GetGroupTab(string path, int snap, int file)
        {
            return GetGroupDir(path,snap) + "\\group_tab_" + FormatSnap(snap)
                + "." + file;
        }
        public string GetSubTab(string path, int snap, int file)
        {
            return GetGroupDir(path, snap) + "\\subhalo_tab_" + FormatSnap(snap)
                + "." + file;
        }
        protected string GetSubhaloFile(string path, int snap)
        {
            return GetGroupDir(path, snap) + "\\subhalos_" + FormatSnap(snap);
        }

        protected string GetGroupFile(string path, int snap)
        {
            return GetGroupDir(path, snap) + "\\fof_" + FormatSnap(snap);
        }

        protected string GetSubID(string path, int snap, int file)
        {
            return GetGroupDir(path, snap) + "\\subhalo_ids_" + FormatSnap(snap)
                + "." + file;
        }

        protected string GetSnapDefault(string path, int snap, string filePrefix, int file)
        {
            String s_snap = FormatSnap(snap);
            return path + "\\snapdir_" + s_snap + "\\" + filePrefix + s_snap +"."+file;
        }

        protected string GetSnapFile(string path, int snap, string prefix, int file)
        {
            DirectoryInfo dir = new DirectoryInfo(GetSnapDir(path, snap));
            // find file matching query, regardless of name
            FileInfo[] files;
            if(snap < 10)
                files = dir.GetFiles(prefix + "00" + snap + "." + file);
            else
                files = dir.GetFiles(prefix + "0"+snap+"."+file);
            if (files.Length > 1)
                throw new Exception("Too many files matching search in " + GetSnapDir(path, snap));
            if (files.Length == 0)
                throw new Exception("No files matching search in " + GetSnapDir(path, snap));
            return files[0].FullName;
        }
        protected string GetSimDBFOFFile(string path, int snap, string prefix, int file)
        {
            DirectoryInfo dir = new DirectoryInfo(GetSnapDir(path, snap));
            // find file matching query, regardless of name
            FileInfo[] files;
            if (snap < 10)
            {
                files = dir.GetFiles(prefix + "00" + snap + "." + file);
            }
            else
                files = dir.GetFiles(prefix + "0" + snap + "." + file);
            if (files.Length > 1)
                throw new Exception("Too many files matching search in " + GetSnapDir(path, snap));
            if (files.Length == 0)
                throw new Exception("No files matching search in " + GetSnapDir(path, snap));
            return files[0].FullName;
        }



        protected string GetHsmlPath(string path, int snap)
        {
            return path + "\\hsmldir_" + FormatSnap(snap) + "\\hsml_" + FormatSnap(snap);
        }

        public int GetZone(float x)
        {
            return (int)Math.Floor(x * globals.zoneinv);
        }

    }
}
