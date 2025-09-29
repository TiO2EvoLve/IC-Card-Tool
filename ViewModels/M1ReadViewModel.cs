using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Models;

namespace D8_Demo.ViewModels;

public partial class M1ReadViewModel : ViewModelBase
{
    public static M1ReadViewModel Instance { get; } = new ();
   
    public ObservableCollection<SectorViewModel> Sectors { get; } = new();
    
    public M1ReadViewModel()
    {
        for (int i = 0; i < 16; i++)
        {
            Sectors.Add(new SectorViewModel(i));
        }
    }
}