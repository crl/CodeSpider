using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;

namespace Translater
{
    public class LangMap
    {
        public string cn="";
        public string en="";
        public string cht ="";
        public string jp = "";
        public string kor = "";

        public int rowIndex=0;
        public string crc="-1";

        public bool invalid = false;
    }

    public class LangsPool
    {
        public static List<string> langsKey = new List<string>
        {
            "en","cht","jp","kor","th"
        };

        public static string LangPrefix= "C:/Users/chenronglong/Desktop/Code/";
        private static Dictionary<string, LangMap> langMapping = new Dictionary<string, LangMap>();
        private static int LastRowNum = 2;
        public static string GetTranslater(string src, string lang)
        {
            var result = "";

            LangMap map;
            if(langMapping.TryGetValue(src,out map))
            {
                switch (lang)
                {
                    case "en":
                        result= map.en;
                        break;
                    case "cht":
                        result = map.cht ;
                        break;
                    case "jp":
                        result = map.jp;
                        break;
                    case "kor":
                        result = map.kor;
                        break;
                }
            }

            return result;
        }

        internal static bool AddTranslater(string src, string dst, string lang, string crc = "-1")
        {
            LangMap map;
            if (langMapping.TryGetValue(src, out map) == false)
            {
                map = new LangMap();
                map.cn = src;
                map.crc = crc;
                langMapping[src] = map;

                map.rowIndex = LastRowNum + 1;
                LastRowNum++;
            }
            if (map.crc == "-1")
            {
                map.crc = crc;
            }

            switch (lang)
            {
                case "en":
                    map.en = dst;
                    break;
                case "cht":
                    map.cht  = dst;
                    break;
                case "jp":
                    map.jp = dst;
                    break;
                case "kor":
                    map.kor = dst;
                    break;
            }
            map.invalid = true;
            return true;
        }

        public static void LoadExcel()
        {
            langMapping.Clear();

            var path = Path.Combine(LangPrefix, "lang.xlsx");
            if (File.Exists(path) == false)
            {
                return;
            }

            using (var fs = new FileStream(path, FileMode.Open))
            {
                var workbook = new XSSFWorkbook(fs);
                var sheet = workbook.GetSheet("lang");
                LastRowNum = sheet.LastRowNum;

                var rowCount = LastRowNum + 1;
                var startRow = sheet.FirstRowNum + 2;

                for (int i = startRow; i < rowCount; i++)
                {
                    var row = sheet.GetRow(i);

                    var key = row.GetCell(1).StringCellValue;
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    LangMap map;
                    if (langMapping.TryGetValue(key, out map) == false)
                    {
                        map = langMapping[key] = new LangMap();
                        map.cn = key;
                        map.rowIndex = i;
                    }

                    map.crc = row.GetCell(0).StringCellValue;
                    map.en = row.GetCell(2)?.StringCellValue;
                    map.cht = row.GetCell(3)?.StringCellValue;
                    map.jp = row.GetCell(4)?.StringCellValue;
                    map.kor = row.GetCell(5)?.StringCellValue;
                }
            }
        }

        public static void SaveExcel()
        {
            ISheet sheet;
            var path = Path.Combine(LangPrefix, "lang.xlsx");
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var workbook = new XSSFWorkbook(fs);
                sheet = workbook.GetSheet("lang");
                if (sheet == null)
                {
                    sheet = workbook.CreateSheet("lang");
                }
                var lastRowNum = sheet.LastRowNum;

                foreach (var item in langMapping.Values)
                {
                    if (item.rowIndex > lastRowNum)
                    {
                        var row = sheet.CreateRow(item.rowIndex);

                        row.CreateCell(0).SetCellValue(item.crc);
                        row.CreateCell(1).SetCellValue(item.cn);
                        row.CreateCell(2).SetCellValue(item.en);
                        row.CreateCell(3).SetCellValue(item.cht);
                        row.CreateCell(4).SetCellValue(item.jp);
                        row.CreateCell(5).SetCellValue(item.kor);
                    }
                    else if (item.invalid)
                    {
                        var row = sheet.GetRow(item.rowIndex);

                        row.Cells[0].SetCellValue(item.crc);
                        var cell = row.GetCell(1) ?? row.CreateCell(1);
                        cell.SetCellValue(item.cn);
                        cell = row.GetCell(2) ?? row.CreateCell(2);
                        cell.SetCellValue(item.en);
                        cell = row.GetCell(3) ?? row.CreateCell(3);
                        cell.SetCellValue(item.cht);
                        cell = row.GetCell(4) ??row.CreateCell(4);
                        cell.SetCellValue(item.jp);
                        cell = row.GetCell(5) ?? row.CreateCell(5);
                        cell.SetCellValue(item.kor);
                    }
                }
            }

            //path = Path.Combine(LangPrefix, "lang_c.xlsx");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var workbook = new XSSFWorkbook();
                var newSheet = workbook.CreateSheet("lang");

                var lastRowNum = sheet.LastRowNum + 1;
                for (int i = sheet.FirstRowNum; i < lastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var newRow = newSheet.CreateRow(i);
                    for (int j = 0; j < row.Cells.Count; j++)
                    {
                        var newCell = newRow.CreateCell(j);
                        newCell.SetCellValue(row.Cells[j].StringCellValue);
                    }
                }
                workbook.Write(fs);
            }
        }

        internal static void Test()
        {
            var map = new LangMap();
            map.cn = "你好";
            map.rowIndex = 2;
            langMapping.Add(map.cn, map);
        }
    }
}
