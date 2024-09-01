using Avalonia.Headless;
using Avalonia;
using TransactionQL.DesktopApp.Tests;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;
using Avalonia.Controls;
using System.Diagnostics;

[assembly: AvaloniaTestApplication(typeof(TestApp))]

public static class TestApp
{
    public static readonly AppBuilder Instance = BuildAvaloniaApp();

    public static AppBuilder BuildAvaloniaApp()
    {
        _ = IconProvider.Current
                .Register<FontAwesomeIconProvider>()
                .Register<MaterialDesignIconProvider>();

        return AppBuilder
            .Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false
            });
    }

    public static void CaptureFrame(Window w, string? fileName = null, bool shouldShow = true)
    {
        if (fileName == null)
        {
            fileName = Path.Join(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        }

        var frame = w.CaptureRenderedFrame();
        frame!.Save(fileName);

        if (shouldShow)
        {
            var absolutePath = Path.GetFullPath(fileName);
            Process.Start("explorer.exe", absolutePath);
        }
    }
}
