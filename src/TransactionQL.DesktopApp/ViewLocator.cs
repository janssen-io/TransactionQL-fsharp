using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        string? name = data?.GetType().FullName!.Replace("ViewModel", "View");
        Type? type = Type.GetType(name ?? string.Empty);

        return type != null ? (Control)Activator.CreateInstance(type)! : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}