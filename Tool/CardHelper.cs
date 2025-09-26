using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using D8_Demo.ViewModels;
using MsBox.Avalonia;

namespace D8_Demo.Tool;

public class CardHelper
{
    [DllImport("dcrf32.dll")] private static extern short dc_pro_commandlink_hex(int icdev, byte slen, ref byte sendbuffer ,ref byte rlen,ref byte databuffer,byte timeout);
    [DllImport("dcrf32.dll")] private static extern short dc_reset(int icdev, int Msec);//复位
    [DllImport("dcrf32.dll")] private static extern int dc_card(int icdev, int mode, ref ulong snr);// 寻卡
    [DllImport("dcrf32.dll")] private static extern short dc_pro_resethex(int icdev, ref byte rlen, ref byte rbuff);//复位
    [DllImport("dcrf32.dll")] private static extern short dc_request(int icdev, int mode, ref uint TagType);
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
    public bool ResetHex()
    {
        byte crlen = 1;
        var recbuff = new byte[100];
        if (dc_pro_resethex(icdev, ref crlen, ref recbuff[0]) != 0)
        {
            return false;
        }
        return true;
    }
    public bool FindCard()
    {
        ulong CardUid = 0;
        return dc_card(icdev, 0x01, ref CardUid) == 0;
    }
    public string Request()
    {
        uint TagType = 0;
        if (dc_request(icdev, 0x00, ref TagType) != 0) return "";
        return TagType.ToString("X");
    }
}