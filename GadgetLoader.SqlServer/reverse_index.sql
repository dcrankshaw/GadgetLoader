/*create table #reverse_index_test
(
	partid bigint not null,
	phkey varbinary(8000) not null, --array of 4 byte ints
	slot varbinary(8000) not null,	--array of 2 byte ints
	primary key(partid)
)
go

create table #reverse_index_insert
(
	partid bigint not null,
	snapnum int not null,
	phkey int not null,
	slot smallint not null, -- 0 indexed
	primary key(partid, snapnum)
)
go*/


WITH merge_index_CTE (partid, oldphkeys, newphkey, oldslots, newslot, snapnum)
AS
(
SELECT t.partid, t.phkey, i.phkey, t.slot, i.slot, i.snapnum
FROM [#reverse_index_test] as t INNER JOIN #reverse_index_insert as i
ON t.partid = i.partid
)
UPDATE #reverse_index_test
SET phkey = SimulationDB.dbo.MergeSlots(m.oldphkeys, m.snapnum, m.newphkey)
FROM merge_index_CTE as m
/*UPDATE #reverse_index_test
SET phkey = */