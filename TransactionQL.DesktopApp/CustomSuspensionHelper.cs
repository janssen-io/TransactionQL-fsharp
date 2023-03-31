using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using TransactionQL.DesktopApp.Application;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public class CustomSuspensionHelper
{
    private Subject<Unit> _launching;

    public CustomSuspensionHelper(Subject<IDisposable> shouldPersistState)
    {
        // Create observables for SuspensionHost to save/restore state.
        _launching = new Subject<Unit>();

        RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
        RxApp.SuspensionHost.ShouldPersistState = shouldPersistState;
        RxApp.SuspensionHost.IsLaunchingNew = _launching;
        RxApp.SuspensionHost.IsResuming = Observable.Never<Unit>();

        // SetupDefaultSuspendResume must be called after setting observables!
        // This method subscribes to them.
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new JsonSuspensionDriver("appstate.json"));
    }

    public void OnFrameworkInitializationCompleted()
    {
        _launching.OnNext(Unit.Default);
    }
}