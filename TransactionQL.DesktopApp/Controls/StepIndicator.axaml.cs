using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
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
        get { return GetValue(InactiveBackgroundProperty); }
        set { SetValue(InactiveBackgroundProperty, value); }
    }

    public static readonly StyledProperty<double> StepWidthProperty =
        AvaloniaProperty.Register<StepIndicator, double>(nameof(StepWidth), 40);

    public double StepWidth
    {
        get { return GetValue(StepWidthProperty); }
        set { SetValue(StepWidthProperty, value); }
    }

    public static readonly StyledProperty<IBrush> ActiveBackgroundProperty =
        AvaloniaProperty.Register<StepIndicator, IBrush>(nameof(ActiveBackground));

    public IBrush ActiveBackground
    {
        get { return GetValue(ActiveBackgroundProperty); }
        set { SetValue(ActiveBackgroundProperty, value); }
    }

    public static readonly StyledProperty<uint> NumberOfStepsProperty =
        AvaloniaProperty.Register<StepIndicator, uint>(nameof(NumberOfSteps), 3);

    public uint NumberOfSteps
    {
        get { return GetValue(NumberOfStepsProperty); }
        set { SetValue(NumberOfStepsProperty, value); }
    }

    public static readonly StyledProperty<ObservableCollection<Step>> ItemsProperty =
        AvaloniaProperty.Register<StepIndicator, ObservableCollection<Step>>(nameof(Items), [new Step(1)]);

    public ObservableCollection<Step> Items
    {
        get { return GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }

    public static readonly StyledProperty<uint> CurrentStepProperty =
        AvaloniaProperty.Register<StepIndicator, uint>(nameof(CurrentStep), 0);

    public uint CurrentStep
    {
        get { return GetValue(CurrentStepProperty); }
        set {
            if (value > Items.Count)
                return;

            SetValue(CurrentStepProperty, value);
            UpdateCompleted();
        }
    }

    Control? _root;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = this.GetTemplateChildren().FirstOrDefault() as Control;

        Items.Clear();
        for (uint i = 1; i <= NumberOfSteps; i++)
        {
            Items.Add(new Step(i));
        }

        UpdateCompleted();
    }

    private void UpdateCompleted()
    {
        for(int i = 0; i < Items.Count; i++)
        {
            Items[i].IsCompleted = i < CurrentStep;
        }

        if (CurrentStep > Items.Count) CurrentStep = (uint)Items.Count;
    }
}