using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CropMaster
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class ImageContainer
    {
        public string Path;
        public string FileName;
        public List<Rectangle> Rectangles;
    }
}
