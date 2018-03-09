using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CropMaster
{
    public partial class MainForm : Form
    {
        string mXmlFilePath = null;
        const string mVersionString = "1.1.0";
        const string mXmlSaveString = "XML 形式で保存(&S)";
        const string mXmlSaveAsString = "XML 形式で名前を付けて保存(&A)";
        bool mIsRecursive = false;
        bool mIsDrawing = false;
        int mMovingRectIndex = -1;
        int mCurrentImageIndex = -1;
        int mCurrentRectIndex = -1;
        int mOldOnRectIndex = -1;
        int mFixedWidth = 256, mFixedHeight = 256;
        AspectRatioType mAspectRatioType = AspectRatioType.Squared;

        Graphics mDrawer;
        Point mMouseDown = new Point();
        RectEditorForm mRectEditorForm;
        List<ImageContainer> mBaseImages = new List<ImageContainer>();
        Color mColor = Color.Red;
        Color mReverseColor
        {
            get { return Color.FromArgb(~mColor.ToArgb() | (0xff << 24)); }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool LockWindowUpdate(IntPtr hWnd);

        public MainForm()
        {
            InitializeComponent();
            InitializeControlMode(false);
            ShowRectEditorForm();

            toolStripComboBox1.SelectedIndex = 0;
            mFixedWidth = (int)toolStripNumericUpDown1.Value;
            mFixedHeight = (int)toolStripNumericUpDown2.Value;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mMouseDown = new Point(e.X, e.Y);
            if (pictureBox1.Image == null || !EnabledDrawRect.Checked)
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    mMovingRectIndex = SearchRectangle(mMouseDown);
                    if (mMovingRectIndex == -1)
                    {
                        // カーソルをデフォルトからクロスに
                        this.pictureBox1.Cursor = Cursors.Cross;
                        mIsDrawing = true;
                    }
                    break;
            }
        }

        private async void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Point mouseCurrent = new Point(e.X, e.Y);
            if (pictureBox1.Image == null || !EnabledDrawRect.Checked)
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (mIsDrawing)
                    {
                        // カーソルをクロスからデフォルトに
                        this.pictureBox1.Cursor = Cursors.Default;
                        AddRectangle(mMouseDown, mouseCurrent);

                        if (EnabledSelectionMove.Checked)
                        {
                            await Task.Run(() => System.Threading.Thread.Sleep(500));
                            NextImage();
                        }
                        mIsDrawing = false;
                    }
                    else if (mMovingRectIndex != -1)
                    {
                        Rectangle rect = mBaseImages[mCurrentImageIndex].Rectangles[mMovingRectIndex];
                        MoveRectangleOnImage(mMouseDown, mouseCurrent, rect);
                        mCurrentRectIndex = mMovingRectIndex;
                        mMovingRectIndex = -1;
                        // 移動した領域上にカーソルがなかったことにする <- 補色で描くため
                        mOldOnRectIndex = -1;
                    }
                    break;
                case MouseButtons.Right:
                    if (mMovingRectIndex == -1)
                        RemoveRectangle(SearchRectangle(mouseCurrent));
                    break;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouseCurrent = new Point(e.X, e.Y);
            if (pictureBox1.Image == null || !EnabledDrawRect.Checked)
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    // 描画フラグチェック
                    if (mIsDrawing)
                        DrawDashStyleRectangle(mMouseDown, mouseCurrent, mColor);
                    else if (mMovingRectIndex != -1)
                    {
                        Rectangle rect = mBaseImages[mCurrentImageIndex].Rectangles[mMovingRectIndex];
                        rect = MoveRectangle(mMouseDown, mouseCurrent, rect);
                        DrawDashStyleRectangle(rect, mReverseColor);
                    }
                    return;
            }
            try
            {
                int onRectIndex = SearchRectangle(mouseCurrent);
                if (mOldOnRectIndex != onRectIndex)
                {
                    if (onRectIndex >= 0)
                    {
                        this.pictureBox1.Cursor = Cursors.Hand;
                        UpdateRectangles(new int[] { onRectIndex });
                    }
                    else
                    {
                        this.pictureBox1.Cursor = Cursors.Default;
                        UpdateRectangles();
                    }
                    mOldOnRectIndex = onRectIndex;
                }
            }
            catch
            {
                mOldOnRectIndex = -1;
                this.pictureBox1.Cursor = Cursors.Default;
            }
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                return;

            if (pictureBox1.Width > 0 && pictureBox1.Height > 0)
            {
                // グラフィックオブジェクトのサイズを更新
                mDrawer = Graphics.FromImage(pictureBox1.Image);
                SetScaleAndPad(pictureBox1.Image);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            NextImageWithRect(true);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            PrevImageWithRect(true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            NextImage();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PrevImage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex <= 0)
                return;

            for (int i = mCurrentImageIndex - 1; i >= 0; i--)
            {
                if (mBaseImages[i].Rectangles.Count == 0)
                {
                    mCurrentImageIndex = i;
                    trackBar1.Value = i + 1;
                    break;
                }
            }
            UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
            UpdateRectangles();
            UpdateRectListView();
            UpdateRectEditorForm(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex >= mBaseImages.Count - 1)
                return;

            for (int i = mCurrentImageIndex + 1; i < mBaseImages.Count; i++)
            {
                if (mBaseImages[i].Rectangles.Count == 0)
                {
                    mCurrentImageIndex = i;
                    trackBar1.Value = i + 1;
                    break;
                }
            }
            UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
            UpdateRectangles();
            UpdateRectListView();
            UpdateRectEditorForm(0);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
        }

        private void OpenFile_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "画像ファイル (.jpg, .jpeg, .png, .bmp, .gif, .tif, .tiff)|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;";
            ofd.Title = "開くファイルを選択してください";
            // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            // 存在しないファイルの名前が指定されたとき警告を表示する
            ofd.CheckFileExists = true;
            // 存在しないパスが指定されたとき警告を表示する
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
                InitializeImageContainers(ofd.FileName);
        }

        private void OpenFolder_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXml_ToolStripMenuItem.Text = mXmlSaveString;
            SaveAsXml_ToolStripMenuItem.Text = mXmlSaveAsString;
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                mXmlFilePath = null;
                InitializeImageContainers(folderBrowserDialog1.SelectedPath, false);
            }
        }

        private void OpenFolderRecursive_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXml_ToolStripMenuItem.Text = mXmlSaveString;
            SaveAsXml_ToolStripMenuItem.Text = mXmlSaveAsString;
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                mXmlFilePath = null;
                InitializeImageContainers(folderBrowserDialog1.SelectedPath, true);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (mBaseImages.Count == 0)
                return;

            int idx = trackBar1.Value - 1;
            mCurrentImageIndex = idx;
            UpdatePictureBox(mBaseImages[idx].Path);
            UpdateRectangles();
            UpdateRectListView();
            UpdateRectEditorForm();
        }

        private void About_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string str = String.Format("プログラム名: CropMaster {0}\n", mVersionString);
            MessageBox.Show(str, "CropMasterについて",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RandomCropThis_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomCropForm f = new RandomCropForm(true);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void RandomCropAll_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomCropForm f = new RandomCropForm(false);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void EnabledAdjust_Click(object sender, EventArgs e)
        {
            EnabledAdjust.Checked = true;
            EnabledPadding.Checked = false;
        }

        private void EnabledPadding_Click(object sender, EventArgs e)
        {
            EnabledAdjust.Checked = false;
            EnabledPadding.Checked = true;
        }

        private void Exit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                mCurrentRectIndex = listView1.SelectedItems[0].Index;
                UpdateRectEditorForm(listView1.SelectedItems[0].Index);           
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var lvi = listView1.GetItemAt(e.X, e.Y);
                if (lvi.Selected)
                    UpdateRectangles(new int[] { lvi.Index });
                else
                    UpdateRectangles();
            }
        }

        private void SelectColor_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            // はじめに選択されている色を設定
            cd.Color = Color.Red;
            cd.AllowFullOpen = true;
            cd.SolidColorOnly = false;
            // [作成した色]に指定した色（RGB値）を表示する
            cd.CustomColors = new int[] {
                0x33, 0x66, 0x99, 0xCC, 0x3300, 0x3333,
                0x3366, 0x3399, 0x33CC, 0x6600, 0x6633,
                0x6666, 0x6699, 0x66CC, 0x9900, 0x9933};

            if (cd.ShowDialog() == DialogResult.OK)
            {
                mColor = cd.Color;
                if (mBaseImages.Count > 0)
                    UpdateRectangles();
            }
        }

        private void RectEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            EnabledRectEditorForm.Checked = false;
        }

        private void EnabledRectEditorForm_Click(object sender, EventArgs e)
        {
            EnabledRectEditorForm.Checked = !EnabledRectEditorForm.Checked;
            if (EnabledRectEditorForm.Checked && mRectEditorForm.IsDisposed)
            {
                ShowRectEditorForm();
                if (mBaseImages.Count > 0)
                {
                    UpdateRectListView();
                    UpdateRectEditorForm();
                }
            }
            else if (!mRectEditorForm.IsDisposed)
            {
                mRectEditorForm.Dispose();
            }
        }

        private void EnabledDrawRect_Click(object sender, EventArgs e)
        {
            EnabledDrawRect.Checked = !EnabledDrawRect.Checked;
            if (mBaseImages.Count > 0)
                UpdateRectangles();
        }

        private void EnabledSelectionMove_Click(object sender, EventArgs e)
        {
            EnabledSelectionMove.Checked = !EnabledSelectionMove.Checked;
        }

        private void MappingRectangle_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("選択中の領域を以降の画像にコピーします\nよろしいですか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

            int idx = mCurrentRectIndex;
            if (idx < 0) idx = mBaseImages[mCurrentImageIndex].Rectangles.Count - 1;
            if (idx >= 0 && idx < mBaseImages[mCurrentImageIndex].Rectangles.Count)
            {
                if (res == DialogResult.Yes)
                    CopyRectanglesForward(mBaseImages[mCurrentImageIndex].Rectangles[idx]);
                else if (res == DialogResult.No) { }
            }
        }

        private async void ExportFillOne_ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                SetControlMode(false);
                string exportDirectory;
                DialogResult dr = folderBrowserDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                    exportDirectory = folderBrowserDialog1.SelectedPath;
                else
                    return;

                Directory.CreateDirectory(exportDirectory);
                var sng = new SerialNameGenerator("image", 5, "png");
                await Task.Run(() => ExportFillImage(mCurrentImageIndex, exportDirectory, sng));

                MessageBox.Show("選択領域の塗りつぶしが完了しました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("選択領域の塗りつぶしに失敗しました", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlMode(true);
            }
        }

        private async void ExportFillAll_ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                SetControlMode(false);
                string exportDirectory;
                DialogResult dr = folderBrowserDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                    exportDirectory = folderBrowserDialog1.SelectedPath;
                else
                    return;

                Directory.CreateDirectory(exportDirectory);
                var sng = new SerialNameGenerator("image", 5, "png");

                using (var progressForm = GetProgressForm(mBaseImages.Count))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < mBaseImages.Count; i++)
                        {
                            ExportFillImage(i, exportDirectory, sng);

                            if (progressForm.isCancelled)
                                throw new OperationCanceledException();
                            else
                                progressForm.IncrementProgressBar();
                        }
                    });
                }
                MessageBox.Show("選択領域の塗りつぶしが完了しました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("選択領域の塗りつぶしはキャンセルされました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                SetControlMode(true);
            }
        }

        private async void ExportCropOne_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetControlMode(false);
                string exportDirectory;
                DialogResult dr = folderBrowserDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                    exportDirectory = folderBrowserDialog1.SelectedPath;
                else
                    return;

                Directory.CreateDirectory(exportDirectory);
                var sng = new SerialNameGenerator("image", 5, "png");
                await Task.Run(() =>　ExportCropImage(mCurrentImageIndex, exportDirectory, sng));

                MessageBox.Show("選択領域の切り出しが完了しました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("選択領域の切り出しに失敗しました", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlMode(true);
            }
        }

        private void Remove_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                for (int i = listView1.SelectedItems.Count - 1; i >= 0; i--)
                    mBaseImages[mCurrentImageIndex].Rectangles.RemoveAt(listView1.SelectedItems[i].Index);
                UpdateRectListView();
                UpdateRectEditorForm();
                UpdateRectangles();
            }
        }

        private async void ExportCropAll_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetControlMode(false);
                string exportDirectory;
                DialogResult dr = folderBrowserDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                    exportDirectory = folderBrowserDialog1.SelectedPath;
                else
                    return;

                Directory.CreateDirectory(exportDirectory);
                var sng = new SerialNameGenerator("image", 5, "png");

                using (var progressForm = GetProgressForm(mBaseImages.Count))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < mBaseImages.Count; i++)
                        {
                            ExportCropImage(i, exportDirectory, sng);

                            if (progressForm.isCancelled)
                                throw new OperationCanceledException();
                            else
                                progressForm.IncrementProgressBar();
                        }
                    });
                }
                MessageBox.Show("選択領域の切り出しが完了しました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("選択領域の切り出しはキャンセルされました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                SetControlMode(true);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((string[])e.Data.GetData(DataFormats.FileDrop, false))[0];
            if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".xml")
            {
                Deserialize(path);
                mXmlFilePath = path;
                SaveXml_ToolStripMenuItem.Text = String.Format("{0} の保存(&S)", Path.GetFileName(path));
                SaveAsXml_ToolStripMenuItem.Text = String.Format("名前を付けて {0} を保存(&A)", Path.GetFileName(path));
            }
            else if (mAvailableFormats.Contains(Path.GetExtension(path).ToLower()))
            {
                mXmlFilePath = null;
                InitializeImageContainers(path);
            }
            else if (Directory.Exists(path))
            {
                mXmlFilePath = null;
                InitializeImageContainers(path, false);
            }
        }

        private void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            LockWindowUpdate(IntPtr.Zero);
            panel1.Update();
            if (e.Type == ScrollEventType.ThumbPosition)
                LockWindowUpdate(IntPtr.Zero);
            else
                LockWindowUpdate(this.Handle);
        }

        private void EnabledFillBox_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateFillBoxState(!EnabledFillBox_ToolStripMenuItem.Checked);
        }

        private void EnabledCheckerBoard_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateCheckerBoardState(!EnabledCheckerBoard_ToolStripMenuItem.Checked);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            mAspectRatioType = (AspectRatioType)toolStripComboBox1.SelectedIndex;
            if (mAspectRatioType == AspectRatioType.Fixed)
            {
                toolStripLabel2.Available = true;
                toolStripLabel3.Available = true;
                toolStripNumericUpDown1.Available = true;
                toolStripNumericUpDown2.Available = true;
            }
            else
            {
                toolStripLabel2.Available = false;
                toolStripLabel3.Available = false;
                toolStripNumericUpDown1.Available = false;
                toolStripNumericUpDown2.Available = false;
            }
        }

        private void toolStripNumericUpDown1_TextChanged(object sender, EventArgs e)
        {
            mFixedWidth = (int)toolStripNumericUpDown1.Value;
        }

        private void toolStripNumericUpDown2_TextChanged(object sender, EventArgs e)
        {
            mFixedHeight = (int)toolStripNumericUpDown2.Value;
        }

        private void DeleteAll_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ImageContainer baseImage in mBaseImages)
            {
                if (baseImage.Rectangles != null)
                    baseImage.Rectangles.Clear();
            }
            UpdateRectangles();
            UpdateRectListView(false);
        }

        private void SelectAll_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Focus();
            listView1.BeginUpdate();
            for (int i = 0; i < listView1.Items.Count; i++)
                listView1.Items[i].Selected = true;
            listView1.EndUpdate();
        }

        private void LoadXml_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "output.xml";
            ofd.Filter = "XML ファイル (*.xml)|*.xml";
            ofd.Title = "開くファイルを選択してください";
            // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            // 存在しないファイルの名前が指定されたとき警告を表示する
            ofd.CheckFileExists = true;
            // 存在しないパスが指定されたとき警告を表示する
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Deserialize(ofd.FileName);
                mXmlFilePath = ofd.FileName;
                SaveXml_ToolStripMenuItem.Text = String.Format("{0} の保存(&S)", Path.GetFileName(ofd.FileName));
                SaveAsXml_ToolStripMenuItem.Text = String.Format("名前を付けて {0} を保存(&A)", Path.GetFileName(ofd.FileName));
            }
        }

        private void SaveXml_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mXmlFilePath != null)
                Serialize(mXmlFilePath);
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = "output.xml";
                sfd.Filter = "XML ファイル (*.xml)|*.xml";
                sfd.Title = "保存先のファイルを選択してください";
                // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                sfd.RestoreDirectory = true;
                // 既に存在するファイル名を指定したとき警告する
                sfd.OverwritePrompt = true;
                // 存在しないパスが指定されたとき警告を表示する
                sfd.CheckPathExists = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Serialize(sfd.FileName);
                    mXmlFilePath = sfd.FileName;
                    SaveXml_ToolStripMenuItem.Text = String.Format("{0} の保存(&S)", Path.GetFileName(sfd.FileName));
                    SaveAsXml_ToolStripMenuItem.Text = String.Format("名前を付けて {0} を保存(&A)", Path.GetFileName(sfd.FileName));
                }
            }
        }

        private void SaveAsXml_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "output.xml";
            sfd.Filter = "XML ファイル (*.xml)|*.xml";
            sfd.Title = "保存先のファイルを選択してください";
            // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            // 既に存在するファイル名を指定したとき警告する
            sfd.OverwritePrompt = true;
            // 存在しないパスが指定されたとき警告を表示する
            sfd.CheckPathExists = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Serialize(sfd.FileName);
                mXmlFilePath = sfd.FileName;
                SaveXml_ToolStripMenuItem.Text = String.Format("{0} の保存(&S)", Path.GetFileName(sfd.FileName));
                SaveAsXml_ToolStripMenuItem.Text = String.Format("名前を付けて {0} を保存(&A)", Path.GetFileName(sfd.FileName));
            }
        }

        private void ExportImglab_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "output.xml";
            sfd.Filter = "XML ファイル (*.xml)|*.xml";
            sfd.Title = "保存先のファイルを選択してください";
            // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            // 既に存在するファイル名を指定したとき警告する
            sfd.OverwritePrompt = true;
            // 存在しないパスが指定されたとき警告を表示する
            sfd.CheckPathExists = true;

            if (sfd.ShowDialog() == DialogResult.OK)
                SerializeOpenCV(sfd.FileName);
        }
    }
}