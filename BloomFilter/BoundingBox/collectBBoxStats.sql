Use SimDBdev
go
if object_id (N'dbo.collectBBoxStats', N'FN') is not null
		drop function dbo.collectBBoxStats
go

Use SimDBdev
go
create function dbo.collectBBoxStats(@padlevel real, @partTS smallint, @fofTS smallint, @fid int)
										returns int
as
begin

declare @phcells varbinary(max)
declare @results table( phkey int, x real, y real, z real )

if(@padlevel = 15)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox15
					where snapnum = @fofTS and fofID = @fid)
end

else if(@padlevel = 20)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox20
					where snapnum = @fofTS and fofID = @fid)
end

else if(@padlevel = 25)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox25
					where snapnum = @fofTS and fofID = @fid)
end

else if(@padlevel = 30)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox30
					where snapnum = @fofTS and fofID = @fid)
end
else if(@padlevel = 0)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox0
					where snapnum = @fofTS and fofID = @fid)
end
else if(@padlevel = 45)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox45
					where snapnum = @fofTS and fofID = @fid)
end
else
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox40
					where snapnum = @fofTS and fofID = @fid)
end




declare @fofpartids varbinary(max)
set @fofpartids = (select partids from SimDBdev.dbo.fofgroups
					where snapnum = @fofTS and fofID = @fid)

if (@fofpartids is NULL)
begin
	return 0
end
else
	declare @fofids_tab table(id bigint)
	insert into @fofids_tab select v from SqlArray.BigIntArrayMax.ToTable(@fofpartids)
	declare @phcells_tab table(cell int)
	insert into @phcells_tab select v from SqlArray.IntArrayMax.ToTable(@phcells)

	declare @bbcells table(numpart int, ids varbinary(max), poss varbinary(max))
	insert into @bbcells select a.numpart, a.id, a.pos from SimDBdev.dbo.snaparr as a inner join @phcells_tab as b
		on a.snapnum = @partTS and a.phkey = b.cell

	declare @potential_pos table(id bigint, x real, y real, z real)
	insert into @potential_pos select b.id, b.x, b.y, b.z from (
		(select numpart, ids, poss from @bbcells) a cross apply
			dbo.idarr_posarr_To_Table(a.numpart, a.ids, a.poss) b)
	--use left join to include any ids whose position is missing, this tells us whether bounding box fully
	--encompassed particles we are looking for
	insert into @results select a.id, a.x, a.y, a.z from @potential_pos as a
		right join @fofids_tab as b on a.id = b.id

	declare @missed int
	set @missed = (select COUNT(*) from @results where [@results].x IS NULL) --

	return @missed
end
go

---------------------------------------------------

--Scripts to calculate various stats about the size of the bounding boxes (e.g. average, number of boxes < 1000 cells, avg size of box with < 1000 particles, etc.)

/*declare @ab varbinary(max)
set @ab = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox20 where snapnum = 60 and fofID = 0)
declare @g int
declare @rank tinyint = 0
exec @g = SqlArray.IntArrayMax.Length @ab, @rank
print @g
*/


declare @counter int = 0
declare @fofTS int = 60
declare @maxfof int
set @maxfof = (select MAX(fofID) from SimDBdev.dbo.fofgroups where snapnum = @fofTS)
declare @phcell varbinary(max)
declare @totalLength float = 0
declare @currentLength int
while @counter <= @maxfof
begin
	set @phcell = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox25 where snapnum = @fofTS and fofID = @counter)
	exec @currentLength = SqlArray.IntArrayMax.Length @phcell, 0
	set @totalLength += @currentLength
	set @counter += 1
end
declare @avg float
set @avg = @totalLength / @maxfof
print @avg