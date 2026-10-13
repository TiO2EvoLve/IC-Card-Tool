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
            return;
        }
        var hex = CardHelper.ResetHex();
        string strRandom = CardHelper.APDU("0084000008");
        if (strRandom.Length !=20) throw new Exception("外部认证未通过！随机数错误：" + strRandom);
        strRandom = strRandom[..16];
        var data = Tools.DesEncrypt(Tools.HexStringToBytes(strRandom), Tools.HexStringToBytes("FFFFFFFFFFFFFFFF"));
        string strCommand = "0082000008" + Convert.ToHexString(data);
        Console.WriteLine(strCommand);
        string st= CardHelper.APDU(strCommand);
        Console.WriteLine(st);
    }
}