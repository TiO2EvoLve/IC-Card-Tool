using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using D8_Demo.Tool;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace D8_Demo.ViewModels;

public partial class LoadM1KeyViewModel : ReactiveObject
{
    private CardHelper _cardHelper;
    private readonly ContentViewModel CVM;
    public ObservableCollection<KeyItem> KeyList { get; set; } = [];

    public LoadM1KeyViewModel(CardHelper cardHelper, ContentViewModel contentViewModel)
    {
        _cardHelper = cardHelper;
        CVM = contentViewModel;
        //初始化数据
        for (int i = 0; i < 16; i++)
        {
            KeyList.Add(new KeyItem("FFFFFFFFFFFF"));
        }
    }

    [ReactiveCommand]
    public void LoadingKey()
    {
        for (int i = 0; i < 16; i++)
        {
            if (_cardHelper.LoadKey(i, KeyList[i].Value))
            {
                CVM.AddLog($"扇区密钥{i}装载成功");
            }
            else
            {
                CVM.AddLog($"扇区密钥{i}装载失败");
            }
        }
    }
}

public partial class KeyItem (string value) : ReactiveObject
{
    [Reactive]
    public partial string Value { get; set; }= value;
    
}