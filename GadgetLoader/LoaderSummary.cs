using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace GadgetLoader
{
    class LoaderSummary
    {

        private string summaryFile;
        private string snapBCPCommands;
        private string FOFBCPCommands;
        private string FFTBCPCommands;
        private string IndexBCPCommands;
        private string bcpFile;
        private string database;
        private string server;
        private string snapTable;
        private string fofTable;
        private string fftTable;
        private string indexTable;
        public List<SnapFileSummary> snapFileSummaries;
        public FOFSummary fofSummary;
        //public FileSummary[] snapFileSummaries;
        public string errorString;

        public LoaderSummary(string s, string db)
        {
            summaryFile = null;
            errorString = String.Empty;
            snapFileSummaries = null;
            snapBCPCommands = "";
            FOFBCPCommands = "";
            FFTBCPCommands = "";
            IndexBCPCommands = "";
            bcpFile = "";
            server = s;
            database = db;
            snapTable = LoaderParamSingleton.getInstance().snapTable;
            fofTable = LoaderParamSingleton.getInstance().FOFTable;
            fftTable = LoaderParamSingleton.getInstance().FFTTable;
            indexTable = LoaderParamSingleton.getInstance().indexTable;
        }

        public LoaderSummary(string outfile)
        {
            summaryFile = outfile;
        }

        public void setBCPFile(string file)
        {
            bcpFile = file;
        }

        public string getBCPfile()
        {
            return bcpFile;
        }

        public void setFile(string path)
        {
            summaryFile = path;
        }

        public void AddSnapBCPCommand(string filename)
        {
            snapBCPCommands += "bcp " + database + "." + snapTable + " in " + filename
                + " -n -S " + server + " -T\r\n";
        }

        public void AddIndexBCPCommand(string filename)
        {
            IndexBCPCommands += "bcp " + database + "." + indexTable + " in " + filename
                + " -n -S " + server + " -T\r\n";


        }

        public void AddFOFBCPCommand(string filename)
        {
            FOFBCPCommands += "bcp " + database + "." + fofTable + " in " + filename
                + " -n -S " + server + " -T\r\n";
        }

        public void AddFFTBCPCommand(string filename)
        {
            snapBCPCommands += "bcp " + database + "." + fftTable + " in " + filename
                + " -n -S " + server + " -T\r\n";
        }

        public void setFileSummaries(List<SnapFileSummary> s)
        {
            snapFileSummaries = s;
        }

        //returns true if no errors, else returns false
        public bool checkFileErrors()
        {
            if (snapFileSummaries != null)
            {
                foreach(SnapFileSummary f in snapFileSummaries)
                {
                    if (f.badStatus)
                        return false;
                }
                return true;
            }
            else
                return false;
        }

        public string toString()
        {
            if (snapFileSummaries == null)
            {
                return errorString;
            }
            else
            {
                string wholeSummary = errorString + "\r\n";
                foreach (SnapFileSummary f in snapFileSummaries)
                {
                    wholeSummary = wholeSummary + f.ToString();
                }
                if (fofSummary != null)
                    wholeSummary += fofSummary.ToString();

                return wholeSummary;
            }
        }

        public void addError(string s)
        {
            errorString = errorString + "\r\n" + s;
        }

        public void writeBCPFile()
        {
            using (StreamWriter writer = new StreamWriter(bcpFile))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine(snapBCPCommands);
                writer.WriteLine(FOFBCPCommands);
                writer.WriteLine(FFTBCPCommands);
                writer.WriteLine(IndexBCPCommands);

            }
        }

        public void writeSummary()
        {
            if (summaryFile == null)
            {
                Console.WriteLine(this.toString());
            }
            else
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(new FileStream(summaryFile, FileMode.Create)))
                    {
                        writer.WriteLine(this.toString());

                    }
                }
                catch (Exception e)
                {
                    this.addError(e.Message);
                    Console.WriteLine(this.toString());
                }
            }

        }
    }


    public class SnapFileSummary
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public TimeSpan duration { get; set; }
        public uint numParticles { get; set; }
        public string inFileName { get; set; }
        public string outFileName { get; set; }
        public string indexFileName { get; set; }
        public string status { get; set; }
        public bool warning { get; set; }
        public string warningMessage { get; set; }
        public bool badStatus { get; set; }
        public string statusMessage { get; set; }
        //public string summaryFileName { get; set; }
        public string fullPath { get; set; }
        public int fileNumber { get; set; }

        public SnapFileSummary()
        {
            warning = false;
            badStatus = false;

        }


        public override string ToString()
        {
            string val =
                "File " + fileNumber + ":\r\n\t" +
                "Start: " + start.ToString() +",\r\n\t" +
                "End: " + end.ToString() + ",\r\n\t" +
                "Time: " + duration.TotalSeconds.ToString() + ",\r\n\t" +
                "Points Contained: " + numParticles + ",\r\n\t" +
                "Input " + inFileName + ",\r\n\t" +
                "Output " + outFileName + ",\r\n" + status;

            if (warning)
            {
                val = val + "Warning: " + warningMessage + "\r\n";
            }
            if (badStatus)
            {
                val = val + "Error: " + statusMessage + "\r\n";
            }
            return val;
        }
    }


    public class FOFSummary
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public TimeSpan duration { get; set; }
        public string status { get; set; }
        public bool warning { get; set; }
        public string warningMessage { get; set; }
        public bool badStatus { get; set; }
        public string statusMessage { get; set; }
        public string summaryFileName { get; set; }
        public string fullPath { get; set; }
        public int fileNumber { get; set; }
        public int numGroups { get; set; }

        public FOFSummary()
        {
            warning = false;
            badStatus = false;

        }


        public override string ToString()
        {
            string val =
                "Start: " + start.ToString() + ",\r\n\t" +
                "End: " + end.ToString() + ",\r\n\t" +
                "Time: " + duration.TotalSeconds.ToString() + ",\r\n\t" +
                "Number of Groups: " + numGroups + "\r\n\t";

            if (warning)
            {
                val = val + "\r\nWarning: " + warningMessage + "\r\n";
            }
            if (badStatus)
            {
                val = val + "\r\nError: " + statusMessage + "\r\n";
            }
            return val;
        }
    }
}
