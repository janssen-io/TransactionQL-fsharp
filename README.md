# Transaction Query Language ![version](https://img.shields.io/github/v/release/janssen-io/transactionql-fsharp?label=version&labelColor=hsl(155%2C%2055%25%2C%2049%25)&color=darkslategray)

![downloads](https://img.shields.io/github/downloads/janssen-io/TransactionQL-fsharp/total)

Transaction QL is a domain specific language to convert bank statements
to [ledger-cli](https://www.ledger-cli.org/) postings.

## Ledger-cli

> Ledger is a powerful, double-entry accounting system that is accessed from the UNIX command-line. Ledger, begun in
> 2003, is written by John Wiegley and released under the BSD license.

Ledger's main component is a posting. In a posting you specify the date, payee
and to/from which accounts the money went. A simple posting might look like
this:

    2019/11/03 New laptop
        Expenses:Technology  €  749.99
        Assets:Checking      € -749.99

The posting starts with a date and a description or payee. Below that are at
least two accounts where money is either taken from or moved into. In this case
the money went from the `Assets:Checking` account into the `Expenses:Technology`
account. Using this format, reports can be generated to tell you exactly how
much you spent or received throughout the year.

Small detail: postings in ledger have to be balanced. To help you with that,
you do not specify an amount for one of the accounts it fills in the
remainder. The posting above can therefore be simplified to:

    2019/11/03 New laptop
        Expenses:Technology  € 749.99
        Assets:Checking

## Project Structure

The solution is divided into three parts.
The first part, TransactionQL.Parser, contains the parser and the
interpreter.
The second part, TransactionQL.Converters, contains the functions to convert
bank statements to a format the interpreter can filter.
The last part, TransactionQL.Console contains the actual command line
application.

## Domain Specific Language

The language allows you to describe filters. When a transaction matches a
filter, it will generate a posting. Filters consist of three parts:

1. Payee line
2. Conditions
3. Posting expression

```
# Rent                               ; Payee line
    Receiver = "NL20INGB0001234567"  ; Condition
    
    Amount > 1000.00                 ; Condition group
    or Amount < 100.00

    Posting {                        ; Posting expression
        Expenses:Rent    € (total)
        Assets:Checking
    }
```

### Payee line

Every filter starts with a payee line. This line will be used on the generated
posting. Variables can be used by prefixing them with the `@`-symbol.
For example, `# Rent (@Name)`. If the variable does not exist, the text is
printed as is.

Individual `@`-symbols are not supported.

### Conditions

After the payee line, there must be one or more conditions. These conditions are
used to match the filter to a specific bank transaction. If all conditions are
met, a posting will be generated according to the _posting expression_.

Conditions are always conjunctions (all conditions must be true), but may
contains disjunctions (any condition must be true) by using `or <condition>`.

#### Properties

To distinguish transactions, the following properties of a bank transaction can
be used in conditions.

NB: This can change between different implementations of a transaction reader.

##### Receiver

The bank account of the person or company that you paid to. For example your
landlord for rent or the supermarket for groceries.

##### Sender

The bank account from which the commodity was paid.

##### Amount

The total amount of the transaction.

##### Date

The date on which the transaction occurred.

##### Description

The details that describe why the transaction was made.

#### Operators

The properties can be compared with values using the following operators:

##### Numeric comparison

To compare numeric values, you can use equal to (`=`), not equal to (`\=`),
greater than (`>`), greater than or equal to (`>=`), less than (`<`)
and less than or equal to (`<=`).

##### Text comparison

To compare text values, you can use equal to (`=`), not equal to (`\=`),
matches regular expression (`matches`) or contains substring (`contains`).
Text values must be put between double quotes (e.g.`"text"`) and regular
expressions must be put between forward slashes (e.g. `/^NL/`).

### Posting Expression

This section of the filter describes the accounts and the amount that goes in
or out of them. The expression always starts with `Posting {` and ends with a
closing curly brace (`}`). To make sure the amount can be split over several
accounts, you can use variables and basic arithmetic in _amount expressions_.

#### Amount expressions

An amount expression is always written between parentheses (`(`, `)`). To
capture the amount of the transaction, you can use the variable `total`. As you
write transactions, the remaining sum is stored in the variable `remainder`.

**Example:** say we have a transaction for €500, then the variable `total` will
always be equal to `500`. The variable `remainder` will update as follows:

```
Expenses:Transport   € (total / 2)      ; remainder is updated to 250
Expenses:Rent        € (remainder - 10) ; remainder is updated to  10
Expenses:Maintenance € (remainder)      ; remainder is updated to   0
```

Which in turn generates the following transactions:

```
Expenses:Transport   € 250.00
Expenses:Rent        € 240.00
Expenses:Maintenance €  10.00
```

The variable `amount` contains the actual transferred amount from the perspective of your account.
That is, when you transfer money it is negative, when you receive money it is positive.

## Command line interface

For more information about the command line interface, run it with the `--help` flag.

## Desktop Application
The desktop application (`TransactionQL.DesktopApp`) runs on multiple operating systems using Avalonia and uses the same functionality as the CLI.

### Loading transactions and filters
When you open the application, you can select transactions, filters and existing accounts using `Ctrl+O` or by clicking on the _folder_ icon in the top left.
The existings accounts must be in [Ledger CLI](https://ledger-cli.org/doc/ledger3.html#Keeping-it-Consistent-1) format (`account My:Account:Name`).

The plugins for the banks (ASN, Bunq and ING in the screenshot below) are loaded from the `ApplicationData` directory.
- Windows: `%AppData/tql/plugins`
- Linux: `~/.config/tql/plugins`.

![img/select_bank.png](img/select_bank.png)
![img/select_transactions.png](img/select_transactions.png)

### Updating transactions 
The app uses the same filters to automatically categorize transactions. Any transactions that could not be categorized automatically are marked with a red dot in the list.
To update them, you can simply go through them (`Ctrl+Q` and `Ctrl+E` for previous/next transaction respectively). You can add a new posting (line) with `Ctrl+D` or by clicking on the _plus_ (`+`) icon above the posting table.
The _Account_ field of the new line is automatically in focus and as you type, it uses fuzzy search to auto-complete the account's name.
Use `<Tab>` to navigate to the currency and amount fields.

> Postings without an account name will be automatically removed. So don't fret it if you accidentally added too many.

![img/gui.png](img/gui.png)

### Saving and exporting
The app automatically saves your progress when you exit the app. You can also force this by pressing the _floppy disk_ icon in the top bar (`Ctrl+S`).
When all transactions have valid postings and titles (no more red dots), you can export it to Ledger CLI format. You can select an existing ledger and the app will append the transactions to the end of the file.
