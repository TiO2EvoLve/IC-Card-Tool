using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;

namespace D8_Demo.ViewModels;

public partial class CardCheckViewModel : ViewModelBase
{
    [ObservableProperty] private string? returnValue;
    [ObservableProperty] private string? cardType = "未检测到卡片";
    [ObservableProperty] private string? aTS = "";
    [ObservableProperty] private string? feature1 = "";
    [ObservableProperty] private string? feature2 = "";
    
    private readonly CardHelper CardHelper = new ();
    private readonly ContentViewModel CVM = ContentViewModel.Instance;
    //读卡间隔
    public int ReadTime;
    //是否可以运行
    public bool CanRun = true;
    private readonly Dictionary<string,string> ChipType = new ()
    {
        {"331A1","FM1280-331-A1"},
        {"334A1","FM1280-334-A1"},
        {"169A3","FM1280-169-A3"},
        {"331E1","FM1280-331-E1"},
        {"169C1","FM1280-169-C1"},
        {"2164","FM1280-2164"},
        {"2165","FM1280-2165"},
        {"165C1","FM1280-165-C1"},
        {"102A1","FM1280-102-A1"},
        
    };
    //TODO :发布时改为私有
    public CardCheckViewModel()
    {
        
    }
    public static CardCheckViewModel Instance { get; } = new ();
    [RelayCommand]
    private async Task CheckChipType()
    {
        if (!CVM.isConnected)
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", "请先打开端口").ShowAsync();
            return;
        }
        await Task.Run(Check);
    }

    async Task Check()
    {
        while (true)
        {
            if(!CanRun) {
                Clear();
                break; // 如果不可以运行，跳出循环
            }
            if (!CardHelper.Reset())
            {
                Console.WriteLine("复位失败");
            }
            
            if (!CardHelper.FindCard())
            {
                Console.WriteLine("寻卡失败");
                Clear();
                continue;
            }
            //复位ATS
            var ats = CardHelper.ResetHex();
            if (ats == "")
            {
                //为M1卡或UL卡
                continue;
            }
            ATS = ats;
            _ = CardHelper.APDU("00A404000C54656D706F726172792E4D46");
            var response = CardHelper.APDU("80CA00F10A");
            if (response == null)
            {
                ReturnValue = "M1卡或UL卡";
                continue;
            }
            if (response.EndsWith("9000"))
            {
                ReturnValue = response;
                //芯片为1280
                FM1280(response);
            }else
            {
                //芯片为1208
                ReturnValue = $"{Toml.GetToml(response[^4..],"desc")}({response[^4..]})" ;
                FM1208();
            }
            //获取特征值1
            Feature1 = CardHelper.Request();
            //获取特征值2
            var uid =  CardHelper.Anticoll();
            if(uid != "") Feature2 = CardHelper.Select(uid);
            await Task.Delay(ReadTime);
        }
    }

    void FM1280(string response)
    {
        foreach (var type in ChipType)
        {
            if (response.Contains(type.Key))
            {
                CardType = "芯片型号为:" + type.Value;
                break;
            }
            CardType = "未知型号芯片";
        }
    }
    void FM1208()
    {
        
        CardType ="芯片型号为:" + "FM1208";
    }
    void Clear()
    {
        ReturnValue = "";
        ATS = "";
        CardType = "未检测到卡片";
    }
}