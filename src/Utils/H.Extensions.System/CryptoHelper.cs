using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace H.Extensions.System;

/// <summary>
/// “对称加密算法”的封装工具类
/// </summary>
internal static class CryptoHelper
{
    /// <summary>
    /// 加密字符串
    /// </summary>
    /// <param name="text">等待加密的文本</param>
    /// <param name="sa">“对称加密算法”实例，要求已设置过KEY和IV</param>
    /// <returns>返回Base64编码的结果</returns>
    public static string Encrypt(string text, SymmetricAlgorithm sa)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        if (sa == null)
            throw new ArgumentNullException(nameof(sa));

        byte[] input = Encoding.UTF8.GetBytes(text);
        byte[] output = Encrypt(input, sa);
        return Convert.ToBase64String(output);
    }

    /// <summary>
    /// 加密字节数组
    /// </summary>
    /// <param name="input"></param>
    /// <param name="sa">“对称加密算法”实例，要求已设置过KEY和IV</param>
    /// <returns></returns>
    public static byte[] Encrypt(byte[] input, SymmetricAlgorithm sa)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        if (sa == null)
            throw new ArgumentNullException(nameof(sa));

        ICryptoTransform transform = sa.CreateEncryptor();
        return transform.TransformFinalBlock(input, 0, input.Length);
    }

    /// <summary>
    /// 解密一个以Base64编码的加密字符串
    /// </summary>
    /// <param name="base64">等待解密的BASE64文本</param>
    /// <param name="sa">“对称加密算法”实例，要求已设置过KEY和IV</param>
    /// <returns></returns>
    public static string Decrypt(string base64, SymmetricAlgorithm sa)
    {
        if (base64 == null)
            throw new ArgumentNullException(nameof(base64));
        if (sa == null)
            throw new ArgumentNullException(nameof(sa));

        byte[] input = Convert.FromBase64String(base64);
        byte[] output = Decrypt(input, sa);
        string result = Encoding.UTF8.GetString(output);
        return result.TrimEnd('\0');
    }

    /// <summary>
    /// 解密字节数组
    /// </summary>
    /// <param name="input"></param>
    /// <param name="sa">“对称加密算法”实例，要求已设置过KEY和IV</param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] input, SymmetricAlgorithm sa)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        if (sa == null)
            throw new ArgumentNullException(nameof(sa));

        ICryptoTransform transform = sa.CreateDecryptor();
        return transform.TransformFinalBlock(input, 0, input.Length);
    }

    /// <summary>
    /// 根据指定的密码，设置“加密算法”的KEY和IV
    /// </summary>
    /// <param name="sa">“对称加密算法”实例</param>
    /// <param name="password">加密或解密的密码</param>
    [SuppressMessage("Microsoft.Reliability", "CA2000")]
    public static void SetKeyIV(SymmetricAlgorithm sa, string password)
    {
        if (sa == null)
            throw new ArgumentNullException(nameof(sa));
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        byte[] pwd = Encoding.UTF8.GetBytes(string.Concat(password, ".dNy+vpbOZ"));

        byte[] md5 = (new MD5CryptoServiceProvider()).ComputeHash(pwd);
        byte[] sha1 = (new SHA1CryptoServiceProvider()).ComputeHash(pwd);

        sa.IV = GetByteArray(md5, sa.IV.Length);
        sa.Key = GetByteArray(sha1, sa.Key.Length);
    }

    private static byte[] GetByteArray(byte[] src, int destLen)
    {
        byte[] dest = new byte[destLen];
        int p = 0;

        while (p < destLen) {
            foreach (byte b in src) {
                if (p >= destLen)
                    return dest;
                dest[p++] = b;
            }
        }

        return dest;
    }
}