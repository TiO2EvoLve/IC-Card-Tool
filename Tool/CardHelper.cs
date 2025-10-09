using D8_Demo.ViewModels;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace D8_Demo.Tool;

public class CardHelper
{
    [DllImport("dcrf32.dll")] private static extern int dc_init(short port, int baud);//打开端口
    [DllImport("dcrf32.dll")]
    private static extern short dc_pro_commandlink_hex(int icdev, byte slen, ref byte sendbuffer, ref byte rlen,
        ref byte databuffer, byte timeout); //发指令

    [DllImport("dcrf32.dll")]
    private static extern short dc_reset(int icdev, uint sec); //复位

    [DllImport("dcrf32.dll")]
    private static extern short dc_card(int icdev, char _Mode, ref ulong Snr);

    [DllImport("dcrf32.dll")]
    private static extern short dc_pro_resethex(int icdev, ref byte rlen, ref byte rbuff); //复位

    [DllImport("dcrf32.dll")]
    private static extern short dc_request(int icdev, char mode, ref ushort TagType); //特征值

    [DllImport("dcrf32.dll")]
    private static extern short dc_select(int icdev, Int32 Snr, [Out] byte[] Size); //选卡

    [DllImport("dcrf32.dll")]
    private static extern short dc_anticoll(int icdev, uint Mode, ref ulong Snr); //获取芯片号
    [DllImport("dcrf32.dll")]
    public static extern int dc_load_key(int icdev, int mode, int secnr, [In] byte[] nkey);  //密码装载到读写模块中
    [DllImport("dcrf32.dll")]
    private static extern short dc_authentication_passaddr(int icdev, byte _Mode,byte _Addr, [In]byte[] passbuff);//M1密码验证
    [DllImport("dcrf32.dll")]
    public static extern short dc_read(int icdev, byte adr, [Out] byte[] sdata);  //M1读扇区
    [DllImport("dcrf32.dll")]
    private static extern int dc_read_hex(int icdev, int adr, [Out] byte[] sdata);//M1读扇区
    [DllImport("dcrf32.dll")]
    private static extern int dc_write(int icdev, int _Adr, byte[] _Data);//M1写扇区

    private readonly ContentViewModel CVM = ContentViewModel.Instance;

    int icdev => CVM.icdev;

    //打开端口
    public bool OpenPort(int port, int hz)
    {
        int st = dc_init(Convert.ToInt16(port), hz);
        Console.WriteLine("ST:"+st);
        if(st < 0) return false;
        Console.WriteLine($"成功打开端口{icdev}");
        CVM.icdev = st;
        return true;
    }
    //向CPU卡发指令
    public string APDU(string sendbuffer)
    {
        byte[] sbuff = Encoding.ASCII.GetBytes(sendbuffer);
        byte len = (byte)(sbuff.Count() / 2);
        byte rlen = 1;
        byte[] rbuff = new byte[64];
        int st = dc_pro_commandlink_hex(icdev, len, ref sbuff[0], ref rlen, ref rbuff[0], 7);
        if (st != 0)
        {
            return null;
        }

        string strrbuff = null;
        for (int w = 0; w < Convert.ToInt16(rlen) * 2; w++)
        {
            strrbuff += (char)rbuff[w];
        }

        return strrbuff;
    }

    //复位
    public bool Reset()
    {
        if (dc_reset(icdev, 2) != 0)
        {
            return false;
        }

        return true;
    }

    //复位
    public string ResetHex()
    {
        byte crlen = 1;
        var recbuff = new byte[100];
        if (dc_pro_resethex(icdev, ref crlen, ref recbuff[0]) != 0)
        {
            return "";
        }

        string num = "";
        foreach (var t in recbuff)
        {
            num += (char)t;
        }

        return num.Replace("\0", "");
    }

    //寻卡
    public bool FindCard()
    {
        ulong CardUid = 0;
        return dc_card(icdev, '0', ref CardUid) == 0;
    }

    //获取特征值
    public string Request()
    {
        ushort TagType = 0;
        Reset();
        var st = dc_request(icdev, '1', ref TagType);
        if (st != 0) return "";
        return TagType.ToString();
    }

    //选卡
    public string Select(string uid)
    {
        Int32 i = Int32.Parse(uid, System.Globalization.NumberStyles.HexNumber);
        byte[] sdata = new byte[1];
        int st = dc_select(icdev, i, sdata);
        if (st != 0)
        {
            return "";
        }

        StringBuilder sber = new StringBuilder();
        foreach (byte b in sdata)
        {
            sber.Append(b > 15 ? Convert.ToString(b, 16) : '0' + Convert.ToString(b, 16));
        }

        string data_r = sber.ToString();
        return (data_r);
    }

    //获取芯片号
    public string Anticoll()
    {
        ulong Snr = 0;
        int st = dc_anticoll(icdev, 0x00, ref Snr);
        if (st != 0)
        {
            return "";
        }
        return Snr.ToString("X").PadLeft(8, '0');
    }
    //加载密码
    public bool LodeKey(int secnr, string strKey, int KeyStyle)
    {
        byte[] hexkey = new byte[6];
        if (dc_load_key(icdev, KeyStyle, secnr, hexkey) != 0)
        {
            return false;
        }
        return true;
    }
    //M1密码验证
    public bool AuthenticationPass(byte _Mode, byte _Addr, byte[] passbuff)
    {
        if (dc_authentication_passaddr(icdev, _Mode, _Addr, passbuff) == 0)
        {
            return true;
        }
        return false;
    }
    //M1读扇区
    public string M1ReadSector(byte address)
    {
        byte[] sdata = new byte[33];
        int st = dc_read(icdev, address, sdata);
        if (st != 0)
        {
            return "";
        }
        // 将 byte[] 转为十六进制字符串返回
        StringBuilder sb = new StringBuilder();
        foreach (byte b in sdata)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }
    //加载密钥
    public bool LoadKey(int secnr,string strKey,int KeyStyle)
    {
        byte[] hexkey = new byte[6];
        hexkey= Enumerable.Range(0, strKey.Length / 2)
            .Select(i => Convert.ToByte(strKey.Substring(i * 2, 2), 16))
            .ToArray();
        int st = dc_load_key(icdev, KeyStyle, secnr, hexkey);
        if (st != 0)
        {
            return false;
        }

        return true;
    }
}