using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using Jhu.SqlServer.Array;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBinary MergePHkeys(SqlBinary oldPHkeys, SqlInt16 newSnap, SqlInt32 newPHkey)
    {
        SqlIntArray keyArray = new SqlIntArray(oldPHkeys);
        int[] keys = keyArray.ToArray();
        keys[(short) newSnap] = (int) newPHkey;
        return SqlIntArray.FromArray(keys).ToSqlBuffer();
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBinary MergeSlots(SqlBinary oldSlots, SqlInt16 newSnap, SqlInt16 newSlot)
    {
        //short[] slots = oldSlots.ToArray();
        SqlSmallIntArray slotArray = new SqlSmallIntArray(oldSlots);
        short[] slots = slotArray.ToArray();
        slots[(short) newSnap] = (short) newSlot;
        return SqlSmallIntArray.FromArray(slots).ToSqlBuffer();
    }
};

