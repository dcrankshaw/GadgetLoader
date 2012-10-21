using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Jhu.SqlServer.Array;
using System.Data.SqlTypes;
using System.Diagnostics;
using GadgetLoader;

namespace BoundingBox
{
    //Creates the Bounding Boxes for a given FOFGroup
    class BoundingBox
    {
        float padding;
        double phboxinv;
        int phbits;
        float box;
        string server;
        string database;
        short snapshot;
        int fofID;
        List<Range> xRanges;
        List<Range> yRanges;
        List<Range> zRanges;
        public int numRepeats;
        public bool overFlow;
        public StreamWriter writer;

        public BoundingBox(string svr, string db, short ts, int fid, StreamWriter w, float pad)
        {
            padding = pad;
            numRepeats = 0;
            phbits = 6;
            box = 1000f;
            phboxinv = ((double)(1 << phbits)) / box;
            xRanges = new List<Range>();
            yRanges = new List<Range>();
            zRanges = new List<Range>();
            server = svr;
            database = db;
            snapshot = ts;
            fofID = fid;
            overFlow = false;
            writer = w;
            GetBoundingBoxBounds();
        }

        private int GetPHKey(double x, double y, double z)
        {
            int ix = (int)Math.Floor(x * phboxinv);
            int iy = (int)Math.Floor(y * phboxinv);
            int iz = (int)Math.Floor(z * phboxinv);
            int id = PeanoHilbertID.GetPeanoHilbertID(phbits, ix, iy, iz);
            /*int fx, fy, fz;
            PeanoHilbertID current = new PeanoHilbertID();
            current.GetPosition(phbits, id, out fx, out fy, out fz);
            bool match = false;
            if (ix == fx && iy == fy && iz == fz)
            {
                match = true;
            }
            //("(x,y,z)\t(ix,iy,iz)\tID\t(fx,fy,fz)");
            writer.WriteLine("({0},{1},{2}\t({3},{4},{5})\t{6}\t({7},{8},{9}\t{10})", x, y, z, ix, iy, iz, id, fx, fy, fz, match);*/
            return id;
        }

        //creates the list of Peano-Hilbert Cells that this bounding box intersects
        public SortedSet<int> GetPHCells()
        {
            SortedSet<int> cells = new SortedSet<int>();
            float oneThird = 1f / 3f;
            float cellLength = (float)Math.Pow(Math.Pow(box, 3) / Math.Pow(8, phbits), oneThird);
            foreach (Range zr in zRanges)
            {
                for (float z = zr.beginning; z <= zr.end; z += cellLength)
                {
                    foreach (Range yr in yRanges)
                    {
                        for (float y = yr.beginning; y <= yr.end; y += cellLength)
                        {
                            foreach (Range xr in xRanges)
                            {
                                for (float x = xr.beginning; z <= xr.end; x += cellLength)
                                {
                                    if (!cells.Add(GetPHKey(x, y, z)))
                                    {
                                        numRepeats++;
                                    }
                                }
                            }

                        }

                    }

                }

            }
            return cells;
        }

        public static string createConnString(string db, string server)
        {
            return "server=" + server + ";database=" + db + ";Trusted_Connection=true;Asynchronous Processing = true";
        }

        /*
         *      string command = "select * from dbo.config where sim = @sim and snapnum = @snapnum";
                SqlCommand myCommand = new SqlCommand(command, configConn);
                SqlParameter timestepParam = new SqlParameter("@snapnum", System.Data.SqlDbType.SmallInt);
                SqlParameter simParam = new SqlParameter("@sim", System.Data.DbType.String);
                timestepParam.Value = opts.timestepInitial;
                simParam.Value = opts.sim;
                myCommand.Parameters.Add(timestepParam);
                myCommand.Parameters.Add(simParam);
         */


        private void GetBoundingBoxBounds()
        {
            try
            {
                SqlConnection conn = new SqlConnection(createConnString(database, server));
                conn.Open();

                SqlDataReader myReader = null;
                string getFoFPartidList = "select x, y, z from SimDBdev.dbo.getinitposFoFGroup(@snap, @fof)";
                SqlCommand partidListCommand = new SqlCommand(getFoFPartidList, conn);
                SqlParameter timestepParam = new SqlParameter("@snap", System.Data.SqlDbType.SmallInt);
                timestepParam.Value = snapshot;
                SqlParameter fofParam = new SqlParameter("@fof", System.Data.SqlDbType.Int);
                fofParam.Value = fofID;
                partidListCommand.Parameters.Add(timestepParam);
                partidListCommand.Parameters.Add(fofParam);
                myReader = partidListCommand.ExecuteReader();
                //float maxX = -1f, maxY = -1f, maxZ = -1f, minX = 1001f, minY = 1001f, minZ = 1001f;
                List<float> XCoords = new List<float>();
                List<float> YCoords = new List<float>();
                List<float> ZCoords = new List<float>();
                int numLines = 0;
                while (myReader.Read())
                {
                    float curX = float.Parse(myReader["x"].ToString());
                    XCoords.Add(curX);
                    float curY = float.Parse(myReader["y"].ToString());
                    YCoords.Add(curY);
                    float curZ = float.Parse(myReader["z"].ToString());
                    ZCoords.Add(curZ);
                    numLines++;
                }
                conn.Close();
                if (numLines == 0)
                {
                    overFlow = true;
                }
                else
                {
                    // add padding to bounding box to account for particle movement
                    GetMinAndMaxForDimension(XCoords, xRanges);
                    GetMinAndMaxForDimension(YCoords, yRanges);
                    GetMinAndMaxForDimension(ZCoords, zRanges);
                }


            }
            catch (Exception e)
            {
                throw new Exception("Bad data in DB " + e.Message);
            }
        }


