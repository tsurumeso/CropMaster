using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Security;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CropMaster
{
    public partial class MainForm : Form
    {
        const string mVersionString = "1.0.0";
        string[] mAvailableFormats = new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif" };
        const int mAlpha = 128;

        float mScale;
        int mPadX, mPadY;

        bool mIsDrawing = false;
        string mWorkingDirectory;
        int mMovingRectIndex = -1;
        int mCurrentImageIndex = -1;
        int mCurrentRectIndex = -1;
        int mOldOnRectIndex = -1;
        int mWHRateIndex = 0;
        int mFixedWidth = 256, mFixedHeight = 256;

        Graphics mDrawer;
        Bitmap mBackgroundImage;
        Rectangle mOldRect = new Rectangle(0, 0, 0, 0);
        List<ImageContainer> mBaseImages = new List<ImageContainer>();
        RandomNumberGenerator mRandomNumGen = RandomNumberGenerator.Create();

        Color mColor = Color.Red;
        Color mReverseColor
        {
            get
            {
                return Color.FromArgb(~mColor.ToArgb() | (0xff << 24));
            }
        }

        RectEditorForm mRectEditorForm;
        Point mMouseDown = new Point();

        delegate void UpdateScaleAndPadDelegate(string path);
        delegate void AdjustRectangleToImageDelegate(ref Rectangle rect);

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

        private Point GetStartPoint(Point p1, Point p2)
        {
            Point p = new Point();
            p.X = Math.Min(p1.X, p2.X);
            p.Y = Math.Min(p1.Y, p2.Y);
            return p;
        }

        private Rectangle GetRectangle(Point mouseDown, Point mouseCurrent)
        {
            if (mWHRateIndex == 0)
            {
                return GetSquareRectangle(mouseDown, mouseCurrent);
            }
            else if (mWHRateIndex == 1)
            {
                return GetFixedRectangle(mouseCurrent);
            }
            else if (mWHRateIndex == 2)
            {
                return GetAnyAspectsRectangle(mouseDown, mouseCurrent);
            }
            return new Rectangle(0, 0, 0, 0);
        }

        private Rectangle GetAnyAspectsRectangle(Point mouseDown, Point mouseCurrent)
        {
            int startX = Math.Min(mouseCurrent.X, mouseDown.X);
            int startY = Math.Min(mouseCurrent.Y, mouseDown.Y);
            int endX = Math.Max(mouseCurrent.X, mouseDown.X);
            int endY = Math.Max(mouseCurrent.Y, mouseDown.Y);

            return new Rectangle(startX, startY, endX - startX, endY - startY);
        }

        private Rectangle GetFixedRectangle(Point mouseCurrent)
        {
            int width = (int)(mFixedWidth * mScale);
            int height = (int)(mFixedHeight * mScale);
            return new Rectangle(mouseCurrent.X - (int)(width / 2), mouseCurrent.Y - (int)(height / 2), width, height);
        }

        private Rectangle GetSquareRectangle(Point mouseDown, Point mouseCurrent)
        {
            Point startPoint = GetStartPoint(mouseDown, mouseCurrent);
            int dx = Math.Abs(mouseCurrent.X - mouseDown.X);
            int dy = Math.Abs(mouseCurrent.Y - mouseDown.Y);
            int width = Math.Min(dx, dy);

            if (mouseCurrent.X < mouseDown.X && mouseCurrent.Y < mouseDown.Y)
            {
                if (dx > dy)
                    return new Rectangle(mouseDown.X - width, startPoint.Y, width, width);
                else
                    return new Rectangle(startPoint.X, mouseDown.Y - width, width, width);
            }
            else if (mouseCurrent.X > mouseDown.X && mouseCurrent.Y < mouseDown.Y)
            {
                if (dx > dy)
                    return new Rectangle(startPoint.X, startPoint.Y, width, width);
                else
                    return new Rectangle(startPoint.X, mouseDown.Y - width, width, width);
            }
            else if (mouseCurrent.X < mouseDown.X && mouseCurrent.Y > mouseDown.Y)
            {
                if (dx > dy)
                    return new Rectangle(mouseDown.X - width, startPoint.Y, width, width);
                else
                    return new Rectangle(startPoint.X, startPoint.Y, width, width);
            }
            else
                return new Rectangle(startPoint.X, startPoint.Y, width, width);
        }

        private void SetScaleAndPad(Image image)
        {
            float pw, ph, iw, ih;

            mPadX = 0;
            mPadY = 0;
            pw = pictureBox1.Width;
            ph = pictureBox1.Height;
            iw = image.Width;
            ih = image.Height;

            if (iw / ih > pw / ph)
            {
                mScale = pw / iw;
                mPadY = (int)((ph - (int)(ih * mScale)) / 2);
            }
            else if (iw / ih < pw / ph)
            {
                mScale = ph / ih;
                mPadX = (int)((pw - (int)(iw * mScale)) / 2);
            }
            else
            {
                mScale = 1f;
            }
        }

        private Rectangle ConvertBoxToImage(Rectangle rect)
        {
            Rectangle imageRect = new Rectangle();
            imageRect.X = (int)((rect.X - mPadX) / mScale);
            imageRect.Y = (int)((rect.Y - mPadY) / mScale);
            imageRect.Width = (int)(rect.Width / mScale);
            imageRect.Height = (int)(rect.Height / mScale);
            return imageRect;
        }

        private Rectangle ConvertImageToBox(Rectangle rect)
        {
            Rectangle boxRect = new Rectangle();
            boxRect.X = (int)(rect.X * mScale) + mPadX;
            boxRect.Y = (int)(rect.Y * mScale) + mPadY;
            boxRect.Width = (int)(rect.Width * mScale);
            boxRect.Height = (int)(rect.Height * mScale);
            return boxRect;
        }

        private Rectangle MoveRectangle(Point mouseDown, Point mouseCurrent, Rectangle rect)
        {
            Rectangle boxRect = new Rectangle();
            int dx = mouseCurrent.X - mouseDown.X;
            int dy = mouseCurrent.Y - mouseDown.Y;
            boxRect.X = (int)(rect.X * mScale) + dx + mPadX;
            boxRect.Y = (int)(rect.Y * mScale) + dy + mPadY;
            boxRect.Width = (int)(rect.Width * mScale);
            boxRect.Height = (int)(rect.Height * mScale);
            return boxRect;
        }

        private void MoveRectangleOnImage(Point mouseDown, Point mouseCurrent, Rectangle rect)
        {
            int dx = mouseCurrent.X - mouseDown.X;
            int dy = mouseCurrent.Y - mouseDown.Y;
            rect.X = (int)(rect.X + dx / mScale);
            rect.Y = (int)(rect.Y + dy / mScale);

            AdjustRectangle(ref rect);
            mBaseImages[mCurrentImageIndex].Rectangles[mMovingRectIndex] = rect;

            UpdateRectListView();
            UpdateRectEditorForm(mMovingRectIndex);
            UpdateRectangles(new int[] { mMovingRectIndex });
        }

        private bool ReloadPictureBox()
        {
            if (mDrawer == null || mBackgroundImage == null)
                return false;

            pictureBox1.Image = new Bitmap(mBackgroundImage);
            mDrawer = Graphics.FromImage(pictureBox1.Image);
            mDrawer.DrawImage(mBackgroundImage, 0, 0, mBackgroundImage.Width, mBackgroundImage.Height);
            return true;
        }

        private void UpdateRectangles(int[] highlightIndices = null)
        {
            if (!ReloadPictureBox())
                return;

            if (mBaseImages[mCurrentImageIndex].Rectangles.Count() != 0 && EnabledDrawRect.Checked)
            {
                using (SolidBrush alphaBrush = new SolidBrush(Color.FromArgb(mAlpha, mColor)))
                using (SolidBrush reverseBrush = new SolidBrush(Color.FromArgb(mAlpha, mReverseColor)))
                using (Pen reversePen = new Pen(mReverseColor))
                {
                    reversePen.DashStyle = DashStyle.Dash;
                    for (int i = 0; i < mBaseImages[mCurrentImageIndex].Rectangles.Count; i++)
                    {
                        var rect = mBaseImages[mCurrentImageIndex].Rectangles[i];
                        if (highlightIndices != null && highlightIndices.Contains(i))
                        {
                            var frameRect = new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 1, rect.Height + 1);
                            mDrawer.DrawRectangle(reversePen, frameRect);
                            mDrawer.FillRectangle(reverseBrush, rect);
                        }
                        else
                            mDrawer.FillRectangle(alphaBrush, rect);
                    }
                }
            }
            pictureBox1.Refresh();
        }

        private void DrawDashStyleRectangle(Point mouseDown, Point mouseCurrent, Color color)
        {
            Rectangle rect = GetRectangle(mouseDown, mouseCurrent);
            DrawDashStyleRectangle(rect, color);
        }

        private void DrawDashStyleRectangle(Rectangle rect, Color color)
        {
            if (!mOldRect.IsEmpty)
            {
                mOldRect.Inflate(2, 2);
                pictureBox1.Invalidate(mOldRect);
                pictureBox1.Update();
            }
            using (var dst = pictureBox1.CreateGraphics())
            using (Pen colorPen = new Pen(color))
            {
                colorPen.DashStyle = DashStyle.Dash;
                dst.DrawRectangle(colorPen, rect);
                mOldRect = rect;
            }
        }

        private void AdjustRectangle(ref Rectangle rect)
        {
            if (EnabledAdjust.Checked)
            {
                if (rect.Width > pictureBox1.Image.Size.Width)
                    rect.Width = pictureBox1.Image.Size.Width;
                if (rect.Height > pictureBox1.Image.Size.Height)
                    rect.Height = pictureBox1.Image.Size.Height;
                if (rect.X < 0)
                    rect.X = 0;
                else if (rect.X + rect.Width > pictureBox1.Image.Size.Width)
                    rect.X = pictureBox1.Image.Size.Width - rect.Width;
                if (rect.Y < 0)
                    rect.Y = 0;
                else if (rect.Y + rect.Height > pictureBox1.Image.Size.Height)
                    rect.Y = pictureBox1.Image.Size.Height - rect.Height;
            }
        }

        private void AddRectangle(Point mouseDown, Point mouseCurrent)
        {
            Rectangle rect = GetRectangle(mouseDown, mouseCurrent);
            Rectangle imageRect = ConvertBoxToImage(rect);

            if (imageRect.Width > 10 && imageRect.Height > 10)
            {
                AdjustRectangle(ref imageRect);
                if (mWHRateIndex == 1)
                {
                    imageRect.Width = mFixedWidth;
                    imageRect.Height = mFixedHeight;
                }
                mBaseImages[mCurrentImageIndex].Rectangles.Add(imageRect);
            }
            UpdateRectListView();
            UpdateRectEditorForm();
            UpdateRectangles();
        }

        private async void CopyRectanglesForward(Rectangle rect)
        {
            try
            {
                SetControlMode(false);
                Point mouseDown = new Point(rect.X, rect.Y);
                Point mouseCurrent = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                Rectangle boxRect = ConvertImageToBox(GetRectangle(mouseDown, mouseCurrent));

                AdjustRectangleToImageDelegate adjustRectangleToImage = new AdjustRectangleToImageDelegate(AdjustRectangle);
                UpdateScaleAndPadDelegate updateScaleAndPad = new UpdateScaleAndPadDelegate(UpdateScaleAndPad);

                using (var progressForm = GetProgressForm(mBaseImages.Count - mCurrentImageIndex - 1))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        for (int i = mCurrentImageIndex + 1; i < mBaseImages.Count; i++)
                        {
                            Invoke(updateScaleAndPad, mBaseImages[i].Path);
                            Rectangle mappedRect = ConvertBoxToImage(boxRect);
                            if (mappedRect.Width > 10 && mappedRect.Height > 10)
                            {
                                Invoke(adjustRectangleToImage, mappedRect);
                                mBaseImages[i].Rectangles.Add(mappedRect);
                            }
                            if (progressForm.isCancelled)
                                throw new OperationCanceledException();
                            else
                                progressForm.IncrementProgressBar();
                        }
                    });
                }
                MessageBox.Show("選択領域のコピーが完了しました", "通知",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("選択領域のコピーはキャンセルされました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                SetControlMode(true);
                UpdateRectListView();
                UpdateRectEditorForm();
                UpdateRectangles();
            }
        }

        private void RemoveRectangle(int idx)
        {
            if (idx >= 0 && idx <= mBaseImages[mCurrentImageIndex].Rectangles.Count)
                mBaseImages[mCurrentImageIndex].Rectangles.RemoveAt(idx);

            if (mBaseImages[mCurrentImageIndex].Rectangles.Count == 0)
                mRectEditorForm.InitializePictureBox();

            UpdateRectListView();
            UpdateRectEditorForm();
            UpdateRectangles();
        }

        private int SearchRectangle(Point p)
        {
            for (int i = mBaseImages[mCurrentImageIndex].Rectangles.Count - 1; i >= 0; i--)
            {
                Rectangle rect = mBaseImages[mCurrentImageIndex].Rectangles[i];
                Rectangle boxRect = ConvertImageToBox(rect);
                if (p.X > boxRect.X && p.X < boxRect.X + boxRect.Width && p.Y > boxRect.Y && p.Y < boxRect.Y + boxRect.Height)
                {
                    return i;
                }
            }
            return -1;
        }

        private void UpdateRectEditorForm(int idx = -1)
        {
            if (mRectEditorForm.IsDisposed) return;

            if (idx < 0)
                idx = mBaseImages[mCurrentImageIndex].Rectangles.Count - 1;
            if (idx >= 0 && idx < mBaseImages[mCurrentImageIndex].Rectangles.Count)
            {
                Bitmap rectImage = CropImage(mBackgroundImage, mBaseImages[mCurrentImageIndex].Rectangles[idx]);
                mRectEditorForm.UpdatePictureBox(rectImage, mBaseImages[mCurrentImageIndex].Rectangles[idx], idx);
            }
            else
                mRectEditorForm.InitializePictureBox();
        }

        public void UpdateRectEditorForm(int idx, bool isNext)
        {
            if (mRectEditorForm.IsDisposed) return;

            if (mBaseImages.Count > 0)
            {
                if (isNext)
                {
                    if (idx < mBaseImages[mCurrentImageIndex].Rectangles.Count - 1 && idx >= 0)
                    {
                        idx++;
                        UpdateRectEditorForm(idx);
                    }
                    else
                        NextImageWithRect(true);
                }
                else
                {
                    if (idx > 0)
                    {
                        idx--;
                        UpdateRectEditorForm(idx);
                    }
                    else
                        PrevImageWithRect(false);
                }
            }
        }

        private void UpdateRectListView(bool flag = true)
        {
            listView1.Items.Clear();
            if (flag)
            {              
                foreach (var c in mBaseImages[mCurrentImageIndex].Rectangles)
                {
                    string[] r = { c.X.ToString(), c.Y.ToString(), c.Width.ToString(), c.Height.ToString() };
                    listView1.Items.Add(new ListViewItem(r));
                }
            }
            listView1.Refresh();
        }

        private void UpdateScaleAndPad(string imagePath)
        {
            if (imagePath != null)
            {
                using (Stream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    MemoryStream ms = new MemoryStream();
                    ms.SetLength(fs.Length);
                    fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                    ms.Flush();
                    Image src = Image.FromStream(ms);

                    SetScaleAndPad(src);
                    src.Dispose();
                }
            }
        }

        private void UpdatePictureBox(string imagePath)
        {
            if (imagePath != null)
            {
                if (pictureBox1.Image != null)
                {
                    mDrawer.Dispose();
                    mBackgroundImage.Dispose();
                    pictureBox1.Image.Dispose();
                }
                // ファイルロックの回避処理 gif 対応版
                using (Stream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    // MemoryStreamを生成しファイルの内容をコピー
                    MemoryStream ms = new MemoryStream();
                    ms.SetLength(fs.Length);
                    fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                    ms.Flush();

                    // フォーマット無視
                    Image src = Image.FromStream(ms);
                    pictureBox1.Image = new Bitmap(src.Width, src.Height);

                    // グラフィックオブジェクトのサイズを更新
                    mDrawer = Graphics.FromImage(pictureBox1.Image);
                    mDrawer.DrawImage(src, 0, 0, src.Width, src.Height);

                    mBackgroundImage = new Bitmap(src);

                    SetScaleAndPad(pictureBox1.Image);
                    toolStripStatusLabel1.Text = String.Format("{0}    ", Path.GetFileName(imagePath));
                    toolStripStatusLabel2.Text = String.Format("{0}/{1}    ", mCurrentImageIndex + 1, mBaseImages.Count);
                    toolStripStatusLabel3.Text = String.Format("{0} x {1}    ", src.Width, src.Height);
                    toolStripStatusLabel4.Text = String.Format("{0}    ", src.PixelFormat.ToString().Replace("Format", ""));
                    src.Dispose();
                }
            }
            else
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
            }
        }

        void NextImageWithRect(bool selectFirstRect)
        {
            int initialIndex = selectFirstRect ? 0 : -1;
            if (mCurrentImageIndex < mBaseImages.Count - 1)
            {
                int oldImageIndex = mCurrentImageIndex;
                for (int i = mCurrentImageIndex + 1; i < mBaseImages.Count; i++)
                {
                    if (mBaseImages[i].Rectangles.Count != 0)
                    {
                        mCurrentImageIndex = i;
                        trackBar1.Value = i + 1;
                        break;
                    }
                }
                if (oldImageIndex != mCurrentImageIndex)
                {
                    UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
                    UpdateRectangles();
                    UpdateRectListView();
                    UpdateRectEditorForm(initialIndex);
                }
            }
        }

        void PrevImageWithRect(bool selectFirstRect)
        {
            int initialIndex = selectFirstRect ? 0 : -1;
            if (mCurrentImageIndex > 0)
            {
                for (int i = mCurrentImageIndex - 1; i >= 0; i--)
                {
                    if (mBaseImages[i].Rectangles.Count != 0)
                    {
                        mCurrentImageIndex = i;
                        trackBar1.Value = i + 1;
                        break;
                    }
                }
                UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
                UpdateRectangles();
                UpdateRectListView();
                UpdateRectEditorForm(initialIndex);
            }
        }

        // Bitmap.Clone()を使った方法より高速らしい?
        private Bitmap CropImage(Bitmap src, Rectangle srcRect)
        {
            Bitmap dst = new Bitmap(srcRect.Width, srcRect.Height);
            Graphics g = Graphics.FromImage(dst);
            Rectangle dstRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
            g.DrawImage(src, dstRect, srcRect, GraphicsUnit.Pixel);
            g.Dispose();
            return dst;
        }

        private int RandomInteger(RandomNumberGenerator mRandomNumGen, int start, int end)
        {
            byte[] rand = new byte[4];
            mRandomNumGen.GetBytes(rand);
            int ret = Math.Abs(BitConverter.ToInt32(rand, 0) % (end - start + 1)) + start;
            return ret;
        }

        private void RandomCrop(int idx, int n, int min, int max)
        {
            Bitmap baseImage = new Bitmap(mBaseImages[idx].Path);
            int baseMin = Math.Min(baseImage.Size.Width, baseImage.Size.Height);
            min = min < baseMin ? min : baseMin;
            max = max < baseMin ? max : baseMin;
            for (int i = 0; i < n; i++)
            {
                int width = RandomInteger(mRandomNumGen, min, max);
                int x = RandomInteger(mRandomNumGen, 0, baseImage.Size.Width - width);
                int y = RandomInteger(mRandomNumGen, 0, baseImage.Size.Height - width);
                mBaseImages[idx].Rectangles.Add(new Rectangle(x, y, width, width));
            }
            baseImage.Dispose();
        }

        public void RandomCropOne(int n, int min, int max)
        {
            RandomCrop(mCurrentImageIndex, n, min, max);
            UpdateRectangles();
            UpdateRectListView();
            UpdateRectEditorForm();
        }

        ProgressForm GetProgressForm(int max)
        {
            ProgressForm progressForm = new ProgressForm(max);
            progressForm.Owner = this;
            progressForm.Left = this.Left + (this.Width - progressForm.Width) / 2;
            progressForm.Top = this.Top + (this.Height - progressForm.Height) / 2;

            return progressForm;
        }

        public async void RandomCropAll(int n, int min, int max)
        {
            try
            {
                SetControlMode(false);
                using (var progressForm = GetProgressForm(mBaseImages.Count))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < mBaseImages.Count; i++)
                        {
                            RandomCrop(i, n, min, max);

                            if (progressForm.isCancelled)
                                throw new OperationCanceledException();
                            else
                                progressForm.IncrementProgressBar();
                        }
                    });
                }
                MessageBox.Show("ランダムクロップが完了しました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("ランダムクロップはキャンセルされました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("ランダムクロップに失敗しました", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlMode(true);
                UpdateRectangles();
                UpdateRectListView();
                UpdateRectEditorForm();
            }
        }

        private List<string> GetFilesRecursive(string dir, string[] ext)
        {
            List<string> files = new List<string>();
            try
            {
                string[] entries = Directory.GetFileSystemEntries(dir);

                foreach (string entry in entries)
                {
                    if (Directory.Exists(entry))
                        files.AddRange(GetFilesRecursive(entry, ext));
                    else if (ext.Contains(Path.GetExtension(entry).ToLower()))
                        files.Add(entry);
                }
            }
            catch { }

            return files;
        }

        private async void InitializeImageContainers(string dir)
        { 
            try
            {
                SetControlMode(false);
                mWorkingDirectory = dir;
                string tmpdir = mWorkingDirectory.TrimEnd('\\') + "\\";
                List<string> files = await Task.Run(() => GetFilesRecursive(mWorkingDirectory, mAvailableFormats));

                if (files.Count <= 0)
                    return;

                if (mBaseImages != null)
                    mBaseImages.Clear();

                using (var progressForm = GetProgressForm(files.Count))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        foreach (string file in files)
                        {
                            int idx = file.IndexOf(tmpdir) + tmpdir.Length;
                            ImageContainer baseImage = new ImageContainer();
                            baseImage.Path = file;
                            baseImage.FileName = file.Remove(0, idx);
                            baseImage.Rectangles = new List<Rectangle>();
                            mBaseImages.Add(baseImage);

                            if (progressForm.isCancelled)
                                throw new OperationCanceledException();
                            else
                                progressForm.IncrementProgressBar();
                        }
                    });
                }
                mCurrentImageIndex = 0;
                trackBar1.Maximum = mBaseImages.Count;
                trackBar1.Value = 1;
                UpdatePictureBox(mBaseImages[0].Path);
                UpdateRectListView(false);
                UpdateRectEditorForm();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("画像の読み込みはキャンセルされました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("画像の読み込みに失敗しました", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlMode(true);
            }
        }

        private void UpdateImageContainers(string currentFilePath, Dictionary<string, ImageContainer> xmlImages)
        {
            string tmpdir = mWorkingDirectory.TrimEnd('\\') + "\\";
            List<string> files = GetFilesRecursive(mWorkingDirectory, mAvailableFormats);

            if (files.Count > 0)
            {
                if (mBaseImages != null)
                    mBaseImages.Clear();

                for (int i = 0; i < files.Count; i++)
                {
                    var baseImage = new ImageContainer();
                    if (!xmlImages.ContainsKey(files[i]))
                    {
                        int idx = files[i].IndexOf(tmpdir) + tmpdir.Length;
                        baseImage.Path = files[i];
                        baseImage.FileName = files[i].Remove(0, idx);
                        baseImage.Rectangles = new List<Rectangle>();
                    }
                    else
                        baseImage = xmlImages[files[i]];

                    if (baseImage.Path == currentFilePath)
                        mCurrentImageIndex = i;

                    mBaseImages.Add(baseImage);
                }
            }
        }

        private void Serialize(string filepath)
        {
            var sw = new StreamWriter(filepath, false, new System.Text.UTF8Encoding(false));
            sw.WriteLine("<Application>");
            sw.WriteLine("  <Headers>");
            sw.WriteLine(String.Format("    <Directory>{0}</Directory>", SecurityElement.Escape(mWorkingDirectory)));
            sw.WriteLine(String.Format("    <CurrentFilePath>{0}</CurrentFilePath>", SecurityElement.Escape(mBaseImages[mCurrentImageIndex].Path)));
            sw.WriteLine(String.Format("    <RectangleColor>{0}</RectangleColor>", mColor.ToArgb()));
            sw.WriteLine("  </Headers>");
            foreach (ImageContainer baseImage in mBaseImages)
            {
                if (baseImage.Rectangles.Count > 0)
                {
                    sw.WriteLine("  <ImageContainer>");
                    sw.WriteLine(String.Format("    <FileName>{0}</FileName>", SecurityElement.Escape(baseImage.FileName)));
                    foreach (var c in baseImage.Rectangles)
                    {
                        sw.WriteLine("    <Rectangle>");
                        sw.WriteLine(String.Format("      <X>{0}</X>", c.X));
                        sw.WriteLine(String.Format("      <Y>{0}</Y>", c.Y));
                        sw.WriteLine(String.Format("      <Width>{0}</Width>", c.Width));
                        sw.WriteLine(String.Format("      <Height>{0}</Height>", c.Height));
                        sw.WriteLine("    </Rectangle>");
                    }
                    sw.WriteLine("  </ImageContainer>");
                }
            }
            sw.WriteLine("</Application>");
            sw.Close();
        }

        private void SerializeOpenCV(string filepath)
        {
            var sw = new StreamWriter(filepath, false, new System.Text.UTF8Encoding(false));
            sw.WriteLine("<?xml version='1.0' encoding='ISO-8859-1'?>");
            sw.WriteLine("<?xml-stylesheet type='text/xsl' href='image_metadata_stylesheet.xsl'?>");
            sw.WriteLine("<dataset>");
            sw.WriteLine("  <name>imglab dataset</name>");
            sw.WriteLine("  <images>");
            foreach (ImageContainer baseImage in mBaseImages)
            {
                if (baseImage.Rectangles.Count > 0)
                {
                    string imgPath = Path.Combine(mWorkingDirectory, baseImage.FileName);
                    sw.WriteLine(String.Format("    <image file=\'{0}\'>", SecurityElement.Escape(imgPath)));
                    foreach (var c in baseImage.Rectangles)
                    {
                        sw.Write(String.Format("      <box top=\'{0}\' ", c.Y));
                        sw.Write(String.Format("left=\'{0}\' ", c.X));
                        sw.Write(String.Format("width=\'{0}\' ", c.Width));
                        sw.WriteLine(String.Format("height=\'{0}\'/>", c.Height));
                    }
                    sw.WriteLine("    </image>");
                }
            }
            sw.WriteLine("  </images>");
            sw.WriteLine("</dataset>");
            sw.Close();
        }

        private async void Deserialize(string filepath)
        {
            try
            {
                SetControlMode(false);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(filepath);
                mWorkingDirectory = xmlDocument.SelectSingleNode("Application/Headers/Directory").InnerText.Trim();
                string currentFilePath = xmlDocument.SelectSingleNode("Application/Headers/CurrentFilePath").InnerText.Trim();
                string rectangleColor = xmlDocument.SelectSingleNode("Application/Headers/RectangleColor").InnerText.Trim();
                mColor = Color.FromArgb(Convert.ToInt32(rectangleColor));
                XmlNodeList xmlNodeList1 = xmlDocument.SelectNodes("Application/ImageContainer");

                using (var progressForm = GetProgressForm(xmlNodeList1.Count))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        var xmlImages = new Dictionary<string, ImageContainer>();
                        foreach (XmlNode xmlNode1 in xmlNodeList1)
                        {
                            string fileName = xmlNode1.SelectSingleNode("FileName").InnerText.Trim();
                            XmlNodeList xmlNodeList2 = xmlNode1.SelectNodes("Rectangle");
                            var tmpRects = new List<Rectangle>();
                            foreach (XmlNode xmlNode2 in xmlNodeList2)
                            {
                                int x = int.Parse(xmlNode2.SelectSingleNode("X").InnerText.Trim());
                                int y = int.Parse(xmlNode2.SelectSingleNode("Y").InnerText.Trim());
                                int width = int.Parse(xmlNode2.SelectSingleNode("Width").InnerText.Trim());
                                int height = int.Parse(xmlNode2.SelectSingleNode("Height").InnerText.Trim());
                                tmpRects.Add(new Rectangle(x, y, width, height));
                            }
                            ImageContainer baseImage = new ImageContainer();
                            baseImage.Path = Path.Combine(mWorkingDirectory, fileName);
                            baseImage.FileName = fileName;
                            baseImage.Rectangles = tmpRects;
                            xmlImages.Add(baseImage.Path, baseImage);

                            if (progressForm.isCancelled)
                                throw new OperationCanceledException();
                            else
                                progressForm.IncrementProgressBar();
                        }
                        UpdateImageContainers(currentFilePath, xmlImages);
                    });
                }
                trackBar1.Maximum = mBaseImages.Count;
                trackBar1.Value = mCurrentImageIndex + 1;
                UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
                UpdateRectangles();
                UpdateRectListView();
                UpdateRectEditorForm();

                MessageBox.Show("XMLの読み込みが完了しました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Xmlの読み込みはキャンセルされました", "通知",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("XMLの読み込みに失敗しました", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlMode(true);
            }
        }

        public void InflateRect(int value, int idx = -1)
        {
            if (mBaseImages.Count > 0)
            {
                if (idx < 0)
                    idx = mBaseImages[mCurrentImageIndex].Rectangles.Count - 1;
                if (idx >= 0 && idx < mBaseImages[mCurrentImageIndex].Rectangles.Count)
                {
                    Rectangle rect = mBaseImages[mCurrentImageIndex].Rectangles[idx];
                    rect.Inflate(value, value);
                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        if (EnabledAdjust.Checked)
                        {
                            if (rect.X < 0) rect.X = 0;
                            else if (rect.X + rect.Width > pictureBox1.Image.Size.Width) rect.X = pictureBox1.Image.Size.Width - rect.Width;
                            if (rect.Y < 0) rect.Y = 0;
                            else if (rect.Y + rect.Height > pictureBox1.Image.Size.Height) rect.Y = pictureBox1.Image.Size.Height - rect.Height;
                        }
                        mBaseImages[mCurrentImageIndex].Rectangles[idx] = rect;
                        UpdateRectListView();
                        UpdateRectEditorForm(idx);
                        UpdateRectangles();
                    }
                }
            }
        }

        public void SetMemberRectangle(string member, int value, int idx = -1)
        {
            if (mBaseImages.Count > 0)
            {
                if (idx < 0)
                    idx = mBaseImages[mCurrentImageIndex].Rectangles.Count - 1;
                if (idx >= 0 && idx < mBaseImages[mCurrentImageIndex].Rectangles.Count)
                {
                    Rectangle rect = mBaseImages[mCurrentImageIndex].Rectangles[idx];

                    // なぜか動かない
                    // PropertyInfo pi = rect.GetType().GetProperty(member);
                    // if (pi != null)
                    //     pi.SetValue(rect, value, null);

                    switch (member)
                    {
                        case "X":
                            rect.X = value;
                            break;
                        case "Y":
                            rect.Y = value;
                            break;
                        case "Width":
                            rect.Width = value;
                            break;
                        case "Height":
                            rect.Height = value;
                            break;
                    }
                    AdjustRectangle(ref rect);
                    mBaseImages[mCurrentImageIndex].Rectangles[idx] = rect;

                    UpdateRectListView();
                    UpdateRectEditorForm(idx);
                    UpdateRectangles();
                }
            }
        }

        private void ExportFillImage(int imageIndex, string exportDirectory, SerialNameGenerator sng)
        {
            if (mBaseImages[imageIndex].Rectangles.Count > 0)
            {
                Bitmap baseImage = new Bitmap(mBaseImages[imageIndex].Path);
                Graphics g = Graphics.FromImage(baseImage);
                SolidBrush colorBrush = new SolidBrush(mColor);

                for (int j = 0; j < mBaseImages[imageIndex].Rectangles.Count; j++)
                {
                    g.FillRectangle(colorBrush, mBaseImages[imageIndex].Rectangles[j]);
                }
                g.Dispose();
                string savePath = exportDirectory + "\\" + sng.Create(exportDirectory, "image" + imageIndex.ToString() + ".png");
                baseImage.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                baseImage.Dispose();
            }
        }

        private void ExportCropImage(int imageIndex, string exportDirectory, SerialNameGenerator sng)
        {
            if (mBaseImages[imageIndex].Rectangles.Count > 0)
            {
                Bitmap baseImage = new Bitmap(mBaseImages[imageIndex].Path);

                int count = 0;
                for (int j = 0; j < mBaseImages[imageIndex].Rectangles.Count; j++)
                {
                    Rectangle rect = mBaseImages[imageIndex].Rectangles[j];
                    Bitmap rectImage = CropImage(baseImage, rect);

                    string newFilePath = exportDirectory + "\\" + sng.Create(exportDirectory, "image" + count.ToString() + ".png");
                    rectImage.Save(newFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    rectImage.Dispose();
                    count++;
                }
                baseImage.Dispose();
            }
        }

        private void ShowRectEditorForm()
        {
            mRectEditorForm = new RectEditorForm();
            mRectEditorForm.Owner = this;
            mRectEditorForm.FormClosed += new FormClosedEventHandler(RectEditorForm_FormClosed);
            mRectEditorForm.Show();
            mRectEditorForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
        }

        private void SetControlMode(bool flag)
        {
            MappingRectangle.Enabled = flag;
            Open_ToolStripMenuItem.Enabled = flag;
            ExportCrop_ToolStripMenuItem.Enabled = flag;
            ExportFill_ToolStripMenuItem.Enabled = flag;
            ExportXml_ToolStripMenuItem.Enabled = flag;
            ImportXml_ToolStripMenuItem.Enabled = flag;
            RandomCrop_ToolStripMenuItem.Enabled = flag;
            RandomCropAll_ToolStripMenuItem.Enabled = flag;
        }

        private void InitializeControlMode(bool flag)
        {
            MappingRectangle.Enabled = flag;
            ExportCrop_ToolStripMenuItem.Enabled = flag;
            ExportFill_ToolStripMenuItem.Enabled = flag;
            ExportXml_ToolStripMenuItem.Enabled = flag;
            RandomCrop_ToolStripMenuItem.Enabled = flag;
            RandomCropAll_ToolStripMenuItem.Enabled = flag;
        }

        public void SetFixedRectangleSize(int width, int height)
        {
            mFixedWidth = width;
            mFixedHeight = height;
        }

        private void NextImage()
        {
            if (mCurrentImageIndex < mBaseImages.Count - 1)
            {
                mCurrentImageIndex++;
                trackBar1.Value++;
                UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
                UpdateRectangles();
                UpdateRectListView();
                UpdateRectEditorForm(0);
            }
        }

        private void PrevImage()
        {
            if (mCurrentImageIndex > 0)
            {
                mCurrentImageIndex--;
                trackBar1.Value--;
                UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
                UpdateRectangles();
                UpdateRectListView();
                UpdateRectEditorForm(0);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mMouseDown = new Point(e.X, e.Y);

            if (pictureBox1.Image != null && EnabledDrawRect.Checked)
            {
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
        }

        private async void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Point mouseCurrent = new Point(e.X, e.Y);

            if (pictureBox1.Image != null && EnabledDrawRect.Checked)
            {
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
                        RemoveRectangle(SearchRectangle(mouseCurrent));
                        break;
                }
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouseCurrent = new Point(e.X, e.Y);

            if (pictureBox1.Image != null && EnabledDrawRect.Checked)
            {
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
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                if (pictureBox1.Width > 0 && pictureBox1.Height > 0)
                {
                    // グラフィックオブジェクトのサイズを更新
                    mDrawer = Graphics.FromImage(pictureBox1.Image);
                    SetScaleAndPad(pictureBox1.Image);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count > 0) NextImageWithRect(true);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count > 0) PrevImageWithRect(true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count > 0) NextImage();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count > 0) PrevImage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count > 0)
            {
                if (mCurrentImageIndex > 0)
                {
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
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (mBaseImages.Count > 0)
            {
                if (mCurrentImageIndex < mBaseImages.Count - 1)
                {
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
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
        }

        private void Open_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                InitializeImageContainers(folderBrowserDialog1.SelectedPath);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (mBaseImages.Count > 0)
            {
                int idx = trackBar1.Value - 1;
                mCurrentImageIndex = idx;
                UpdatePictureBox(mBaseImages[idx].Path);
                UpdateRectangles();
                UpdateRectListView();
                UpdateRectEditorForm();
            }
        }

        private void About_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string str = String.Format(
                "プログラム名: CropMaster {0}\n",
                mVersionString);
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
                //UpdateRectangles(listView1.SelectedIndices.OfType<int>().ToArray());
            }
        }

        private void ColorSelect_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            // はじめに選択されている色を設定
            //cd.Color = TextBox1.BackColor;
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

        private void ImportXml_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.FileName = "output.xml";
            ofd.Filter = "XMLファイル(*.xml)|*.xml";
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

        private void UpdateFillBoxState(bool enabled)
        {
            toolStripButton2.Checked = enabled;
            EnabledFillBox_ToolStripMenuItem.Checked = enabled;
            if (enabled)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.Dock = DockStyle.Fill;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox1.Dock = DockStyle.None;
            }
            pictureBox1.Refresh();
        }

        private void UpdateCheckerBoardState(bool enabled)
        {
            toolStripButton3.Checked = enabled;
            EnabledCheckerBoard_ToolStripMenuItem.Checked = enabled;
            if (enabled)
                panel1.BackgroundImage = Properties.Resources.CheckerBoard;
            else
                panel1.BackgroundImage = null;
            panel1.Refresh();
        }

        private void Remove_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                for (int i = listView1.SelectedItems.Count - 1; i >= 0; i--)
                {
                    mBaseImages[mCurrentImageIndex].Rectangles.RemoveAt(listView1.SelectedItems[i].Index);
                }
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
            if (File.Exists(path))
                Deserialize(path);
            else if (Directory.Exists(path))
                InitializeImageContainers(path);
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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.FileName = "output.xml";
            ofd.Filter = "XMLファイル(*.xml)|*.xml";
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
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            UpdateFillBoxState(!toolStripButton2.Checked);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            UpdateCheckerBoardState(!toolStripButton3.Checked);
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
            mWHRateIndex = toolStripComboBox1.SelectedIndex;
            if (mWHRateIndex == 1)
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

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                InitializeImageContainers(folderBrowserDialog1.SelectedPath);
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.FileName = "output.xml";
            sfd.Filter = "XMLファイル(*.xml)|*.xml";
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
            }
        }

        private void DeleteAll_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ImageContainer baseImage in mBaseImages)
            {
                if (baseImage.Rectangles != null)
                {
                    baseImage.Rectangles.Clear();
                }
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

        private void ExportXml_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.FileName = "output.xml";
            sfd.Filter = "XMLファイル(*.xml)|*.xml";
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
            }
        }

        private void ExportImglab_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.FileName = "output.xml";
            sfd.Filter = "XMLファイル(*.xml)|*.xml";
            sfd.Title = "保存先のファイルを選択してください";
            // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            // 既に存在するファイル名を指定したとき警告する
            sfd.OverwritePrompt = true;
            // 存在しないパスが指定されたとき警告を表示する
            sfd.CheckPathExists = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SerializeOpenCV(sfd.FileName);
            }
        }
    }
}