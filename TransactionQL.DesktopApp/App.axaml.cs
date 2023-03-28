using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TransactionQL.DesktopApp.Application;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp;

public partial class App : Avalonia.Application
{
    private readonly Subject<IDisposable> _shouldPersistState = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Create the AutoSuspendHelper.
        var suspension = new AutoSuspendHelper(ApplicationLifetime);
        RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new JsonSuspensionDriver("appstate.json"));
        if (ApplicationLifetime is IControlledApplicationLifetime controlled)
            controlled.Exit += OnControlledOnExit;

        RxApp.SuspensionHost.ShouldPersistState = _shouldPersistState;
        suspension.OnFrameworkInitializationCompleted();

        // Load the saved view model state.
        var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow(state);
        else
            new MainWindow(state).Show();

        // TODO: is this the best way to start a long running, light-weight background task?
        StartPeriodicPersistence();

        base.OnFrameworkInitializationCompleted();
    }

    private async void StartPeriodicPersistence()
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync())
        {
            var manual = new ManualResetEvent(false);
            _shouldPersistState.OnNext(Disposable.Create(() => manual.Set()));
            manual.WaitOne();
        }
    }

    private void OnControlledOnExit(object o,
        ControlledApplicationLifetimeExitEventArgs controlledApplicationLifetimeExitEventArgs)
    {
        var manual = new ManualResetEvent(false);
        _shouldPersistState.OnNext(Disposable.Create(() => manual.Set()));
        manual.WaitOne();
    }
}