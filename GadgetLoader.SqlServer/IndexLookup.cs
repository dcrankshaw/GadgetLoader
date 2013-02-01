using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Jhu.SqlServer.Array;

public partial class UserDefinedFunctions
{

    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "AddSlotsToPhkeyFillRow",
        TableDefinition = "phkey INT, slots VARBINARY(8000)")]
    public static IEnumerable IndexLookup(SqlBytes phkeyBinary, SqlBytes slotBinary) //, out long outId, out SqlBinary finalFilter, out int hashFunctions, out int expectedCap)
    {

        List<PHkeyAndSlots> results = new List<PHkeyAndSlots>();
        int[] phkeys = (new SqlIntArrayMax(phkeyBinary)).ToArray();
        Int16[] slotsShort = (new SqlSmallIntArrayMax(slotBinary)).ToArray();
        Int32[] slots = new Int32[slotsShort.Length];
        for (int i = 0; i < slotsShort.Length; ++i)
        {
            slots[i] = (Int32) slotsShort[i];
        }
        if (phkeys.Length != slots.Length)
            throw new Exception("Different number of slots and phkeys");
        if (phkeys.Length == 0)
            throw new Exception("provided phkey and slot arrays are empty");

        if (phkeys.Length == 1)
        {
            results.Add(new PHkeyAndSlots(phkeys[0], new List<Int32>(slots)));
            return results;
        }
        List<Int32> currentSlots = new List<Int32>();
        currentSlots.Add((Int32) slots[0]);
        int currentPhkey = phkeys[0];
        for (int i = 1; i < phkeys.Length; ++i)
        {
            if (phkeys[i] != currentPhkey)
            {
                results.Add(new PHkeyAndSlots(currentPhkey, currentSlots));
                currentSlots.Clear();
                currentPhkey = phkeys[i];
            }
            currentSlots.Add(slots[i]);
        }
        return results;
    }

    public static void AddSlotsToPhkeyFillRow(Object obj, out SqlInt32 phkey, out SqlBytes slots)
    {
        PHkeyAndSlots row = (PHkeyAndSlots) obj;
        phkey = row.phkey;
        //slots = SqlIntArrayMax.FromArray(row.slots.ToArray()).ToSqlBuffer();
        SqlIntArrayMax slot1d = SqlIntArrayMax.FromArray(row.slots.ToArray());
        int[] lengths = {1, row.slots.Count};
        slot1d.Reshape(lengths);
        slots = slot1d.ToSqlBuffer();
    }

    private class PHkeyAndSlots
    {
        public int phkey;
        public List<Int32> slots;

        public PHkeyAndSlots(int ph, List<Int32> s)
        {
            phkey = ph;
            slots = new List<Int32>(s);
        }
    }

    ////////////////////////////////////////////////////////////////////////



    /***********************************************************************
     * Find positions of all particles in a specified fof group at a specified time
     * OUTLINE
        Get array listing all particle ids of index
        Compute which index tables we need to look in
        construct id_pos map (that will be filled out below) of id -> x, y, z pos components
        for each index table:
            join partids from halo with index table on partid where snapnum = particle snapnum
            create map of phkey -> list of relevant slots in that phkey (sorted?)
            for each phkey in map:
                get id array, pos array
                look up specified slots in id array
                look up specified slots in pos array (how to do with 2d array?)
        return id_pos map
    ************************************************************************/
    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "AddSlotsToPhkeyFillRow",
        TableDefinition = "phkey INT, slots VARBINARY(8000)")]
    public static IEnumerable IndexLookup2(SqlInt16 partsnap, SqlInt16 fofsnap, SqlInt16 fofID)
    {
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();
            string command = "select partids from SimulationDB.dbo.FoFGroups where snapnum = @fofsnap and fofID = @fofgroup";
            SqlCommand getFOFGroupIDsCommand = new SqlCommand(command, connection);
            SqlParameter snapParam = new SqlParameter("@fofsnap", System.Data.SqlDbType.SmallInt);
            snapParam.Value = fofsnap;
            SqlParameter fofidParam = new SqlParameter("@fofgroup", System.Data.SqlDbType.SmallInt);
            snapParam.Value = fofID;
            getFOFGroupIDsCommand.Parameters.Add(snapParam);
            getFOFGroupIDsCommand.Parameters.Add(fofidParam);
            SqlDataReader getFOFGroupIDsReader = getFOFGroupIDsCommand.ExecuteReader();
            Int64[] partids = null;
            int numlines = 0;
            while (getFOFGroupIDsReader.Read())
            {
                partids = (new SqlBigIntArrayMax((SqlBytes)getFOFGroupIDsReader["partids"]).ToArray();
                ++numlines;
            }
            Debug.Assert(numlines == 1);
            HashSet<Int64> blocks = new HashSet<Int64>();
            foreach (Int64 id in partids)
            {
                blocks.Add((id - 1) / 1024 / 1024);
            }
            // Id -> (component -> value)
            Dictionary<long,Dictionary<string,float>> idPositionMap = new Dictionary<long,Dictionary<string,float>>();
            foreach (Int64 blockID in blocks)
            {
                command = "select partids from SimulationDB.dbo.FoFGroups where snapnum = @fofsnap and fofID = @fofgroup";
                /*(SqlCommand getFOFGroupIDsCommand = new SqlCommand(command, connection);
                SqlParameter snapParam = new SqlParameter("@fofsnap", System.Data.SqlDbType.SmallInt);
                snapParam.Value = fofsnap;
                SqlParameter fofidParam = new SqlParameter("@fofgroup", System.Data.SqlDbType.SmallInt);
                snapParam.Value = fofID;
                getFOFGroupIDsCommand.Parameters.Add(snapParam);
                getFOFGroupIDsCommand.Parameters.Add(fofidParam);
                SqlDataReader getFOFGroupIDsReader = getFOFGroupIDsCommand.ExecuteReader();
                Int64[] partids = null;
                int numlines = 0;
                while (getFOFGroupIDsReader.Read())
                {
                    partids = (new SqlBigIntArrayMax((SqlBytes)getFOFGroupIDsReader["partids"]).ToArray();
                    ++numlines;
                }*/
            }

        }
    }


};
