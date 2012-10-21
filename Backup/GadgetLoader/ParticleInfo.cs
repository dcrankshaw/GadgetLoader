using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    public struct Structs
    {
        public Int16 snapnum;
        public Int32 phkey;
        public UInt64 id;
        public float x, y, z;
        public float vx, vy, vz;
        public float hsml, density, veldisp;

        public override string ToString()
        {
            return snapnum + "," + phkey+ "," + id + "," + x + "," + y + "," + z + "," + vx + "," + vy + "," + vz + "," + hsml + "," + density + "," + veldisp;
        }

        public void WriteBinary(BinaryWriter writer)
        {
            writer.Write(snapnum);
            writer.Write(phkey);
            writer.Write(id);
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);
//            writer.Write(hsml);
//            writer.Write(density);
//            writer.Write(veldisp);
        }

        public void WriteBinaryNoHsml(BinaryWriter writer)
        {
            writer.Write(snapnum);
            writer.Write(phkey);
            writer.Write(id);
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);
        }
    }
    /// <summary>
    /// Particle data form file where particles ordered according to FOF/SubHalo hierarchy.
    /// </summary>
    public struct FOFOrderedParticle
    {
        public Int32 phkey;
        public UInt64 particleId;
        public long id;
        public float x, y, z;
        public float vx, vy, vz;
        //public float hsml;
        public float density,veldisp;

        public override string ToString()
        {
            return id + "," + phkey + "," + particleId + "," + x + "," + y + "," + z ;
        }

        public void WriteBinary(BinaryWriter writer, short snapnum)
        {
            writer.Write(id);
            writer.Write(particleId);
            writer.Write(snapnum);
            writer.Write(phkey);
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);
//            writer.Write(hsml);
            writer.Write(density);
            writer.Write(veldisp);
        }

    }



    #region Millennium-II
    /// <summary>
    /// Represents the center of mass positions and velocities for the FOF groups
    /// </summary>
    public class GroupTabInfo
    {
        public float cmx, cmy, cmz;
        public float cvx, cvy, cvz;
    }


    /// <summary>
    /// Represents a FOF group from the Millennium-II.
    /// </summary>
    public class GroupInfo
    {
        public long fofId;
        public short snapnum;
        public float redshift;
        public int Length; // number of particles in group
        public float Mass; // total mass
        public GroupTabInfo centerOfMassData;
        public float x, y, z;
        public int ix, iy, iz;
        public int phkey;
        public float M_Mean200;
        public float R_Mean200;
        public float M_Crit200;
        public float R_Crit200;
        public float M_TopHat200;
        public float R_TopHat200;

        /* ----- VELOCITY DISPERSIONS MAY NOT BE IN FILE, CHECK FIRST ----- */
        public float VelDisp_Mean200;
        public float VelDisp_Crit200;
        public float VelDisp_TopHat200;
        /* ---------------------------------------------------------------- */

        public int numSubs; // number of subhalos
        public int firstSubIndex; // first subhalo index
        public int lastSubIndex
        {
            get { return firstSubIndex + numSubs - 1; }
        }
            

        public long firstSubID; // first subhalo index

        public int offset;


        public override string ToString()
        {
            return snapnum + "," + fofId + "," + x + "," + y + "," + z;
        }

        public void WriteBinary(BinaryWriter writer, bool writeVelDisp)
        {
            writer.Write(fofId);
            writer.Write(snapnum);
            writer.Write(redshift);
            writer.Write(Length);
            writer.Write(Mass);

            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(ix);
            writer.Write(iy);
            writer.Write(iz);
            writer.Write(phkey);

            writer.Write(centerOfMassData.cmx);
            writer.Write(centerOfMassData.cmy);
            writer.Write(centerOfMassData.cmz);
            writer.Write(centerOfMassData.cvx);
            writer.Write(centerOfMassData.cvy);
            writer.Write(centerOfMassData.cvz);

            writer.Write(M_Crit200);
            writer.Write(R_Crit200);
            writer.Write(M_Mean200);
            writer.Write(R_Mean200);
            writer.Write(M_TopHat200);
            writer.Write(R_TopHat200);
            if (writeVelDisp)
            {
                writer.Write(VelDisp_Crit200);
                writer.Write(VelDisp_Mean200);
                writer.Write(VelDisp_TopHat200);
            }
            writer.Write(numSubs);
            writer.Write(firstSubID);
        }
    }

    public class SubhaloInfo
    {
        public long subhaloFOFId; // id based on rank in fof group
        public long fofId;
        public long subhaloFileId; // id based on snapnum and rank in file (trees know of this).
        public short snapnum;
        public float redshift;
        public int length;
        public float x, y, z;
        public int ix, iy, iz;
        public int phkey;
        public float vx, vy, vz;
        public float cmx, cmy, cmz;
        public float sx, sy, sz;
        public float mass, hmradius, veldisp, velMax, velMaxRad;
        public long mostBoundId;
        // public int GrNr ; // ???
        public int offset;


        public override string ToString()
        {
            return snapnum + "," + subhaloFileId + "," + x + "," + y + "," + z + "," + vx + "," + vy + "," + vz;
        }

        public void WriteBinary(BinaryWriter writer)
        {
            writer.Write(subhaloFOFId);
            writer.Write(fofId);
            writer.Write(subhaloFileId);
            writer.Write(snapnum);
            writer.Write(redshift);
            writer.Write(length);
            writer.Write(mass);

            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(ix);
            writer.Write(iy);
            writer.Write(iz);
            writer.Write(phkey);

            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);

            writer.Write(cmx);
            writer.Write(cmy);
            writer.Write(cmz);

            writer.Write(sx);
            writer.Write(sy);
            writer.Write(sz);

            writer.Write(hmradius);
            writer.Write(veldisp);
            writer.Write(velMax);
            writer.Write(velMaxRad);
            writer.Write(mostBoundId);
        }
    }
    #endregion

    #region XXLFiles
    /// <summary>
    /// Represents a FOF group from the Millennium-XXL.
    /// </summary>
    public class GroupInfo_MXXL
    {
        public long fofId;
        public short snapnum; // param
        public float redshift; // param

        public int Length; // number of particles in group
        public int offset;
        public long GroupNr;
        public float cmx, cmy, cmz;
        public float vx, vy, vz;
        public float x, y, z;
        public int ix, iy, iz; //derived
        public int phkey; // derived
        public float M_Mean200;
        public float M_Crit200;
        public float M_TopHat200;
        public float VelDisp;

        /* ----- VELOCITY DISPERSIONS MAY NOT BE IN FILE, CHECK FIRST ----- */
        public float VelDisp_Mean200;
        public float VelDisp_Crit200;
        public float VelDisp_TopHat200;
        /* ---------------------------------------------------------------- */

        public int numSubs; // number of subhalos
        public int firstSubIndex; // first subhalo index
        public int lastSubIndex
        {
            get { return firstSubIndex + numSubs - 1; }
        }


        public long firstSubID; // first subhalo index, obtained from actual firstSubhalo

        public void WriteBinary(BinaryWriter writer, bool writeVelDisp)
        {
            writer.Write(fofId);
            writer.Write(snapnum);
            writer.Write(redshift);
            writer.Write(Length);
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(ix);
            writer.Write(iy);
            writer.Write(iz);
            writer.Write(phkey);
            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);
            writer.Write(cmx);
            writer.Write(cmy);
            writer.Write(cmz);
            writer.Write(M_Crit200);
            writer.Write(M_Mean200);
            writer.Write(M_TopHat200);
            writer.Write(VelDisp);
            if (writeVelDisp)
            {
                writer.Write(VelDisp_Crit200);
                writer.Write(VelDisp_Mean200);
                writer.Write(VelDisp_TopHat200);
            }
            writer.Write(numSubs);
            writer.Write(firstSubID);
        }
    }


    public class SubhaloInfo_MXXL 
    {
        public long subhaloFOFId; // id based on rank in fof group
        public long fofId;
        public long subhaloFileId; // id based on snapnum and rank in file (trees know of this).
        public short snapnum; //param
        public float redshift; //param
        
        
        public int length;
        public int offset;
        public long GrNr, SubNr;

        public float x, y, z;
        public int ix, iy, iz; // derived
        public int phkey;// derived
        public float vx, vy, vz;
        public float cmx, cmy, cmz;
        public float sx, sy, sz;
        public float veldisp, velMax, velMaxRad,hmradius;
        public float[] shape = new float[6];
        public float bindingEnergy, potentialEnergy;
        public float[] profile = new float[9];

        public void WriteBinary(BinaryWriter writer)
        {
            writer.Write(subhaloFOFId);
            writer.Write(fofId);
            writer.Write(subhaloFileId);
            writer.Write(snapnum);
            writer.Write(redshift);
            writer.Write(length);

            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(ix);
            writer.Write(iy);
            writer.Write(iz);
            writer.Write(phkey);

            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);

            writer.Write(cmx);
            writer.Write(cmy);
            writer.Write(cmz);

            writer.Write(sx);
            writer.Write(sy);
            writer.Write(sz);

            writer.Write(veldisp);
            writer.Write(velMax);
            writer.Write(velMaxRad);
            writer.Write(hmradius);
            writer.Write(bindingEnergy);
            writer.Write(potentialEnergy);
            for(int k = 0; k < 6; k++) // correct for floats that are too small for the database.
                writer.Write(Math.Abs(shape[k]) < 1e-37?0:shape[k]);
            for(int k = 0; k < 9; k++)
                writer.Write(profile[k]);
        }
    }
    #endregion 

    public struct TreedataInfo : TreeInfo
    {
        public int length;
        public float M_Mean200, M_Crit200, M_TopHat;
        public float x, y, z;
        public float vx, vy, vz;
        public float VelDisp, Vmax, vMaxRad;
        public float sx, sy, sz;
        public long mostboundid;
        public int snapnum;
        public float redshift;
        public int fileNr;
        public int subhaloindex;
        public float subhalfmass;

        public long haloId;
        public long Id
        {
            get { return haloId;}
        }
        public long treeid;
        public long subhaloFileId;
        public long subhaloFOFId;
        public long fofId;
        public long lastProgenitorId;
        public long LastProgenitorId
        {
            get { return lastProgenitorId; }
        }
        public long mainLeafId;
        public long MainLeafId
        {
            get { return mainLeafId; }
            set { lastProgenitorId = value; }
        }
        public long descendantId;
        public long firstHaloInFOFGroupId;
        public long nextHaloInFOFGroupId;
        public long firstProgenitorId;
        public long nextProgenitorId;
        public int dummy1, dummy2;
        public int ix, iy, iz;
        public int phkey;
        public int RandomInt;

        public override string ToString()
        {
            return snapnum + "," + haloId + "," + x + "," + y + "," + z + "," + vx + "," + vy + "," + vz;
        }

        public void WriteBinary(BinaryWriter writer)
        {
            writer.Write(haloId);
            writer.Write(subhaloFOFId);
            writer.Write(fofId);
            writer.Write(treeid);
            writer.Write(descendantId);
            writer.Write(lastProgenitorId);
            writer.Write(mainLeafId);
            writer.Write(firstProgenitorId); // ?
            writer.Write(nextProgenitorId); // ?
            writer.Write(firstHaloInFOFGroupId);
            writer.Write(nextHaloInFOFGroupId);

            writer.Write(snapnum);
            writer.Write((float)redshift);
            writer.Write(length);
            writer.Write(M_Crit200);
            writer.Write(M_Mean200);
            writer.Write(M_TopHat);

            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(ix);
            writer.Write(iy);
            writer.Write(iz);
            writer.Write(phkey);

            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);
            writer.Write(VelDisp);
            writer.Write(Vmax);
            writer.Write(vMaxRad);

            writer.Write(sx);
            writer.Write(sy);
            writer.Write(sz);
            writer.Write(mostboundid);
            writer.Write(fileNr);
            writer.Write(subhaloindex);
            writer.Write(subhaloFileId);
            writer.Write(subhalfmass);
            writer.Write(RandomInt);
            // MORE MORE MORE MORE

        }
    }

    public interface Trees
    {
        TreeInfo get(long i);
    }
    public class GalaxyTrees : Trees
    {
        private GalaxyInfo[] trees;
        public GalaxyTrees(GalaxyInfo[] _trees)
        {
            this.trees = _trees;
        }
        public TreeInfo get(long i)
        {
            return trees[i];
        }
    }

    public class TreedataTrees : Trees
    {
        private TreedataInfo[] trees;
        public TreedataTrees(TreedataInfo[] _trees)
        {
            this.trees = _trees;
        }
        public TreeInfo get(long i)
        {
            return trees[i];
        }
    }
    public interface TreeInfo
    {
        long Id
        {
            get;
        }
        long LastProgenitorId
        {
            get;
        }
        long MainLeafId
        {
            get;
            set;
        }
    }

    public struct GalaxyInfo : TreeInfo
    {
        public long GalID;
        public long Id
        {
            get { return GalID; }
        }
        public long HaloID;


        public long FirstProgGal;
        public long NextProgGal;
        public long LastProgGal;
        public long LastProgenitorId
        {
            get { return LastProgGal; }
        }
        public long mainLeafId;
        public long MainLeafId
        {
           get { return mainLeafId; }

           set { mainLeafId = value; }
        }
        public long FileTreeNr;
        public long DescendantGal;
        public long SubID      ;
        public long MMSubID   ; // ?
        public int PeanoKey   ;
        public float Redshift;
        public int Type;
        public int SnapNum ;
        public float CentralMvir;//           : 0.0, $
        public float x, y, z;//             : fltarr(3), $
        public int ix, iy, iz;//             : fltarr(3), $
        public float vx, vy, vz;//             : fltarr(3), $
//        public float  Spin    ;//              : 0.0, $
        public int Len      ;//             : 0L, $
        public float Mvir;//             : 0.0, $
        public float Rvir;//             : 0.0, $
        public float Vvir;//             : 0.0, $
        public float Vmax;//             : 0.0, $
        public float InfallRadius;//               : 0.0, $   ; radius just before galaxy falls in
        public float Infallmass;//         : 0.0, $         ; mass just before galaxy falls in
        public float Orimergetime;//          :0.0, $
        //   public long Hotradius  ;//            : 0.0, $
        //     public long retainfrac ;//          :0.0,$
        public float ColdGas;//       : 0.0, $
//          public long  ColdAvailable ;//        : 0.0, $
        public float StellarMass;//         : 0.0, $
        public float BulgeMass;// ;//         : 0.0, $
        public float HotGas;//            : 0.0, $
        public float EjectedMass;//          : 0.0, $
        public float BlackHoleMass;//         : 0.0, $
       //   public float  major      ;//         : 0.0, $
       //  public float minor       ;//        : 0.0, $
       //   public int majorsnap   ;//        : 0L,   $
        public float MetalsColdGas  ;//       : 0.0, $
        public float MetalsStellarMass  ;//   : 0.0, $
        public float MetalsBulgeMass   ;//    : 0.0, $
        public float  MetalsHotGas     ;//     : 0.0, $
        public float MetalsEjectedMass ;//    : 0.0, $
        public float Sfr     ;//              : 0.0, $
        public float SfrBulge  ;//            : 0.0, $
        public float MergeMass  ;//           :0.0, $
        public float reheatedMass  ;//        :0.0, $
        public float reheatedMassfake ;//     :0.0, $
        public float dEjectedmass;//          :0.0, $
//          public float  StB       ;//           : 0.0, $
        public float XrayLum    ;//           : 0.0, $
        public float BulgeSize   ;//          :0.0, $    ; half mass radius for bulge
        public float Stellardisk  ;//         :0.0,  $   ;3 times scale length for star disk
        public float   GasDiskRadius ;//        : 0.0, $ ; 3 times scale length for gas disk
        public float CoolingRadius   ;//      : 0.0, $
        public float[] Mag          ;//         : fltarr(5),  $
        public float[] MagBulge    ;//          : fltarr(5),  $
        public float[] MagDust    ;//           : fltarr(5),  $
        public float MassWeightAge  ;//       : 0.0 ,        $
        public int RandomInt;

        public float MergeSat;
        public float ICM;
        public float MetalsICM;
        public int DisruptionOn;
        public int MergeOn;


        public override string ToString()
        {
            return GalID + "," + x + "," + y + "," + z;
        }
        public void WriteBinary(BinaryWriter writer)
        {
            writer.Write(GalID);
            writer.Write(HaloID);
            writer.Write(-1L); // dummy fofId
            writer.Write(-1L); // dummy subhaloIdId
            writer.Write(SubID);
            writer.Write(DescendantGal);
            writer.Write(LastProgGal);
            writer.Write(mainLeafId);
            writer.Write(FirstProgGal);
            writer.Write(NextProgGal);
            writer.Write(FileTreeNr);
            writer.Write(SnapNum);
            writer.Write(Redshift);
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(ix);
            writer.Write(iy);
            writer.Write(iz);
            writer.Write(PeanoKey);
            writer.Write(vx);
            writer.Write(vy);
            writer.Write(vz);
            writer.Write(Type);
            writer.Write(Len)      ;//             : 0L, $
            writer.Write(CentralMvir);
            writer.Write(Mvir);//             : 0.0, $
            writer.Write(Rvir);//             : 0.0, $
            writer.Write(Vvir);//             : 0.0, $
            writer.Write(Vmax);//             : 0.0, $
            writer.Write(InfallRadius);//               : 0.0, $   ; radius just before galaxy falls in
            writer.Write(Infallmass);//         : 0.0, $         ; mass just before galaxy falls in
            writer.Write(Orimergetime);//          :0.0, $
            writer.Write(MergeSat);//          :0.0, $
            writer.Write(ColdGas);//       : 0.0, $
            writer.Write(StellarMass);//         : 0.0, $
            writer.Write(BulgeMass);// ;//         : 0.0, $
            writer.Write(HotGas);//            : 0.0, $
            writer.Write(EjectedMass);//          : 0.0, $
            writer.Write(BlackHoleMass);//         : 0.0, $
            writer.Write(MetalsColdGas)  ;//       : 0.0, $
            writer.Write(MetalsStellarMass ) ;//   : 0.0, $
            writer.Write(MetalsBulgeMass)   ;//    : 0.0, $
            writer.Write(MetalsHotGas)     ;//     : 0.0, $
            writer.Write(MetalsEjectedMass) ;//    : 0.0, $
            writer.Write(Sfr)     ;//              : 0.0, $
            writer.Write(SfrBulge)  ;//            : 0.0, $
            writer.Write(MergeMass ) ;//           :0.0, $
            writer.Write(XrayLum)    ;//           : 0.0, $
            writer.Write(BulgeSize)   ;//          :0.0, $    ; half mass radius for bulge
            writer.Write(Stellardisk)  ;//         :0.0,  $   ;3 times scale length for star disk
            writer.Write(GasDiskRadius) ;//        : 0.0, $ ; 3 times scale length for gas disk
            writer.Write(CoolingRadius )  ;//      : 0.0, $
            writer.Write(DisruptionOn);
            writer.Write(MergeOn);
            writer.Write(ICM);
            writer.Write(MetalsICM);
            writer.Write(RandomInt);
            
            for(int i = 0; i < Mag.Length; i++)
              writer.Write(Mag[i])          ;//         : fltarr(5),  $
            for(int i = 0; i < MagBulge.Length; i++)
              writer.Write(MagBulge[i])          ;//         : fltarr(5),  $
            for(int i = 0; i < MagDust.Length; i++)
              writer.Write(MagDust[i])          ;//         : fltarr(5),  $
            writer.Write(MassWeightAge)  ;//       : 0.0 ,        $

            
        }

    
        public void WriteCSV(TextWriter writer)
        {
            writer.Write(GalID);
            writer.Write(",");
            writer.Write(HaloID);
            writer.Write(",");
            writer.Write(-1L); // dummy fofId
            writer.Write(",");
            writer.Write(-1L); // dummy subhaloIdId
            writer.Write(",");
            writer.Write(SubID);
            writer.Write(",");
            writer.Write(DescendantGal);
            writer.Write(",");
            writer.Write(LastProgGal);
            writer.Write(",");
            writer.Write(mainLeafId);
            writer.Write(",");
            writer.Write(FirstProgGal);
            writer.Write(",");
            writer.Write(NextProgGal);
            writer.Write(",");
            writer.Write(FileTreeNr);
            writer.Write(",");
            writer.Write(SnapNum);
            writer.Write(",");
            writer.Write(Redshift);
            writer.Write(",");
            writer.Write(x);
            writer.Write(",");
            writer.Write(y);
            writer.Write(",");
            writer.Write(z);
            writer.Write(",");
            writer.Write(ix);
            writer.Write(",");
            writer.Write(iy);
            writer.Write(",");
            writer.Write(iz);
            writer.Write(",");
            writer.Write(PeanoKey);
            writer.Write(",");
            writer.Write(vx);
            writer.Write(",");
            writer.Write(vy);
            writer.Write(",");
            writer.Write(vz);
            writer.Write(",");
            writer.Write(Type);
            writer.Write(",");
            writer.Write(Len);//             : 0L, $
            writer.Write(",");
            writer.Write(CentralMvir);
            writer.Write(",");
            writer.Write(Mvir);//             : 0.0, $
            writer.Write(",");
            writer.Write(Rvir);//             : 0.0, $
            writer.Write(",");
            writer.Write(Vvir);//             : 0.0, $
            writer.Write(",");
            writer.Write(Vmax);//             : 0.0, $
            writer.Write(",");
            writer.Write(InfallRadius);//               : 0.0, $   ; radius just before galaxy falls in
            writer.Write(",");
            writer.Write(Infallmass);//         : 0.0, $         ; mass just before galaxy falls in
            writer.Write(",");
            writer.Write(Orimergetime);//          :0.0, $
            writer.Write(",");
            writer.Write(MergeSat);//          :0.0, $
            writer.Write(",");
            writer.Write(ColdGas);//       : 0.0, $
            writer.Write(",");
            writer.Write(StellarMass);//         : 0.0, $
            writer.Write(",");
            writer.Write(BulgeMass);// ;//         : 0.0, $
            writer.Write(",");
            writer.Write(HotGas);//            : 0.0, $
            writer.Write(",");
            writer.Write(EjectedMass);//          : 0.0, $
            writer.Write(",");
            writer.Write(BlackHoleMass);//         : 0.0, $
            writer.Write(",");
            writer.Write(MetalsColdGas);//       : 0.0, $
            writer.Write(",");
            writer.Write(MetalsStellarMass);//   : 0.0, $
            writer.Write(",");
            writer.Write(MetalsBulgeMass);//    : 0.0, $
            writer.Write(",");
            writer.Write(MetalsHotGas);//     : 0.0, $
            writer.Write(",");
            writer.Write(MetalsEjectedMass);//    : 0.0, $
            writer.Write(",");
            writer.Write(Sfr);//              : 0.0, $
            writer.Write(",");
            writer.Write(SfrBulge);//            : 0.0, $
            writer.Write(",");
            writer.Write(MergeMass);//           :0.0, $
            writer.Write(",");
            writer.Write(XrayLum);//           : 0.0, $
            writer.Write(",");
            writer.Write(BulgeSize);//          :0.0, $    ; half mass radius for bulge
            writer.Write(",");
            writer.Write(Stellardisk);//         :0.0,  $   ;3 times scale length for star disk
            writer.Write(",");
            writer.Write(GasDiskRadius);//        : 0.0, $ ; 3 times scale length for gas disk
            writer.Write(",");
            writer.Write(CoolingRadius);//      : 0.0, $
            writer.Write(",");
            writer.Write(DisruptionOn);
            writer.Write(",");
            writer.Write(MergeOn);
            writer.Write(",");
            writer.Write(ICM);
            writer.Write(",");
            writer.Write(MetalsICM);
            writer.Write(",");
            writer.Write(RandomInt);
            
            for(int i = 0; i < Mag.Length; i++)
              writer.Write(Mag[i])          ;//         : fltarr(5),  $
            for(int i = 0; i < MagBulge.Length; i++)
              writer.Write(MagBulge[i])          ;//         : fltarr(5),  $
            for(int i = 0; i < MagDust.Length; i++)
              writer.Write(MagDust[i])          ;//         : fltarr(5),  $
            writer.Write(MassWeightAge)  ;//       : 0.0 ,        $

            writer.Write("\r\n");
        }

    }

    public struct ParticleGroupID
    {
        public Int16 snapnum;
        public Int64 particleId;
        public Int64 subhaloId;
        public Int64 fofId;

        public override string ToString()
        {
            return snapnum + "," + particleId + "," + fofId + "," + subhaloId;
        }
        public void WriteBinary(BinaryWriter writer)
        {
            writer.Write(snapnum);
            writer.Write(particleId);
            writer.Write(fofId);
            writer.Write(subhaloId);
        }
    }    
    
    public class ParticleComparator : IComparer<Structs>
    {
        public int Compare(Structs one, Structs two)
        {
            int comp = one.snapnum.CompareTo(two.snapnum);
            if (comp == 0)
                comp = one.phkey.CompareTo(two.phkey);
            if (comp == 0)
                comp = one.id.CompareTo(two.id);
            return comp;
        }

    }

    public class TreeComparator : IComparer<TreedataInfo>
    {
        public int Compare(TreedataInfo one, TreedataInfo two)
        {
            return one.haloId.CompareTo(two.haloId);
        }
    }
    public class GalaxyComparator : IComparer<GalaxyInfo>
    {
        public int Compare(GalaxyInfo one, GalaxyInfo two)
        {
            return one.GalID.CompareTo(two.GalID);
        }
    }

    public class ParticleGroupIDComparator : IComparer<ParticleGroupID>
    {
        public int Compare(ParticleGroupID one, ParticleGroupID two)
        {
            int comp = one.snapnum.CompareTo(two.snapnum);
            if (comp == 0)
                comp = one.fofId.CompareTo(two.fofId);
            if (comp == 0)
                comp = one.subhaloId.CompareTo(two.subhaloId);
            return comp;
        }
    }
}
/*
namespace millimil
{
    class Program
    {
        static void Main(string[] args)
        {
//            writeScripts(@"d:\data\millimil\bulkinsert.sql", @"d:\data\millimil\transform.bat");
//            TransformSnapshot(args);
            TransformGroupIDs(new string[]{@"D:\data\millimil\ids\","50"});
        }

        
        public static void TransformGroupIDs(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: millimil.exe rootdir snapnum");
                return;
            }
            string rootdir = args[0];
            string snapnum = args[1];
            short l_snapnum = short.Parse(snapnum);
            long prefix = l_snapnum*1000000000000L;

            if (snapnum.Length == 1)
                snapnum = "00" + snapnum;
            else if (snapnum.Length == 2)
                snapnum = "0" + snapnum;


            string subhaloDir = rootdir + "/postproc_" + snapnum;
            string fofFile = subhaloDir + "/group_tab_" + snapnum + ".";
            string subhaloFile = subhaloDir + "/sub_tab_" + snapnum + ".";
            string idsFile = subhaloDir + "/sub_ids_" + snapnum + ".";
            
    
            for(int ifile = 0; ifile < 8; ifile++)
            {
                long idPrefix = prefix+((long)ifile)*100000000L;

                using(BinaryReader fofreader = new BinaryReader(new FileStream(fofFile+ifile,FileMode.Open)))
                using(BinaryReader subhaloreader = new BinaryReader(new FileStream(subhaloFile+ifile,FileMode.Open)))
                using (BinaryReader idreader = new BinaryReader(new FileStream(idsFile + ifile, FileMode.Open)))
                using(BinaryWriter binwriter = new BinaryWriter(new FileStream(idsFile + ifile+".mssqlserver",FileMode.Create)))
                //using(StreamWriter csvwriter = new StreamWriter(new FileStream(idsFile + ifile+".csv",FileMode.Create)))
                {

                    int Ngroups = fofreader.ReadInt32();
                    int Nids = fofreader.ReadInt32();
                    int TotNgroups = fofreader.ReadInt32();
                    int Nfiles = fofreader.ReadInt32();
                    int[] GroupLen = new int[Ngroups];
                    for(int i = 0; i < Ngroups; i++)
                        GroupLen[i] = fofreader.ReadInt32();

                    Ngroups = subhaloreader.ReadInt32();
                    Nids = subhaloreader.ReadInt32();
                    TotNgroups = subhaloreader.ReadInt32();
                    Nfiles = subhaloreader.ReadInt32();
                    int Nsubs = subhaloreader.ReadInt32();

                    int[] NsubPerHalo = new int[Ngroups];
                    int[] FirstSubOfHalo = new int[Ngroups];
                    for (int i = 0; i < Ngroups; i++)
                        NsubPerHalo[i] = subhaloreader.ReadInt32();
                    for (int i = 0; i < Ngroups; i++)
                        FirstSubOfHalo[i] = subhaloreader.ReadInt32();

                    int[] SubLen = new int[Nsubs];
                    int[] SubOffset = new int[Nsubs];
                    int[] SubParentHalo = new int[Nsubs];
                    int sum = 0;
                    for (int i = 0; i < Nsubs; i++)
                    {
                        SubLen[i] = subhaloreader.ReadInt32();
                        sum += SubLen[i];
                    }
                    for (int i = 0; i < Nsubs; i++)
                        SubOffset[i] = subhaloreader.ReadInt32();
                    for (int i = 0; i < Nsubs; i++)
                        SubParentHalo[i] = subhaloreader.ReadInt32();


                    Console.WriteLine("Snapnum = {0}, file = {1}, Nids = {2}, Sum = {3}", snapnum, ifile, Nids, sum);

                    Ngroups = idreader.ReadInt32();
                    Nids  = idreader.ReadInt32();
                    TotNgroups = idreader.ReadInt32();
                    int NTask = idreader.ReadInt32();


                    byte[] bytes = idreader.ReadBytes(8*Nids);
                    int bytecount = 0;
                    int count = 0;
                    ParticleGroupID[] ids = new ParticleGroupID[Nids];
                    long subHaloId = -1;
                    int igroup = -1;
                    long fofId = -1;

                    int isub = 0;
                    for(int ifof = 0; ifof < Ngroups; ifof++)
                    {
                        int fofCount = 0;
                        int nsub = NsubPerHalo[ifof];
                        for (int ifofsub = 0; ifofsub < nsub; ifofsub++)
                        {
                            subHaloId = idPrefix + isub;
                            igroup = SubParentHalo[isub];
                            fofId = idPrefix + igroup;
                            for (int i = 0; i < SubLen[isub]; i++)
                            {
                                ids[count] = new ParticleGroupID();
                                ids[count].snapnum = l_snapnum;
                                ids[count].fofId = fofId;
                                ids[count].subhaloId = subHaloId;
                                ids[count].particleId = BitConverter.ToInt64(bytes, bytecount);
                                bytecount += 8;
                                count++;
                                fofCount++;
                            }
                            isub++;
                        }
                        for (int i = fofCount; i < GroupLen[ifof]; i++)
                        {
                            ids[count] = new ParticleGroupID();
                            ids[count].snapnum = l_snapnum;
                            ids[count].fofId = fofId;
                            ids[count].subhaloId = -1;
                            ids[count].particleId = BitConverter.ToInt64(bytes, bytecount);
                            bytecount += 8;
                            count++;
                        }
                    }
                    Array.Sort(ids, new ParticleGroupIDComparator());
                    for (int i = 0; i < ids.Length; i++)
                    {
                        ids[i].writeBin(binwriter);
//                        csvwriter.Write(ids[i].ToString());
//                        csvwriter.Write("\r\n");
                    }

                }
            }
        }

        public static int GetPHKey(int bits, double boxinv, Particle p)
        {
            int ix = (int)Math.Floor(p.x * boxinv);
            int iy = (int)Math.Floor(p.y * boxinv);
            int iz = (int)Math.Floor(p.z * boxinv);
            return (int)PeanoHilbertID.GetPeanoHilbertID(bits, ix, iy, iz);
        }

        public static void writeScripts(string outfile, string transformFile)
        {
            using(StreamWriter writer = new StreamWriter(new FileStream(outfile, FileMode.Create)))
            using (StreamWriter writer2 = new StreamWriter(new FileStream(transformFile, FileMode.Create)))
            {
                for (int i = 0; i <= 63; i++)
                {
                    string s = "";
                    if (i <= 9)
                        s = "0";
                    for (int k = 0; k <= 7; k++)
                    {
                        writer2.WriteLine(@"D:\VS.CS\millimil\millimil.exe L:\\data\\millimil\\snapshots\\snap_milli_0{0}{1}.{2} {1}", s, i, k);
                        writer.WriteLine("bulk insert MillimilSnapshots from 'L:\\data\\millimil\\snapshots\\snap_milli_0{0}{1}.{2}.mssqlserver' with(datafiletype='native', tablock)", s, i, k);
                    }
                }
            }

        }
    }
   
}
*/