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
            lbSimulation.SetSelected(0, true);
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
            if(chkTransformFOFOrderedSnapshots.Checked)
                runner.Add(RefreshFOFOrderedSnapshotsProcess());
            if(chkTransformGroups.Checked)
                runner.Add(RefreshGroupsProcess());
            if(chkTransformHaloTrees.Checked)
                runner.Add(RefreshHaloTreesProcess());
            if (chkTransformGalaxies.Checked)
                runner.Add(RefreshGalaxyTreesProcess());
            if (chkTransformSimDBFoF.Checked)
                runner.Add(RefreshSimDBFOFsProcess());
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
            process.samplingRate = Single.Parse(txtSamplingRate.Text);
            process.firstSnap = Int16.Parse(txtFirstSnap.Text);
            process.lastSnap = Int16.Parse(txtLastSnap.Text);
            process.writeArrays = chbWriteArrays.Checked;
            return process;
        }

        private FOFOrderedSnapshotsProcess RefreshFOFOrderedSnapshotsProcess()
        {
            FOFOrderedSnapshotsProcess process = new FOFOrderedSnapshotsProcess(globalParameters);

            process.inPath = txtFOFOrderedSourceDir.Text;
            process.outPath = txtFOFOrderedTargetDir.Text;
            process.snapshotFilePrefix = txtFOFOrderedSnapshotPrefix.Text;
            process.firstSnap = Int16.Parse(txtFirstFOFOrderedSnap.Text);
            process.lastSnap = Int16.Parse(txtLastFOFOrderedSnap.Text);
            process.snapshotTable = txtFOFOrderedParticleTableName.Text;
            return process;
        }

        private GroupProcess RefreshGroupsProcess()
        {
            GroupProcess process;
            if ("milliMXXL".Equals(lbSimulation.SelectedItem.ToString())
            || "centiMXXL".Equals(lbSimulation.SelectedItem.ToString())
            || "Millennium-XXL".Equals(lbSimulation.SelectedItem.ToString()))// chkIsMXXL.Checked)
                process = new XXLGroupProcess(globalParameters);
            else if("Millennium-II".Equals(lbSimulation.SelectedItem.ToString()))
                process = new GroupProcess(globalParameters);
            else
            {
                DebugOut.PrintLine("Currently no support for millimil or Millennium FOF groups and subhalos.");
                return null;
            }
            process.inPath = txtFOFSubhaloSourceDir.Text;
            process.outPath = txtFOFSubhaloTargetDir.Text;
            process.writeFOFs = chkWriteFOF.Checked;
            process.writeSubHalos = chkWriteSubHalos.Checked;
            process.writeSubhaloIDs = chkIDs.Checked;
            process.hasVelDisp = chkDisp.Checked;
            process.fofTable = txtFOFTable.Text;
            process.subhaloTable = txtSubhaloTable.Text;
            process.firstSnap = Int16.Parse(txtFirstGroupsSnapshot.Text);
            process.lastSnap = Int16.Parse(txtLastGroupsSnapshot.Text);
            string sim = lbSimulation.SelectedItem.ToString();
            return process;
        }
        private HaloTreeProcess RefreshHaloTreesProcess()
        {
            HaloTreeProcess process = new HaloTreeProcess(globalParameters);
            // mergertree organised data
            process.inPath = txtHaloTreesSourceDir.Text;
            process.outPath = txtHaloTreesTargetDir.Text;
            process.treesPrefix = txtTreePrefix.Text;
            process.treeIdsPrefix = txtTreeIdsPrefix.Text;

            process.hasMainLeafId = chkMainLeafId.Checked;
            process.haloTreeTable = txtHaloTreeTable.Text;
            process.firstVolume = Int32.Parse(txtFirstVolume.Text);
            process.lastVolume = Int32.Parse(txtLastVolume.Text);
            return process;
        }

        private GalaxyTreeProcess RefreshGalaxyTreesProcess()
        {
            GalaxyTreeProcess process = new GalaxyTreeProcess(globalParameters);
            // mergertree organised data
            process.inPath = txtGalaxiesSourceDir.Text;
            process.outPath = txtGalaxiesTargetDir.Text;
            process.galaxyFilePrefix = txtGalaxyFilePrefix.Text;

            process.firstVolume = Int32.Parse(txtGalaxiesFirstVolume.Text);
            process.lastVolume = Int32.Parse(txtGalaxiesLastVolume.Text);
               
            process.hasMainLeafId = chkGalaxiesHaveMainLeaf.Checked;
            process.nummag = Int32.Parse(txtNMag.Text);
            process.galaxyTable = txtGalaxyTable.Text;
            return process;
        }

        private SimDBFOFProcess RefreshSimDBFOFsProcess()
        {
            SimDBFOFProcess process = new SimDBFOFProcess(globalParameters);
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

        private void btnFOFOrderedTargetDir_Click(object sender, EventArgs e)
        {
            ChooseDirectory(txtFOFOrderedTargetDir);
        }

        private void btnFOFOrderedSourceDir_Click(object sender, EventArgs e)
        {
            ChooseSourceDirectory(txtFOFOrderedSourceDir, txtFOFOrderedTargetDir);
        }

        private void btnFOFSubhaloTargetDir_Click(object sender, EventArgs e)
        {
            ChooseDirectory(txtFOFSubhaloTargetDir);
        }

        private void btnFOFSubhaloSourceDir_Click(object sender, EventArgs e)
        {
            ChooseSourceDirectory(txtFOFSubhaloSourceDir, txtFOFSubhaloTargetDir);
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


 



    }

}