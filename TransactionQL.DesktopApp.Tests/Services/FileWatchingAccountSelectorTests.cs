using System.Collections.Specialized;
using TransactionQL.DesktopApp.Services;

namespace TransactionQL.DesktopApp.Tests.Services;

public class FileWatchingAccountSelectorTests
{
    [Fact]
    public void ItContains_InitialAccounts()
    {
        // Arrange
        var file = Path.GetFullPath("Services/filewatchingaccounts.txt");
        var watcher = FilewatchingAccountSelector.Monitor(file, DummyDispatch);

        // Assert
        Assert.Collection(watcher.AvailableAccounts,
            a => Assert.Equal("Assets:Checking", a),
            a => Assert.Equal("Assets:Savings", a),
            a => Assert.Equal("Expenses:Living", a),
            a => Assert.Equal("Expenses:Recreation", a));
    }

    [Fact]
    public void ItMonitors_Adds()
    {
        // Arrange
        var file = Path.GetFullPath("Services/filewatchingaccounts.txt");
        var watcher = FilewatchingAccountSelector.Monitor(file, DummyDispatch);
        ManualResetEvent waitForUpdate = new(false);
        watcher.AvailableAccounts.CollectionChanged += (sender, args) =>
        {
            // Ensure we only signal the MRE when the last expected
            // account is added (AddRange adds them one by one)
            if (args.Action == NotifyCollectionChangedAction.Add
                && watcher.AvailableAccounts.Count == 5)
            {
                waitForUpdate.Set();
            }
        };

        // Act
        using StreamWriter write = new(new FileStream(file, FileMode.Append));
        write.WriteLine("");
        write.WriteLine("account Income:Test");
        write.Flush();
        write.Close();

        waitForUpdate.WaitOne(500);

        // Assert
        Assert.Collection(watcher.AvailableAccounts,
            a => Assert.Equal("Assets:Checking", a),
            a => Assert.Equal("Assets:Savings", a),
            a => Assert.Equal("Expenses:Living", a),
            a => Assert.Equal("Expenses:Recreation", a),
            a => Assert.Equal("Income:Test", a));
    }

    private static void DummyDispatch(Action a) => a.Invoke();
}
