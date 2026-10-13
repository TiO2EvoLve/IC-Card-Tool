using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
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
    
    public static byte[] DesEncrypt(byte[] data, byte[] key8, byte[] iv = null)
    {
        iv ??= new byte[8];
        var engine = new DesEngine();                    // 单 DES
        var cipher = new CbcBlockCipher(engine);
        var padded = new PaddedBufferedBlockCipher(cipher, new ZeroBytePadding());

        padded.Init(true, new ParametersWithIV(new DesParameters(key8), iv));
        byte[] output = new byte[padded.GetOutputSize(data.Length)];
        int len = padded.ProcessBytes(data, 0, data.Length, output, 0);
        padded.DoFinal(output, len);
        return output;
    }

}