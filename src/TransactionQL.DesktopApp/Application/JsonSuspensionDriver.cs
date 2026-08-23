using ReactiveUI;
using ReactiveUI.Primitives;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Application;

public class JsonSuspensionDriver : ISuspensionDriver
{
    private readonly string _file;

    private readonly JsonSerializerOptions _settings = new();

    public JsonSuspensionDriver(string file)
    {
        _file = file;
    }

    public IObservable<object?> LoadState() => LoadState(JsonTypeInfo.CreateJsonTypeInfo<MainWindowViewModel>(_settings));

    public IObservable<RxVoid> InvalidateState()
    {
        if (File.Exists(_file))
        {
            File.Delete(_file);
        }

        return Observable.Return(RxVoid.Default);
    }

    public IObservable<RxVoid> SaveState<T>(T state) => SaveAnyState(state);

    public IObservable<RxVoid> SaveState<T>(T state, JsonTypeInfo<T> typeInfo) => SaveAnyState(state);

    public IObservable<T?> LoadState<T>(JsonTypeInfo<T> typeInfo)
    {
        if (!File.Exists(_file))
        {
            return Observable.Throw<T>(new Exception("Invalid File"));
        }

        using var stateStream = new FileStream(_file, FileMode.Open, FileAccess.Read, FileShare.Read);
        T? state = default;
        try
        {
            state = JsonSerializer.Deserialize<T>(stateStream, typeInfo);
        }
        catch { } // TODO log? Show warning?
        return Observable.Return(state ?? default);
    }

    private IObservable<RxVoid> SaveAnyState<T>(T? state)
    {
        if (state != null)
        {
            string lines = JsonSerializer.Serialize(state, _settings);
            File.WriteAllText(_file, lines);
        }

        return Observable.Return(RxVoid.Default);
    }
}