create table #reverse_index_test
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
go


insert into #reverse_index_test values(0, 7, 11)
insert into #reverse_index_insert values(0, 7, 11, 13)
insert into #reverse_index_test values(1, 8, 12)
insert into #reverse_index_insert values(1, 8, 12, 14)
insert into #reverse_index_test values(2, 9, 13)
insert into #reverse_index_insert values(2, 9, 13, 15)
insert into #reverse_index_test values(3, 10, 14)
insert into #reverse_index_insert values(3, 10, 14, 16)
insert into #reverse_index_test values(4, 11, 15)
insert into #reverse_index_insert values(4, 11, 15, 17)
insert into #reverse_index_test values(5, 12, 16)
insert into #reverse_index_insert values(5, 12, 16, 18)
insert into #reverse_index_test values(6, 13, 17)
insert into #reverse_index_insert values(6, 13, 17, 19)
insert into #reverse_index_test values(7, 14, 18)
insert into #reverse_index_insert values(7, 14, 18, 20)
insert into #reverse_index_test values(8, 15, 19)
insert into #reverse_index_insert values(8, 15, 19, 21)
insert into #reverse_index_test values(9, 16, 20)
insert into #reverse_index_insert values(9, 16, 20, 22)

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