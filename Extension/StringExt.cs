using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringExt
{
    public static bool IsDigit(this string str) {
        if (str == string.Empty)
            return false;
        return str.All(char.IsDigit);
    }

    public static bool IsContainDigit(this string str) {
        return str.Any(char.IsDigit);
    }

    public static string ReplaceAll(this string str, string oldVal, string newVal) {
        return Regex.Replace(str, oldVal, newVal);
    }

    public static int TakeOutNumber(this string str) {
        string strNum = Regex.Replace(str, "[^0-9]+", "");
        return strNum.IsDigit() ? int.Parse(strNum) : 0;
    }

    public static bool TakeOutNumber(this string str, out int outNum) {
        string strNum = Regex.Replace(str, "[^0-9]+", "");
        outNum = strNum.IsDigit() ? int.Parse(strNum) : 0;
        return strNum.IsDigit();
    }
    
        public static string SubstringLeft(this string str, string separator) {
        return str.Substring(0, str.IndexOf(separator));
    }

    public static string SubstringRight(this string str, string separator) {
        return str.Substring(str.IndexOf(separator) + 1);
    }

    public static string SubstringCenter(this string str, string start, string end) {
        return str.SubstringRight(start).SubstringLeft(end);
    }
}
