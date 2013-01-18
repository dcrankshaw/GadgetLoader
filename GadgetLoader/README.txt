Basic Loader Workflow:

A powershell script takes the raw data from the HHPC and transfers it to a server (or set of servers)
one snapshot at a time.
In addition, it enters the configuration information about the simulation that the recently transferred
snapshot belongs to, where in the file system it is located, and where the loader ouuput should be written to
in a configuration table in a database on that server. That server also has a SQLJob running (Ani Thakar set this up,
and we haven't really tested it, so talk to him if it is not working) that checks for unprocessed data every x amount
of time. When it sees unprocessed data, it calls the GadgetLoader executable on that server with the following paramaters:

./GadgetLoader -z=[SNAPSHOT] -s -m=[SIMULATIONNAME] -v=[SERVER] -d=[DATABASE] -q=[BCP_SCRIPT_FILE]

-z: Is the snapshot number within this simulation to look up. It uses this information along with the simulation name to 
	find the right entry in the configuration table

-s: indicates loading the particle info

-t: indicates loading the FFT data

-f: indicates loading the FOF group data

-m: Is the simulation name to identify e.g. this snapshot 40 from all the other snapshot 40s

-v: Is the server the paramater and configuration tables are on and the server that the BCP script generated will load in to

-d: Is the database on that server with the param and config tables and that the BCP script will load into

-q: Is the location on disk that the BCP script file will be written to

Once the loader has finished processing a snapshot, a summary file will be written to the directory that contains all of the
processed snapshot data, and a BCP script will be written to the location provided. At this point, the processed files
are in SQL Native format and can be loaded into the database. They can also be transferred if you want to process the files
on a different server than where they are being stored. However, in this case, the BCP script will need to be updated to
reflect that location change. A simple python script is probably the easiest way.





Table Schema:
The name of each column (case sensitive) is followed by an explanation of the column's purpose

Parameters table: contains information about the simulations for the loader.
It is read in LoaderParamSingleton.cs starting on line 54

phbits: depth of the peano-hilbert index
boxsize: length of one side of the simulation box in Mpc (should be 1000)
snapFilePrefix: snapshot_
writeArrays: whether to write each particle to a separate row or to write an entire cell to a row (1 indicates that arrays should be used)
particlesPerSnap: 1024^3. This information allows for sanity checks while loading the data.
groupTabPrefix: group_tab_
groupIDPrefix: group_ids_

CREATE TABLE MyDatabase.dbo.params
(
	phbits int,
	boxsize int,
	snapFilePrefix varchar(100),
	writeArrays bit,
	particlesPerSnap long,
	groupTabPrefix varchar(100),
	groupIDPrefix varchar(100),
)


Snapshot Configuration Table: contains information about how to load a particular snapshot

sim: the name of the simulation this snapshot belongs to
snapnum: the snapshot number (0-63)
inpath: where the raw binary files produced by the simulation are located
outpath: where the processed files in MSSQL native format should be written to (they can then be loaded using a BCP script)

CREATE TABLE MyDatabase.dbo.config
(
	sim varchar(200),
	snapnum int,
	inpath varchar(200),
	outpath varchar(200),	--these can be made longer if need be
)


Snapshot Data: The data output by the simulation

CREATE TABLE MyDatabase.dbo.[MySimulation]_ParticleData
(
	snapnum smallint not null, --the snapshot number
	phkey int not null, -- Peano-Hilbert index of this cell
	numpart int not null, -- the number of particles in the cell
	pos varbinary(max) not null, -- the positions of all the particles in the cell in a SqlArray
	vel varbinary(max) not null, -- the velocities of all the particles in the cell in a SqlArray
	id varbinary(max) not null, -- the IDs of all the particles in the cell in a SqlArray
	primary key (snapnum, phkey)
)

Friend of Friends group data:

CREATE TABLE MyDatabase.dbo.[MySimulation]_FoFGroupsData
(
	snapnum smallint not null, -- the snapshot this fof group was identified in
	fofID int not null, -- an ID assigned to the fof group. Unique within a timestep but not throughout multiple time steps
	numparts int not null, -- the number of particles in the group
	partids varbinary(max) not null, -- a SqlArray of all the particles in the group
	primary key (snapnum, fofID)
)

CREATE TABLE MyDatabase.dbo.[My_Simulation]_ReverseIndex
(
	partid bigint not null,
	phkey varbinary(8000) not null, --array of 4 byte ints
	slot varbinary(8000) not null,	--array of 2 byte ints
	primary key(partid)
)