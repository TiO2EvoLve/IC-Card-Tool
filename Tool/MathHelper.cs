using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace D8_Demo.Tool;

public class MathHelper
{
    [DllImport(@"HDMATH20B.dll", CharSet = CharSet.Ansi)]
    public static extern UInt16 HD_3DES_MAC(string svHex, string svKey, ref byte srHex);

    [DllImport(@"HDMATH20B.dll", CharSet = CharSet.Ansi)]
    public static extern UInt16 HD_DES_MAC(string svHex, string svKey, ref byte srHex);

    [DllImport(@"HDMATH20B.dll", CharSet = CharSet.Ansi)]
    public static extern UInt16 HD_3DES_Encrypt(string svHex, string svKey, short ivMode, ref byte srHex);

    [DllImport(@"HDMATH20B.dll", CharSet = CharSet.Ansi)]
    public static extern UInt16 HD_3DES_Decrypt(string svHex, string svKey, short ivMode, ref byte srHex);

    [DllImport(@"HDMATH20B.dll", CharSet = CharSet.Ansi)]
    public static extern UInt16 HD_DES_Encrypt(string svHex, string svKey, short ivMode, ref byte srHex);

    [DllImport(@"HDMATH20B.dll", CharSet = CharSet.Ansi)]
    public static extern UInt16 HD_DES_Decrypt(string svHex, string svKey, short ivMode, ref byte srHex);

