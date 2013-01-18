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
	snapnum smallint not null,
	phkey int not null,
	slot smallint not null, -- 0 indexed
	primary key(partid, snapnum)
)
go*/


WITH merge_index_CTE (partid, oldphkeys, newphkey, snapnum, oldslots, newslot)
AS
(
SELECT t.partid, t.phkey, i.phkey, i.snapnum, t.slot, i.slot
FROM SimulationDB.dbo.revindextest_ReverseIndex as t INNER JOIN SimulationDB.dbo.revindextest_RIinsert_1 as i
ON t.partid = i.partid
)
UPDATE SimulationDB.dbo.revindextest_ReverseIndex
SET phkey = SimulationDB.dbo.MergePHkeys(m.oldphkeys, m.snapnum, m.newphkey), slot = SimulationDB.dbo.MergeSlots(m.oldslots, m.snapnum, m.newslot)
FROM merge_index_CTE as m
GO
