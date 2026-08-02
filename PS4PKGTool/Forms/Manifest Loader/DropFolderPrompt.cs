using System;
using System.IO;
using System.Windows.Forms;
using PS4PKGTool.Utilities.PS4PKGToolHelper;

namespace PS4PKGTool
{
    public partial class DropFolderPrompt : DarkUI.Forms.DarkForm
    {
        public bool ScanRecursively { get; private set; } = true;
        public bool AddToDirectories { get; private set; } = false;
        public bool Confirmed { get; private set; } = false;

        public DropFolderPrompt(string folderPath, bool alreadySaved)
        {
            InitializeComponent();
            this.Icon = Helper.AppIcon;

            // Show folder name + parent for context
            string displayPath = folderPath;
            if (displayPath.Length > 70)
                displayPath = "..." + displayPath.Substring(displayPath.Length - 67);

            darkLabelPath.Text = displayPath;
            darkLabelPath2.Text = Path.GetDirectoryName(folderPath);

            chkRecursive.Checked = true;
            chkAddToDirectories.Checked = !alreadySaved;
            chkAddToDirectories.Enabled = !alreadySaved;
            if (alreadySaved)
                chkAddToDirectories.Text = "Add to saved directories (already added)";
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            ScanRecursively = chkRecursive.Checked;
            AddToDirectories = chkAddToDirectories.Checked;
            Confirmed = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Confirmed = false;
            this.Close();
        }
    }
}
