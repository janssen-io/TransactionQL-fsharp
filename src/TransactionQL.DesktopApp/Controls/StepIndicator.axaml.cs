using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.ObjectModel;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Controls;

public class Step : ViewModelBase
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public bool IsLeader { get; }

    private bool _isCompleted = false;

    public bool IsCompleted
    {
        get => _isCompleted;
        set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
    }

    public Step(uint name, bool isCompleted = false)
    {
        Name = name.ToString();
        IsCompleted = isCompleted;
        IsLeader = name == 1;
    }
}
public class StepIndicator : TemplatedControl
{
    public static readonly StyledProperty<IBrush> InactiveBackgroundProperty =
        AvaloniaProperty.Register<StepIndicator, IBrush>(nameof(InactiveBackground));

    public IBrush InactiveBackground
    {
        get => GetValue(InactiveBackgroundProperty);
        set => SetValue(InactiveBackgroundProperty, value);
    }

    public static readonly StyledProperty<double> StepWidthProperty =
        AvaloniaProperty.Register<StepIndicator, double>(nameof(StepWidth), 40);

    public double StepWidth
    {
        get => GetValue(StepWidthProperty);
        set => SetValue(StepWidthProperty, value);
    }

    public static readonly StyledProperty<IBrush> ActiveBackgroundProperty =
        AvaloniaProperty.Register<StepIndicator, IBrush>(nameof(ActiveBackground));

    public IBrush ActiveBackground
    {
        get => GetValue(ActiveBackgroundProperty);
        set => SetValue(ActiveBackgroundProperty, value);
    }

    public static readonly StyledProperty<uint> NumberOfStepsProperty =
        AvaloniaProperty.Register<StepIndicator, uint>(nameof(NumberOfSteps), 3);

    public uint NumberOfSteps
    {
        get => GetValue(NumberOfStepsProperty);
        set => SetValue(NumberOfStepsProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<Step>> ItemsProperty =
        AvaloniaProperty.Register<StepIndicator, ObservableCollection<Step>>(nameof(Items), []);

    public ObservableCollection<Step> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly StyledProperty<uint> CurrentStepProperty =
        AvaloniaProperty.Register<StepIndicator, uint>(nameof(CurrentStep), 0);

    public uint CurrentStep
    {
        get => GetValue(CurrentStepProperty);
        set
        {
            if (value > Items.Count)
            {
                return;
            }

            _ = SetValue(CurrentStepProperty, value);
            UpdateCompleted();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Items = [];
        for (uint i = 1; i <= NumberOfSteps; i++)
        {
            Items.Add(new Step(i));
        }

        UpdateCompleted();
    }

    private void UpdateCompleted()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].IsCompleted = i < CurrentStep;
        }

        if (CurrentStep > Items.Count)
        {
            CurrentStep = (uint)Items.Count;
        }
    }
}