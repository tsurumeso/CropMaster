using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Security;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CropMaster
{
    partial class MainForm
    {
        string mWorkingDirectory = null;
        string[] mAvailableFormats = new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif" };
        float mScale;
        int mPadX, mPadY;
        const int mAlpha = 128;

        Bitmap mBackgroundImage;
        Rectangle mOldBbox = new Rectangle(0, 0, 0, 0);
        RandomNumberGenerator mRandGen = RandomNumberGenerator.Create();

        delegate void UpdateScaleAndPadDelegate(string path);
        delegate void AdjustBboxToImageDelegate(ref Rectangle bbox);

        private Point GetStartPoint(Point p1, Point p2)
        {
            Point p = new Point();
            p.X = Math.Min(p1.X, p2.X);
            p.Y = Math.Min(p1.Y, p2.Y);
            return p;
        }

        private Rectangle GetBbox(Point mouseDown, Point mouseCurrent)
        {
            if (mAspectRatioType == AspectRatioType.Square)
                return GetSquareBbox(mouseDown, mouseCurrent);
            else if (mAspectRatioType == AspectRatioType.Fixed)
                return GetFixedBbox(mouseCurrent);
            else if (mAspectRatioType == AspectRatioType.Free)
                return GetFreeRatioBbox(mouseDown, mouseCurrent);

            return new Rectangle(0, 0, 0, 0);
        }

        private Rectangle GetFreeRatioBbox(Point mouseDown, Point mouseCurrent)
        {
            int startX = Math.Min(mouseCurrent.X, mouseDown.X);
            int startY = Math.Min(mouseCurrent.Y, mouseDown.Y);
            int endX = Math.Max(mouseCurrent.X, mouseDown.X);
            int endY = Math.Max(mouseCurrent.Y, mouseDown.Y);

            return new Rectangle(startX, startY, endX - startX, endY - startY);
        }

        private Rectangle GetFixedBbox(Point mouseCurrent)
        {
            int width = (int)(mFixedWidth * mScale);
            int height = (int)(mFixedHeight * mScale);
            return new Rectangle(mouseCurrent.X - (int)(width / 2), mouseCurrent.Y - (int)(height / 2), width, height);
        }

        private Rectangle GetSquareBbox(Point mouseDown, Point mouseCurrent)
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
                mPadY = (int)Math.Round((ph - (int)Math.Round(ih * mScale)) / 2);
            }
            else if (iw / ih < pw / ph)
            {
                mScale = ph / ih;
                mPadX = (int)Math.Round((pw - (int)Math.Round(iw * mScale)) / 2);
            }
            else
                mScale = 1f;
        }

        private Rectangle ConvertFormBboxToImgBbox(Rectangle bbox)
        {
            Rectangle imageRectangle = new Rectangle();
            imageRectangle.X = (int)Math.Round((bbox.X - mPadX) / mScale);
            imageRectangle.Y = (int)Math.Round((bbox.Y - mPadY) / mScale);
            imageRectangle.Width = (int)Math.Round(bbox.Width / mScale);
            imageRectangle.Height = (int)Math.Round(bbox.Height / mScale);
            return imageRectangle;
        }

        private Rectangle ConvertImgBboxToFormBbox(Rectangle bbox)
        {
            Rectangle boxRectangle = new Rectangle();
            boxRectangle.X = (int)Math.Round(bbox.X * mScale) + mPadX;
            boxRectangle.Y = (int)Math.Round(bbox.Y * mScale) + mPadY;
            boxRectangle.Width = (int)Math.Round(bbox.Width * mScale);
            boxRectangle.Height = (int)Math.Round(bbox.Height * mScale);
            return boxRectangle;
        }

        private Rectangle MoveBbox(Point mouseDown, Point mouseCurrent, Rectangle bbox)
        {
            Rectangle boxRectangle = new Rectangle();
            int dx = mouseCurrent.X - mouseDown.X;
            int dy = mouseCurrent.Y - mouseDown.Y;
            boxRectangle.X = (int)(bbox.X * mScale) + dx + mPadX;
            boxRectangle.Y = (int)(bbox.Y * mScale) + dy + mPadY;
            boxRectangle.Width = (int)(bbox.Width * mScale);
            boxRectangle.Height = (int)(bbox.Height * mScale);
            return boxRectangle;
        }

        private void MoveBboxOnImage(Point mouseDown, Point mouseCurrent, Rectangle bbox)
        {
            int dx = mouseCurrent.X - mouseDown.X;
            int dy = mouseCurrent.Y - mouseDown.Y;
            bbox.X = (int)(bbox.X + dx / mScale);
            bbox.Y = (int)(bbox.Y + dy / mScale);

            AdjustBbox(ref bbox);
            mBaseImages[mCurrentImageIndex].Bboxs[mMovingBboxIndex] = bbox;

            UpdateBboxListView();
            UpdateBboxEditorForm(mMovingBboxIndex);
            UpdateBboxs(new int[] { mMovingBboxIndex });
        }

        private bool ReloadPictureBox()
        {
            if (mDrawer == null || mBackgroundImage == null)
                return false;

            pictureBox1.Image = new Bitmap(mBackgroundImage);
            mDrawer = Graphics.FromImage(pictureBox1.Image);
            return true;
        }

        private void UpdateBboxs(int[] highlightIndices = null)
        {
            if (!ReloadPictureBox())
                return;

            if (mBaseImages[mCurrentImageIndex].Bboxs.Count() != 0 && EnabledDrawBbox.Checked)
            {
                using (SolidBrush alphaBrush = new SolidBrush(Color.FromArgb(mAlpha, mColor)))
                using (SolidBrush reverseBrush = new SolidBrush(Color.FromArgb(mAlpha, mReverseColor)))
                using (Pen reversePen = new Pen(mReverseColor))
                {
                    reversePen.DashStyle = DashStyle.Dash;
                    for (int i = 0; i < mBaseImages[mCurrentImageIndex].Bboxs.Count; i++)
                    {
                        var bbox = mBaseImages[mCurrentImageIndex].Bboxs[i];
                        if (highlightIndices != null && highlightIndices.Contains(i))
                        {
                            var frameBbox = new Rectangle(bbox.X - 1, bbox.Y - 1, bbox.Width + 1, bbox.Height + 1);
                            mDrawer.DrawRectangle(reversePen, frameBbox);
                            mDrawer.FillRectangle(reverseBrush, bbox);
                        }
                        else
                            mDrawer.FillRectangle(alphaBrush, bbox);
                    }
                }
            }
            pictureBox1.Refresh();
        }

        private void DrawDashStyleBbox(Point mouseDown, Point mouseCurrent, Color color)
        {
            Rectangle bbox = GetBbox(mouseDown, mouseCurrent);
            DrawDashStyleBbox(bbox, color);
        }

        private void DrawDashStyleBbox(Rectangle bbox, Color color)
        {
            if (!mOldBbox.IsEmpty)
            {
                mOldBbox.Inflate(2, 2);
                pictureBox1.Invalidate(mOldBbox);
                pictureBox1.Update();
            }
            using (var dst = pictureBox1.CreateGraphics())
            using (var colorPen = new Pen(color))
            {
                colorPen.DashStyle = DashStyle.Dash;
                dst.DrawRectangle(colorPen, bbox);
                mOldBbox = bbox;
            }
        }

        private void AdjustBbox(ref Rectangle bbox)
        {
            if (EnabledAdjust.Checked)
            {
                if (bbox.Width > pictureBox1.Image.Size.Width)
                    bbox.Width = pictureBox1.Image.Size.Width;
                if (bbox.Height > pictureBox1.Image.Size.Height)
                    bbox.Height = pictureBox1.Image.Size.Height;
                if (bbox.X < 0)
                    bbox.X = 0;
                else if (bbox.X + bbox.Width > pictureBox1.Image.Size.Width)
                    bbox.X = pictureBox1.Image.Size.Width - bbox.Width;
                if (bbox.Y < 0)
                    bbox.Y = 0;
                else if (bbox.Y + bbox.Height > pictureBox1.Image.Size.Height)
                    bbox.Y = pictureBox1.Image.Size.Height - bbox.Height;
            }
        }

        private void AddBbox(Point mouseDown, Point mouseCurrent)
        {
            Rectangle bbox = GetBbox(mouseDown, mouseCurrent);
            Rectangle imageBbox = ConvertFormBboxToImgBbox(bbox);

            if (imageBbox.Width > 10 && imageBbox.Height > 10)
            {
                AdjustBbox(ref imageBbox);
                if (mAspectRatioType == AspectRatioType.Fixed)
                {
                    imageBbox.Width = mFixedWidth;
                    imageBbox.Height = mFixedHeight;
                }
                mBaseImages[mCurrentImageIndex].Bboxs.Add(imageBbox);
            }
            UpdateBboxListView();
            UpdateBboxEditorForm();
            UpdateBboxs();
        }

        private async void CopyBboxsForward(Rectangle bbox)
        {
            try
            {
                SetControlMode(false);
                Point mouseDown = new Point(bbox.X, bbox.Y);
                Point mouseCurrent = new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height);
                Rectangle formBbox = ConvertImgBboxToFormBbox(GetBbox(mouseDown, mouseCurrent));

                AdjustBboxToImageDelegate adjustBboxToImage = new AdjustBboxToImageDelegate(AdjustBbox);
                UpdateScaleAndPadDelegate updateScaleAndPad = new UpdateScaleAndPadDelegate(UpdateScaleAndPad);

                using (var progressForm = GetProgressForm(mBaseImages.Count - mCurrentImageIndex - 1))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        for (int i = mCurrentImageIndex + 1; i < mBaseImages.Count; i++)
                        {
                            Invoke(updateScaleAndPad, mBaseImages[i].Path);
                            Rectangle mappedBbox = ConvertFormBboxToImgBbox(formBbox);
                            if (mappedBbox.Width > 10 && mappedBbox.Height > 10)
                            {
                                Invoke(adjustBboxToImage, mappedBbox);
                                mBaseImages[i].Bboxs.Add(mappedBbox);
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
                UpdateBboxListView();
                UpdateBboxEditorForm();
                UpdateBboxs();
            }
        }

        private void RemoveBbox(int idx)
        {
            if (idx >= 0 && idx <= mBaseImages[mCurrentImageIndex].Bboxs.Count)
                mBaseImages[mCurrentImageIndex].Bboxs.RemoveAt(idx);

            if (mBaseImages[mCurrentImageIndex].Bboxs.Count == 0)
                mBboxEditorForm.InitializePictureBox();

            UpdateBboxListView();
            UpdateBboxEditorForm();
            UpdateBboxs();
        }

        private int SearchBbox(Point p)
        {
            for (int i = mBaseImages[mCurrentImageIndex].Bboxs.Count - 1; i >= 0; i--)
            {
                Rectangle bbox = mBaseImages[mCurrentImageIndex].Bboxs[i];
                Rectangle formBbox = ConvertImgBboxToFormBbox(bbox);
                if (p.X > formBbox.X && p.X < formBbox.X + formBbox.Width && 
                    p.Y > formBbox.Y && p.Y < formBbox.Y + formBbox.Height)
                    return i;
            }
            return -1;
        }

        private void UpdateBboxEditorForm(int idx = -1)
        {
            if (mBboxEditorForm.IsDisposed)
                return;

            if (idx < 0)
                idx = mBaseImages[mCurrentImageIndex].Bboxs.Count - 1;
            if (idx >= 0 && idx < mBaseImages[mCurrentImageIndex].Bboxs.Count)
            {
                Bitmap bboxImage = CropImage(mBackgroundImage, mBaseImages[mCurrentImageIndex].Bboxs[idx]);
                mBboxEditorForm.UpdatePictureBox(bboxImage, mBaseImages[mCurrentImageIndex].Bboxs[idx], idx);
            }
            else
                mBboxEditorForm.InitializePictureBox();
        }

        public void UpdateBboxEditorForm(int idx, bool isNext)
        {
            if (mBboxEditorForm.IsDisposed || mBaseImages.Count == 0)
                return;

            if (isNext)
            {
                if (idx < mBaseImages[mCurrentImageIndex].Bboxs.Count - 1 && idx >= 0)
                    UpdateBboxEditorForm(++idx);
                else
                    NextImageWithBbox(true);
            }
            else
            {
                if (idx > 0)
                    UpdateBboxEditorForm(--idx);
                else
                    PrevImageWithBbox(false);
            }
        }

        private void UpdateBboxListView(bool flag = true)
        {
            listView1.Items.Clear();
            if (flag)
            {
                foreach (var c in mBaseImages[mCurrentImageIndex].Bboxs)
                {
                    string[] r = { c.X.ToString(), c.Y.ToString(), c.Width.ToString(), c.Height.ToString() };
                    listView1.Items.Add(new ListViewItem(r));
                }
            }
            listView1.Refresh();
        }

        private void UpdateScaleAndPad(string imagePath)
        {
            if (imagePath == null)
                return;

            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            using (var ms = new MemoryStream())
            {
                ms.SetLength(fs.Length);
                fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                ms.Flush();
                using (var src = Image.FromStream(ms))
                    SetScaleAndPad(src);
            }
        }

        private void UpdatePictureBox(string imagePath)
        {
            if (imagePath == null)
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
                return;
            }
            else if (pictureBox1.Image != null)
            {
                mDrawer.Dispose();
                mBackgroundImage.Dispose();
                pictureBox1.Image.Dispose();
            }

            // ファイルロックの回避処理 gif 対応版
            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            using (var ms = new MemoryStream())
            {
                ms.SetLength(fs.Length);
                fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                ms.Flush();

                // フォーマット無視
                using (var src = Image.FromStream(ms))
                {
                    // グラフィックオブジェクトのサイズを更新
                    pictureBox1.Image = new Bitmap(src.Width, src.Height);
                    mDrawer = Graphics.FromImage(pictureBox1.Image);
                    mDrawer.DrawImage(src, 0, 0, src.Width, src.Height);
                    mBackgroundImage = new Bitmap(src);
                    SetScaleAndPad(pictureBox1.Image);

                    toolStripStatusLabel1.Text = String.Format("{0}    ", Path.GetFileName(imagePath));
                    toolStripStatusLabel2.Text = String.Format("{0}/{1}    ", mCurrentImageIndex + 1, mBaseImages.Count);
                    toolStripStatusLabel3.Text = String.Format("{0} x {1}    ", src.Width, src.Height);
                    toolStripStatusLabel4.Text = String.Format("{0}    ", src.PixelFormat.ToString().Replace("Format", ""));
                }
            }
        }

        void NextImageWithBbox(bool selectFirstBbox)
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex >= mBaseImages.Count)
                return;

            int initialIndex = selectFirstBbox ? 0 : -1;
            int oldImageIndex = mCurrentImageIndex;
            for (int i = mCurrentImageIndex + 1; i < mBaseImages.Count; i++)
            {
                if (mBaseImages[i].Bboxs.Count != 0)
                {
                    mCurrentImageIndex = i;
                    trackBar1.Value = i + 1;
                    break;
                }
            }
            if (oldImageIndex != mCurrentImageIndex)
            {
                UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
                UpdateBboxs();
                UpdateBboxListView();
                UpdateBboxEditorForm(initialIndex);
            }
        }

        void PrevImageWithBbox(bool selectFirstBbox)
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex <= 0)
                return;

            int initialIndex = selectFirstBbox ? 0 : -1;
            for (int i = mCurrentImageIndex - 1; i >= 0; i--)
            {
                if (mBaseImages[i].Bboxs.Count != 0)
                {
                    mCurrentImageIndex = i;
                    trackBar1.Value = i + 1;
                    break;
                }
            }
            UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
            UpdateBboxs();
            UpdateBboxListView();
            UpdateBboxEditorForm(initialIndex);
        }

        // Bitmap.Clone()を使った方法より高速らしい?
        private Bitmap CropImage(Bitmap src, Rectangle srcBbox)
        {
            Bitmap dst = new Bitmap(srcBbox.Width, srcBbox.Height);
            using (Graphics g = Graphics.FromImage(dst))
            {
                Rectangle dstBbox = new Rectangle(0, 0, srcBbox.Width, srcBbox.Height);
                g.DrawImage(src, dstBbox, srcBbox, GraphicsUnit.Pixel);
            }
            return dst;
        }

        ProgressForm GetProgressForm(int max)
        {
            ProgressForm progressForm = new ProgressForm(max);
            progressForm.Owner = this;
            progressForm.Left = this.Left + (this.Width - progressForm.Width) / 2;
            progressForm.Top = this.Top + (this.Height - progressForm.Height) / 2;

            return progressForm;
        }

        static private int RandomInteger(RandomNumberGenerator rng, int start, int end)
        {
            byte[] rand = new byte[4];
            rng.GetBytes(rand);
            int ret = Math.Abs(BitConverter.ToInt32(rand, 0) % (end - start + 1)) + start;
            return ret;
        }

        private void RandomCrop(int idx, int n, int min, int max)
        {
            using (var baseImage = new Bitmap(mBaseImages[idx].Path))
            {
                int baseMin = Math.Min(baseImage.Size.Width, baseImage.Size.Height);
                min = min < baseMin ? min : baseMin;
                max = max < baseMin ? max : baseMin;
                for (int i = 0; i < n; i++)
                {
                    int width = RandomInteger(mRandGen, min, max);
                    int x = RandomInteger(mRandGen, 0, baseImage.Size.Width - width);
                    int y = RandomInteger(mRandGen, 0, baseImage.Size.Height - width);
                    mBaseImages[idx].Bboxs.Add(new Rectangle(x, y, width, width));
                }
            }
        }

        public void RandomCropOne(int n, int min, int max)
        {
            RandomCrop(mCurrentImageIndex, n, min, max);
            UpdateBboxs();
            UpdateBboxListView();
            UpdateBboxEditorForm();
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
                UpdateBboxs();
                UpdateBboxListView();
                UpdateBboxEditorForm();
            }
        }

        static private void GetFiles(string dir, string[] ext, List<string> files)
        {
            try
            {
                string[] entries = Directory.GetFileSystemEntries(dir);
                foreach (string entry in entries)
                {
                    if (!Directory.Exists(entry) && ext.Contains(Path.GetExtension(entry).ToLower()))
                        files.Add(entry);
                }
            }
            catch { }
        }

        static private void GetFilesRecursive(string dir, string[] ext, List<string> files)
        {
            try
            {
                string[] entries = Directory.GetFileSystemEntries(dir);
                foreach (string entry in entries)
                {
                    if (Directory.Exists(entry))
                        GetFilesRecursive(entry, ext, files);
                    else if (ext.Contains(Path.GetExtension(entry).ToLower()))
                        files.Add(entry);
                }
            }
            catch { }
        }

        private void InitializeImageContainers(string path)
        {
            SetControlMode(false);
            mIsRecursive = false;
            mWorkingDirectory = Path.GetDirectoryName(path);
            if (mBaseImages != null)
                mBaseImages.Clear();

            ImageContainer baseImage = new ImageContainer();
            baseImage.Path = path;
            baseImage.FileName = Path.GetFileName(path);
            baseImage.Bboxs = new List<Rectangle>();
            mBaseImages.Add(baseImage);

            mCurrentImageIndex = 0;
            trackBar1.Maximum = mBaseImages.Count;
            trackBar1.Value = 1;
            UpdatePictureBox(mBaseImages[0].Path);
            UpdateBboxListView(false);
            UpdateBboxEditorForm();
            SetControlMode(true);
        }

        private async void InitializeImageContainers(string workingDirectory, bool isRecursive = false)
        {
            try
            {
                SetControlMode(false);
                var files = new List<string>();
                mIsRecursive = isRecursive;
                mWorkingDirectory = workingDirectory;
                string trimdir = workingDirectory.TrimEnd('\\') + "\\";
                if (isRecursive)
                    GetFilesRecursive(workingDirectory, mAvailableFormats, files);
                else
                    GetFiles(workingDirectory, mAvailableFormats, files);

                if (files.Count == 0)
                    return;
                else if (mBaseImages != null)
                    mBaseImages.Clear();

                using (var progressForm = GetProgressForm(files.Count))
                {
                    progressForm.Show();
                    await Task.Run(() =>
                    {
                        int removeIndex = files[0].IndexOf(trimdir) + trimdir.Length;
                        foreach (string file in files)
                        {
                            ImageContainer baseImage = new ImageContainer();
                            baseImage.Path = file;
                            baseImage.FileName = file.Remove(0, removeIndex);
                            baseImage.Bboxs = new List<Rectangle>();
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
                UpdateBboxListView(false);
                UpdateBboxEditorForm();
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
            List<string> files = new List<string>();
            string trimdir = mWorkingDirectory.TrimEnd('\\') + "\\";
            if (mIsRecursive)
                GetFilesRecursive(mWorkingDirectory, mAvailableFormats, files);
            else
                GetFiles(mWorkingDirectory, mAvailableFormats, files);

            if (files.Count == 0)
                return;
            else if (mBaseImages != null)
                mBaseImages.Clear();

            int removeIndex = files[0].IndexOf(trimdir) + trimdir.Length;
            for (int i = 0; i < files.Count; i++)
            {
                var baseImage = new ImageContainer();
                if (!xmlImages.ContainsKey(files[i]))
                {
                    baseImage.Path = files[i];
                    baseImage.FileName = files[i].Remove(0, removeIndex);
                    baseImage.Bboxs = new List<Rectangle>();
                }
                else
                    baseImage = xmlImages[files[i]];

                if (baseImage.Path == currentFilePath)
                    mCurrentImageIndex = i;

                mBaseImages.Add(baseImage);
            }
        }

        private void Serialize(string filepath)
        {
            var sw = new StreamWriter(filepath, false, new System.Text.UTF8Encoding(false));
            sw.WriteLine("<Application>");
            sw.WriteLine("  <Headers>");
            sw.WriteLine(String.Format("    <Directory>{0}</Directory>", SecurityElement.Escape(mWorkingDirectory)));
            sw.WriteLine(String.Format("    <IsRecursive>{0}</IsRecursive>", mIsRecursive.ToString()));
            sw.WriteLine(String.Format("    <CurrentFilePath>{0}</CurrentFilePath>", SecurityElement.Escape(mBaseImages[mCurrentImageIndex].Path)));
            sw.WriteLine("  </Headers>");
            foreach (ImageContainer baseImage in mBaseImages)
            {
                if (baseImage.Bboxs.Count > 0)
                {
                    sw.WriteLine("  <ImageContainer>");
                    sw.WriteLine(String.Format("    <FileName>{0}</FileName>", SecurityElement.Escape(baseImage.FileName)));
                    foreach (var c in baseImage.Bboxs)
                    {
                        sw.WriteLine("    <Bbox>");
                        sw.WriteLine(String.Format("      <X>{0}</X>", c.X));
                        sw.WriteLine(String.Format("      <Y>{0}</Y>", c.Y));
                        sw.WriteLine(String.Format("      <Width>{0}</Width>", c.Width));
                        sw.WriteLine(String.Format("      <Height>{0}</Height>", c.Height));
                        sw.WriteLine("    </Bbox>");
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
                if (baseImage.Bboxs.Count > 0)
                {
                    string imgPath = Path.Combine(mWorkingDirectory, baseImage.FileName);
                    sw.WriteLine(String.Format("    <image file=\'{0}\'>", SecurityElement.Escape(imgPath)));
                    foreach (var c in baseImage.Bboxs)
                    {
                        sw.WriteLine(String.Format("      <box top=\'{0}\' left=\'{1}\' width=\'{2}\' height=\'{3}\'/>",
                            c.Y, c.X, c.Width, c.Height));
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
                // 古いバージョンとの互換性維持
                XmlNode isRecursiveNode = xmlDocument.SelectSingleNode("Application/Headers/IsRecursive");
                if (isRecursiveNode != null)
                    mIsRecursive = Convert.ToBoolean(isRecursiveNode.InnerText);
                else
                    mIsRecursive = true;
                string currentFilePath = xmlDocument.SelectSingleNode("Application/Headers/CurrentFilePath").InnerText.Trim();
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
                            XmlNodeList xmlNodeList2 = xmlNode1.SelectNodes("Bbox");
                            var tmpBboxs = new List<Rectangle>();
                            foreach (XmlNode xmlNode2 in xmlNodeList2)
                            {
                                int x = int.Parse(xmlNode2.SelectSingleNode("X").InnerText.Trim());
                                int y = int.Parse(xmlNode2.SelectSingleNode("Y").InnerText.Trim());
                                int width = int.Parse(xmlNode2.SelectSingleNode("Width").InnerText.Trim());
                                int height = int.Parse(xmlNode2.SelectSingleNode("Height").InnerText.Trim());
                                tmpBboxs.Add(new Rectangle(x, y, width, height));
                            }
                            ImageContainer baseImage = new ImageContainer();
                            baseImage.Path = Path.Combine(mWorkingDirectory, fileName);
                            baseImage.FileName = fileName;
                            baseImage.Bboxs = tmpBboxs;
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
                UpdateBboxs();
                UpdateBboxListView();
                UpdateBboxEditorForm();
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

        public void InflateBbox(int value, int idx = -1)
        {
            if (mBaseImages.Count == 0)
                return;

            if (idx < 0)
                idx = mBaseImages[mCurrentImageIndex].Bboxs.Count - 1;
            if (idx >= 0 && idx < mBaseImages[mCurrentImageIndex].Bboxs.Count)
            {
                Rectangle bbox = mBaseImages[mCurrentImageIndex].Bboxs[idx];
                bbox.Inflate(value, value);
                if (bbox.Width > 0 && bbox.Height > 0)
                {
                    if (EnabledAdjust.Checked)
                    {
                        if (bbox.X < 0)
                            bbox.X = 0;
                        else if (bbox.X + bbox.Width > pictureBox1.Image.Size.Width)
                            bbox.X = pictureBox1.Image.Size.Width - bbox.Width;
                        if (bbox.Y < 0)
                            bbox.Y = 0;
                        else if (bbox.Y + bbox.Height > pictureBox1.Image.Size.Height)
                            bbox.Y = pictureBox1.Image.Size.Height - bbox.Height;
                    }
                    mBaseImages[mCurrentImageIndex].Bboxs[idx] = bbox;
                    UpdateBboxListView();
                    UpdateBboxEditorForm(idx);
                    UpdateBboxs();
                }
            }
        }

        public void SetBboxMember(string member, int value, int idx = -1)
        {
            if (mBaseImages.Count == 0)
                return;

            if (idx < 0)
                idx = mBaseImages[mCurrentImageIndex].Bboxs.Count - 1;
            if (idx >= 0 && idx < mBaseImages[mCurrentImageIndex].Bboxs.Count)
            {
                Rectangle bbox = mBaseImages[mCurrentImageIndex].Bboxs[idx];

                // なぜか動かない
                // PropertyInfo pi = bbox.GetType().GetProperty(member);
                // if (pi != null)
                //     pi.SetValue(bbox, value, null);

                switch (member)
                {
                    case "X":
                        bbox.X = value;
                        break;
                    case "Y":
                        bbox.Y = value;
                        break;
                    case "Width":
                        bbox.Width = value;
                        break;
                    case "Height":
                        bbox.Height = value;
                        break;
                }
                AdjustBbox(ref bbox);
                mBaseImages[mCurrentImageIndex].Bboxs[idx] = bbox;

                UpdateBboxListView();
                UpdateBboxEditorForm(idx);
                UpdateBboxs();
            }
        }

        private void ExportFillImage(int imageIndex, string exportDirectory, SerialNameGenerator sng)
        {
            if (mBaseImages[imageIndex].Bboxs.Count == 0)
                return;

            using (var baseImage = new Bitmap(mBaseImages[imageIndex].Path))
            using (Graphics g = Graphics.FromImage(baseImage))
            using (var colorBrush = new SolidBrush(mColor))
            {
                string savePath = exportDirectory + "\\" + sng.Create(exportDirectory) + ".png";
                for (int j = 0; j < mBaseImages[imageIndex].Bboxs.Count; j++)
                    g.FillRectangle(colorBrush, mBaseImages[imageIndex].Bboxs[j]);
                baseImage.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void ExportCropImage(int imageIndex, string exportDirectory, SerialNameGenerator sng)
        {
            if (mBaseImages[imageIndex].Bboxs.Count == 0)
                return;

            int count = 0;
            using (var baseImage = new Bitmap(mBaseImages[imageIndex].Path))
            {
                for (int j = 0; j < mBaseImages[imageIndex].Bboxs.Count; j++)
                {
                    Rectangle bbox = mBaseImages[imageIndex].Bboxs[j];
                    using (Bitmap bboxImage = CropImage(baseImage, bbox))
                    {
                        string newFilePath = exportDirectory + "\\" + sng.Create(exportDirectory) + ".png";
                        bboxImage.Save(newFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    count++;
                }
            }
        }

        private void ShowBboxEditorForm()
        {
            mBboxEditorForm = new BboxEditorForm();
            mBboxEditorForm.Owner = this;
            mBboxEditorForm.FormClosed += new FormClosedEventHandler(BboxEditorForm_FormClosed);
            mBboxEditorForm.Show();
            mBboxEditorForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
        }

        private void SetControlMode(bool flag)
        {
            MappingBbox.Enabled = flag;
            OpenFolder_ToolStripMenuItem.Enabled = flag;
            OpenFolderRecursive_ToolStripMenuItem.Enabled = flag;
            ExportCrop_ToolStripMenuItem.Enabled = flag;
            ExportFill_ToolStripMenuItem.Enabled = flag;
            ExportImglab_ToolStripMenuItem.Enabled = flag;
            SaveAsXml_ToolStripMenuItem.Enabled = flag;
            SaveXml_ToolStripMenuItem.Enabled = flag;
            LoadXml_ToolStripMenuItem.Enabled = flag;
            RandomCrop_ToolStripMenuItem.Enabled = flag;
            RandomCropAll_ToolStripMenuItem.Enabled = flag;
            toolStripButton5.Enabled = flag;
        }

        private void InitializeControlMode(bool flag)
        {
            MappingBbox.Enabled = flag;
            ExportCrop_ToolStripMenuItem.Enabled = flag;
            ExportFill_ToolStripMenuItem.Enabled = flag;
            ExportImglab_ToolStripMenuItem.Enabled = flag;
            SaveAsXml_ToolStripMenuItem.Enabled = flag;
            SaveXml_ToolStripMenuItem.Enabled = flag;
            RandomCrop_ToolStripMenuItem.Enabled = flag;
            RandomCropAll_ToolStripMenuItem.Enabled = flag;
            toolStripButton5.Enabled = flag;
        }

        public void SetFixedBboxSize(int width, int height)
        {
            mFixedWidth = width;
            mFixedHeight = height;
        }

        private void NextImage()
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex >= mBaseImages.Count - 1)
                return;

            mCurrentImageIndex++;
            trackBar1.Value++;
            UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
            UpdateBboxs();
            UpdateBboxListView();
            UpdateBboxEditorForm(0);
        }

        private void PrevImage()
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex <= 0)
                return;

            mCurrentImageIndex--;
            trackBar1.Value--;
            UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
            UpdateBboxs();
            UpdateBboxListView();
            UpdateBboxEditorForm(0);
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
    }
}
