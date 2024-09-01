using System.Diagnostics.CodeAnalysis;
using System.Text;
using TransactionQL.DesktopApp.Services;
using Xunit.Abstractions;

namespace TransactionQL.DesktopApp.Tests.Services;

[SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "File delete has exists-check as safeguard")]
public class FileWatchingAccountSelectorTests : IDisposable
{
    private string _fileName;
    private readonly ITestOutputHelper _output;

    public FileWatchingAccountSelectorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ItContains_InitialAccounts()
    {
        _fileName = Path.Join(Path.GetTempPath(), nameof(ItContains_InitialAccounts));
        FillAccountsFile();

        _output.WriteLine(_fileName);

        // Arrange
        var watcher = await FilewatchingAccountSelector.Monitor(_fileName, DummyDispatch);

        // Assert
        Assert.Collection(watcher.AvailableAccounts,
            a => Assert.Equal("Assets:Checking", a),
            a => Assert.Equal("Assets:Savings", a),
            a => Assert.Equal("Expenses:Living", a),
            a => Assert.Equal("Expenses:Recreation", a));
    }

    [Fact]
    public async Task ItMonitors_Adds()
    {
        _fileName = Path.Join(Path.GetTempPath(), nameof(ItMonitors_Adds));
        FillAccountsFile();

        _output.WriteLine(_fileName);

        // Arrange
        var watcher = await FilewatchingAccountSelector.Monitor(_fileName, DummyDispatch);
        ManualResetEvent waitForUpdate = new(false);
        watcher.AccountsChanged += (sender, args) =>
        {
            // Ensure we only signal the MRE when the last expected
            // account is added (AddRange adds them one by one)
            waitForUpdate.Set();
        };

        // Act
        using StreamWriter write = new(new FileStream(_fileName, FileMode.Append), Encoding.Default, 4*1024*1024);
        write.WriteLine("");
        write.WriteLine("account Income:Test");
        write.Flush();
        write.Close();

        waitForUpdate.WaitOne(FilewatchingAccountSelector.DebounceMillis + 50);

        // Assert
        Assert.Collection(watcher.AvailableAccounts,
            a => Assert.Equal("Assets:Checking", a),
            a => Assert.Equal("Assets:Savings", a),
            a => Assert.Equal("Expenses:Living", a),
            a => Assert.Equal("Expenses:Recreation", a),
            a => Assert.Equal("Income:Test", a));
    }

    [Fact]
    public async Task ItMonitors_Deletes()
    {
        _fileName = Path.Join(Path.GetTempPath(), nameof(ItMonitors_Deletes));
        FillAccountsFile();

        _output.WriteLine(_fileName);

        // Arrange
        var watcher = await FilewatchingAccountSelector.Monitor(_fileName, DummyDispatch);
        ManualResetEvent waitForUpdate = new(false);
        watcher.AccountsChanged += (sender, args) =>
        {
            waitForUpdate.Set();
        };

        // Act
        string[] contents;
        using (StreamReader read = new(new FileStream(_fileName, FileMode.Open)))
        {
            contents = read.ReadToEnd().Split(Environment.NewLine);
        }

        using StreamWriter write = new(new FileStream(_fileName, FileMode.Truncate, FileAccess.Write), Encoding.Default, 4*1024*1024);
        foreach (var line in contents)
        {
            if (line != "account Expenses:Recreation")
                write.WriteLine(line);
        }
        write.Flush();
        write.Close();

        waitForUpdate.WaitOne(FilewatchingAccountSelector.DebounceMillis + 50);

        // Assert
        Assert.Collection(watcher.AvailableAccounts,
            a => Assert.Equal("Assets:Checking", a),
            a => Assert.Equal("Assets:Savings", a),
            a => Assert.Equal("Expenses:Living", a));
    }

    [Fact]
    public async Task ItMonitors_Replace()
    {
        _fileName = Path.Join(Path.GetTempPath(), nameof(ItMonitors_Replace));
        FillAccountsFile();

        _output.WriteLine(_fileName);

        // Arrange
        var newFile = Path.GetTempFileName();
        using (StreamWriter write = new(new FileStream(newFile, FileMode.Append)))
        {
            write.WriteLine("account Test:Account:One");
            write.WriteLine("account Test:Account:Two");
            write.Flush();
        }

        var watcher = await FilewatchingAccountSelector.Monitor(_fileName, DummyDispatch);

        ManualResetEvent waitForUpdate = new(false);
        watcher.AccountsChanged += (sender, args) =>
        {
            waitForUpdate.Set();
        };

        // Act
        File.Copy(newFile, _fileName, overwrite: true);

        if (File.Exists(newFile))
            File.Delete(newFile);

        waitForUpdate.WaitOne(FilewatchingAccountSelector.DebounceMillis + 50);

        // Assert
        Assert.Collection(watcher.AvailableAccounts,
            a => Assert.Equal("Test:Account:One", a),
            a => Assert.Equal("Test:Account:Two", a));
    }

    private static void DummyDispatch(Action a) => a.Invoke();

    private void FillAccountsFile()
    {
        string contents = """
            commodity EUR
            commodity USD

            account Assets:Checking
            account Assets:Savings

            tag Holiday

            account Expenses:Living
            account Expenses:Recreation

            tag Event
            """;

        using StreamWriter write = new(new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write));
        write.Write(contents);
        write.Flush();
        write.Close();
    }

    public void Dispose()
    {
        if (File.Exists(_fileName))
            File.Delete(_fileName);
    }
}
