using System.IO;

namespace TransactionQL.DesktopApp.Services;

/// <summary>
/// Interface to make conversion of file paths to streams testable.
/// </summary>
public interface IStreamFiles
{
    Stream Open(string path);
}

public class FilesystemStreamer : IStreamFiles
{
    public static readonly FilesystemStreamer Instance = new();
    public Stream Open(string path) => new FileStream(path, FileMode.Open);
}