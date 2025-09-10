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
    
}