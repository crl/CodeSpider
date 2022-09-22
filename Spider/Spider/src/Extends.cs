using System;

namespace Spider
{
    public static class Extends
    {
        public static string ReplaceAt(this string str, int index, int length, string replace)
        {
            return str.Remove(index, Math.Min(length, str.Length - index)).Insert(index, replace);
        }

        public static string InsertStringIfNotHas(this string str, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return str;
            }
            var index = str.IndexOf(key);
            if (index == -1)
            {
                str = key + "\r\n" + str;
            }

            return str;
        }
    }
}