using System;
using System.Windows.Forms;

namespace CropMaster
{
    public partial class SizeEditorForm : Form
    {
        public SizeEditorForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetFixedRectangleSize((int)numericUpDown1.Value, (int)numericUpDown2.Value);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
