using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PS4PKGTool.Utilities.PS4PKGToolHelper;

namespace PS4PKGTool
{
    public partial class DLC : DarkUI.Forms.DarkForm
    {
        List<PS4_Tools.PKG.Official.StoreItems> Items = new List<PS4_Tools.PKG.Official.StoreItems>();

        public DLC(List<PS4_Tools.PKG.Official.StoreItems> items)
        {

            InitializeComponent();
            this.Icon = Helper.AppIcon;
            darkDataGridView1.ScrollBars = ScrollBars.Both;

            Items = items;
        }

        private void DLC_Load(object sender, EventArgs e)
        {

            this.Text = "Addon : " + Helper.PKG.CurrentPKGTitle;

            darkDataGridView1.DataSource = Items;
            darkDataGridView1.Columns["Store_Content_Platform"].Visible = false;

        }

    }
}
