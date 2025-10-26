using H.Extensions.System;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System;

public static class AesHelper
{
    /// <summary>
    /// 加密方法
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static byte[] ToAesBytes(this byte[] plainText,string password)
    {
        return CryptoHelper.Encrypt(plainText, GetAesCryptoServiceProvider(password));
    }    

    /// <summary>
    /// 解密方法
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static byte[] FromAesBytes(this byte[] cipherText,string password)
    {
        return CryptoHelper.Decrypt(cipherText, GetAesCryptoServiceProvider(password));
    }

    
    /// <summary>
    /// 加密方法
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string ToAesString(this string plainText,string password)
    {
        return CryptoHelper.Encrypt(plainText, GetAesCryptoServiceProvider(password));
    }

    /// <summary>
    /// 解密方法
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string FromAesString(this string cipherText,string password)
    {
        return CryptoHelper.Encrypt(cipherText, GetAesCryptoServiceProvider(password));
    }

    private static AesCryptoServiceProvider GetAesCryptoServiceProvider(string password)
    {
        AesCryptoServiceProvider sa = new AesCryptoServiceProvider();
        CryptoHelper.SetKeyIV(sa, password);
        return sa;
    }
}