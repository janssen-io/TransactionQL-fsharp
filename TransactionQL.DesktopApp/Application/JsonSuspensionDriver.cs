using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Application;

public class JsonSuspensionDriver : ISuspensionDriver
{
    private readonly string _file;

    private readonly JsonSerializerSettings _settings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public JsonSuspensionDriver(string file)
    {
        _file = file;
    }

    public IObservable<Unit> InvalidateState()
    {
        if (File.Exists(_file))
        {
            File.Delete(_file);
        }

        return Observable.Return(Unit.Default);
    }

    public IObservable<object> LoadState()
    {
        if (!File.Exists(_file))
        {
            return Observable.Throw<object>(new Exception("Invalid File"));
        }

        string lines = File.ReadAllText(_file);
        object? state = JsonConvert.DeserializeObject<object>(lines, _settings);
        return Observable.Return(state ?? new MainWindowViewModel());
    }

    public IObservable<Unit> SaveState(object state)
    {
        string lines = JsonConvert.SerializeObject(state, _settings);
        File.WriteAllText(_file, lines);
        return Observable.Return(Unit.Default);
    }
}