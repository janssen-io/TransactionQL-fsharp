module QLInterpreterTests

open System
open TransactionQL.Parser
open Interpretation
open QLInterpreter
open AST
open Xunit

let env = { Variables = Map.ofList []; Row = Map.ofList []; DateFormat = "yyyy/MM/dd" }
let uncurry f (a, b) = f a b

let eval' = uncurry eval >> Interpretation.result
let evalFilter' = uncurry evalFilter >> Interpretation.result

let testAmount (Line (Account _, amount)) expectedAmount =
    match expectedAmount with
    | None -> Assert.Equal(None, amount)
    | Some f -> 
        let (Commodity _, f') = Option.get amount
        Assert.Equal(f, f')
        
[<Fact>]
let ``Expressions: variables`` () =
    let variables = Map.ofList [("total", 50.00)]
    let actual = eval' ({ env with Variables = variables }, (Variable "total"))
    let expected = 50.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: numbers`` () =
    let actual = eval' ({ env with Variables = Map.ofList [] }, (ExprNum 50.00))
    let expected = 50.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: addition`` () =
    let actual = eval' ({ env with Variables = Map.ofList [] }, (Add (ExprNum 50.00, ExprNum 10.00)))
    let expected = 60.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: subtraction`` () =
    let actual = eval' ({ env with Variables = Map.ofList [] }, (Subtract (ExprNum 50.00, ExprNum 10.00)))
    let expected = 40.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: division`` () =
    let actual = eval' ({ env with Variables = Map.ofList [] }, (Divide (ExprNum 50.00, ExprNum 10.00)))
    let expected = 5.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: multiplication`` () =
    let actual = eval' ({ env with Variables = Map.ofList [] }, (Multiply (ExprNum 50.00, ExprNum 10.00)))
    let expected = 500.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: nested`` () =
    let variables = Map.ofList [
        ("total", 50.00)
        ("remainder", 30.00)]

    let actual = 
        eval' ({ env with Variables = variables },
            Divide (
                Add (
                    Variable "total",
                    Variable "remainder" ),
                ExprNum 2.0))
    let expected = 40.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Conditions: string operations - equalto`` () =
    let env' = { env with Row = Map.ofList [("Creditor","Value")] }
    Assert.True(evalFilter' (env', Filter (Column "Creditor", EqualTo, (String "Value"))))

[<Fact>]
let ``Conditions: string operations - notequalto`` () =
    let env' = { env with Row = Map.ofList [("Creditor","Value")] }
    Assert.True(evalFilter' (env', Filter (Column "Creditor", NotEqualTo, (String "value"))))

[<Fact>]
let ``Conditions: string operations - contains`` () =
    let env' = { env with Row = Map.ofList [("Creditor","Value")] }
    Assert.True(evalFilter' (env', Filter (Column "Creditor", Contains, (String "al"))))

[<Fact>]
let ``Conditions: regular expressions`` () =
    let env' = { env with Row = Map.ofList [("Creditor","Value")] }
    Assert.True(evalFilter' (env', Filter (Column "Creditor", Matches, (RegExp "^(v|V)al.*"))))

[<Fact>]
let ``Conditions: regular expressions are case sensitive`` () =
    let env' = { env with Row = Map.ofList [("Creditor","Value")] }
    Assert.False(evalFilter' (env', Filter (Column "Creditor", Matches, (RegExp "^val.*"))))

[<Fact>]
let ``Conditions: or groups`` () =
    let env' = { env with Row = Map.ofList [("A", "1"); ("B", "5")] }
    Assert.True(evalFilter' (env', OrGroup [
        Filter (Column "A", EqualTo, Number 0.0)
        Filter (Column "B", EqualTo, Number 5.0)
    ]))

    Assert.False(evalFilter' (env', OrGroup [
        Filter (Column "A", EqualTo, Number 2.0)
        Filter (Column "B", EqualTo, Number 2.0)
    ]))

[<Fact>]
let ``Conditions: nested or groups`` () =
    let env' = { env with Row = Map.ofList [("A", "1"); ("B", "5")] }
    Assert.True(evalFilter' (env', OrGroup [
        Filter (Column "A", EqualTo, Number 0.0)
        OrGroup [
            Filter (Column "A", EqualTo, Number 1.0)
            Filter (Column "B", EqualTo, Number 2.0)
        ]
    ]))

    Assert.False(evalFilter' (env', OrGroup [
        Filter (Column "A", EqualTo, Number 2.0)
        OrGroup [
            Filter (Column "A", EqualTo, Number 3.0)
            Filter (Column "B", EqualTo, Number 2.0)
        ]
    ]))

