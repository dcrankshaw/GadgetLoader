/*
Code to decompress a compressed position array to get the original positions back out
*/

declare @posarr varbinary(max)
declare @idarr varbinary(max)
declare @numpart int
set @numpart = (select numpart from SimDBdev.dbo.compressedarraysnapshots where snapnum = 30 and phkey = 0)
print @numpart
set @posarr = (select pos from SimDBdev.dbo.compressedarraysnapshots where snapnum = 30 and phkey = 0)
set @idarr = (select id from SimDBdev.dbo.compressedarraysnapshots where snapnum = 30 and phkey = 0)
declare @pos_reshaped varbinary(max)
declare @size varbinary(100)
set @size = SqlArray.IntArray.Vector_2(3, @numpart)
set @pos_reshaped = SqlArray.SmallIntArrayMax.Reshape(@posarr, @size)
--select * from SqlArray.SmallIntArrayMax.ToTable_2(@pos_reshaped)
declare @posc table(id bigint, xc smallint, yc smallint, zc smallint)

insert into @posc select a2.id, a2.x, a2.y, q.v as z from
	(select a1.id, a1.x, a1.ind, q.v as y from
		(select p.v as id, q.v as x, p.li as ind from SqlArray.BigIntArrayMax.ToTable(@idarr) p
		inner join SqlArray.SmallIntArrayMax.ToTable_2(@pos_reshaped)q
on p.li = q.i1 and q.i0 = 0) a1 inner join SqlArray.SmallIntArrayMax.ToTable_2(@pos_reshaped) q
on a1.ind = q.i1 and q.i0 = 1) a2 inner join SqlArray.SmallIntArrayMax.ToTable_2(@pos_reshaped) q
on a2.ind = q.i1 and q.i0 = 2
declare @x0 real = (select x0 from SimDBdev.dbo.compressedarraysnapshots where snapnum = 30 and phkey = 0)
declare @y0 real = (select y0 from SimDBdev.dbo.compressedarraysnapshots where snapnum = 30 and phkey = 0)
declare @z0 real = (select z0 from SimDBdev.dbo.compressedarraysnapshots where snapnum = 30 and phkey = 0)
declare @scinv real = (select sc_inv from SimDBdev.dbo.compressedarraysnapshots where snapnum = 30 and phkey = 0)
declare @pos table(id bigint, x real, y real, z real)
insert into @pos select id, (xc*@scinv + @x0), (yc*@scinv + @y0), (zc*@scinv + @z0) from @posc
select * from @pos
/*declare @pos_arr varbinary(max)
set @pos_arr = (select pos from SimDBdev.dbo.snaparr where snapnum = 30 and phkey = 0)
select v from SqlArray.RealArrayMax.ToTable(@pos_arr)
*/