using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Linq;

namespace TransactionQL.DesktopApp.Controls;

public class Dropzone : TemplatedControl
{
    public static readonly StyledProperty<string?> FileTypesProperty =
        AvaloniaProperty.Register<Dropzone, string?>(nameof(FileTypes));

    public string? FileTypes
    {
        get => GetValue(FileTypesProperty);
        set => SetValue(FileTypesProperty, value);
    }

    public static readonly StyledProperty<string> FileDescriptionProperty =
        AvaloniaProperty.Register<Dropzone, string>(nameof(FileDescription));

    public string FileDescription
    {
        get => GetValue(FileDescriptionProperty);
        set => SetValue(FileDescriptionProperty, value);
    }

    public static readonly StyledProperty<string> FileNameProperty =
        AvaloniaProperty.Register<Dropzone, string>(nameof(FileName));

    public string FileName
    {
        get => GetValue(FileNameProperty);
        set
        {
            _ = SetValue(FileNameProperty, value);
            Projektanker.Icons.Avalonia.Icon? icon = VisualChildren
                .FirstOrDefault()
                .FindLogicalDescendantOfType<Projektanker.Icons.Avalonia.Icon>();

            if (icon != null)
            {
                icon.Value = "fa-solid fa-file-circle-check";
            }
        }
    }

    public static readonly StyledProperty<IBrush> BrowseForegroundProperty =
        AvaloniaProperty.Register<Dropzone, IBrush>(nameof(BrowseForeground));

    public IBrush BrowseForeground
    {
        get => GetValue(BrowseForegroundProperty);
        set => SetValue(BrowseForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> HighlightProperty =
        AvaloniaProperty.Register<Dropzone, IBrush>(nameof(Highlight));

    public IBrush Highlight
    {
        get => GetValue(HighlightProperty);
        set => SetValue(HighlightProperty, value);
    }

    public static readonly StyledProperty<IBrush> IconForegroundProperty =
        AvaloniaProperty.Register<Dropzone, IBrush>(nameof(IconForeground));

    public IBrush IconForeground
    {
        get => GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<Dropzone, BoxShadows>(nameof(BoxShadow));

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    private Border? _area;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        AddHandler(DragDrop.DropEvent, OnFileDrop);
        AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);

        _area = this.GetTemplateChildren().First() as Border;
        if (_area != null)
        {
            InitializeControls(_area);
        }
    }

    private void InitializeControls(Border root)
    {
        Button? browse = root.FindLogicalDescendantOfType<Button>();
        Projektanker.Icons.Avalonia.Icon? icon = root.FindLogicalDescendantOfType<Projektanker.Icons.Avalonia.Icon>();

        if (browse != null)
        {
            browse.Click += SelectFile;
        }

        if (!string.IsNullOrEmpty(FileName) && icon != null)
        {
            icon.Value = "fa-solid fa-file-circle-check";
        }
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (sender == this && _area != null)
        {
            _area.BorderBrush = Highlight;
        }
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (sender == this && _area != null)
        {
            _area.BorderBrush = new SolidColorBrush(new Color(0, 0, 0, 0));
        }
    }

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        if (sender != this)
        {
            return;
        }

        if (_area != null)
        {
            _area.BorderBrush = new SolidColorBrush(new Color(0, 0, 0, 0));
        }

        // We only care about files
        System.Collections.Generic.IEnumerable<IStorageItem>? files = e.Data.GetFiles();
        if (files == null)
        {
            return;
        }

        IStorageItem file = files.First();
        FileName = file.Path.AbsolutePath;
    }

    private async void SelectFile(object? sender, RoutedEventArgs routedEventArgs)
    {
        System.Collections.Generic.IEnumerable<FilePickerFileType> fileTypes = (FileTypes ?? string.Empty)
            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(extension => new FilePickerFileType(extension) { Patterns = [extension] });


        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = $"Select {FileDescription}",
            FileTypeFilter = fileTypes.Append(FilePickerFileTypes.All).ToList(),
        };


        TopLevel? top = TopLevel.GetTopLevel(sender as Visual);
        if (top == null)
        {
            return;
        }

        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await top.StorageProvider.OpenFilePickerAsync(options);
        if (files.Count > 0)
        {
            Dispatcher.UIThread.Post(() => FileName = files[0].Path.AbsolutePath);
        }
    }
}