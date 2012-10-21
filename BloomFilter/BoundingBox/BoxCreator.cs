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
    class BoxCreator
    {
        string server { get; set; }
        string database { get; set; }
        short timestepInitial { get; set; }
        short timestepFinal { get; set; }
        int minFofID { get; set; }
        int maxFofID { get; set; }
        short timestepDelta { get; set; }
        string logFileName { get; set; }
        float padlevel { get; set; }


        public BoxCreator(string svr, string db, short startTS, short endTS, int startID, int endID, float pad)
        {
            server = svr;
            database = db;
            timestepInitial = startTS;
            timestepFinal = endTS;
            minFofID = startID;
            maxFofID = endID;
            padlevel = pad;
            if (maxFofID < 0)
            {
                maxFofID = Int32.MaxValue;
            }

            timestepDelta = 10;
            logFileName = "D:\\loader_testing\\phcells_padlevel_" + (int) padlevel + ".txt";
            //logFileName = "D:\\loader_testing\\ph_inverse_test.txt";
        }

        public void createBoxes()
        {
            using (StreamWriter writer = new StreamWriter(logFileName, false))
            {
                //writer.WriteLine("(x,y,z)\t(ix,iy,iz)\tID\t(fx,fy,fz)");
                DateTime start = DateTime.Now;
                int numFofGroups = 0;

                for (short t = timestepInitial; t <= timestepFinal; t += timestepDelta)
                {
                    bool done = false;
                    for (int i = minFofID; i <= maxFofID && !done; i++, numFofGroups++)
                    {
                        BoundingBox current = new BoundingBox(server, database, t, i, writer, padlevel);
                        if (current.overFlow == true)
                        {
                            done = true;
                            numFofGroups--;
                        }
                        else
                        {
                            SortedSet<int> cells = current.GetPHCells();
                            SqlIntArrayMax sqlCellArray = SqlIntArrayMax.FromArray(cells.ToArray<int>());
                            SqlBytes b = sqlCellArray.ToSqlBuffer();
                            string queryStatement = "INSERT INTO dbo.fofgroupsBoundingBox" + (int)Math.Round(padlevel) + "(snapnum, fofID, numcells, phcells) VALUES(@snap, @fof, @numcells, @cells)";
                            using (SqlConnection conn = new SqlConnection(BoundingBox.createConnString(database, server)))
                            using (SqlCommand cmd = new SqlCommand(queryStatement, conn))
                            {
                                SqlParameter snapParam = cmd.Parameters.Add("@snap", System.Data.SqlDbType.SmallInt);
                                snapParam.Value = t;
                                SqlParameter fofParam = cmd.Parameters.Add("@fof", System.Data.SqlDbType.Int);
                                fofParam.Value = i;
                                SqlParameter cellParam = cmd.Parameters.Add("@cells", System.Data.SqlDbType.VarBinary);
                                cellParam.Value = b;
                                SqlParameter numCellParam = cmd.Parameters.Add("@numcells", System.Data.SqlDbType.Int);
                                numCellParam.Value = cells.Count;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                                conn.Close();

                            }

                        }
                        /*writer.WriteLine("Snap: {0}\tFoFID: {1}\tRepeats: {2}\tNum Cells: {3}", timestepInitial, i, current.numRepeats, cells.Count);
                        foreach (int cell in cells)
                        {
                            writer.WriteLine(cell);
                        }
                        */
                    }
                    Console.Out.WriteLine("Padding {0}, Timestep {1} created", padlevel, t);
                }
                DateTime end = DateTime.Now;
                TimeSpan duration = end - start;
                writer.WriteLine("Processing Time: {0}", duration.TotalSeconds);
                writer.WriteLine("{0} FoF Groups processed", numFofGroups);
            }   
        }
    }
}
