using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace D8_Demo.Tool;

public class Tools
{
    /// <summary>
    /// 将十六进制字符串的字节对顺序颠倒
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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
    /// <summary>
    /// 将十六进制字符串转换为字节数组
    /// </summary>
    /// <param name="hexStr"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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
    
    /// 3DES加密
    /// <summary>
    /// 3DES 加密（ECB 模式，无填充）—— 智能卡场景常用
    /// </summary>
    /// <param name="key">24字节密钥</param>
    /// <param name="data">8字节倍数的明文</param>
    /// <returns>密文</returns>
    public static byte[] EncryptEcb(byte[] data, byte[] key)
    {
        if (key.Length == 8)
        {
            byte[] k24 = new byte[24];
            Array.Copy(key, 0, k24, 0, 8);
            Array.Copy(key, 0, k24, 8, 8);
            Array.Copy(key, 0, k24, 16, 8);
            key = k24;
        }

        var engine = new DesEdeEngine();
        engine.Init(true, new KeyParameter(key));

        byte[] output = new byte[data.Length];
        for (int i = 0; i < data.Length; i += 8)
            engine.ProcessBlock(data, i, output, i);
        return output;
    }

}