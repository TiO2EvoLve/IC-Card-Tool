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
        var ats = CardHelper.ResetHex();
        
        string strRandom = CardHelper.APDU("0084000008");
        if (strRandom.Length < 16) throw new Exception("外部认证未通过！随机数错误：" + strRandom);
        strRandom = strRandom[..16];
        string EnRandom = MathHelper.DES_Encrypt(strRandom, "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        string strCommand = "0082000008" + EnRandom;
        string strResult = CardHelper.APDU(strCommand);
        string st = strResult.Substring(strResult.Length - 4, 4);
        Console.WriteLine(st);
    }
}