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
    

