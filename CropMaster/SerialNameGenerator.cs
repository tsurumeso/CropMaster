﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CropMaster
{
    public class SerialNameGenerator
    {
        int digits;
        int initialMax;
        string formatPattern;
        string tagName;
        string oldDir;

        public SerialNameGenerator(string tagName, int digits, string formatPattern)
        {
            initialMax = -1;
            oldDir = "";
            this.formatPattern = formatPattern;
            this.tagName = tagName;
            this.digits = digits;
        }

        public string Create(string dir)
        {
            if (oldDir != dir)
            {
                string pattern = String.Format(@"(?i)_(\d{{{0}}})\.({1})$", digits, formatPattern);
                DirectoryInfo di = new DirectoryInfo(dir);

                initialMax = di.GetFiles(tagName + "_*.*")          // パターンに一致するファイルを取得する
                    .Select(fi => Regex.Match(fi.Name, pattern))    // ファイルの中で数値のものを探す
                    .Where(m => m.Success)                          // 該当するファイルだけに絞り込む
                    .Select(m => Int32.Parse(m.Groups[1].Value))    // 数値を取得する
                    .DefaultIfEmpty(0)                              // １つも該当しなかった場合は 0 とする
                    .Max();                                         // 最大値を取得する
            }
            string serial = (++initialMax).ToString().PadLeft(digits, '0');
            return tagName + "_" + serial;
        }
    }
}