        // Partitions the coords into either one or two ranges, then adds the ranges into
        // provided list
        private void GetMinAndMaxForDimension(List<float> coords, List<Range> dimension)
        {
            coords.Sort();
            float maxDist = 0.0f;
            int smallCoordIndex = -1;
            int bigCoordIndex = -1;
            for (int i = 0; i < coords.Count() - 1; ++i)
            {
                float dist = coords[i + 1] - coords[i];
                if (dist > maxDist) {
                    maxDist = dist;
                    smallCoordIndex = i;
                    bigCoordIndex = i + 1;
                }
            }
            // Need to compare the first and last values also
            // This is the wrap around logic
            float wraparoundDist = box - coords[coords.Count() - 1] + coords[0];
            // This means we can use the naive BBox form
            if (wraparoundDist > maxDist)
            {
                if ((coords[0] - padding) < 0)
                {
                    Range firstRange = new Range(0.0f, coords[coords.Count() - 1] + padding);
                    Range secondRange = new Range(box + coords[0] - padding, box);
                    dimension.Add(firstRange);
                    dimension.Add(secondRange);
                }
                else if ((coords[coords.Count() - 1] + padding) > box)
                {
                    Range firstRange = new Range(0.0f, coords[coords.Count() - 1] + padding - box);
                    Range secondRange = new Range(coords[0] - padding, box);
                    dimension.Add(firstRange);
                    dimension.Add(secondRange);
                }
                else
                {
                    Range range = new Range(coords[0] - padding, coords[coords.Count() - 1] + padding);
                    dimension.Add(range);
                }

            }
            // we need to create two bounding boxes
            else
            {
                Range firstRange = new Range(0.0f, coords[smallCoordIndex] + padding);
                dimension.Add(firstRange);
                Range secondRange = new Range(coords[bigCoordIndex] - padding, box);
                dimension.Add(secondRange);
                                
            }

        }

/*        private void GetPosList()
        {
            try
            {
                SqlConnection conn = new SqlConnection(createConnString(database, server));
                conn.Open();

                SqlDataReader myReader = null;
                string getFoFPartidList = "select x, y, z from SimDBdev.dbo.getinitposFoFGroup(@snap, @fof)";
                SqlCommand partidListCommand = new SqlCommand(getFoFPartidList, conn);
                SqlParameter timestepParam = new SqlParameter("@snap", System.Data.SqlDbType.SmallInt);
                timestepParam.Value = snapshot;
                SqlParameter fofParam = new SqlParameter("@fof", System.Data.SqlDbType.Int);
                fofParam.Value = fofID;
                partidListCommand.Parameters.Add(timestepParam);
                partidListCommand.Parameters.Add(fofParam);
                myReader = partidListCommand.ExecuteReader();
                //float maxX = -1f, maxY = -1f, maxZ = -1f, minX = 1001f, minY = 1001f, minZ = 1001f;
                int numLines = 0;
                while (myReader.Read())
                {
                    float curX = float.Parse(myReader["x"].ToString());
                    float curY = float.Parse(myReader["y"].ToString());
                    float curZ = float.Parse(myReader["z"].ToString());
                    if (curX < minX)
                        minX = curX;
                    if (curX > maxX)
                        maxX = curX;
                    if (curY < minY)
                        minY = curY;
                    if (curY > maxY)
                        maxY = curY;
                    if (curZ < minZ)
                        minZ = curZ;
                    if (curZ > maxZ)
                        maxZ = curZ;
                    numLines++;
                }
                conn.Close();
                if (numLines == 0)
                {
                    overFlow = true;
                }
                else
                {
                    // add padding to bounding box to account for particle movement
                    minX = minX - padding;
                    minX = minX > 0f ? minX : 0f;
                    minY = minY - padding;
                    minY = minY > 0f ? minY : 0f;
                    minZ = minZ - padding;
                    minZ = minZ > 0f ? minZ : 0f;

                    maxX = maxX + padding;
                    maxX = maxX < box ? maxX : 1000f;
                    maxY = maxY + padding;
                    maxY = maxY < box ? maxY : 1000f;
                    maxZ = maxZ + padding;
                    maxZ = maxZ < box ? maxZ : 1000f;
                }


            }
            catch (Exception e)
            {
                throw new Exception("Bad data in DB " + e.Message);
            }
        }
        */

    }

    struct Range
    {
        //inclusive
        public float beginning;
        //inclusive
        public float end;

        public Range(float b, float e)
        {

            beginning = b;
            end = e;
        }
    }
}
