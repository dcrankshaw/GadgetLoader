using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GadgetLoader
{
    public partial class frmMain : Form
    {
        GlobalParameters globalParameters;
        Runner runner;

        List<Control> enabledControls = new List<Control>();

        public frmMain()
        {
            InitializeComponent();
            //lbSimulation.SetSelected(0, true);
            foreach (Control c in Controls)
                if (c.Enabled)
                    enabledControls.Add(c);
        }
#region Control flow    
     
        private void btnGo_Click(object sender, EventArgs e)
        {
            if (btnGo.Text == "Go")
            {
                try
                {
                    RefreshProcess();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                    return;
                }
                StartProcessing();
                Thread t = new Thread(new ThreadStart(DoProcessing));
                t.Start();
            }
            else if (btnGo.Text == "Stop")
            {
                StopProcessing();
            }

        }

        private void DoProcessing()
        {
            DebugOut.ClearCommands();
            runner.Run();
        }

        private void StartProcessing()
        {
            RefreshProcess();
            foreach (Control c in Controls)
                c.Enabled = false;
            btnGo.Enabled = true;
            txtOutput.Enabled = true;
            btnGo.Text = "Stop";
            timerDebug.Enabled = true;
        }

        private void StopProcessing()
        {
            foreach (Control c in enabledControls)// not all Controls, only those that were enabled at beginning
                c.Enabled = true;
            btnGo.Text = "Go";
            runner.isProcessing = false;
            timerDebug.Enabled = false;
        }


        private void timerDebug_Tick(object sender, EventArgs e)
        {
            txtOutput.Text = DebugOut.GetDebug();
            txtOutput.SelectionStart = txtOutput.Text.Length;
            txtOutput.ScrollToCaret();

            if (!runner.isProcessing && btnGo.Text == "Stop")
                StopProcessing();
        }
#endregion
#region Process Initialisations
        /// <summary>
        /// Create new Process and fill with parameters form GUI.
        /// </summary>
        private void RefreshProcess()
        {
            runner = new Runner(txtSQLCommandsFile.Text);
            RefreshGlobalParameters();
            if(chkTransformSnapshots.Checked)
                runner.Add(RefreshSnapshotsProcess());
            if (chkTransformSimDBFoF.Checked)
                runner.Add(RefreshSimDBFOFsProcess());
            if (chkFFTData.Checked)
                runner.Add(RefreshFFTDataProcess());
        }

        private void RefreshGlobalParameters()
        {
            float box = Single.Parse(txtBox.Text);
            int phBits = Int32.Parse(txtPHBits.Text);
            int numZones = Int32.Parse(txtNumZones.Text);
            int maxRandom = Int32.Parse(txtMaxRandom.Text);
            globalParameters = new GlobalParameters(phBits, numZones, box, maxRandom
                , txtSQLCommandsFile.Text);

        }

        private SnapshotsProcess RefreshSnapshotsProcess()
        {
            SnapshotsProcess process = new SnapshotsProcess(globalParameters);
                
            process.inPath = txtSnapshotSourceDir.Text;
            process.outPath = txtSnapshotTargetDir.Text;
            process.snapshotFilePrefix = txtSnapshotFilePrefix.Text;
            process.firstSnapshotFile = Int32.Parse(txtFirstSnapshotFile.Text);
            process.lastSnapshotFile = Int32.Parse(txtLastSnapshotFile.Text);
            process.firstSnap = Int16.Parse(txtFirstSnap.Text);
            process.lastSnap = Int16.Parse(txtLastSnap.Text);
            process.writeArrays = chbWriteArrays.Checked;
            return process;
        }

        

        private IndraFOFProcess RefreshSimDBFOFsProcess()
        {
            IndraFOFProcess process = new IndraFOFProcess(globalParameters);
            process.inPath = FoFSimDBsrcpath.Text;
            process.outPath = FoFSimDBdstpath.Text;
            process.groupTabFilePrefix = groupTabPrefix.Text;
            process.groupIDFilePrefix = FofSimDBgroupprefix.Text;
            process.firstSnapshotFile = Int32.Parse(FoFSimDBfirstfile.Text);
            process.lastSnapshotFile = Int32.Parse(FoFSimDBlastfile.Text);
            process.firstSnap = Int16.Parse(FoFSimDBfirstsnap.Text);
            process.lastSnap = Int16.Parse(FoFSimDBlastsnap.Text);
            return process;
        }

        private IndraFFTDataProcess RefreshFFTDataProcess()
        {
            IndraFFTDataProcess process = new IndraFFTDataProcess(globalParameters);
            process.inPath = FFTSrcPath.Text;
            process.outPath = FFTDstPath.Text;
            process.filePrefix = FFTFilePrefix.Text;
            process.fileExtension = ".dat";
            process.firstSnap = Int16.Parse(FFTFirstFile.Text);
            process.lastSnap = Int16.Parse(FFTLastFile.Text);
            return process;
        }
        
#endregion


        #region File/Dir Choosers
        /// <summary>
        /// Utility method for choosing a file for a text box
        /// </summary>
        /// <param name="target"></param>
        private void ChooseFile(TextBox target)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.FileName = target.Text;
            DialogResult result = f.ShowDialog(this);
            if (result == DialogResult.OK)
                target.Text = f.FileName;
        }

        /// <summary>
        /// Utility method for choosing a directory for a text box
        /// </summary>
        /// <param name="target"></param>
        private void ChooseDirectory(TextBox target)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.SelectedPath = target.Text;
            DialogResult result = f.ShowDialog(this);
            if (result == DialogResult.OK)
                target.Text = f.SelectedPath;
        }
        private void ChooseSourceDirectory(TextBox source, TextBox target)
        {
            ChooseDirectory(source);
            if (target.Text.Trim() == "")
                target.Text = source.Text + "_out";
        }

        private void btnSnapshots_Click(object sender, EventArgs e)
        {
            ChooseFile(txtSnapshotsFile);
        }

        private void btnSQLCommandsFile_Click(object sender, EventArgs e)
        {
            ChooseFile(txtSQLCommandsFile);
        }
        private void btnSnapshotsSourceDir_Click(object sender, EventArgs e)
        {
            ChooseSourceDirectory(txtSnapshotSourceDir, txtSnapshotTargetDir);
        }

        private void btnSnapshotsTargetDir_Click(object sender, EventArgs e)
        {
            ChooseDirectory(txtSnapshotTargetDir);

        }

        
        #endregion

        private void txtBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSnapshotFilePrefix_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtFirstSnap_TextChanged(object sender, EventArgs e)
        {

        }

        /*private void txtSnapshotsFile_TextChanged(object sender, EventArgs e)
        {

        }*/

        private void txtLastSnapshotFile_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSnapshotTargetDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSQLCommandsFile_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabPage6_Click(object sender, EventArgs e)
        {

        }

        private void ProcessGadgetFOFschckbox(object sender, EventArgs e)
        {

        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label85_Click(object sender, EventArgs e)
        {

        }

        private void FoFSimDBsrcpath_TextChanged(object sender, EventArgs e)
        {

        }

        private void label91_Click(object sender, EventArgs e)
        {

        }

        private void tabSnapshots1_Click(object sender, EventArgs e)
        {

        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void checkBox13_CheckedChanged_1(object sender, EventArgs e)
        {

        }


 



    }

}