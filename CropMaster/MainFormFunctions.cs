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
        Rectangle mOldRect = new Rectangle(0, 0, 0, 0);
        RandomNumberGenerator rng = RandomNumberGenerator.Create();

        delegate void UpdateScaleAndPadDelegate(string path);
        delegate void AdjustRectangleToImageDelegate(ref Rectangle rect);

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
                return GetSquareRectangle(mouseDown, mouseCurrent);
            else if (mWHRateIndex == 1)
                return GetFixedRectangle(mouseCurrent);
            else if (mWHRateIndex == 2)
                return GetAnyAspectsRectangle(mouseDown, mouseCurrent);

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
                mScale = 1f;
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
                if (p.X > boxRect.X && p.X < boxRect.X + boxRect.Width && 
                    p.Y > boxRect.Y && p.Y < boxRect.Y + boxRect.Height)
                    return i;
            }
            return -1;
        }

        private void UpdateRectEditorForm(int idx = -1)
        {
            if (mRectEditorForm.IsDisposed)
                return;

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
            if (mRectEditorForm.IsDisposed || mBaseImages.Count == 0)
                return;

            if (isNext)
            {
                if (idx < mBaseImages[mCurrentImageIndex].Rectangles.Count - 1 && idx >= 0)
                    UpdateRectEditorForm(++idx);
                else
                    NextImageWithRect(true);
            }
            else
            {
                if (idx > 0)
                    UpdateRectEditorForm(--idx);
                else
                    PrevImageWithRect(false);
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
            if (imagePath == null)
                return;

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

        void NextImageWithRect(bool selectFirstRect)
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex >= mBaseImages.Count)
                return;

            int initialIndex = selectFirstRect ? 0 : -1;
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

        void PrevImageWithRect(bool selectFirstRect)
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex <= 0)
                return;

            int initialIndex = selectFirstRect ? 0 : -1;
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
            Bitmap baseImage = new Bitmap(mBaseImages[idx].Path);
            int baseMin = Math.Min(baseImage.Size.Width, baseImage.Size.Height);
            min = min < baseMin ? min : baseMin;
            max = max < baseMin ? max : baseMin;
            for (int i = 0; i < n; i++)
            {
                int width = RandomInteger(rng, min, max);
                int x = RandomInteger(rng, 0, baseImage.Size.Width - width);
                int y = RandomInteger(rng, 0, baseImage.Size.Height - width);
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

        private async void InitializeImageContainers(string workingDirectory, bool isRecursive = false)
        {
            try
            {
                var files = new List<string>();
                SetControlMode(false);
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
            string trimdir = mWorkingDirectory.TrimEnd('\\') + "\\";
            List<string> files = new List<string>();
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
                    baseImage.Rectangles = new List<Rectangle>();
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
            if (mBaseImages.Count == 0)
                return;

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

        public void SetMemberRectangle(string member, int value, int idx = -1)
        {
            if (mBaseImages.Count == 0)
                return;

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

        private void ExportFillImage(int imageIndex, string exportDirectory, SerialNameGenerator sng)
        {
            if (mBaseImages[imageIndex].Rectangles.Count == 0)
                return;

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

        private void ExportCropImage(int imageIndex, string exportDirectory, SerialNameGenerator sng)
        {
            if (mBaseImages[imageIndex].Rectangles.Count == 0)
                return;

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
            OpenRecursive_ToolStripMenuItem.Enabled = flag;
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
            MappingRectangle.Enabled = flag;
            ExportCrop_ToolStripMenuItem.Enabled = flag;
            ExportFill_ToolStripMenuItem.Enabled = flag;
            ExportImglab_ToolStripMenuItem.Enabled = flag;
            SaveAsXml_ToolStripMenuItem.Enabled = flag;
            SaveXml_ToolStripMenuItem.Enabled = flag;
            RandomCrop_ToolStripMenuItem.Enabled = flag;
            RandomCropAll_ToolStripMenuItem.Enabled = flag;
            toolStripButton5.Enabled = flag;
        }

        public void SetFixedRectangleSize(int width, int height)
        {
            mFixedWidth = width;
            mFixedHeight = height;
        }

        private void NextImage()
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex >= mBaseImages.Count)
                return;

            mCurrentImageIndex++;
            trackBar1.Value++;
            UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
            UpdateRectangles();
            UpdateRectListView();
            UpdateRectEditorForm(0);
        }

        private void PrevImage()
        {
            if (mBaseImages.Count == 0 || mCurrentImageIndex <= 0)
                return;

            mCurrentImageIndex--;
            trackBar1.Value--;
            UpdatePictureBox(mBaseImages[mCurrentImageIndex].Path);
            UpdateRectangles();
            UpdateRectListView();
            UpdateRectEditorForm(0);
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
