using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;

namespace D8_Demo.ViewModels;

public partial class CardCheckViewModel : ViewModelBase
{
    [ObservableProperty] private string? returnValue;
    [ObservableProperty] private string? cardType = "未检测到卡片";
    [ObservableProperty] private string? aTS = "";
    
    private readonly CardHelper CardHelper = new ();
    
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
    [RelayCommand]
    private async Task CheckChipType()
    {
        await Task.Run(Check);
    }

    async Task Check()
    {
        while (true)
        {
            // if (!CardHelper.Reset())
            // {
            //     Console.WriteLine("复位失败");
            // }
            
            if (!CardHelper.FindCard())
            {
                Console.WriteLine("寻卡失败");
                Clear();
            }
            if (!CardHelper.ResetHex())
            {
                Console.WriteLine("hex复位失败");
                await Task.Delay(1000);
                continue;
            }
           
            var response = CardHelper.APDU("80CA00F10A");
            if (response == null)
            {
                ReturnValue = "M1卡或UL卡";
                return;
            }

            Console.WriteLine(response);
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
            Console.WriteLine(CardHelper.Request()); 
            await Task.Delay(2000);
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
            else
            {
                CardType = "未知型号芯片";
            }
        }
    }
    void FM1208()
    {
        
    }

    void Clear()
    {
        ReturnValue = "";
        CardType = "未检测到卡片";
    }
}