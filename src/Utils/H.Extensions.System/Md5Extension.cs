using System;
using System.Text;

namespace System.Security.Cryptography;

public static class Md5Extension
{
    /// <summary>
    /// 获取字节数组的MD5值
    /// </summary>
    /// <param name="plainText">原始字节数组</param>
    /// <param name="salt"></param>
    public static string ToMd5HexString(this string plainText,string salt)
    {
        using (var md5 = MD5.Create())
        {
            var text = plainText;
            if (!string.IsNullOrEmpty(salt))
            {
                text += salt;
            }
            var bytes = Encoding.UTF8.GetBytes(text);
            var hashBytes = md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var hashByte in hashBytes)
            {
                sb.Append(hashByte.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 获取字节数组的MD5值
    /// </summary>
    /// <param name="plainText">原始字节数组</param>
    public static string ToMd5HexString(this string plainText)
    {
        return ToMd5HexString(plainText, string.Empty);
    }
}