using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using D8_Demo.ViewModels;
using DryIoc;

namespace D8_Demo;

public class ViewLocator : IDataTemplate
{
    private readonly IResolver _resolver;

    public ViewLocator(IResolver resolver)
    {
        _resolver = resolver;
    }

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)_resolver.Resolve(type);
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}