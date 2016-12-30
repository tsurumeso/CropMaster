using System;
using System.Windows.Forms;

namespace CropMaster
{
    public partial class ProgressForm : Form
    {
        bool cancellationFlag = false;
        public bool isCancelled
        {
            get
            {
                return cancellationFlag;
            }
        }

        public ProgressForm(int max)
        {
            InitializeComponent();
            progressBar1.Maximum = max;
            progressBar1.Value = 0;
        }

        public void IncrementProgressBar()
        {
            progressBar1.Value++;
            label1.Text = String.Format("{0} %", 100 * progressBar1.Value / progressBar1.Maximum);
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            cancellationFlag = true;
        }
    }
}