    //MD5
    static public string MD5_Hash(string str_md5_in)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] bytes_md5_in = UTF8Encoding.Default.GetBytes(str_md5_in);
        byte[] bytes_md5_out = md5.ComputeHash(bytes_md5_in);
        string str_md5_out = BitConverter.ToString(bytes_md5_out);
        //str_md5_out = str_md5_out.Replace("-", "");
        return str_md5_out;
    }

    //SHA1
    public static string SHA1_Hash(string str_sha1_in)
    {
        SHA1 sha1 = new SHA1CryptoServiceProvider();
        byte[] bytes_sha1_in = UTF8Encoding.Default.GetBytes(str_sha1_in);
        byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
        string str_sha1_out = BitConverter.ToString(bytes_sha1_out);
        //str_sha1_out = str_sha1_out.Replace("-", "");
        return str_sha1_out;
    }

    public static string DES_Encrypt(string strRandom, string key)
    {
        try
        {
            //byte[] rbuff = new byte[64];
            byte[] rbuff = new byte[strRandom.Length * 4];
            int st = HD_3DES_Encrypt(strRandom, key, 0, ref rbuff[0]);
            if (st != 0x9000)
            {
                throw new Exception("3DES Encrypt Error NO:" + st);
            }

            string Erandom = Encoding.Default.GetString(rbuff).Replace("\0", "");
            return Erandom;
        }
        catch (Exception err)
        {
            throw new Exception("3DES Encrypt Error:" + err.Message);
        }
    }

    //20191121新加3DESCBC加密
    public static string DES_EncryptCBC(string strRandom, string key)
    {
        try
        {
            //byte[] rbuff = new byte[64];
            byte[] rbuff = new byte[strRandom.Length * 4];
            int st = HD_3DES_Encrypt(strRandom, key, 1, ref rbuff[0]);
            if (st != 0x9000)
            {
                throw new Exception("3DES Encrypt Error NO:" + st);
            }

            string Erandom = Encoding.Default.GetString(rbuff).Replace("\0", "");
            return Erandom;
        }
        
        catch (Exception err)
        {
            throw new Exception("3DES Encrypt Error:" + err.Message);
        }
    }

    public static string DES_Decrypt(string strRandom, string key) //20210909新增
    {
        try
        {
            //byte[] rbuff = new byte[64];
            byte[] rbuff = new byte[strRandom.Length * 4];
            int st = HD_3DES_Decrypt(strRandom, key, 0, ref rbuff[0]);
            if (st != 0x9000)
            {
                throw new Exception("3DES Encrypt Error NO:" + st);
            }

            string Erandom = Encoding.Default.GetString(rbuff).Replace("\0", "");
            return Erandom;
        }
        catch (Exception err)
        {
            throw new Exception("3DES Decrypt Error:" + err.Message);
        }
    }

    //20191121新加3DESCBC加密
    public static string DES_DecryptCBC(string strRandom, string key) //20210909新增
    {
        try
        {
            //byte[] rbuff = new byte[64];
            byte[] rbuff = new byte[strRandom.Length * 4];
            int st = HD_3DES_Decrypt(strRandom, key, 1, ref rbuff[0]);
            if (st != 0x9000)
            {
                throw new Exception("3DES Decrypt Error NO:" + st);
            }

            string Erandom = Encoding.Default.GetString(rbuff).Replace("\0", "");
            return Erandom;
        }
        catch (Exception err)
        {
            throw new Exception("3DES Encrypt Error:" + err.Message);
        }
    }
    

    public static string DES_MAC(string data, string key) //201901281401
    {
        byte[] srHex = new byte[200];
        int st = HD_3DES_MAC(data, key, ref srHex[0]);
        if (st != 0x9000)
        {
            throw new Exception("3DES_MAC Error NO:" + st);
        }

        string textddd1 = null;
        for (int w = 0; w < 16; w++)
        {
            textddd1 += (char)srHex[w];
        }

        string hexmacout = textddd1.Substring(0, 8);
        return hexmacout;
    }

    public static string SM4_MAC(string initial, string data, string key) //20230727
    {
        string rt = initial;
        int i = data.Length / 32;
        string[] ss = new string[i];
        for (int j = 0; j < i; j++)
        {
            ss[j] = data.Substring(j * 32, 32);
        }

        //SM4时密钥长度一定为16字节
        for (int j = 0; j < i; j++)
        {
            rt = xor2(rt + "," + ss[j]);
            rt = MainSm4.Encrypt_ECB(rt, key, true);
        }

        return rt;
    }

    public static string SM4_MAC2(string data, string key) //20230727
    {
        int i = data.Length / 32;
        string[] ss = new string[i];
        for (int j = 0; j < i; j++)
        {
            ss[j] = data.Substring(j * 32, 32);
        }

        string rt = ss[0];
        //SM4时密钥长度一定为16字节
        for (int j = 1; j < i; j++)
        {
            rt = xor2(rt + "," + ss[j]);
            rt = MainSm4.Encrypt_ECB(rt, key, true);
        }

        return rt;
    }

    public static string stringtoascii(string value)
    {
        try
        {
            byte[] sbuff = System.Text.Encoding.ASCII.GetBytes(value); //数组array为对应的ASCII数组    
            string ASCIIstr2 = null;
            for (int i = 0; i < sbuff.Length; i++)
            {
                int asciicode = (int)(sbuff[i]);
                ASCIIstr2 += Convert.ToString(asciicode, 16); //字符串ASCIIstr2 为对应的ASCII字符串???
            }

            return ASCIIstr2;
        }
        catch
        {
            throw;
        }
    }

    public static string DES_MACreal(string data, string key) //20191120 8字节MAC
    {
        byte[] srHex = new byte[200];
        int st = HD_3DES_MAC(data, key, ref srHex[0]);
        if (st != 0x9000)
        {
            throw new Exception("3DES_MAC Error NO:" + st);
        }

        string textddd1 = null;
        for (int w = 0; w < 16; w++)
        {
            textddd1 += (char)srHex[w];
        }

        string hexmacout = textddd1.Substring(0, 16);
        return hexmacout;
    }

    public static string SDES_MAC(string data, string key) //201901281401  20190130
    {
        byte[] srHex = new byte[200];
        int st = HD_DES_MAC(data, key, ref srHex[0]);
        if (st != 0x9000)
        {
            throw new Exception("3DES_MAC Error NO:" + st);
        }

        string textddd1 = null;
        for (int w = 0; w < 16; w++)
        {
            textddd1 += (char)srHex[w];
        }

        string hexmacout = textddd1.Substring(0, 8);
        return hexmacout;
    }

    public static string a3DES_MAC(string data, string key)
    {
        try
        {
            data = data.Replace(" ", "");
            data = getDate_0(data);
            byte[] srHex = new byte[100];
            ushort revalue = HD_3DES_MAC(data, key, ref srHex[0]);
            if (revalue != 0x9000) throw new Exception("MAC计算命令错误,revalue=" + revalue);
            string MAC = Encoding.Default.GetString(srHex);
            return MAC;
        }
        catch (Exception err)
        {
            throw new Exception("3DES_MAC Error:" + err.Message);
        }
    }

    public static string aDES_MAC(string data, string key)
    {
        try
        {
            data = data.Replace(" ", "");
            data = getDate_0(data);
            byte[] srHex = new byte[100];
            ushort revalue = HD_DES_MAC(data, key, ref srHex[0]);
            if (revalue != 0x9000) throw new Exception("MAC计算命令错误,revalue=" + revalue);
            string MAC = Encoding.Default.GetString(srHex);
            return MAC;
        }
        catch (Exception err)
        {
            throw new Exception("3DES_MAC Error:" + err.Message);
        }
    }

    /// <summary>
    /// 计算MAC之前的报文长度修正，报文长度能被8整除报文后加8000000000000000
    /// </summary>
    /// <param name="APDU"></param>
    /// <returns></returns>
    public static string getDate_0(string APDU)
    {
        try
        {
            int f = (APDU.Length) / 2 % 8;
            if (f != 0)
            {
                APDU = APDU + "80";
                while (((APDU.Length) / 2 % 8) != 0)
                {
                    APDU = APDU + "00";
                }
            }
            else
            {
                APDU = APDU + "8000000000000000";
            }

            return (APDU);
        }
        catch (Exception err2)
        {
            throw new Exception("getDate_0报文长度修正错误:" + err2.Message);
        }
    }


    //密钥分散
    public static string getkeyL1(string key_set, string factor1)
    {
        byte[] srHex = new byte[16];
        UInt16 revalue_HD;
        string keyXX;

        long num1 = Int64.Parse(factor1, System.Globalization.NumberStyles.HexNumber);
        num1 = ~num1;
        string factor1_ = num1.ToString("X").PadLeft(16, '0');

        byte[] srHexXX = new byte[32];
        revalue_HD = 0;
        if (key_set.Length == 32)
        {
            revalue_HD = HD_3DES_Encrypt(factor1 + factor1_, key_set, 0, ref srHexXX[0]);
        }
        else
        {
            revalue_HD = HD_DES_Encrypt(factor1 + factor1_, key_set, 0, ref srHexXX[0]);
        }

        if (revalue_HD != 0x9000)
        {
            string str = "error:密钥分散错误，revalue_HD= " + revalue_HD;
            return (str);
        }

        keyXX = Encoding.Default.GetString(srHexXX);
        Array.Clear(srHexXX, 0, srHexXX.Length); //清空缓存

        return (keyXX);
    }

    //字符串转16进制字节数组 
    public static byte[] strToHexByte(string hexString)
    {
        hexString = hexString.Replace(" ", "");
        if ((hexString.Length % 2) != 0)
            hexString += " ";
        byte[] returnBytes = new byte[hexString.Length / 2];
        for (int i = 0; i < returnBytes.Length; i++)
            returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        return returnBytes;
    }

    // 字节数组转16进制字符串
    public static string byteToHexStr(byte[] bytes)
    {
        string returnStr = "";
        if (bytes != null)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                returnStr += bytes[i].ToString("X2");
            }
        }

        return returnStr;
    }

    public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
    {
        string hexString = string.Empty;
        if (bytes != null)
        {
            StringBuilder strB = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                strB.Append(bytes[i].ToString("X2"));
            }

            hexString = strB.ToString();
        }

        return hexString;
    }

    public static string xor2(string param) //20230727复制
    {
        string[] p = param.Split(',');
        int i = 0;
        int j = p[0].Length / 2;
        string s1 = "";
        while (i < j)
        {
            string str0 = p[0].Substring(2 * i, 2);
            string str1 = p[1].Substring(2 * i, 2);

            s1 = s1 + (Convert.ToInt64(str0, 16) ^ Convert.ToInt64(str1, 16)).ToString("X2");
            i++;
        }

        return s1;
    }

    public static long ComputeCRC16(byte[] buf, int nLen)
    {
        WORDtoDBYTE crc, y;
        crc.w = 0;
        crc.l = 0;
        crc.h = 0;
        y.l = 0;
        y.h = 0;
        for (int i = 0; i < nLen; i++)
        {
            y.w = CRCTAB[buf[i] ^ crc.h];
            y.h ^= crc.l;
            crc.w = y.w;
        }

        return crc.w;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct WORDtoDBYTE
    {
        [FieldOffset(0)] public long w;
        [FieldOffset(0)] public byte l;
        [FieldOffset(1)] public byte h;
    }

    public static long[] CRCTAB =
    {
        //CRCTB1: ;前字节
        0x00000, 0x01021, 0x02042, 0x03063,
        0x04084, 0x050a5, 0x060c6, 0x070e7,
        0x08108, 0x09129, 0x0a14a, 0x0b16b,
        0x0c18c, 0x0d1ad, 0x0e1ce, 0x0f1ef,
        0x01231, 0x00210, 0x03273, 0x02252,
        0x052b5, 0x04294, 0x072f7, 0x062d6,
        0x09339, 0x08318, 0x0b37b, 0x0a35a,
        0x0d3bd, 0x0c39c, 0x0f3ff, 0x0e3de,
        0x02462, 0x03443, 0x00420, 0x01401,
        0x064e6, 0x074c7, 0x044a4, 0x05485,
        0x0a56a, 0x0b54b, 0x08528, 0x09509,
        0x0e5ee, 0x0f5cf, 0x0c5ac, 0x0d58d,
        0x03653, 0x02672, 0x01611, 0x00630,
        0x076d7, 0x066f6, 0x05695, 0x046b4,
        0x0b75b, 0x0a77a, 0x09719, 0x08738,
        0x0f7df, 0x0e7fe, 0x0d79d, 0x0c7bc,
        0x048c4, 0x058e5, 0x06886, 0x078a7,
        0x00840, 0x01861, 0x02802, 0x03823,
        0x0c9cc, 0x0d9ed, 0x0e98e, 0x0f9af,
        0x08948, 0x09969, 0x0a90a, 0x0b92b,
        0x05af5, 0x04ad4, 0x07ab7, 0x06a96,
        0x01a71, 0x00a50, 0x03a33, 0x02a12,
        0x0dbfd, 0x0cbdc, 0x0fbbf, 0x0eb9e,
        0x09b79, 0x08b58, 0x0bb3b, 0x0ab1a,
        0x06ca6, 0x07c87, 0x04ce4, 0x05cc5,
        0x02c22, 0x03c03, 0x00c60, 0x01c41,
        0x0edae, 0x0fd8f, 0x0cdec, 0x0ddcd,
        0x0ad2a, 0x0bd0b, 0x08d68, 0x09d49,
        0x07e97, 0x06eb6, 0x05ed5, 0x04ef4,
        0x03e13, 0x02e32, 0x01e51, 0x00e70,
        0x0ff9f, 0x0efbe, 0x0dfdd, 0x0cffc,
        0x0bf1b, 0x0af3a, 0x09f59, 0x08f78,
        //CRCTB2: ;后字节
        0x09188, 0x081a9, 0x0b1ca, 0x0a1eb,
        0x0d10c, 0x0c12d, 0x0f14e, 0x0e16f,
        0x01080, 0x000a1, 0x030c2, 0x020e3,
        0x05004, 0x04025, 0x07046, 0x06067,
        0x083b9, 0x09398, 0x0a3fb, 0x0b3da,
        0x0c33d, 0x0d31c, 0x0e37f, 0x0f35e,
        0x002b1, 0x01290, 0x022f3, 0x032d2,
        0x04235, 0x05214, 0x06277, 0x07256,
        0x0b5ea, 0x0a5cb, 0x095a8, 0x08589,
        0x0f56e, 0x0e54f, 0x0d52c, 0x0c50d,
        0x034e2, 0x024c3, 0x014a0, 0x00481,
        0x07466, 0x06447, 0x05424, 0x04405,
        0x0a7db, 0x0b7fa, 0x08799, 0x097b8,
        0x0e75f, 0x0f77e, 0x0c71d, 0x0d73c,
        0x026d3, 0x036f2, 0x00691, 0x016b0,
        0x06657, 0x07676, 0x04615, 0x05634,
        0x0d94c, 0x0c96d, 0x0f90e, 0x0e92f,
        0x099c8, 0x089e9, 0x0b98a, 0x0a9ab,
        0x05844, 0x04865, 0x07806, 0x06827,
        0x018c0, 0x008e1, 0x03882, 0x028a3,
        0x0cb7d, 0x0db5c, 0x0eb3f, 0x0fb1e,
        0x08bf9, 0x09bd8, 0x0abbb, 0x0bb9a,
        0x04a75, 0x05a54, 0x06a37, 0x07a16,
        0x00af1, 0x01ad0, 0x02ab3, 0x03a92,
        0x0fd2e, 0x0ed0f, 0x0dd6c, 0x0cd4d,
        0x0bdaa, 0x0ad8b, 0x09de8, 0x08dc9,
        0x07c26, 0x06c07, 0x05c64, 0x04c45,
        0x03ca2, 0x02c83, 0x01ce0, 0x00cc1,
        0x0ef1f, 0x0ff3e, 0x0cf5d, 0x0df7c,
        0x0af9b, 0x0bfba, 0x08fd9, 0x09ff8,
        0x06e17, 0x07e36, 0x04e55, 0x05e74,
        0x02e93, 0x03eb2, 0x00ed1, 0x01ef0,
    };
}