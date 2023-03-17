using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TransactionQL.DesktopApp.Application
{
    internal class Workspace
    {
        public string[] Accounts { get; init;  } = new string[0];

        public string FilterPath { get; init; } = string.Empty;

        public static Workspace Open(string path) => Open(path, WorkspaceOptions.Default);
        public static Workspace Open(string path, WorkspaceOptions options)
        {
            return new Workspace
            {
                Accounts = ReadAccounts(options.AccountsFile)
            };
        }

        private static string[] ReadAccounts(string accountsFile)
        {
            using var reader = new StreamReader(accountsFile);
            var contents = reader.ReadToEnd();
            var regex = new Regex("^account ([a-zA-Z:]+)");
            return contents
                .Split(Environment.NewLine)
                .Select(line =>
                {
                    var m = regex.Match(line);
                    if (m.Success)
                        return (success: true, name: m.Captures[0].Value);
                    else
                        return (success: false, name: string.Empty);

                })
                .Where(account => account.success)
                .Select(account => account.name)
                .ToArray();
        }
    }

    internal class WorkspaceOptions
    {
        public string AccountsFile { get; init; } = string.Empty;

        public static WorkspaceOptions Default = new()
        {
            AccountsFile = "accounts.ldg"
        };
    }
}
