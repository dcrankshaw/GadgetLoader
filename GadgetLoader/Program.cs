using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using NDesk.Options;
using System.Data.SqlClient;
using System.IO;







 
namespace GadgetLoader
{
    static class Program
    {
        // from http://stackoverflow.com/questions/7198639/c-application-both-gui-and-commandline
        // This makes having a GUI optional
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        [STAThread]
        static void Main(string[] args)
        {
            AttachConsole(ATTACH_PARENT_PROCESS);

            CommandLineOptions opts = new CommandLineOptions();
            
            //defines the command line parameters
            var par = new OptionSet()
            {
                //need to throw option exceptions for all options that have non-optionable values
                { "g|gui", "launches the gui, all other options are ignored",
                    v => {if (v != null) opts.gui = true; } },

                { "z|timestep=", "sets the {TIMESTEP} to transform",
                    (short v) => {opts.timestep = v; } },
                { "1|firstsnap", "true if this is the first snapshot loaded for this simulation",
                    v => {opts.firstSnap = true; } },   // DEPRECATED OPTION
                /*{ "d|database=", "dictates what database the data will be loaded into",
                    v => {opts.database = v; } },
                { "r|server=", "dictates what server the database is on",
                    v => {opts.server = v; } },*/
                { "t|fft", "loads the fft data",
                    v => {opts.fft = true; } },
                { "f|fof", "loads the fof data",
                    v => {opts.fof = true; } },
                { "s|snap", "loads the particle data",
                    v => {opts.snap = true; } },
                /*{ "p|phbits=", "sets the {NUMBER} of bits to use when calculating Peano-Hilbert index",
                    (int v) => {opts._phbits = v; } },
                { "n|nzones=", "sets the {NUMBER} of zones",
                    (int v) => { opts._nzones = v; } },
                { "b|box=", "sets the {SIZE} of the simulation box",
                    (int v) => { opts._boxsize = v; } },*/
                { "m|sim=", "the {NAME} of the simulation this timestep belongs to",
                    v => { if (v == null)
                                    throw new OptionException ("Missing the simulation name for option -sim", "-sim");
                                 opts.sim = v; } },
                { "v|server=", "the name of the database {SERVER}",
                    v => { if (v == null)
                                    throw new OptionException ("Missing the server name for for option -server", "-server");
                                 opts.server = v; } },
                { "d|database=", "the {NAME} of the SQL database",
                    v => { if (v == null)
                        throw new OptionException("Missing the database name for for option -database", "-database");
                                 opts.database = v; } },
                { "q|sqlcf=", "sets the {NAME} of the SQL command file to write to",
                    v => { if (v == null)
                                    throw new OptionException ("Missing the name of the SQL commands file for option -sqlcf", "-sqlcf");
                                 opts.sqlCF = v; } },
                { "h|help", "Show this message and exit",
                    v => opts.show_help = v != null },
            };
            
            
            if (args.Length > 0)
            {
                try
                {
                    par.Parse(args);
                }
                    // TODO: SUMMARY
                catch (OptionException e)
                {
                    Console.Write("GadgetLoader: ");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Try 'GadgetLoader --help' for more information.");
                    return;
                }

                if (!opts.validOptions())
                {
                    ShowHelp(par);
                    return;
                }

                if (opts.show_help)
                {
                    ShowHelp(par);
                    return;
                }

                //creates a gui
                if (opts.gui)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmMain());
                }
                else
                {
                    Runner runner = new Runner(opts.sqlCF);
                    LoaderParamSingleton.getInstance(opts);
                    GlobalParameters globalParameters = RefreshGlobalParameters(opts);

                    try
                    {
                        RefreshProcess(globalParameters, runner, opts);
                        runner.Run();
                    }

                    catch (Exception e)
                    {
                        globalParameters.summary.addError(e.Message);
                    }
                    finally
                    {
                        globalParameters.summary.writeSummary();
                        globalParameters.summary.writeBCPFile();
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    }
                }

            }
            //If no options, run gui
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
            }
        }

        /// <summary>
        /// Creates the help string to display on the command line
        /// </summary>
        /// <param name="p">The OptionSet detailing the command line args</param>
        public static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: GadgetLoader [OPTIONS]");
            Console.WriteLine("If no arguments specified, the GUI is launched\n");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            System.Windows.Forms.SendKeys.SendWait("{ENTER}");
        }

        #region Process Initialization




       
        /// <summary>
        /// Create new Process and initialize with parsed command line parameters
        /// </summary>
        /// <param name="gp">The GlobalParameters object containing global state</param>
        /// <param name="runner">The Runner object to execute the process</param>
        /// <param name="opts">The command line options</param>
        private static void RefreshProcess(GlobalParameters gp, Runner runner, CommandLineOptions opts)
        {
            string inpath = "";
            string outpath = "";

            //read snapshot configuration from database
            try
            {

                SqlConnection configConn = new SqlConnection(LoaderParamSingleton.createConnString(opts.database, opts.server));
                configConn.Open();

                SqlDataReader myReader = null;
                string command = "select * from dbo.config where sim = @sim and snapnum = @snapnum";
                SqlCommand myCommand = new SqlCommand(command, configConn);
                SqlParameter timestepParam = new SqlParameter("@snapnum", System.Data.SqlDbType.SmallInt);
                SqlParameter simParam = new SqlParameter("@sim", System.Data.DbType.String);
                timestepParam.Value = opts.timestep;
                simParam.Value = opts.sim;
                myCommand.Parameters.Add(timestepParam);
                myCommand.Parameters.Add(simParam);
                myReader = myCommand.ExecuteReader();

                //Can add any other properties we want to read from DB here
                int numLines = 0;
                while (myReader.Read())
                {
                    //TODO: do i need to escape backslashes in the paths?
                    //I don't think so, it's been working fine so far
                    inpath = myReader["inpath"].ToString();
                    outpath = myReader["outpath"].ToString();
                    numLines++;
                }

                configConn.Close();


                if (numLines > 1)
                {
                    throw new ConfigurationException(
                        "Multiple entries matching this snapshot in configuration table");
                }
                else if (numLines < 1)
                {
                    throw new ConfigurationException(
                        "Zero entries matching this snapshot in configuration table");
                }



                gp.summary.setFile(outpath + "\\summary");




            }
            /*catch (Exception e)
            {
                Console.Out.WriteLine("Caugth DB exception");
                gp.summary.addError(e.Message);
                return;
            }*/

            catch (Exception)
            {
                throw;
            }

            if (opts.snap)
            {
                Console.WriteLine("Adding Snapshot process to runner");
                runner.Add(RefreshSnapshotsProcess(gp, inpath, outpath, opts));
            }
            if (opts.fof)
            {
                runner.Add(RefreshIndraFOFProcess(gp, inpath, outpath, opts));
            }
            if (opts.fft)
            {
                runner.Add(RefreshIndraFFTProcess(gp, inpath, outpath, opts));
            }

        }

        /// <summary>
        /// Creates a new SnapshotsProcess to process the main snapshot files
        /// </summary>
        /// <param name="gp">The global parameters</param>
        /// <param name="inpath">Where the snapshot files are located on disk</param>
        /// <param name="outpath">Where the processed files should be written to before
        /// being bulk loaded into the database</param>
        /// <param name="opts">The command line options</param>
        /// <returns>A new SnapshotsProcess ready to be executed</returns>
        private static SnapshotsProcess RefreshSnapshotsProcess(GlobalParameters gp, string inpath, string outpath, CommandLineOptions opts)
        {
            SnapshotsProcess process = new SnapshotsProcess(gp);

            process.inPath = inpath;
            process.outPath = outpath;

            process.snapshotFilePrefix = LoaderParamSingleton.getInstance().snapFilePrefix;
            
            /* If we don't set the first and last snapshot files manually, the loader
             * will read how many subfiles the snapshot is supposed to have and read
             * that many*/
            
            
            //process.firstSnapshotFile = LoaderParamSingleton.getInstance().snapStartFile;
            //process.lastSnapshotFile = LoaderParamSingleton.getInstance().snapEndFile;
            process.snapNumber = opts.timestep;

            //TODO: HARDCODED
            process.writeArrays = true;

            return process;
        }

        /// <summary>
        /// Creates a new IndraFofProcess to process the FoF files generated by Gadget
        /// </summary>
        /// <param name="gp">The global parameters</param>
        /// <param name="inpath">Where the fof files are located on disk</param>
        /// <param name="outpath">Where the processed files should be written to before
        /// being bulk loaded into the database</param>
        /// <param name="opts">The command line options</param>
        /// <returns>A new IndraFoFProcess ready to be executed</returns>
        private static IndraFOFProcess RefreshIndraFOFProcess(GlobalParameters gp, string inpath, string outpath, CommandLineOptions opts)
        {
            IndraFOFProcess process = new IndraFOFProcess(gp);
            process.inPath = inpath;
            process.outPath = outpath;
            process.groupTabFilePrefix = LoaderParamSingleton.getInstance().groupTabPrefix;
            process.groupIDFilePrefix = LoaderParamSingleton.getInstance().groupIDPrefix;
            //process.firstSnapshotFile = LoaderParamSingleton.getInstance().fofFirstSnapshot;
            //process.lastSnapshotFile = LoaderParamSingleton.getInstance().fofLastSnapshot;
            process.snapnumber = opts.timestep;
            return process;
        }

        /// <summary>
        /// Creates a new IndraFFTProcess to process the FFT files generated by Gadget
        /// </summary>
        /// <param name="gp">The global parameters</param>
        /// <param name="inpath">Where the FFT files are located on disk</param>
        /// <param name="outpath">Where the processed files should be written to before
        /// being bulk loaded into the database</param>
        /// <param name="opts">The command line options</param>
        /// <returns>A new IndraFFTProcess ready to be executed</returns>
        private static IndraFFTDataProcess RefreshIndraFFTProcess(GlobalParameters gp, string inpath, string outpath, CommandLineOptions opts)
        {
            IndraFFTDataProcess process = new IndraFFTDataProcess(gp);
            process.inPath = inpath;
            process.outPath = outpath;
            process.filePrefix = LoaderParamSingleton.getInstance().fftFilePrefix;
            process.fileExtension = LoaderParamSingleton.getInstance().fftFileExtension;
            process.snapnumber = opts.timestep;
            return process;
        }

        /// <summary>
        /// Creates a new GlobalParameters object containing the global state of the loader
        /// </summary>
        /// <param name="opts">The commandline options</param>
        /// <returns></returns>
        private static GlobalParameters RefreshGlobalParameters(CommandLineOptions opts)
        {
            return new GlobalParameters(LoaderParamSingleton.getInstance().phbits, 1, LoaderParamSingleton.getInstance().boxSize, 1, opts.sqlCF, opts.server, opts.database);
        }


        #endregion

        //TODO: Can eventually change this to write to error log
        //SUMMARY TODO
        /// <summary>
        /// Currently unused error message wrapper to easily change where the errors are recorded
        /// </summary>
        /// <param name="e"></param>
        public static void RecordError(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// CommandLineOptions class uses the NDesk.Options library
    /// http://www.ndesk.org/Options
    /// </summary>
    public class CommandLineOptions
    {
        public bool gui { get; set; }
        public bool fof { get; set; }
        public bool fft { get; set; }
        public bool snap { get; set; }
        public string sqlCF { get; set; }
        public bool show_help { get; set; }
        public short timestep {get; set; }
        public string server { get; set; }
        public string database { get; set; }
        public string sim { get; set; }
        public bool firstSnap { get; set; }

        public CommandLineOptions()
        {
            gui = false;
            fof = false;
            fft = false;
            snap = false;
            firstSnap = false;
            sqlCF = null;
            database = null;
            sim = null;
            server = null;
        }

        /// <summary>
        /// Validates that the set of arguments provided includes all required arguments
        /// </summary>
        /// <returns>True if a valid argument set, false otherwise</returns>
        public bool validOptions()
        {
            if (gui)
                return true;
            if (show_help)
                return true;
            else
            {
                if (sqlCF == null || database == null || sim == null || server == null)
                    return false;
                return true;
            }

        }
    }

    /// <summary>
    /// Various hardcoded parameters for the loader. No longer in use.
    /// </summary>
    internal static class LoaderParameters
    {

        internal const long particlesPerSnap = 134217728; //512^3

        //hardocde these for now, they can be more intelligently determined later
        internal const int bloomfilterSize = 19171;
        internal const int numberHashFunctions = 27;
        internal const int expectedSize = 500;

        //GLOBAL
        internal const int phbits = 6;
        internal const int boxSize = 1000;
        //internal const string configTableConnectionString = "server=localhost;database=SimDBdev;Trusted_Connection=true;Asynchronous Processing = true";
        
        internal const string database = "SimulationDB";
        internal const string server = "gw18";
        internal const string snapTable = "dbo.LoaderTests";
        internal const string FFTTable = "dbo.fft";
        internal const string FOFTable = "dbo.fof";
        internal const string configTableConnectionString = "server=" + server + ";database=" + database + ";Trusted_Connection=true;Asynchronous Processing = true";

        
        //SNAPSHOT particle data
        internal const int snapStartFile = 0;
        internal const int snapEndFile = 2;
        internal const string snapFilePrefix = "snapshot_";
        internal const Boolean writeArrays = true;

        //FOF
        internal const string groupTabPrefix = "grouptab_";
        internal const string groupIDPrefix = "groupid_";
        //internal const int fofStartFile = 0;
        //internal const int fofEndFile = 31;
        internal const int fofFirstSnapshot = 0;
        internal const int fofLastSnapshot = 63;

        //FFT
        internal const string fftFilePrefix = "";
        internal const string fftInPath = "";
        internal const string fftOutPath = "";
        internal const string fftFileExtension = ".dat";
        //internal const int fftFirstSnapshot = 0;
        //internal const int fftLastSnapshot = 188;

    }
}