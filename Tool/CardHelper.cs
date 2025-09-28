using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using D8_Demo.ViewModels;


namespace D8_Demo.Tool;

public class CardHelper
{
    [DllImport("dcrf32.dll")] private static extern short dc_pro_commandlink_hex(int icdev, byte slen, ref byte sendbuffer ,ref byte rlen,ref byte databuffer,byte timeout);
    [DllImport("dcrf32.dll")] private static extern short dc_reset(int icdev, int Msec);//复位
    [DllImport("dcrf32.dll")] private static extern int dc_card(int icdev, int mode, ref ulong snr);// 寻卡
    [DllImport("dcrf32.dll")] private static extern short dc_pro_resethex(int icdev, ref byte rlen, ref byte rbuff);//复位
    [DllImport("dcrf32.dll")] private static extern short dc_request(int icdev, char mode, ref ushort TagType);
    [DllImport("dcrf32.dll")] private static extern short dc_select(int icdev, Int32 Snr, [Out] byte[] Size);
    [DllImport("dcrf32.dll")] private static extern short dc_anticoll(int icdev,uint Mode,ref ulong Snr);
    
    private readonly ContentViewModel CVM = ContentViewModel.Instance;
    
    int icdev => CVM.icdev; 
    public string APDU(string sendbuffer)
    {
        byte[] sbuff = System.Text.Encoding.ASCII.GetBytes(sendbuffer);
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
    public bool Reset()
    {
        if (dc_reset(icdev, 2) != 0)
        {
            return false;
        }

        return true;
    }
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
    public bool FindCard()
    {
        ulong CardUid = 0;
        return dc_card(icdev, 0x01, ref CardUid) == 0;
    }
    public string Request()
    {
        ushort TagType = 0;
        Reset();
        var st = dc_request(icdev, '1', ref TagType);
        if (st != 0) return "";
        return TagType.ToString();
    }

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
    public string Anticoll()
    {
        ulong Snr = 0;
        int st = dc_anticoll(icdev, 0x00, ref Snr);
        if (st != 0)
        {
            return "";
        }
        return Snr.ToString("X").PadLeft(8,'0');
    }
}