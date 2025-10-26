using System;
using System.Text;

namespace System;

public static class Base64Extension
{
    public static string ToBase64String(this byte[] plainText)
    {
        return Convert.ToBase64String(plainText);
    }

    public static string ToBase64String(this string plainText)
    {
        if (plainText is null)
            throw new ArgumentNullException(nameof(plainText));
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }
    
    public static string FromBase64String(this string plainText)
    {
        var bytes = Convert.FromBase64String(plainText);
        return Encoding.UTF8.GetString(bytes);
    }
}