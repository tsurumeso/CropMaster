using System;
using System.Drawing;
using System.Windows.Forms;

namespace CropMaster
{
    public partial class BboxEditorForm : Form
    {
        int currentBboxIndex = -1;

        public BboxEditorForm()
        {
            InitializeComponent();
        }

        //[System.Security.Permissions.UIPermission(
        //System.Security.Permissions.SecurityAction.Demand,
        //Window = System.Security.Permissions.UIPermissionWindow.AllWindows)]
        //protected override bool ProcessDialogKey(Keys keyData)
        //{
        //    //左キーが押されているか調べる
        //    if ((keyData & Keys.KeyCode) == Keys.Left)
        //    {
        //        ((MainForm)this.Owner).HorizontalShift(-value, currentRectIndex);
        //        //左キーの本来の処理（左側のコントロールにフォーカスを移す）を
        //        //させたくないときは、trueを返す
        //        return true;
        //    }
        //    else if ((keyData & Keys.KeyCode) == Keys.Right)
        //    {
        //        ((MainForm)this.Owner).HorizontalShift(value, currentRectIndex);
        //        return true;
        //    }
        //    else if ((keyData & Keys.KeyCode) == Keys.Up)
        //    {
        //        ((MainForm)this.Owner).VerticalShift(-value, currentRectIndex);
        //        return true;
        //    }
        //    else if ((keyData & Keys.KeyCode) == Keys.Down)
        //    {
        //        ((MainForm)this.Owner).VerticalShift(value, currentRectIndex);
        //        return true;
        //    }
        //    else if ((keyData & Keys.KeyCode) == Keys.Add)
        //    {
        //        ((MainForm)this.Owner).InflateRect(value, currentRectIndex);
        //        return true;
        //    }
        //    else if ((keyData & Keys.KeyCode) == Keys.Subtract)
        //    {
        //        ((MainForm)this.Owner).InflateRect(-value, currentRectIndex);
        //        return true;
        //    }

        //    return base.ProcessDialogKey(keyData);
        //}

        private void ChangeNumericValuesManualy(Rectangle bbox)
        {
            numericUpDownEx2.ValueChanged -= numericUpDown2_ValueChanged;
            numericUpDownEx3.ValueChanged -= numericUpDown3_ValueChanged;
            numericUpDownEx4.ValueChanged -= numericUpDown4_ValueChanged;
            numericUpDownEx5.ValueChanged -= numericUpDown5_ValueChanged;

            numericUpDownEx2.Value = bbox.X;
            numericUpDownEx3.Value = bbox.Y;
            numericUpDownEx4.Value = bbox.Width;
            numericUpDownEx5.Value = bbox.Height;

            numericUpDownEx2.ValueChanged += numericUpDown2_ValueChanged;
            numericUpDownEx3.ValueChanged += numericUpDown3_ValueChanged;
            numericUpDownEx4.ValueChanged += numericUpDown4_ValueChanged;
            numericUpDownEx5.ValueChanged += numericUpDown5_ValueChanged;
        }

        public void UpdatePictureBox(Image bboxImage, Rectangle bbox, int idx)
        {
            currentBboxIndex = idx;
            ChangeNumericValuesManualy(bbox);

            if (bboxImage == null && pictureBox1.Image != null)
                pictureBox1.Image.Dispose();

            pictureBox1.Image = bboxImage;
        }

        public void InitializePictureBox()
        {
            currentBboxIndex = -1;
            ChangeNumericValuesManualy(new Rectangle(0, 0, 1, 1));

            if (pictureBox1.Image != null)
                pictureBox1.Image.Dispose();

            pictureBox1.Image = null;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ShowGrid_ToolStripMenuItem.Checked)
            {
                Graphics g = e.Graphics;
                Pen p = new Pen(Color.FromArgb(128, Color.Black), 1.0f);
                g.DrawLine(p, pictureBox1.Size.Width / 2, 0, pictureBox1.Size.Width / 2, pictureBox1.Size.Height);
                g.DrawLine(p, 0, pictureBox1.Size.Height / 2, pictureBox1.Size.Width, pictureBox1.Size.Height / 2);
            }
        }

        private void Output_ToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.FileName = "newfile.png";
            sfd.Filter = "PNGファイル(*.png)|*.png";
            sfd.Title = "保存先のファイルを選択してください";
            // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            // 既に存在するファイル名を指定したとき警告する
            sfd.OverwritePrompt = true;
            // 存在しないパスが指定されたとき警告を表示する
            sfd.CheckPathExists = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void ShowGrid_ToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            ShowGrid_ToolStripMenuItem.Checked = !ShowGrid_ToolStripMenuItem.Checked;
            pictureBox1.Refresh();
        }

        private void File_ToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OutputAs_ToolStripMenuItem.Enabled = (pictureBox1.Image != null);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).UpdateBboxEditorForm(currentBboxIndex, true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).UpdateBboxEditorForm(currentBboxIndex, false);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetBboxMember("X", (int)numericUpDownEx2.Value, currentBboxIndex);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetBboxMember("Y", (int)numericUpDownEx3.Value, currentBboxIndex);
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetBboxMember("Width", (int)numericUpDownEx4.Value, currentBboxIndex);
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetBboxMember("Height", (int)numericUpDownEx5.Value, currentBboxIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).InflateBbox((int)numericUpDownEx1.Value, currentBboxIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).InflateBbox(-(int)numericUpDownEx1.Value, currentBboxIndex);
        }
    }
}
