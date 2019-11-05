module Tests

open Xunit
open FParsec
open AST

let test<'T> parser txt (expected:'T) =
    match run parser txt with
    | Success(actual, _, _) -> Assert.Equal(expected, actual)
    | Failure(msg, _, _) -> Assert.True(false, msg)

[<Fact>]
let ``qstring parses quoted strings`` () =
    let txt = "\"Quoted \\\" String\""
    let expected = "Quoted \" String"
    test QLParser.qstring txt (String expected)

[<Fact>]
let ``qregex parses regular expressions`` () =
    let txt = @"/(a|b)*[^a-z]\/?$select=\{\}{2,}\*\?\+\(\)/"
    let expected = @"(a|b)*[^a-z]/?$select=\{\}{2,}\*\?\+\(\)"
    test QLParser.qregex txt (Regex expected)

[<Fact>]
let ``Expressions!`` () =
    test QLParser.qexpression "{total/2}" (
        Divide (
            Variable "total"
            , ExprNum 2.0
        )
    )

[<Fact>]
let ``Simple Expressions`` () =
    test QLParser.qexpression "{total}" (Variable "total")
    test QLParser.qexpression "{13.37}" (ExprNum 13.37)

[<Fact(Skip = "Parentheses are not implemented yet")>]
let ``Nested expressions`` () =
    test QLParser.qexpression "({total} + 10) / 2" (
        Divide (
            Add (Variable "total", ExprNum 10.0)
            , ExprNum 2.0)
    )

[<Fact>]
let ``qaccount parses words separated by colons`` () =
    test QLParser.qaccount "Expenses:Recreation:Hobby" (Account ["Expenses"; "Recreation"; "Hobby"])

[<Fact>]
let ``qcommodity parses string literals`` () =
    test QLParser.qcommodity "\"Vanguard SP500\"" (Commodity "Vanguard SP500")

[<Fact>]
let ``qcommodity parses words`` () =
    test QLParser.qcommodity "EUR" (Commodity "EUR")

[<Fact>]
let ``qamount parses numbers`` =
    test QLParser.qamount "EUR 100.00" (Amount (Commodity "EUR", 100.00))

[<Fact>]
let ``qamount parses expressions`` =
    test QLParser.qamount "{total}" (AmountExpression (Commodity "EUR", Variable "total"))

[<Fact>]
let ``qtransaction expression`` () =
    test QLParser.qtransaction "Expenses:Living:Food EUR {total - 5.25}" (
        Trx (
            Account ["Expenses";"Living";"Food"]
            , Some <| AmountExpression (
                Commodity "EUR"
                , Subtract (Variable "total", ExprNum 5.25))
        ))

[<Fact>]
let ``qtransaction amount`` () =
    test QLParser.qtransaction "Expenses:Living:Food EUR 13.37" (
        Trx (
            Account ["Expenses";"Living";"Food"]
            , Some <| Amount (Commodity "EUR", 13.37)))

[<Fact>]
let ``qtransaction just account`` () =
    test QLParser.qtransaction "Expenses" (Trx (Account ["Expenses"], None))

[<Fact>]
let ``empty qposting`` () =
    test QLParser.qposting "Posting { }" (Posting [])

[<Fact>]
let ``qposting with two transactions`` () =
    let posting = 
        "Posting {
            Expenses:Rent    EUR {total}
            Expenses:Rent    EUR 5.00
            Assets:Checking
        }"
    test QLParser.qposting posting (Posting [
        Trx (Account ["Expenses"; "Rent"], Some <| AmountExpression (Commodity "EUR", Variable "total"))
        Trx (Account ["Expenses"; "Rent"], Some <| Amount (Commodity "EUR", 5.0))
        Trx (Account ["Assets"; "Checking"], None)
    ])

[<Fact>]
let ``qcolumnIdentifier parses words starting with a capital letter`` () =
    test QLParser.qcolumnIdentifier "Creditor" (Column "Creditor")

[<Fact>]
let ``qfilter parses <column> <op> <expr>`` () =
    test QLParser.qfilter "Creditor matches /some regex/" (
        Matches (Column "Creditor", Regex "some regex")
    )

[<Fact>]
let ``qpayee parses # <words>`` () =
    let payee = "Some long string"
    test QLParser.qpayee (sprintf "# %s" payee) (Payee payee)

[<Fact>]
let ``qdescription parses an entire description`` () =
    let description = """# Full description test
            Creditor = "NL"
            Amount >= 50.00

            posting {
                Assets:TestAccount  EUR {total / 2}
                Assets:TestSavings  EUR {remainder}
                Expenses:Development
            }
            """
    test QLParser.qdescription description (
        Description (
            Payee "Full description test", [
                Equals (Column "Creditor", String "NL")
                GreaterThanOrEqualTo (Column "Amount", Number 50.0)]
            , Posting [
                Trx (
                    Account ["Assets"; "TestAccount"],
                    Some <| AmountExpression (
                        Commodity "EUR",
                        Divide (Variable "total", ExprNum 2.0)))
                Trx (
                    Account ["Assets"; "TestSavings"],
                    Some <| AmountExpression (
                        Commodity "EUR",
                        Variable "remainder"))
                Trx (
                    Account ["Expenses"; "Development"],
                    None)]))