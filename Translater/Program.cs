using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Translater
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            /*LangsPool.Test();
            LangsPool.SaveExcel();
            return;*/

            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            var dir = dialog.SelectedPath;
            if (string.IsNullOrEmpty(dir))
            {
                MessageBox.Show("文件夹路径不能为空", "提示");
                return;
            }

            var langPath = Path.Combine(dir, "lang.xlsx");
            if (File.Exists(langPath))
            {
                LangsPool.LangPrefix = dir;
            }

            var list = Directory.GetFiles(dir, "*.xml", System.IO.SearchOption.AllDirectories);
            var len = list.Length;
            if (list.Length == 0)
            {
                MessageBox.Show("文件夹内没有xml", "提示");
                return;
            }
            LangsPool.LoadExcel();
            var i = 0;
            foreach (var path in list)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName.EndsWith("_cn") == false)
                {
                    continue;
                }
                Console.WriteLine("{0}\t {1}/{2}", path, i++, len);
                transformFile(path);
            }
            Console.WriteLine("All Complete!!!");
            Console.ReadLine();
        }


        private static void transformFile(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            //LangsPool.LoadExcel();
            var langs = doc.SelectSingleNode("langs");
            var crc = langs.Attributes["crc"].Value;
            var list = doc.SelectNodes("langs/item");
            foreach (XmlNode item in list)
            {
                var text = item.InnerText.Trim('\"', '\'');
                foreach (var lang in LangsPool.langsKey)
                {
                    var value = LangsPool.GetTranslater(text, lang);
                    if (string.IsNullOrEmpty(value))
                    {
                        var result = Translater.Translate(text, "zh", lang);

                        if (result != null)
                        {
                            LangsPool.AddTranslater(text, result.trans_result[0].dst, lang, crc);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Thread.Sleep(2000);
                        }
                    }
                }
            }
            LangsPool.SaveExcel();
        }
    }
}
