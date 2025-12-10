using System;

namespace D8_Demo.Tool;

public class Tools
{
    public static string ChangeHexPairs(string hex)
    {
        if (hex.Length % 2 != 0) throw new ArgumentException("数据位数不合法");
        var reversedHex = new char[hex.Length];
        var j = 0;
        for (var i = hex.Length - 2; i >= 0; i -= 2)
        {
            reversedHex[j++] = hex[i];
            reversedHex[j++] = hex[i + 1];
        }
        return new string(reversedHex);
    }
    public static byte[] HexStringToBytes(string hexStr)
    {
        if (string.IsNullOrEmpty(hexStr) || hexStr.Length % 2 != 0)
            throw new ArgumentException("十六进制字符串不能为空且长度为偶数", nameof(hexStr));

        byte[] bytes = new byte[hexStr.Length / 2];
        for (int i = 0; i < hexStr.Length; i += 2)
        {
            if (!byte.TryParse(hexStr.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
            {
                throw new ArgumentException($"十六进制字符串包含非法字符：{hexStr.Substring(i, 2)}", nameof(hexStr));
            }

            bytes[i / 2] = b;
        }

        return bytes;
    }
    
}