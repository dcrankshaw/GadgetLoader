Use SimDBdev
go
if object_id (N'dbo.getposFoFGroup', N'TF') is not null
		drop function dbo.getposFoFGroup
go

Use SimDBdev
go
create function dbo.getposFoFGroup (@padlevel int, @sn_part smallint, @sn_fof smallint, @fid int)
										returns @results table( phkey int, x real, y real, z real )
as
begin

declare @phcells varbinary(max)

if(@padlevel = 15)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox15
					where snapnum = @sn_fof and fofID = @fid)
end

else if(@padlevel = 20)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox20
					where snapnum = @sn_fof and fofID = @fid)
end

else if(@padlevel = 25)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox25
					where snapnum = @sn_fof and fofID = @fid)
end

else if(@padlevel = 30)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox30
					where snapnum = @sn_fof and fofID = @fid)
end
else if(@padlevel = 0)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox0
					where snapnum = @sn_fof and fofID = @fid)
end
else if(@padlevel = 45)
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox45
					where snapnum = @sn_fof and fofID = @fid)
end
else
begin
set @phcells = (select phcells from SimDBdev.dbo.fofgroupsBoundingBox40
					where snapnum = @sn_fof and fofID = @fid)
end

declare @fofpartids varbinary(max)
set @fofpartids = (select partids from SimDBdev.dbo.fofgroups
					where snapnum = @sn_fof and fofID = @fid)

if (@fofpartids is NULL)
begin
	return
end
else
	declare @fofids_tab table(id bigint)
	insert into @fofids_tab select v from SqlArray.BigIntArrayMax.ToTable(@fofpartids)
	declare @phcells_tab table(cell int)
	insert into @phcells_tab select v from SqlArray.IntArrayMax.ToTable(@phcells)

	declare @bbcells table(numpart int, ids varbinary(max), poss varbinary(max))
	insert into @bbcells select a.numpart, a.id, a.pos from SimDBdev.dbo.snaparr as a inner join @phcells_tab as b
		on a.snapnum = @sn_part and a.phkey = b.cell

	declare @potential_pos table(id bigint, x real, y real, z real)
	insert into @potential_pos select b.id, b.x, b.y, b.z from (
		(select numpart, ids, poss from @bbcells) a cross apply
			dbo.idarr_posarr_To_Table(a.numpart, a.ids, a.poss) b)
	--use left join to include any ids whose position is missing, this tells us whether bounding box fully
	--encompassed particles we are looking for
	insert into @results select a.id, a.x, a.y, a.z from @potential_pos as a
		right join @fofids_tab as b on a.id = b.id

	return
end
go


---------------------------------------------------------------------------------


Use SimDBdev
go
if object_id (N'dbo.idarr_posarr_To_Table ', N'TF') is not null
		drop function dbo.idarr_posarr_To_Table
go

Use SimDBdev
go
create function dbo.idarr_posarr_To_Table (@numpart int, @idarr varbinary(max), @posarr varbinary(max))
				returns @results table(id bigint, x real, y real, z real )
as
begin

declare @size varbinary(100)
declare @pos_reshaped varbinary(max)
set @size = SqlArray.IntArray.Vector_2(3, @numpart)
set @pos_reshaped = SqlArray.RealArrayMax.Reshape(@posarr, @size)
insert into @results select a2.id, a2.x, a2.y, q.v as z from
	(select a1.id, a1.x, a1.ind, q.v as y from
		(select p.v as id, q.v as x, p.li as ind from SqlArray.BigIntArrayMax.ToTable(@idarr) p
		inner join SqlArray.RealArrayMax.ToTable_2(@pos_reshaped)q
on p.li = q.i1 and q.i0 = 0) a1 inner join SqlArray.RealArrayMax.ToTable_2(@pos_reshaped) q
on a1.ind = q.i1 and q.i0 = 1) a2 inner join SqlArray.RealArrayMax.ToTable_2(@pos_reshaped) q
on a2.ind = q.i1 and q.i0 = 2

return

end
go


--------------------------------------------------------------------------------


Use SimDBdev
go
if object_id (N'dbo.getinitposFoFGroup', N'TF') is not null
		drop function dbo.getinitposFoFGroup
go

Use SimDBdev
go
create function dbo.getinitposFoFGroup (@sn_fof smallint, @fid int)
										returns @results table( x real, y real, z real )
as
begin

declare @fofpartids varbinary(max)
set @fofpartids = (select partids from SimDBdev.dbo.fofgroups
					where snapnum = @sn_fof and fofID = @fid)
if (@fofpartids is NULL)
begin
	return
end
else
	declare @fofids_tab table(id bigint)
	insert into @fofids_tab select v from SqlArray.BigIntArrayMax.ToTable(@fofpartids)

	insert into @results select snap.x, snap.y, snap.z from dbo.id_pos_orig as snap inner join @fofids_tab as fofs
		on fofs.id = snap.orig_id and snap.snapnum = 0

	return
end
go

--------------------------------------------------------------------------------------

Use SimDBdev
go
if object_id (N'dbo.getposFoFGroupNoArr', N'TF') is not null
		drop function dbo.getposFoFGroupNoArr
go

Use SimDBdev
go
create function dbo.getposFoFGroupNoArr (@sn_fof smallint, @fid int, @sn_part smallint)
										returns @results table(ts smallint, id bigint, x real, y real, z real )
as
begin

declare @fofpartids varbinary(max)
set @fofpartids = (select partids from SimDBdev.dbo.fofgroups
					where snapnum = @sn_fof and fofID = @fid)
if (@fofpartids is NULL)
begin
	return
end
else
	declare @fofids_tab table(id bigint)
	insert into @fofids_tab select v from SqlArray.BigIntArrayMax.ToTable(@fofpartids)

	insert into @results select snap.snapnum, snap.orig_id ,snap.x, snap.y, snap.z from dbo.id_pos_orig as snap inner join @fofids_tab as fofs
		on fofs.id = snap.orig_id and snap.snapnum = @sn_part

	return
end
go