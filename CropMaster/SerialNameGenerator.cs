using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CropMaster
{
    public class SerialNameGenerator
    {
        int dig;
        string formatPattern;
        string tagName;
        int initialMax;
        string oldDir;

        public SerialNameGenerator(string tagName, int dig, string formatPattern)
        {
            initialMax = -1;
            oldDir = "";
            this.formatPattern = formatPattern;
            this.tagName = tagName;
            this.dig = dig;
        }

        public string Create(string dir)
        {
            if (oldDir != dir)
            {
                string pattern = String.Format(@"(?i)_(\d{{{0}}})\.({1})$", dig, formatPattern);
                DirectoryInfo di = new DirectoryInfo(dir);

                initialMax = di.GetFiles(tagName + "_*.*")          // パターンに一致するファイルを取得する
                    .Select(fi => Regex.Match(fi.Name, pattern))    // ファイルの中で数値のものを探す
                    .Where(m => m.Success)                          // 該当するファイルだけに絞り込む
                    .Select(m => Int32.Parse(m.Groups[1].Value))    // 数値を取得する
                    .DefaultIfEmpty(0)                              // １つも該当しなかった場合は 0 とする
                    .Max();                                         // 最大値を取得する
            }
            string sn = (++initialMax).ToString().PadLeft(dig, '0');
            return tagName + "_" + sn;
        }
    }
}