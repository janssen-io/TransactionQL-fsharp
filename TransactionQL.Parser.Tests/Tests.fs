module Tests

open System
open Xunit
open FParsec
open AST

let test<'T> parser txt (expected:'T) =
    match run parser txt with
    | Success(actual, _, _) -> Assert.Equal(expected, actual)
    | Failure(msg, _, _) -> Assert.True(false, msg)

[<Fact>]
let ``qword parses alphanumerical characters`` () = 
    let txt = "Word123"
    test QLParser.qword txt (Word txt)
    
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
let ``qarithop parses`` () =
    test QLParser.qarithop "+" Add
    test QLParser.qarithop "-" Subtract
    test QLParser.qarithop "*" Multiply
    test QLParser.qarithop "/" Divide
 
[<Fact>]
let ``qboolop parses`` () =
    test QLParser.qboolop "=" Equals
    test QLParser.qboolop "/=" NotEquals
    test QLParser.qboolop ">" GreaterThan
    test QLParser.qboolop ">=" GreaterThanOrEqualTo
    test QLParser.qboolop "<" LessThan
    test QLParser.qboolop "<=" LessThanOrEqualTo
    test QLParser.qboolop "contains" Substring
    test QLParser.qboolop "matches" Matches

[<Fact>]
let ``Expressions!`` () =
    test QLParser.qexpression "{total/2}" (
        Expression (
            ExprWord "total"
            , Divide
            , ExprNum 2.0
        )
    )

[<Fact>]
let ``Simple Expressions`` () =
    test QLParser.qexpression "{total}" (ExprWord "total")
    test QLParser.qexpression "{13.37}" (ExprNum 13.37)

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
let ``qtransaction`` () =
    test QLParser.qtransaction "Expenses:Living:Food EUR {total-5.25}" (
        Account ["Expenses";"Living";"Food"]
        , Commodity "EUR"
        , Expression (ExprWord "total", Subtract, ExprNum 5.25)
    )

