using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using TransactionQL.DesktopApp.Application;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp;

public partial class App : Avalonia.Application, IDisposable
{
    private readonly Subject<IDisposable> _shouldPersistState = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Use a custom AutoSuspendHelper to also be able to save app state on-demand.
        var suspensionHelper = new CustomSuspensionHelper(_shouldPersistState, ApplicationLifetime);

        // Load the state and start the app
        var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(state);
            state.StateSaved += OnStateSaved;
            desktop.Exit += OnControlledOnExit;
        }
        else if (!Design.IsDesignMode)
        {
            throw new Exception("App must be started with Desktop ApplicationLifetime");
        }

        suspensionHelper.OnFrameworkInitializationCompleted();
        base.OnFrameworkInitializationCompleted();
    }

    private void OnControlledOnExit(object? o,
        ControlledApplicationLifetimeExitEventArgs controlledApplicationLifetimeExitEventArgs)
    {
        // Only save app state if application exited for the right reasons
        // TODO: verify that uncaught exceptions/app crashes do indeed set a different ApplicationExitCode
        if (controlledApplicationLifetimeExitEventArgs.ApplicationExitCode == 0)
            SaveState();
    }

    private void OnStateSaved(object? sender, EventArgs args)
    {
        SaveState();
    }

    private void SaveState()
    {
        var manual = new ManualResetEvent(false);
        _shouldPersistState.OnNext(Disposable.Create(() => manual.Set()));
        if (!manual.WaitOne(TimeSpan.FromSeconds(10))) throw new Exception("Could not save state");
    }

    public void Dispose()
    {
        _shouldPersistState.Dispose();
    }
}