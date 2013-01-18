create table SimDBdev.dbo.params
(
	phbits int not null,
	boxSize int not null,
	snapStartFile int not null,
	snapEndFile int not null,
	snapFilePrefix varchar(100) not null,
	writeArrays bit not null,
	particlesPerSnap bigint not  null,
	groupTabPrefix varchar(100) not null,
	groupIDPrefix varchar(100) not null,
)
go

create table SimDBdev.dbo.testsimsnapshots
(
	snapnum smallint not null,
	phkey int not null,
	numpart int not null,
	pos varbinary(max) not null,
	vel varbinary(max) not null,
	id varbinary(max) not null,
	primary key (snapnum, phkey)
)
go

create table SimDBdev.dbo.testsimfofGroups
(
	snapnum smallint not null,
	fofID int not null,
	numparts int not null,
	partids varbinary(max) not null,
	primary key (snapnum, fofID)
)
go

/*create table SimDBdev.dbo.testsimFFTData
(

)*/

-- parameter table values
insert into SimDBdev.dbo.params values(6, 1000, 0, 31, 'snapshot_', 'True', 134217728, 'group_tab_', 'group_ids_')


--compressed table declaration
create table SimDBdev.dbo.compressedarraysnapshots
(
	snapnum smallint not null,
	phkey int not null,
	x0 real not null,
	y0 real not null,
	z0 real not null,
	numpart int not null,
	sc_inv real not null,
	pos varbinary(max) not null,
	vel varbinary(max) not null,
	id varbinary(max) not null,
	primary key (snapnum, phkey)
)
go