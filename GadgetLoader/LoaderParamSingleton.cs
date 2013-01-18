using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace GadgetLoader
{
    class LoaderParamSingleton
    {
        //hardocde these for now, they can be more intelligently determined later
        /*internal const int bloomfilterSize = 19171;
        internal const int numberHashFunctions = 27;
        internal const int expectedSize = 500;*/

        private static LoaderParamSingleton instance;
        public const int SNAPS_IN_SIMULATION = 64;


        /* SqlConnection configConn = new SqlConnection(LoaderParamSingleton.getInstance().configTableConnectionString);
                configConn.Open();

                SqlDataReader myReader = null;
                string command = "select * from dbo.config where snapnum = @snapnum";
                SqlCommand myCommand = new SqlCommand(command, configConn);
                SqlParameter timestepParam = new SqlParameter("@snapnum", System.Data.SqlDbType.SmallInt);
                timestepParam.Value = opts.timestep;
                myCommand.Parameters.Add(timestepParam);
                myReader = myCommand.ExecuteReader();

                //Can add any other properties we want to read from DB here
                int numLines = 0;
                while (myReader.Read())
                {
                    inpath = myReader[1].ToString();
                    outpath = myReader[2].ToString();
                    numLines++;
                }

                configConn.Close();
         * 
         */

        private LoaderParamSingleton(CommandLineOptions opts)
        {
            database = opts.database;
            server = opts.server;
            sim = opts.sim;
            firstSnapLoaded = opts.firstSnap;
            snapTable = "dbo." + sim + "_ParticleData";
            FOFTable = "dbo." + sim + "_FoFGroupsData";
            FFTTable = "dbo." + sim + "_FFTData";
            if (firstSnapLoaded)
            {
                indexTable = "dbo." + sim + "_ReverseIndex";
            }
            else
            {
                indexTable = "dbo." + sim + "_RIinsert_" + opts.timestep;
            }
            try
            {
                SqlConnection paramConn = new SqlConnection(createConnString(opts.database, opts.server));
                paramConn.Open();

                SqlDataReader myReader = null;
                string command = "select top 1 * from dbo.params";
                SqlCommand myCommand = new SqlCommand(command, paramConn);
                myReader = myCommand.ExecuteReader();
                int numLines = 0;
                while (myReader.Read())
                {
                    phbits = Int32.Parse(myReader["phbits"].ToString());
                    boxSize = Int32.Parse(myReader["boxSize"].ToString());
                    //snapStartFile = Int32.Parse(myReader["snapStartFile"].ToString());
                    //snapEndFile = Int32.Parse(myReader["snapEndFile"].ToString());
                    snapFilePrefix = myReader["snapFilePrefix"].ToString();
                    writeArrays = Boolean.Parse(myReader["writeArrays"].ToString());
                    particlesPerSnap = Int64.Parse(myReader["particlesPerSnap"].ToString());
                    groupTabPrefix = myReader["groupTabPrefix"].ToString();
                    groupIDPrefix = myReader["groupIDPrefix"].ToString();
                    //fofFirstSnapshot = Int32.Parse(myReader["fofFirstSnapshot"].ToString());
                    //fofLastSnapshot = Int32.Parse(myReader["fofLastSnapshot"].ToString());
                    //fftFilePrefix = myReader["fftFilePrefix"].ToString();
                    //fftInPath = myReader["fftInPath"].ToString();
                    //fftOutPath = myReader["fftOutPath"].ToString();
                    //fftFileExtension = myReader["fftFileExtension"].ToString();
                    numLines++;
                }
                if (numLines != 1)
                {
                    throw new ConfigurationException("Bad parameter data in DB");
                }
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Bad parameter data in DB");
            }


        }

        public static LoaderParamSingleton getInstance(CommandLineOptions opts)
        {
            if (instance == null)
                instance = new LoaderParamSingleton(opts);
            return instance;
        }

        public static LoaderParamSingleton getInstance()
        {
            if (instance == null)
                throw new ConfigurationException("Parameters used before being loaded from DB.");
            else
                return instance;
        }

        internal long particlesPerSnap;// = 134217728; //512^3
        //GLOBAL
        internal int phbits; // = 6;
        internal int boxSize; // = 1000;
        //internal string configTableConnectionString = "server=localhost;database=SimDBdev;Trusted_Connection=true;Asynchronous Processing = true";

        

        //SNAPSHOT particle data
        //internal int snapStartFile; // = 0;
        //internal int snapEndFile; // = 2;
        internal string snapFilePrefix; // = "snapshot_";
        internal Boolean writeArrays; // = true;

        //FOF
        internal string groupTabPrefix; // = "grouptab_";
        internal string groupIDPrefix; // = "groupid_";
        //internal int fofStartFile; // = 0;
        //internal int fofEndFile; // = 31;
        internal int fofFirstSnapshot; // = 0;
        internal int fofLastSnapshot; // = 63;

        //FFT
        internal string fftFilePrefix; // = "";
        internal string fftInPath; // = "";
        internal string fftOutPath; // = "";
        internal string fftFileExtension; // = ".dat";
        //internal int fftFirstSnapshot; // = 0;
        //internal int fftLastSnapshot; // = 188;

        //LOCAL data
        internal string database;
        internal string server;
        internal string snapTable;
        internal string FOFTable;
        internal string FFTTable;
        internal string sim;
        internal string indexTable;

        internal Boolean createReverseIndex = true;
        internal Boolean firstSnapLoaded;

        public static string createConnString(string db, string server)
        {
            return "server=" + server + ";database=" + db + ";Trusted_Connection=true;Asynchronous Processing = true";
        }

    }
}
