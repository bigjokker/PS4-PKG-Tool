using System;

namespace PS4PKGTool
{
    partial class ManifestLoaderPrompt
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.darkLabelInfo = new DarkUI.Controls.DarkLabel();
            this.darkLabelTitle = new DarkUI.Controls.DarkLabel();
            this.btnLoadFromManifest = new DarkUI.Controls.DarkButton();
            this.btnScanFromDirectory = new DarkUI.Controls.DarkButton();
            this.btnLaunchEmpty = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            //
            // darkLabelTitle
            //
            this.darkLabelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.darkLabelTitle.ForeColor = System.Drawing.Color.Gainsboro;
            this.darkLabelTitle.Location = new System.Drawing.Point(20, 15);
            this.darkLabelTitle.Name = "darkLabelTitle";
            this.darkLabelTitle.Size = new System.Drawing.Size(380, 22);
            this.darkLabelTitle.TabIndex = 0;
            this.darkLabelTitle.Text = "Welcome to PS4 PKG Tool";
            //
            // darkLabelInfo
            //
            this.darkLabelInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.darkLabelInfo.ForeColor = System.Drawing.Color.Gainsboro;
            this.darkLabelInfo.Location = new System.Drawing.Point(20, 40);
            this.darkLabelInfo.Name = "darkLabelInfo";
            this.darkLabelInfo.Size = new System.Drawing.Size(380, 55);
            this.darkLabelInfo.TabIndex = 1;
            this.darkLabelInfo.Text = "";
            //
            // btnLoadFromManifest
            //
            this.btnLoadFromManifest.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnLoadFromManifest.Location = new System.Drawing.Point(23, 103);
            this.btnLoadFromManifest.Name = "btnLoadFromManifest";
            this.btnLoadFromManifest.Padding = new System.Windows.Forms.Padding(5);
            this.btnLoadFromManifest.Size = new System.Drawing.Size(115, 32);
            this.btnLoadFromManifest.TabIndex = 2;
            this.btnLoadFromManifest.Text = "Load Manifest";
            this.btnLoadFromManifest.Click += new System.EventHandler(this.btnLoadFromManifest_Click);
            //
            // btnScanFromDirectory
            //
            this.btnScanFromDirectory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnScanFromDirectory.Location = new System.Drawing.Point(152, 103);
            this.btnScanFromDirectory.Name = "btnScanFromDirectory";
            this.btnScanFromDirectory.Padding = new System.Windows.Forms.Padding(5);
            this.btnScanFromDirectory.Size = new System.Drawing.Size(115, 32);
            this.btnScanFromDirectory.TabIndex = 3;
            this.btnScanFromDirectory.Text = "Scan Directory";
            this.btnScanFromDirectory.Click += new System.EventHandler(this.btnScanFromDirectory_Click);
            //
            // btnLaunchEmpty
            //
            this.btnLaunchEmpty.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnLaunchEmpty.Location = new System.Drawing.Point(281, 103);
            this.btnLaunchEmpty.Name = "btnLaunchEmpty";
            this.btnLaunchEmpty.Padding = new System.Windows.Forms.Padding(5);
            this.btnLaunchEmpty.Size = new System.Drawing.Size(115, 32);
            this.btnLaunchEmpty.TabIndex = 4;
            this.btnLaunchEmpty.Text = "Launch Empty";
            this.btnLaunchEmpty.Click += new System.EventHandler(this.btnLaunchEmpty_Click);
            //
            // ManifestLoaderPrompt
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(420, 155);
            this.Controls.Add(this.btnLaunchEmpty);
            this.Controls.Add(this.btnScanFromDirectory);
            this.Controls.Add(this.btnLoadFromManifest);
            this.Controls.Add(this.darkLabelInfo);
            this.Controls.Add(this.darkLabelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManifestLoaderPrompt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Welcome";
            this.ResumeLayout(false);
        }

        #endregion

        private DarkUI.Controls.DarkLabel darkLabelTitle;
        private DarkUI.Controls.DarkLabel darkLabelInfo;
        private DarkUI.Controls.DarkButton btnLoadFromManifest;
        private DarkUI.Controls.DarkButton btnScanFromDirectory;
        private DarkUI.Controls.DarkButton btnLaunchEmpty;
    }
}
