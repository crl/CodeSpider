using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;

namespace Translater
{
    public class TranResult
    {
        public string from;
        public string to;

        public TranResultItem[] trans_result;
    }
    public class TranResultItem
    {
        public string src;
        public string dst;
    }

    /// <summary>
    /// https://fanyi-api.baidu.com/
    /// </summary>
    public class Translater
    {  // 改成您的APP ID
        private static string appId = "20220727001284925";
        // 改成您的密钥
        private static string secretKey = "uRU28ZSzNrhmdrLHbk4N";
        public static TranResult Translate(string q, string from, string to)
        {
            Random rd = new Random();
            string salt = rd.Next(100000).ToString();

            string sign = EncryptString(appId + q + salt + secretKey);
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(q);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            try
            {
                var o = Json.Decode<TranResult>(retString);
                if (o.trans_result == null)
                {
                    Console.WriteLine("Translate Error:{0}", retString);
                    return null;
                }
                return o;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return null;
        }
        // 计算MD5值
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
