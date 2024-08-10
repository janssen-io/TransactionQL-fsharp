using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public partial class DataWizardWindow : Window
{
    public DataWizardWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.KeyDown += HandleKeyDown;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ((SelectDataWindowViewModel)DataContext!).DataSelected += (_, _) => Close();
        ((SelectDataWindowViewModel)DataContext).SelectionCancelled += (sender, data) => Close();
    }

    public void NextPage(object? sender, RoutedEventArgs e)
    {
        Pages.Next();
        Progress.CurrentStep++;
    }

    public void PreviousPage(object? sender, RoutedEventArgs e)
    {
        Pages.Previous();
        Progress.CurrentStep--;
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;

    private void Dragbar_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;

        PointerPoint currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void Dragbar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void Dragbar_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }
}