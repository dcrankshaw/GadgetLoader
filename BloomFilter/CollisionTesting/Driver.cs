using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BloomFilter;
using GadgetLoader;
using System.Threading;

namespace CollisionTesting
{

    class CollideFile
    {
        int j;
        short snap;
        String prefix;
        String halopath;
        public List<DataPoint> datapoints;

        public CollideFile(int fileNumber)
        {
            j = fileNumber;
            snap = 45;
            prefix = "\\\\hhpc-hn7\\wangjie\\snapdir_045\\snapshot_045.";
            halopath = "H:\\scripts\\snap_45";
            datapoints = initDataPoints(halopath); //figure out halo path
        }

        public void startColliding()
        {
            String filename = prefix + j;
            SnapFile curSnap = new SnapFile(filename);
            Console.Out.WriteLine("Processing file " + j);
            Console.Out.Flush();
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


            }
            // now sort before writing files
            // TODO
            //TBD this may not be necessary if the particles 
            // are sorted in the files already.
            Array.Sort<Structs>(parts, new ParticleComparator());

            Cell cell = new Cell(snap);
            int currentPHkey = -1;
            
            //for (int i = 0; i < 10000; i++)
            for (int i = 0; i < curSnap.numSample; i++)
            {
                if (parts[i].phkey != currentPHkey)
                {
                    if (cell.Count > 0)
                        processCell(cell, datapoints);
                    currentPHkey = parts[i].phkey;
                    cell.Init(currentPHkey);
                }
                cell.AddToCell(parts[i]);
                if (i % 100000 == 0)
                {
                    Console.Out.WriteLine("Particle " + i + " from file " + j);
                    Console.Out.Flush();
                }

            }
            if (cell.Count > 0)
                processCell(cell, datapoints);
        }

        public static int GetPHKey(double x, double y, double z)
        {
            int phbits = 6;
            float box = 1000;
            double phboxinv = ((double)(1 << phbits)) / box;
            int ix = (int)Math.Floor(x * phboxinv);
            int iy = (int)Math.Floor(y * phboxinv);
            int iz = (int)Math.Floor(z * phboxinv);
            return PeanoHilbertID.GetPeanoHilbertID(phbits, ix, iy, iz);
        }

        public void processCell(Cell currentCell, List<DataPoint> datapoints)
        {
            long[] cellIds = currentCell.IdToArray();
            foreach (DataPoint current in datapoints)
            {
                Filter<long> cellFilter = new Filter<long>(2, 0.5f, null, current.filterSize, current.hashFunctions);
                for (int i = 0; i < cellIds.Length; i++)
                {
                    cellFilter.Add(cellIds[i]);
                }
                current.testFilter(cellFilter);
            }
        }

        //read in halos from file
        public List<DataPoint> initDataPoints(string haloPath)
        {
            List<List<long>> haloList = new List<List<long>>();
            using (TextReader reader = new StreamReader(haloPath))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    line.Trim();
                    string del = " ";
                    char[] delim = del.ToCharArray();
                    string[] ids = line.Split(delim);
                    List<long> current = new List<long>();
                    foreach(string id in ids)
                    {
                        current.Add(long.Parse(id));
                    }
                    haloList.Add(current);
                }
            }

            List<DataPoint> datapoints = new List<DataPoint>();
            for (int size = 10000; size <= 64000; size += 10000)
            {
                for (int hashes = 5; hashes <= 30; hashes += 5)
                {
                    datapoints.Add(new DataPoint(hashes, size, haloList));
                }
            }

            return datapoints;
        }

    }
    
    class Driver
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.UtcNow;
            Dictionary<int, CollideFile> colliders = new Dictionary<int, CollideFile>();
            Dictionary<int, Thread> threads = new Dictionary<int, Thread>();
            Dictionary<Int32, Tuple<long, double>> totals = new Dictionary<int,Tuple<long, double>>();

            for (int size = 10000; size <= 64000; size += 10000)
            {
                for (int hashes = 5; hashes <= 30; hashes += 5)
                {
                    int key = size + hashes;
                    totals[key] = new Tuple<long, double>(0, 0);
                }
            }

            Console.Out.WriteLine("Starting threads");
            int outer = 16;
            int inner = 2;
            for (int i = 0; i < outer; i++)
            {
                for (int j = 0; j < inner; j++)
                {
                    int fileNum = inner * i + j;
                    CollideFile currentCollider = new CollideFile(fileNum);
                    colliders[fileNum] = currentCollider;
                    Thread currentThread = new Thread(new ThreadStart(currentCollider.startColliding));
                    threads[fileNum] = currentThread;
                    currentThread.Start();
                }
                for (int j = 0; j < inner; j++)
                {
                    int fileNum = inner * i + j;
                    threads[fileNum].Join();
                    CollideFile currentCollider = colliders[fileNum];
                    foreach (DataPoint d in currentCollider.datapoints)
                    {
                        int key = d.filterSize + d.hashFunctions;
                        Tuple<long, double> prev = totals[key];
                        totals[key] = new Tuple<long, double>(prev.Item1 + d.sum, prev.Item2 + d.numIntersects);
                    }
                }
                colliders.Clear();
                threads.Clear();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Console.Out.WriteLine("Round " + (i+1) + " of files processed");
                Console.Out.Flush();
            }
            Console.Out.WriteLine("Threads Complete");

            DateTime end = DateTime.UtcNow;
            
            using (StreamWriter writer = new StreamWriter("H:\\scripts\\filter_testing_results.txt", false))
            {
                writer.WriteLine("Hashes\t Size\t Mean Collisions");
                Console.Out.WriteLine("Hashes\t Size\t Mean Collisions");
                foreach (int key in totals.Keys)
                {
                    int hashes = key % 10000;
                    int filtersize = (key / 10000) * 10000;
                    double mean = totals[key].Item1 / totals[key].Item2;
                    String finalResult =  hashes + "\t " + filtersize + "\t " + mean;
                    Console.Out.WriteLine(finalResult);
                    writer.WriteLine(finalResult);

                }
                String elapsedTime = "Processing Time: " + end.Subtract(start).ToString();
                Console.Out.WriteLine(elapsedTime);
                writer.WriteLine(elapsedTime);
            }
        }

       
    }
}
