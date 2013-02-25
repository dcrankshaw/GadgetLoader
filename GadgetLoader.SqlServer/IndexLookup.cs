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

//    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "AddSlotsToPhkeyFillRow",
//        TableDefinition = "phkey INT, slots VARBINARY(8000)")]
//    public static IEnumerable IndexLookup(SqlBytes phkeyBinary, SqlBytes slotBinary) //, out long outId, out SqlBinary finalFilter, out int hashFunctions, out int expectedCap)
//    {
//
//        List<PHkeyAndSlots> results = new List<PHkeyAndSlots>();
//        int[] phkeys = (new SqlIntArrayMax(phkeyBinary)).ToArray();
//        Int16[] slotsShort = (new SqlSmallIntArrayMax(slotBinary)).ToArray();
//        Int32[] slots = new Int32[slotsShort.Length];
//        for (int i = 0; i < slotsShort.Length; ++i)
//        {
//            slots[i] = (Int32) slotsShort[i];
//        }
//        if (phkeys.Length != slots.Length)
//            throw new Exception("Different number of slots and phkeys");
//        if (phkeys.Length == 0)
//            throw new Exception("provided phkey and slot arrays are empty");
//
//        if (phkeys.Length == 1)
//        {
//            results.Add(new PHkeyAndSlots(phkeys[0], new List<Int32>(slots)));
//            return results;
//        }
//        List<Int32> currentSlots = new List<Int32>();
//        currentSlots.Add((Int32) slots[0]);
//        int currentPhkey = phkeys[0];
//        for (int i = 1; i < phkeys.Length; ++i)
//        {
//            if (phkeys[i] != currentPhkey)
//            {
//                results.Add(new PHkeyAndSlots(currentPhkey, currentSlots));
//                currentSlots.Clear();
//                currentPhkey = phkeys[i];
//            }
//            currentSlots.Add(slots[i]);
//        }
//        return results;
//    }
//
//    public static void AddSlotsToPhkeyFillRow(Object obj, out SqlInt32 phkey, out SqlBytes slots)
//    {
//        PHkeyAndSlots row = (PHkeyAndSlots) obj;
//        phkey = row.phkey;
//        //slots = SqlIntArrayMax.FromArray(row.slots.ToArray()).ToSqlBuffer();
//        SqlIntArrayMax slot1d = SqlIntArrayMax.FromArray(row.slots.ToArray());
//        int[] lengths = {1, row.slots.Count};
//        slot1d.Reshape(lengths);
//        slots = slot1d.ToSqlBuffer();
//    }

    private struct PHkeyAndSlots
    {
        public int phkey;
        public List<Int32> slots;

        public PHkeyAndSlots(int ph, List<Int32> s)
        {
            phkey = ph;
            slots = new List<Int32>(s);
        }
    }

    public static void ReturnPosDataFillRow(Object obj, out SqlInt64 partid, out SqlSingle x, out SqlSingle y, out SqlSingle z)
    {
        PosData current = (PosData)obj;
        partid = current.id;
        x = current.x;
        y = current.y;
        z = current.z;
    }

    public static void ReturnPosDataWithSnapFillRow(Object obj, out SqlInt64 partid, out SqlInt16 snap, out SqlSingle x, out SqlSingle y, out SqlSingle z)
    {
        PosDataWithSnap current = (PosDataWithSnap)obj;
        partid = current.id;
        snap = current.snap;
        x = current.x;
        y = current.y;
        z = current.z;
    }

    ////////////////////////////////////////////////////////////////////////

    private struct PosData
    {
        public long id;
        public float x;
        public float y;
        public float z;

        public PosData(long _id, float _x, float _y, float _z)
        {
            id = _id;
            x = _x;
            y = _y;
            z = _z;
        }
    }

    private class PosDataWithSnap
    {
        public short snap;
        public long id;
        public float x;
        public float y;
        public float z;

        public PosDataWithSnap(short _snap, long _id, float _x, float _y, float _z)
        {
            snap = _snap;
            id = _id;
            x = _x;
            y = _y;
            z = _z;
        }
    }

    private struct PhkeySlot
    {
        public int snap;
        public int phkey;
        public int slot;

        public PhkeySlot(int sn, int p, int s)
        {
            snap = sn;
            phkey = p;
            slot = s;
        }

    }

    public static void ReturnPhkeySlotData(object obj, out int snap, out int phkey, out int slot)
    {
        PhkeySlot current = (PhkeySlot)obj;
        snap = current.snap;
        phkey = current.phkey;
        slot = current.slot;
    }


    /***********************************************************************
     * Find positions of all particles in a specified fof group at a specified time
     * OUTLINE
        Get array listing all particle ids of index
        Compute which index tables we need to look in
        construct id_pos map (that will be filled out below) of id -> x, y, z pos components
        for each index table:
            join partids from fofgroup with index table on partid where snapnum = particle_snapnum
            create map of phkey -> list of relevant slots in that phkey (sorted?)
            for each phkey in map:
                get id array, pos array
                look up specified slots in id array
                look up specified slots in pos array (how to do with 2d array?)
        return id_pos map
    ************************************************************************/
    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "ReturnPhkeySlotData",
        TableDefinition = "snap INT, phkey INT, slot INT", DataAccess=DataAccessKind.Read)]
    public static IEnumerable FindPHkeysAndSlots(SqlInt16 partsnap, SqlInt16 fofsnap, SqlString temp_partid_table_name, SqlInt32 numparts)
    {
        //string temp_partid_table = "#temp_partid_list";
        List<PhkeySlot> keysAndSlots = new List<PhkeySlot>();
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();
            string getFoFGroupIDsCommandString = "select partid from " + temp_partid_table_name.ToString();
            SqlCommand getFoFGroupIDsListCommand = new SqlCommand(getFoFGroupIDsCommandString, connection);
            SqlDataReader getFOFGroupIDsReader = getFoFGroupIDsListCommand.ExecuteReader();
            Int64[] partids = new Int64[(int) numparts];
            int numlines = 0;
            while (getFOFGroupIDsReader.Read())
            {
                partids[numlines] = Int64.Parse(getFOFGroupIDsReader["partid"].ToString());
                ++numlines;
            }
            getFOFGroupIDsReader.Close();
            // Find the index blocks we need to look in
            HashSet<Int64> blocks = new HashSet<Int64>();
            foreach (Int64 id in partids)
            {
                blocks.Add((id - 1) / 1024 / 1024);
            }

            //means we want all snaps
            string rawCommand = "";
            if (partsnap < 0)
            {
                rawCommand = "select a.snap, a.phkey, a.slot from particleDB.dbo.index{0} a, " + temp_partid_table_name.ToString() + " b "
                    + " where a.partid = b.partid";
            }
            else
            {
                rawCommand = "select a.snap, a.phkey, a.slot from particleDB.dbo.index{0} a, " + temp_partid_table_name.ToString() + " b "
                    + " where a.partid = b.partid and a.snap = @partsnap";
            }
            
            

            foreach (Int64 blockID in blocks)
            {
                string joinFoFGroupIndexCommandString = string.Format(rawCommand, blockID);
                SqlCommand joinFofGroupIndexCommand = new SqlCommand(joinFoFGroupIndexCommandString, connection);
                SqlParameter partsnapParam = new SqlParameter("@partsnap", System.Data.SqlDbType.SmallInt);
                partsnapParam.Value = partsnap;
                joinFofGroupIndexCommand.Parameters.Add(partsnapParam);
                SqlDataReader joinReader = joinFofGroupIndexCommand.ExecuteReader();
                while (joinReader.Read())
                {
                    int snap = Int32.Parse(joinReader["snap"].ToString());
                    int phkey = Int32.Parse(joinReader["phkey"].ToString());
                    short slot = Int16.Parse(joinReader["slot"].ToString());
                    PhkeySlot current = new PhkeySlot(snap, phkey, slot);
                    keysAndSlots.Add(current);
                }
                joinReader.Close();
            }
        }
        return keysAndSlots;
    }

    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "ReturnPosDataFillRow",
        TableDefinition = "partid BIGINT, x REAL, y REAL, z REAL", DataAccess = DataAccessKind.Read)]
    public static IEnumerable LookupData(SqlInt16 partsnap, SqlString phkeytable)
    {
        //string temp_partid_table = "#temp_partid_list";

        List<PosData> idPositionMap = new List<PosData>();
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();
            // Phkey -> (List of slots in that phkey)
            Dictionary<int, List<int>> phkeySlotMap = new Dictionary<int, List<int>>();
            string rebuildPhkeySlotMapString = "select phkey, slot from " + phkeytable.ToString();
            SqlCommand rebuildPhkeySlotMapCommand = new SqlCommand(rebuildPhkeySlotMapString, connection);
            SqlDataReader mapReader = rebuildPhkeySlotMapCommand.ExecuteReader();
            while (mapReader.Read())
            {
                int phkey = Int32.Parse(mapReader["phkey"].ToString());
                short slot = Int16.Parse(mapReader["slot"].ToString());                    
                List<int> slotList = null;
                if (!phkeySlotMap.TryGetValue(phkey, out slotList))
                {
                    slotList = new List<int>();
                }
                slotList.Add(slot);
                phkeySlotMap[phkey] = slotList;
            }
            mapReader.Close();

            string getActualDataString = "select distinct a.phkey, a.id, a.pos from SimulationDB.dbo.snaparr a, " + phkeytable.ToString() + " b where a.snapnum = @partsnap and a.phkey = b.phkey";
            SqlParameter partsnapParam2 = new SqlParameter("@partsnap", System.Data.SqlDbType.SmallInt);
            partsnapParam2.Value = partsnap;
            SqlCommand getActualDataCommand = new SqlCommand(getActualDataString, connection);
            getActualDataCommand.Parameters.Add(partsnapParam2);
            SqlDataReader dataReader = getActualDataCommand.ExecuteReader();
            while (dataReader.Read())
            {
                long[] ids = (new SqlBigIntArrayMax(new SqlBytes((byte[])dataReader["id"])).ToArray());
                SqlRealArrayMax positions = new SqlRealArrayMax(new SqlBytes((byte[])dataReader["pos"]));
                int phkey = Int32.Parse(dataReader["phkey"].ToString());
                //int[][] currentSlots = {(phkeySlotMap[phkey].ToArray())};
                //ids.GetItems(currentSlots);
                //int[] lengths = { ids.Length, 3 };
                //positions.Reshape(lengths);
                //int[] lengths[
                float[] posArray = positions.ToArray();
                foreach (short slot in phkeySlotMap[phkey])
                {
                    //PosData currentPosComps = new PosData(ids[slot], posArray[slot, 0], posArray[slot, 1], posArray[slot, 2]);
                    PosData currentPosComps = new PosData(ids[slot], posArray[slot * 3 + 0], posArray[slot * 3 + 1], posArray[slot * 3 + 2]);
                    idPositionMap.Add(currentPosComps);
                }
            }
            dataReader.Close();
        }
        return idPositionMap;
    }

    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "ReturnPosDataFillRow",
        TableDefinition = "partid BIGINT, x REAL, y REAL, z REAL", DataAccess = DataAccessKind.Read)]
    public static IEnumerable LookupDataNoSlots(SqlInt16 partsnap, SqlString phkeytable, SqlInt32 numparts, SqlString temp_partid_table_name)
    {
        //string temp_partid_table = "#temp_partid_list";

        List<PosData> idPositionMap = new List<PosData>();
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();
            // Phkey -> (List of slots in that phkey)
            List<int> phkeyList = new List<int>();
            string rebuildPhkeySlotMapString = "select distinct phkey from " + phkeytable.ToString();
            SqlCommand rebuildPhkeySlotMapCommand = new SqlCommand(rebuildPhkeySlotMapString, connection);
            SqlDataReader mapReader = rebuildPhkeySlotMapCommand.ExecuteReader();
            while (mapReader.Read())
            {
                int phkey = Int32.Parse(mapReader["phkey"].ToString());
                //short slot = Int16.Parse(mapReader["slot"].ToString());
                phkeyList.Add(phkey);
            }
            mapReader.Close();

            string getFoFGroupIDsCommandString = "select partid from " + temp_partid_table_name.ToString();
            SqlCommand getFoFGroupIDsListCommand = new SqlCommand(getFoFGroupIDsCommandString, connection);
            SqlDataReader getFOFGroupIDsReader = getFoFGroupIDsListCommand.ExecuteReader();
            List<Int64> haloPartIDs = new List<Int64>();
            int numlines = 0;
            while (getFOFGroupIDsReader.Read())
            {
                haloPartIDs.Add(Int64.Parse(getFOFGroupIDsReader["partid"].ToString()));
                ++numlines;
            }
            getFOFGroupIDsReader.Close();


            string getActualDataString = "select a.phkey, a.id, a.pos from SimulationDB.dbo.snaparr a, " + phkeytable.ToString() + " b where a.snapnum = @partsnap and a.phkey = b.phkey";
            SqlParameter partsnapParam2 = new SqlParameter("@partsnap", System.Data.SqlDbType.SmallInt);
            partsnapParam2.Value = partsnap;
            SqlCommand getActualDataCommand = new SqlCommand(getActualDataString, connection);
            getActualDataCommand.Parameters.Add(partsnapParam2);
            SqlDataReader dataReader = getActualDataCommand.ExecuteReader();
            while (dataReader.Read())
            {
                List<Int64> ids = new List<Int64>((new SqlBigIntArrayMax(new SqlBytes((byte[])dataReader["id"])).ToArray()));

                SqlRealArrayMax positions = new SqlRealArrayMax(new SqlBytes((byte[])dataReader["pos"]));
                int phkey = Int32.Parse(dataReader["phkey"].ToString());

                float[] posArray = positions.ToArray();
                for (int i = 0; i < haloPartIDs.Count; ++i)
                {
                    int slot = ids.BinarySearch(haloPartIDs[i]);
                    if (slot >= 0)
                    {
                        PosData currentPosComps = new PosData(ids[slot], posArray[slot * 3 + 0], posArray[slot * 3 + 1], posArray[slot * 3 + 2]);
                        haloPartIDs.RemoveAt(i);
                        --i;
                        idPositionMap.Add(currentPosComps);
                    }
                }

            }
            dataReader.Read();
        }
        return idPositionMap;
    }


    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "ReturnPosDataFillRow",
        TableDefinition = "partid BIGINT, x REAL, y REAL, z REAL", DataAccess = DataAccessKind.Read)]
    public static IEnumerable LookupDataNoIndex(SqlBytes ids, SqlBytes positions, SqlBytes idsInGroup)
    {
        List<Int64> idsArray = new List<Int64>((new SqlBigIntArrayMax(ids)).ToArray());
        float[] posArray = (new SqlRealArrayMax(positions)).ToArray();
        long[] groupIdsArray = (new SqlBigIntArrayMax(idsInGroup)).ToArray();
        List<PosData> joinedData = new List<PosData>();
        foreach (int haloId in groupIdsArray)
        {
            int index = idsArray.BinarySearch(haloId);
            if (index >= 0)
            {
                joinedData.Add(new PosData(idsArray[index], posArray[3 * index + 0], posArray[3 * index + 1], posArray[3 * index + 2]));
            }
        }
        /*for (int i = 0; i < idsArray.Length; ++i)
        {
            joinedData.Add(new PosData(idsArray[i], posArray[3 * i + 0], posArray[3 * i + 1], posArray[3 * i + 2]));
        }*/
        return joinedData;
    }


    [Microsoft.SqlServer.Server.SqlFunction(FillRowMethodName = "ReturnPosDataWithSnapFillRow",
        TableDefinition = "partid BIGINT, snap SMALLINT, x REAL, y REAL, z REAL", DataAccess = DataAccessKind.Read)]
    public static IEnumerable LookupDataAllTimesteps(SqlString phkeytable)
    {
        //string temp_partid_table = "#temp_partid_list";

        List<PosDataWithSnap> idPositionMap = new List<PosDataWithSnap>();
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();
            // Phkey -> (List of slots in that phkey)
            List<Dictionary<int, List<int>>> phkeySlotMap = new List<Dictionary<int, List<int>>>();
            // initialize dictionaries for all timesteps
            for (int i = 0; i < 64; ++i)
            {
                phkeySlotMap.Add(new Dictionary<int, List<int>>());
            }
            string rebuildPhkeySlotMapString = "select snap, phkey, slot from " + phkeytable.ToString();
            SqlCommand rebuildPhkeySlotMapCommand = new SqlCommand(rebuildPhkeySlotMapString, connection);
            SqlDataReader mapReader = rebuildPhkeySlotMapCommand.ExecuteReader();
            while (mapReader.Read())
            {
                int snap = Int32.Parse(mapReader["snap"].ToString());
                int phkey = Int32.Parse(mapReader["phkey"].ToString());
                short slot = Int16.Parse(mapReader["slot"].ToString());
                List<int> slotList = null;
                if (!phkeySlotMap[snap].TryGetValue(phkey, out slotList))
                {
                    slotList = new List<int>();
                }
                slotList.Add(slot);
                phkeySlotMap[snap][phkey] = slotList;
            }
            mapReader.Close();

            // TODO: the faster way to do this might be to do each timestep separately, I think we will stay in memory that way???
            // I should figure out what the bottleneck here is though

            string getActualDataStringRaw = "select distinct a.phkey, a.id, a.pos from SimulationDB.dbo.snaparr a, "
                + phkeytable.ToString() + " b where a.snapnum = b.snap and a.phkey = b.phkey and a.snapnum = {0}";
            for (short currentSnap = 0; currentSnap < 64; ++currentSnap)
            {
                string getActualDataString = string.Format(getActualDataStringRaw, currentSnap);
                SqlCommand getActualDataCommand = new SqlCommand(getActualDataString, connection);
                SqlDataReader dataReader = getActualDataCommand.ExecuteReader();
                while (dataReader.Read())
                {
                    long[] ids = (new SqlBigIntArrayMax(new SqlBytes((byte[])dataReader["id"])).ToArray());
                    SqlRealArrayMax positions = new SqlRealArrayMax(new SqlBytes((byte[])dataReader["pos"]));
                    int phkey = Int32.Parse(dataReader["phkey"].ToString());
                    float[] posArray = positions.ToArray();
                    foreach (short slot in phkeySlotMap[currentSnap][phkey])
                    {
                        //PosData currentPosComps = new PosData(ids[slot], posArray[slot, 0], posArray[slot, 1], posArray[slot, 2]);
                        PosDataWithSnap currentPosComps = new PosDataWithSnap(currentSnap, ids[slot], posArray[slot * 3 + 0], posArray[slot * 3 + 1], posArray[slot * 3 + 2]);
                        idPositionMap.Add(currentPosComps);
                    }
                }
                dataReader.Close();
            }

        }
        return idPositionMap;
    }


};
