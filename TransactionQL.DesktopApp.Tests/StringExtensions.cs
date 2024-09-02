using System.Text;

namespace TransactionQL.DesktopApp.Tests;

internal static class StringExtensions
{
    public static Stream ToStream(this string s)
        => new MemoryStream(Encoding.Default.GetBytes(s));
}
