module QLParserTests

open Xunit
open FParsec
open System
open TransactionQL.Parser.AST
open TransactionQL.Parser

let test<'T> parser txt (expected: 'T) =
    match run parser txt with
    | Success(actual, _, _) -> Assert.Equal(expected, actual)
    | Failure(msg, _, _) -> Assert.True(false, msg)

let trx (accounts, amount) =
    { Account = accounts
      Amount = amount
      Tags = [||] }

let parseEquals<'T> (parser: Parser<'T, unit>) a b =
    let parsedA = run parser a
    let parsedB = run parser b

    match (parsedA, parsedB) with
    | Success(a', _, _), Success(b', _, _) -> Assert.Equal(a', b')
    | Success(_, _, _), Failure(msg, _, _) -> Assert.True(false, msg)
    | Failure(msg, _, _), Success(_, _, _) -> Assert.True(false, msg)
    | Failure(msgA, _, _), Failure(msgB, _, _) ->
        Assert.True(false, System.String.Join(Environment.NewLine, msgA, msgB))

[<Fact>]
let ``String: "quoted strings"`` () =
    let txt = "\"Quoted \\\" String\""
    let expected = "Quoted \" String"
    test QLParser.qstring txt (String expected)

[<Fact>]
let ``Regex: /regular expressions/`` () =
    let txt = @"/(a|b)*[^a-z]\/?$select=\{\}{2,}\*\?\+\(\)/"
    let expected = @"(a|b)*[^a-z]/?$select=\{\}{2,}\*\?\+\(\)"
    test QLParser.qregex txt (RegExp expected)

[<Fact>]
let ``Expressions: simple`` () =
    test QLParser.qexpression "(total/2)" (Divide(Variable "total", ExprNum 2.0))

[<Fact>]
let ``Expressions: atoms`` () =
    test QLParser.qexpression "(total)" (Variable "total")
    test QLParser.qexpression "(13.37)" (ExprNum 13.37)

[<Fact>]
let ``Expressions: nested`` () =
    test
        QLParser.qexpression
        "((5 + (total - ((1 / 2) * remainder))))"
        (Add(ExprNum 5.0, Subtract(Variable "total", Multiply(Divide(ExprNum 1.0, ExprNum 2.0), Variable "remainder"))))

[<Fact>]
let ``Expressions: precedence`` () =
    parseEquals QLParser.qexpression "(5 + 2 * 3)" "(5 + (2 * 3))"
    parseEquals QLParser.qexpression "(5 * 2 + 3)" "((5 * 2) + 3)"

[<Fact>]
let ``Expressions: subtraction is left associative`` () =
    parseEquals QLParser.qexpression "(5 - 2 - 3)" "((5 - 2) - 3)"

[<Fact>]
let ``Accounts: words separated by colons`` () =
    test QLParser.qaccount "Expenses:Recreation:Hobby" (AccountLiteral [ "Expenses"; "Recreation"; "Hobby" ])

[<Fact>]
let ``Accounts: variables`` () =
    test QLParser.qaccount "(account:default)" (AccountVariable "account:default")

[<Fact>]
let ``Commodity: string literals`` () =
    test QLParser.qcommodity "\"Vanguard SP500\"" (Commodity "Vanguard SP500")

[<Fact>]
let ``Commodity: words`` () =
    test QLParser.qcommodity "EUR" (Commodity "EUR")

[<Fact>]
let ``Amount: numbers`` () =
    test QLParser.qamount "EUR 100.00" (Amount(Commodity "EUR", 100.00))

[<Fact>]
let ``Amount: expressions`` () =
    test QLParser.qamount "EUR (total)" (AmountExpression(Commodity "EUR", Variable "total"))

[<Fact>]
let ``Transactions: expression`` () =
    test
        QLParser.qtransaction
        "Expenses:Living:Food EUR (total - 5.25)"
        (trx (
            AccountLiteral [ "Expenses"; "Living"; "Food" ],
            Some
            <| AmountExpression(Commodity "EUR", Subtract(Variable "total", ExprNum 5.25))
        ))

[<Fact>]
let ``Transactions: amount`` () =
    test
        QLParser.qtransaction
        "Expenses:Living:Food EUR 13.37"
        (trx (AccountLiteral [ "Expenses"; "Living"; "Food" ], Some <| Amount(Commodity "EUR", 13.37)))

[<Fact>]
let ``Transactions: just account`` () =
    test QLParser.qtransaction "Expenses" (trx (AccountLiteral [ "Expenses" ], None))

[<Fact>]
let ``Transactions: amount with tag`` () =
    test
        QLParser.qtransaction
        "Expenses:Living:Food EUR 13.37 ; Key: value"
        { Account = AccountLiteral [ "Expenses"; "Living"; "Food" ]
          Amount = Amount(Commodity "EUR", 13.37) |> Some
          Tags = [|"Key: value"|] }

[<Fact>]
let ``Transactions: just account with tag`` () =
    test
        QLParser.qtransaction
        "Expenses:Living:Food ; Key: value"
        { Account = AccountLiteral [ "Expenses"; "Living"; "Food" ]
          Amount = None
          Tags = [|"Key: value"|] }

[<Fact>]
let ``Posting: empty`` () =
    test QLParser.qposting "Posting { }" (Posting(None, []))

[<Fact>]
let ``Posting: transaction with a note`` () =
    let posting =
        "Posting {
            note Test-Note
            Expenses:Test   EUR 10.00
            Assets:Checking
        }"

    test QLParser.qposting "Posting { }" (Posting(None, []))

