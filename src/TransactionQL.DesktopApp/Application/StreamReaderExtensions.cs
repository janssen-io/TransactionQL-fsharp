using System.Collections.Generic;
using System.IO;

namespace TransactionQL.DesktopApp.Application;

internal static class StreamReaderExtensions
{
    public static IEnumerable<string?> StreamLines(this StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            yield return reader.ReadLine();
        }
    }
}