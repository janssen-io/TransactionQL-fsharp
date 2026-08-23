using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;
using System;
using System.Reactive.Linq;
using TransactionQL.DesktopApp.Application;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public class CustomSuspensionHelper
{
    private readonly Signal<RxVoid> _launching;

    public CustomSuspensionHelper(Signal<IDisposable> shouldPersistState, IApplicationLifetime lifetime)
    {
        // Create observables for SuspensionHost to save/restore state.
        _launching = new Signal<RxVoid>();

        if (lifetime is IClassicDesktopStyleApplicationLifetime && !Design.IsDesignMode)
        {
            RxSuspension.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
            RxSuspension.SuspensionHost.ShouldPersistState = shouldPersistState;
            RxSuspension.SuspensionHost.IsLaunchingNew = _launching;
            RxSuspension.SuspensionHost.IsResuming = Observable.Never<RxVoid>();

            // SetupDefaultSuspendResume must be called after setting observables!
            // This method subscribes to them.
            _ = RxSuspension.SuspensionHost.SetupDefaultSuspendResume(new JsonSuspensionDriver("appstate.json"));
        }
    }

    public void OnFrameworkInitializationCompleted()
    {
        _launching.OnNext(RxVoid.Default);
    }
}