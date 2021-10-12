using System;
using System.Windows.Forms;

namespace PersonalViewMigrationTool
{
    internal partial class FilterDialogForm : Form
    {
        internal DateTime? MigrateViewsCreatedAfter { get; private set; }

        internal FilterDialogForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            MigrateViewsCreatedAfter = dtpMigrateViewsAfter.Value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            MigrateViewsCreatedAfter = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
