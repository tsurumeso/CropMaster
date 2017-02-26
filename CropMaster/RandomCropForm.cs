using System;
using System.Windows.Forms;

namespace CropMaster
{
    public partial class RandomCropForm : Form
    {
        bool isOne = true;

        public RandomCropForm(bool isOne)
        {
            this.isOne = isOne;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int min = 0;
            int max = 0;
            if (radioButton1.Checked)
            {
                min = (int)numericUpDownEx2.Value;
                max = min;
            }
            else
            {
                min = (int)numericUpDownEx3.Value;
                max = (int)numericUpDownEx4.Value;
            }

            if (isOne)
                ((MainForm)this.Owner).RandomCropOne((int)numericUpDownEx1.Value, min, max);
            else
                ((MainForm)this.Owner).RandomCropAll((int)numericUpDownEx1.Value, min, max);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
