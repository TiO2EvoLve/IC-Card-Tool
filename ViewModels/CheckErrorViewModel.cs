using System;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;


namespace D8_Demo.ViewModels;

public partial class CheckErrorViewModel
{
    private readonly CardHelper CardHelper = CardHelper.Instance;//卡片工具类
    
    //单例模式
    public static CheckErrorViewModel Instance { get; } = new();
    [RelayCommand]
    private void CheckError()
    {
        
        if (!CardHelper.Reset() || CardHelper.FindCard() == 0)
        {
            Console.WriteLine("复位失败或未检测到卡片");
            return;
        }

        _ = CardHelper.ResetHex();

        string strRandom = CardHelper.APDU("0084000008");

        Console.WriteLine($"随机数原始返回值：{strRandom ?? "<null>"}");

        if (string.IsNullOrWhiteSpace(strRandom))
        {
            return;
        }

        if (strRandom.Length < 16)
        {
            throw new Exception($"随机数长度不足：{strRandom}，长度={strRandom.Length}");
        }

        strRandom = strRandom[..16];

        byte[] randomBytes = Tools.HexStringToBytes(strRandom);
        byte[] keyBytes = Tools.HexStringToBytes("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

        byte[] data = Tools.EncryptEcb(randomBytes, keyBytes);

        string strCommand = "0082000008" + BitConverter.ToString(data).Replace("-", "");

        Console.WriteLine($"外部认证指令：{strCommand}");

        string st = CardHelper.APDU(strCommand);

        Console.WriteLine($"认证返回值：{st ?? "<null>"}");

        
    }
    
}