[<Fact>]
let ``Inference: number column - greaterthan`` () =
    let env' = { env with Row = Map.ofList [("Amount","5")] }
    Assert.True(evalFilter' (env', Filter (Column "Amount", GreaterThan, (Number 2.0))))

[<Fact>]
let ``Inference: number column - greaterthanorequalto`` () =
    let env' = { env with Row = Map.ofList [("Amount","5")] }
    Assert.True(evalFilter' (env', Filter (Column "Amount", GreaterThanOrEqualTo, (Number 5.0))))

[<Fact>]
let ``Inference: number column - lessthan`` () =
    let env' = { env with Row = Map.ofList [("Amount","1")] }
    Assert.True(evalFilter' (env', Filter (Column "Amount", LessThan, (Number 2.0))))

[<Fact>]
let ``Inference: number column - lessthanorequalto`` () =
    let env' = { env with Row = Map.ofList [("Amount","2")] }
    Assert.True(evalFilter' (env', Filter (Column "Amount", LessThanOrEqualTo, (Number 2.0))))

[<Fact>]
let ``Inference: number column - equalto`` () =
    let env' = { env with Row = Map.ofList [("Amount","5")] }
    Assert.True(evalFilter' (env', Filter (Column "Amount", EqualTo, (Number 5.0))))

[<Fact>]
let ``Inference: number column - notequalto`` () =
    let env' = { env with Row = Map.ofList [("Amount","5")] }
    Assert.True(evalFilter' (env', Filter (Column "Amount", NotEqualTo, (Number 2.0))))

[<Fact>]
let ``Posting lines: No amount`` () =
    let env' = { env with Variables = Map.add "remainder" 10.00 env.Variables }
    let transaction = Trx (Account ["Expenses"; "Food"], None)
    let (Interpretation (updatedEnv, Line (_, amount))) = generatePostingLine env' transaction
    Assert.Equal(None, amount)
    Assert.Equal(0.0, Map.find "remainder" updatedEnv.Variables)

[<Fact>]
let ``Posting lines: with amount`` () =
    let env' = { env with Variables = Map.add "remainder" 20.00 env.Variables }
    let transaction = Trx (Account ["Expenses"; "Food"], Some <| Amount (Commodity "€", 5.00))
    let (Interpretation (updatedEnv, Line (_, amount))) = generatePostingLine env' transaction
    Assert.Equal(Some (Commodity "€", 5.0), amount)
    Assert.Equal(25.0, Map.find "remainder" updatedEnv.Variables)

[<Fact>]
let ``Posting lines: with amount expression`` () =
    let env' = { env with Variables = Map.add "remainder" 10.00 env.Variables }
    let transaction = Trx (Account ["Expenses"; "Food"], Some <| AmountExpression (Commodity "€", ExprNum 20.00))
    let (Interpretation (updatedEnv, Line (_, amount))) = generatePostingLine env' transaction
    Assert.Equal(Some (Commodity "€", 20.0), amount)
    Assert.Equal(30.0, Map.find "remainder" updatedEnv.Variables)

[<Fact>]
let ``Posting: multiple lines`` () =
    let env' = { env with Variables = Map.add "remainder" 0.00 env.Variables }
    let transactions = Posting [
        Trx (Account ["Expenses"; "Food"], Some <| AmountExpression (Commodity "€", ExprNum 20.00))
        Trx (Account ["Assets"; "Checking"], None)
    ]
    let (Interpretation (_, lines)) = generatePosting env' transactions
    Assert.Equal(2, lines.Length)

[<Fact>]
let ``Posting: updates remainder between lines`` () =
    let env' = 
        { env with
            Variables = Map.ofList [
                ("remainder", 0.00)
        ]}
    let transactions = Posting [
        Trx (Account ["Expenses"; "Food"], Some <| 
            Amount (Commodity "€", 50.00))
        Trx (Account ["Assets"; "Receivables"], Some <|
            AmountExpression (Commodity "€", Subtract (ExprNum 20.00, Variable "remainder")))
        Trx (Account ["Assets"; "Checking"], Some <|
            AmountExpression (Commodity "€", Multiply (ExprNum -1.0, Variable "remainder")))
    ]
    let (Interpretation (newEnv, lines)) = generatePosting env' transactions
    Assert.Equal(3, lines.Length)
    Assert.Equal(0.0, Map.find "remainder" newEnv.Variables)
    testAmount lines.[0] (Some 50.0)
    testAmount lines.[1] (Some -30.0)
    testAmount lines.[2] (Some -20.0)

