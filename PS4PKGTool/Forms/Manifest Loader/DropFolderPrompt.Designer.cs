using System;

namespace PS4PKGTool
{
    partial class DropFolderPrompt
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.darkLabelTitle = new DarkUI.Controls.DarkLabel();
            this.darkLabelPath = new DarkUI.Controls.DarkLabel();
            this.darkLabelPath2 = new DarkUI.Controls.DarkLabel();
            this.chkRecursive = new DarkUI.Controls.DarkCheckBox();
            this.chkAddToDirectories = new DarkUI.Controls.DarkCheckBox();
            this.btnScan = new DarkUI.Controls.DarkButton();
            this.btnCancel = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            //
            // darkLabelTitle
            //
            this.darkLabelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.darkLabelTitle.ForeColor = System.Drawing.Color.Gainsboro;
            this.darkLabelTitle.Location = new System.Drawing.Point(20, 15);
            this.darkLabelTitle.Name = "darkLabelTitle";
            this.darkLabelTitle.Size = new System.Drawing.Size(380, 22);
            this.darkLabelTitle.TabIndex = 0;
            this.darkLabelTitle.Text = "Folder Dropped";
            //
            // darkLabelPath
            //
            this.darkLabelPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.darkLabelPath.ForeColor = System.Drawing.Color.Gainsboro;
            this.darkLabelPath.Location = new System.Drawing.Point(20, 42);
            this.darkLabelPath.Name = "darkLabelPath";
            this.darkLabelPath.Size = new System.Drawing.Size(380, 18);
            this.darkLabelPath.TabIndex = 1;
            //
            // darkLabelPath2
            //
            this.darkLabelPath2.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular);
            this.darkLabelPath2.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            this.darkLabelPath2.Location = new System.Drawing.Point(20, 60);
            this.darkLabelPath2.Name = "darkLabelPath2";
            this.darkLabelPath2.Size = new System.Drawing.Size(380, 16);
            this.darkLabelPath2.TabIndex = 2;
            //
            // chkRecursive
            //
            this.chkRecursive.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRecursive.ForeColor = System.Drawing.Color.Gainsboro;
            this.chkRecursive.Location = new System.Drawing.Point(23, 88);
            this.chkRecursive.Name = "chkRecursive";
            this.chkRecursive.Size = new System.Drawing.Size(250, 22);
            this.chkRecursive.TabIndex = 3;
            this.chkRecursive.Text = "Scan subfolders recursively";
            //
            // chkAddToDirectories
            //
            this.chkAddToDirectories.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAddToDirectories.ForeColor = System.Drawing.Color.Gainsboro;
            this.chkAddToDirectories.Location = new System.Drawing.Point(23, 113);
            this.chkAddToDirectories.Name = "chkAddToDirectories";
            this.chkAddToDirectories.Size = new System.Drawing.Size(380, 22);
            this.chkAddToDirectories.TabIndex = 4;
            this.chkAddToDirectories.Text = "Add to saved directories";
            //
            // btnScan
            //
            this.btnScan.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnScan.Location = new System.Drawing.Point(198, 148);
            this.btnScan.Name = "btnScan";
            this.btnScan.Padding = new System.Windows.Forms.Padding(5);
            this.btnScan.Size = new System.Drawing.Size(100, 30);
            this.btnScan.TabIndex = 5;
            this.btnScan.Text = "Scan";
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            //
            // btnCancel
            //
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location = new System.Drawing.Point(303, 148);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(5);
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // DropFolderPrompt
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(420, 195);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.chkAddToDirectories);
            this.Controls.Add(this.chkRecursive);
            this.Controls.Add(this.darkLabelPath2);
            this.Controls.Add(this.darkLabelPath);
            this.Controls.Add(this.darkLabelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DropFolderPrompt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add PKG Folder";
            this.ResumeLayout(false);
        }

        private DarkUI.Controls.DarkLabel darkLabelTitle;
        private DarkUI.Controls.DarkLabel darkLabelPath;
        private DarkUI.Controls.DarkLabel darkLabelPath2;
        private DarkUI.Controls.DarkCheckBox chkRecursive;
        private DarkUI.Controls.DarkCheckBox chkAddToDirectories;
        private DarkUI.Controls.DarkButton btnScan;
        private DarkUI.Controls.DarkButton btnCancel;
    }
}
