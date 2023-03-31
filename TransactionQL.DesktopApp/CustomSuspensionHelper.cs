using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using TransactionQL.DesktopApp.Application;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public class CustomSuspensionHelper
{
    private Subject<Unit> _launching;

    public CustomSuspensionHelper(Subject<IDisposable> shouldPersistState, IApplicationLifetime lifetime)
    {
        // Create observables for SuspensionHost to save/restore state.
        _launching = new Subject<Unit>();

        if (lifetime is IClassicDesktopStyleApplicationLifetime && !Design.IsDesignMode)
        {
            RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
            RxApp.SuspensionHost.ShouldPersistState = shouldPersistState;
            RxApp.SuspensionHost.IsLaunchingNew = _launching;
            RxApp.SuspensionHost.IsResuming = Observable.Never<Unit>();

            // SetupDefaultSuspendResume must be called after setting observables!
            // This method subscribes to them.
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new JsonSuspensionDriver("appstate.json"));
        }
    }

    public void OnFrameworkInitializationCompleted()
    {
        _launching.OnNext(Unit.Default);
    }
}