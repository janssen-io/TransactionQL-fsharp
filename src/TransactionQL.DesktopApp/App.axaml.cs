using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using TransactionQL.DesktopApp.Application;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;
using TransactionQL.Shared.Disposables;

namespace TransactionQL.DesktopApp;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Framework/template code")]
public partial class App : Avalonia.Application, IDisposable
{
    private readonly Subject<IDisposable> _shouldPersistState = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

        ServiceCollection collection = new();
        collection.AddTql();

        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Use a custom AutoSuspendHelper to also be able to save app state on-demand.
            CustomSuspensionHelper suspensionHelper = new(_shouldPersistState, ApplicationLifetime);

            // Load the state and start the app
            MainWindowViewModel state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow { DataContext = state };
            state.StateSaved += OnStateSaved;
            desktop.Exit += OnControlledOnExit;

            suspensionHelper.OnFrameworkInitializationCompleted();
        }
        else if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("App must be started with Desktop ApplicationLifetime");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnControlledOnExit(object? o,
        ControlledApplicationLifetimeExitEventArgs controlledApplicationLifetimeExitEventArgs)
    {
        // Only save app state if application exited for the right reasons
        if (controlledApplicationLifetimeExitEventArgs.ApplicationExitCode == 0)
        {
            SaveState();
        }
    }

    private void OnStateSaved(object? sender, EventArgs args)
    {
        SaveState();
    }

    private void SaveState()
    {
        ManualResetEvent manual = new(false);
        _shouldPersistState.OnNext(Disposable.Create(() => manual.Set()));

        if (!manual.WaitOne(TimeSpan.FromSeconds(10)))
        {
            throw new TimeoutException("Could not save state within 10 seconds.");
        }
    }

    public void Dispose()
    {
        _shouldPersistState.Dispose();
    }
}