using System;
using System.Drawing;
using System.Windows.Forms;

namespace CropMaster
{
    public partial class RectEditorForm : Form
    {
        int currentRectIndex = -1;

        public RectEditorForm()
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

        private void ChangeNumericValuesManualy(Rectangle rect)
        {
            numericUpDownEx2.ValueChanged -= numericUpDown2_ValueChanged;
            numericUpDownEx3.ValueChanged -= numericUpDown3_ValueChanged;
            numericUpDownEx4.ValueChanged -= numericUpDown4_ValueChanged;
            numericUpDownEx5.ValueChanged -= numericUpDown5_ValueChanged;

            numericUpDownEx2.Value = rect.X;
            numericUpDownEx3.Value = rect.Y;
            numericUpDownEx4.Value = rect.Width;
            numericUpDownEx5.Value = rect.Height;

            numericUpDownEx2.ValueChanged += numericUpDown2_ValueChanged;
            numericUpDownEx3.ValueChanged += numericUpDown3_ValueChanged;
            numericUpDownEx4.ValueChanged += numericUpDown4_ValueChanged;
            numericUpDownEx5.ValueChanged += numericUpDown5_ValueChanged;
        }

        public void UpdatePictureBox(Image rectImage, Rectangle rect, int idx)
        {
            currentRectIndex = idx;
            ChangeNumericValuesManualy(rect);

            if (rectImage == null && pictureBox1.Image != null)
                pictureBox1.Image.Dispose();

            pictureBox1.Image = rectImage;
        }

        public void InitializePictureBox()
        {
            currentRectIndex = -1;
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
            ((MainForm)this.Owner).UpdateRectEditorForm(currentRectIndex, true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).UpdateRectEditorForm(currentRectIndex, false);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetMemberRectangle("X", (int)numericUpDownEx2.Value, currentRectIndex);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetMemberRectangle("Y", (int)numericUpDownEx3.Value, currentRectIndex);
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetMemberRectangle("Width", (int)numericUpDownEx4.Value, currentRectIndex);
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).SetMemberRectangle("Height", (int)numericUpDownEx5.Value, currentRectIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).InflateRect((int)numericUpDownEx1.Value, currentRectIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).InflateRect(-(int)numericUpDownEx1.Value, currentRectIndex);
        }
    }
}