[<Fact>]
let ``Query: given a matching row, a posting is generated`` () = 
    let ql =
        Query (
            Payee "a payee",
            [Filter (Column "Amount", GreaterThan, Number 0.00)],
            Posting [
                Trx (Account ["Expenses"; "Misc"], Some <| AmountExpression (Commodity "€", Variable "total"))
                Trx (Account ["Assets"; "Checking"], None)
            ]
        )
    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let env' = { env with Row = row }
    let (Interpretation (_, entry)) = evalQuery env' ql
    Assert.NotEqual(None, entry)

    let ({ Header = Header _; Lines = lines; Comments = _ }) = Option.get entry
    Assert.Equal(2, lines.Length)

[<Fact>]
let ``Query: given a row that does not match, no posting is generated`` () =
    let ql = 
        Query (
            Payee "a payee",
            [Filter (Column "Amount", GreaterThan, Number 0.00)],
            Posting [
                Trx (Account ["Expenses"; "Misc"], Some <| AmountExpression (Commodity "€", Variable "total"))
                Trx (Account ["Assets"; "Checking"], None)
            ]
        )
    let row = Map.ofList [ ("Amount", "-10.00"); ("Date", "2019/06/01") ]
    let env' = { env with Row = row }
    let (Interpretation (_, entry)) = evalQuery env' ql
    Assert.Equal(None, entry)

[<Fact>]
let ``Program: multiple matching queries only applies the first match`` () =
    let program = 
        Program [
            Query (
                Payee "first payee",
                [Filter (Column "Amount", GreaterThan, Number 0.00)],
                Posting [
                    Trx (Account ["Expenses"; "Misc"], Some <| AmountExpression (Commodity "€", Variable "total"))
                    Trx (Account ["Assets"; "Checking"], Some <| AmountExpression (
                            Commodity "€",
                            Multiply (ExprNum -1.0, Variable "remainder")
                        )
                    )
                ]
            )
            Query (
                Payee "second payee",
                [Filter (Column "Amount", GreaterThan, Number 0.00)],
                Posting [
                    Trx (Account ["Expenses"; "Subscription"], Some <| AmountExpression (Commodity "€", Variable "total"))
                    Trx (Account ["Assets"; "Savings"], None)
                ]
            )
        ]
    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation (_, entry)) = evalProgram { env with Row = row } program
    Assert.NotEqual(None, entry)

    let ({ Header = Header (date, title); Lines = lines; Comments = _ }) = Option.get entry
    Assert.Equal(new DateTime(2019, 6, 1), date)
    Assert.Equal("first payee", title)
    Assert.Equal(2, lines.Length)

[<Fact>]
let ``Program: multiple queries only applies the match`` () =
    let program = 
        Program [
            Query (
                Payee "first payee",
                [Filter (Column "Amount", LessThan, Number 0.00)],
                Posting [
                    Trx (Account ["Expenses"; "Misc"], Some <| AmountExpression (Commodity "€", Variable "total"))
                    Trx (Account ["Assets"; "Checking"], Some <| AmountExpression (
                            Commodity "€",
                            Multiply (ExprNum -1.0, Variable "remainder")
                        )
                    )
                ]
            )
            Query (
                Payee "second payee",
                [Filter (Column "Amount", GreaterThan, Number 0.00)],
                Posting [
                    Trx (Account ["Expenses"; "Subscription"], Some <| AmountExpression (Commodity "€", Variable "total"))
                    Trx (Account ["Assets"; "Savings"], None)
                ]
            )
        ]
    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation (_, entry)) = evalProgram { env with Row = row } program

    let ({ Header = Header (date, title); Lines = lines; Comments = _ }) = Option.get entry
    Assert.Equal(new DateTime(2019, 6, 1), date)
    Assert.Equal("second payee", title)
    Assert.Equal(2, lines.Length)

[<Fact>]
let ``Program: no matches`` () =
    let program = Program [ ]
    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation (_, entry)) = evalProgram { env with Row = row } program
    Assert.Equal(None, entry)