[<Fact>]
let ``Posting: two transactions`` () =
    let posting =
        "Posting {
            Expenses:Rent    EUR (total)
            Expenses:Rent    EUR 5.00
            Assets:Checking
        }"

    test
        QLParser.qposting
        posting
        (Posting(
            None,
            [ trx (AccountLiteral [ "Expenses"; "Rent" ], Some <| AmountExpression(Commodity "EUR", Variable "total"))
              trx (AccountLiteral [ "Expenses"; "Rent" ], Some <| Amount(Commodity "EUR", 5.0))
              trx (AccountLiteral [ "Assets"; "Checking" ], None) ]
        ))

[<Fact>]
let ``Columns: words starting with a capital letter`` () =
    test QLParser.qcolumnIdentifier "Creditor" (Column "Creditor")

[<Fact>]
let ``Filters: <column> <op> <expr>`` () =
    test QLParser.qfilter "Creditor matches /some regex/" (Filter(Column "Creditor", Matches, RegExp "some regex"))
    test QLParser.qfilter "Creditor = 3.0" (Filter(Column "Creditor", EqualTo, Number 3.0))

[<Fact>]
let ``Filters: or groups`` () =
    test
        QLParser.qfilter
        ("A = 1.0\n or B = 2.0\n or C = 3.0")
        (OrGroup
            [ Filter(Column "A", EqualTo, Number 1.0)
              OrGroup
                  [ Filter(Column "B", EqualTo, Number 2.0)
                    Filter(Column "C", EqualTo, Number 3.0) ] ])

[<Fact>]
let ``Payee: # <words>`` () =
    let payee = "Some long string"
    let words = payee.Split () |> (Array.map Word) |> List.ofArray
    test QLParser.qpayee $"# %s{payee}\n" (Interpolation words)

[<Fact>]
let ``Payee: # <variable>`` () =
    let variable = "Name"
    test QLParser.qpayee $"# @%s{variable}\n"  (Interpolation [ColumnToken (Column variable)])

[<Fact>]
let ``Payee: # <interpolation>`` () =
    let variable = "Name"
    test QLParser.qpayee $"# Some @%s{variable} string\n" (Interpolation [ (Word "Some"); (ColumnToken (Column variable)); (Word "string") ])

[<Fact>]
let ``Payee: # <odd chars>`` () =
    let variable = "Name"
    test 
        QLParser.qpayee
        $"# Some <Test> & Sons (@%s{variable}) string\n"
        (Interpolation [
            Word "Some"
            Word "<Test>"
            Word "&"
            Word "Sons"
            Word "("
            ColumnToken (Column "Name")
            Word ")"
            Word "string"
        ])

[<Fact>]
let ``Query: <payee> <filters> <posting>`` () =
    let query = [
        "# Full description @Creditor"
        "    Creditor = \"NL\""
        "    Amount >= 50.00"
        ""
        "    posting {"
        "        Assets:TestAccount  EUR (total / 2)"
        "        Assets:TestSavings  EUR (remainder)"
        "        Expenses:Development"
        "    }"
    ]
    test
        QLParser.qquery
        (String.concat Environment.NewLine query)
        (Query(
            Interpolation [ Word "Full"; Word "description"; ColumnToken (Column "Creditor") ],
            [ Filter(Column "Creditor", EqualTo, String "NL")
              Filter(Column "Amount", GreaterThanOrEqualTo, Number 50.0) ],
            Posting(
                None,
                [ trx (
                      AccountLiteral [ "Assets"; "TestAccount" ],
                      Some <| AmountExpression(Commodity "EUR", Divide(Variable "total", ExprNum 2.0))
                  )
                  trx (
                      AccountLiteral [ "Assets"; "TestSavings" ],
                      Some <| AmountExpression(Commodity "EUR", Variable "remainder")
                  )
                  trx (AccountLiteral [ "Expenses"; "Development" ], None) ]
            )
        ))

[<Fact>]
let ``Queries: multiple queries`` () =
    let queries = [
        "# First query"
        "   Creditor = \"NL\""
        ""
        "   posting {"
        "       Test:Account"
        "   }"
        ""
        "# Second query"
        "   Creditor = \"BE\""
        ""
        "   A = 5.0"
        "   or B = 2.0"
        ""
        "   C = 1.0"
        ""
        "   posting {"
        "       Assets:Checking"
        "   }"
    ]

    test
        QLParser.qprogram
        (String.concat Environment.NewLine queries)
        ([ Query(
               Interpolation [ Word "First"; Word "query" ],
               [ Filter(Column "Creditor", EqualTo, String "NL") ],
               Posting(None, [ trx (AccountLiteral [ "Test"; "Account" ], None) ])
           )

           Query(
               Interpolation [ Word "Second"; Word "query" ],
               [ Filter(Column "Creditor", EqualTo, String "BE")
                 OrGroup
                     [ Filter(Column "A", EqualTo, Number 5.0)
                       Filter(Column "B", EqualTo, Number 2.0) ]
                 Filter(Column "C", EqualTo, Number 1.0) ],
               Posting(None, [ trx (AccountLiteral [ "Assets"; "Checking" ], None) ])
           ) ])

[<Fact>]
let ``Metadata: Key = value pairs`` () =
    let txt = "metadata lsp.accounts = ./accounts.ldg"
    let expected = ("lsp.accounts", "./accounts.ldg") |> Metadata
    test QLParser.qmetadata txt expected