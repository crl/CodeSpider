using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Spider
{
    public class MatchFileVO
    {
        private static List<string> ASSETS =new List<string>()
        {
            "/Assets/RawRes/ClientLua/","ClientLua",
            "/Assets/RawRes/ClientLuaPlugin/","ClientLua",
            "/Assets/Scripts/","Code",
            "/TempRes/","Config"
        };

        internal string filePath;

        public MatchLineVO[] lines;

        public MatchFileVO(int len)
        {
            this.lines = new MatchLineVO[len];
        }

        private StringBuilder sb;

        /// <summary>
        /// 是否跳过替换
        /// </summary>
        public bool skipReplace = false;

        internal string Load()
        {
            if (sb == null)
            {
                sb = new StringBuilder();
            }

            sb.Remove(0, sb.Length);
            foreach (var line in lines)
            {
                sb.AppendLine(line.rawContent);
            }

            return sb.ToString();
        }

        internal void Save()
        {
            if (skipReplace == false)
            {
                var content = Load();
                File.WriteAllText(filePath, content, Encoding.UTF8);
            }

            for (int i = 0,len=ASSETS.Count; i < len; i+=2)
            {
                var asset = ASSETS[i];
                //保存语言包
                var assetsIndex = filePath.IndexOf(asset);
                if (assetsIndex == -1)
                {
                    continue;
                }

                var saveCrcNamePath = filePath.Substring(assetsIndex + asset.Length);
                var savePath = string.Format("{0}/Lang/{1}", filePath.Substring(0, assetsIndex), ASSETS[i + 1]);
                var crc = Crc32.GetCrc32(saveCrcNamePath);
                if (Directory.Exists(savePath) == false)
                {
                    Directory.CreateDirectory(savePath);
                }

                var sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sb.AppendLine(string.Format("<langs path=\"{0}\" crc=\"{1}\">", saveCrcNamePath, crc));

                var dic=new Dictionary<string,List<MatchTextVO>>();
                foreach (var line in lines)
                {
                    foreach (var matchText in line.matchTexts)
                    {
                        List<MatchTextVO> list;
                        if (dic.TryGetValue(matchText.value, out list)==false)
                        {
                            list = dic[matchText.value] = new List<MatchTextVO>();
                        }

                        list.Add(matchText);
                    }
                }

                foreach (var dicKey in dic.Keys)
                {
                    var list = dic[dicKey];
                    var line = "";
                    foreach (var item in list)
                    {
                        line += string.Format("{0},{1}|", item.line, item.index);
                    }
                    sb.AppendLine(string.Format("\t<item line=\"{0}\"><![CDATA[{1}]]></item>",
                        line,
                        dicKey));
                }

                sb.AppendLine("</langs>");
                savePath = string.Format("{0}/{1}_cn.xml", savePath, crc);
                File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
                break;
            }
        }

        internal List<MatchTextVO> getMatchTexts()
        {
            var result = new List<MatchTextVO>();
            foreach (var matchLine in lines)
            {
                foreach (var matchText in matchLine.matchTexts)
                {
                    result.Add(matchText);
                }
            }
            return result;
        }
    }

    public class MatchLineVO
    {
        public string rawContent;
        public List<MatchTextVO> matchTexts=new List<MatchTextVO>();
    }

    public class MatchTextVO
    {
        public int line;
        public MatchFileVO file;
        public int index;
        public string value;
    }
}