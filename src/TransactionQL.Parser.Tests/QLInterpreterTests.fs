module QLInterpreterTests

open System
open TransactionQL.Parser
open Interpretation
open QLInterpreter
open AST
open Xunit

let env =
    { Variables = Map.empty
      EnvVars = Map.empty
      Row = Map.ofList []
      DateFormat = "yyyy/MM/dd" }

let uncurry f (a, b) = f a b

let eval' = uncurry eval >> Interpretation.result
let evalFilter' = uncurry evalFilter >> Interpretation.result

let trx (accounts, amount) =
    { Account = accounts
      Amount = amount
      Tag = None } : Transaction

let line (accounts, amount) =
    { Account = accounts
      Amount = amount
      Tag = None } : Line

let testAmount ({ Amount = amount }: Line) expectedAmount =
    match expectedAmount with
    | None -> Assert.Equal(None, amount)
    | Some f ->
        let (_, f') = Option.get amount
        Assert.Equal(f, f')

[<Fact>]
let ``Expressions: variables`` () =
    let variables = Map.ofList [ ("total", 50.00) ]
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
    let actual =
        eval' ({ env with Variables = Map.ofList [] }, (Add(ExprNum 50.00, ExprNum 10.00)))

    let expected = 60.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: subtraction`` () =
    let actual =
        eval' ({ env with Variables = Map.ofList [] }, (Subtract(ExprNum 50.00, ExprNum 10.00)))

    let expected = 40.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: division`` () =
    let actual =
        eval' ({ env with Variables = Map.ofList [] }, (Divide(ExprNum 50.00, ExprNum 10.00)))

    let expected = 5.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: multiplication`` () =
    let actual =
        eval' ({ env with Variables = Map.ofList [] }, (Multiply(ExprNum 50.00, ExprNum 10.00)))

    let expected = 500.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Expressions: nested`` () =
    let variables = Map.ofList [ ("total", 50.00); ("remainder", 30.00) ]

    let actual =
        eval' ({ env with Variables = variables }, Divide(Add(Variable "total", Variable "remainder"), ExprNum 2.0))

    let expected = 40.00
    Assert.Equal(expected, actual)

[<Fact>]
let ``Conditions: string operations - equalto`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Creditor", "Value") ] }

    Assert.True(evalFilter' (env', Filter(Column "Creditor", EqualTo, (String "Value"))))

[<Fact>]
let ``Conditions: string operations - notequalto`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Creditor", "Value") ] }

    Assert.True(evalFilter' (env', Filter(Column "Creditor", NotEqualTo, (String "value"))))

[<Fact>]
let ``Conditions: string operations - contains`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Creditor", "Value") ] }

    Assert.True(evalFilter' (env', Filter(Column "Creditor", Contains, (String "al"))))

[<Fact>]
let ``Conditions: regular expressions`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Creditor", "Value") ] }

    Assert.True(evalFilter' (env', Filter(Column "Creditor", Matches, (RegExp "^(v|V)al.*"))))

[<Fact>]
let ``Conditions: regular expressions are case insensitive`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Creditor", "Value") ] }

    Assert.True(evalFilter' (env', Filter(Column "Creditor", Matches, (RegExp "^val.*"))))

[<Fact>]
let ``Conditions: regular expressions `` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Creditor", "Value") ] }

    Assert.False(evalFilter' (env', Filter(Column "Creditor", Matches, (RegExp "^fail.*"))))

[<Fact>]
let ``Conditions: or groups`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("A", "1"); ("B", "5") ] }

    Assert.True(
        evalFilter' (
            env',
            OrGroup
                [ Filter(Column "A", EqualTo, Number 0.0)
                  Filter(Column "B", EqualTo, Number 5.0) ]
        )
    )

    Assert.False(
        evalFilter' (
            env',
            OrGroup
                [ Filter(Column "A", EqualTo, Number 2.0)
                  Filter(Column "B", EqualTo, Number 2.0) ]
        )
    )

[<Fact>]
let ``Conditions: nested or groups`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("A", "1"); ("B", "5") ] }

    Assert.True(
        evalFilter' (
            env',
            OrGroup
                [ Filter(Column "A", EqualTo, Number 0.0)
                  OrGroup
                      [ Filter(Column "A", EqualTo, Number 1.0)
                        Filter(Column "B", EqualTo, Number 2.0) ] ]
        )
    )

    Assert.False(
        evalFilter' (
            env',
            OrGroup
                [ Filter(Column "A", EqualTo, Number 2.0)
                  OrGroup
                      [ Filter(Column "A", EqualTo, Number 3.0)
                        Filter(Column "B", EqualTo, Number 2.0) ] ]
        )
    )

[<Fact>]
let ``Inference: number column - greaterthan`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Amount", "5") ] }

    Assert.True(evalFilter' (env', Filter(Column "Amount", GreaterThan, (Number 2.0))))

[<Fact>]
let ``Inference: number column - greaterthanorequalto`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Amount", "5") ] }

    Assert.True(evalFilter' (env', Filter(Column "Amount", GreaterThanOrEqualTo, (Number 5.0))))

[<Fact>]
let ``Inference: number column - lessthan`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Amount", "1") ] }

    Assert.True(evalFilter' (env', Filter(Column "Amount", LessThan, (Number 2.0))))

[<Fact>]
let ``Inference: number column - lessthanorequalto`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Amount", "2") ] }

    Assert.True(evalFilter' (env', Filter(Column "Amount", LessThanOrEqualTo, (Number 2.0))))

[<Fact>]
let ``Inference: number column - equalto`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Amount", "5") ] }

    Assert.True(evalFilter' (env', Filter(Column "Amount", EqualTo, (Number 5.0))))

[<Fact>]
let ``Inference: number column - notequalto`` () =
    let env' =
        { env with
            Row = Map.ofList [ ("Amount", "5") ] }

    Assert.True(evalFilter' (env', Filter(Column "Amount", NotEqualTo, (Number 2.0))))

[<Fact>]
let ``Accounts: variable`` () =
    let account = AccountVariable "account:default"
    let env' = { env with EnvVars = Map.ofList [ ("account:default", "Default:Checking") ] }
    let accountParts = evalAccount env' account

    Assert.Collection(accountParts,
        (fun part -> Assert.Equal("Default", part)),
        (fun part -> Assert.Equal("Checking", part)))

[<Fact>]
let ``Payee: words`` () =
    let payeeParts = Interpolation [ Word "American"; Word "Express"]
    let payee = evalPayee env payeeParts
    Assert.Equal("American Express", payee)

[<Fact>]
let ``Payee: variable`` () =
    let payeeParts = Interpolation [ ColumnToken (Column "Name") ]
    let env' = { env with Row = Map.ofList [ ("Name", "American Express") ]}
    let payee = evalPayee env' payeeParts
    Assert.Equal("American Express", payee)

[<Fact>]
let ``Payee: variable (undefined)`` () =
    let payeeParts = Interpolation [ ColumnToken (Column "Name") ]
    let payee = evalPayee env payeeParts
    Assert.Equal("@Name", payee)

[<Fact>]
let ``Payee: interpolation`` () =
    let payeeParts = Interpolation [ Word "Monthly:"; ColumnToken (Column "Name") ]
    let env' = { env with Row = Map.ofList [ ("Name", "American Express") ]}
    let payee = evalPayee env' payeeParts
    Assert.Equal("Monthly: American Express", payee)

[<Fact>]
let ``Posting lines: No amount`` () =
    let env' =
        { env with
            Variables = Map.add "remainder" 10.00 env.Variables }

    let transaction =
        { Account = AccountLiteral [ "Expenses"; "Food" ]
          Amount = None
          Tag = None }

    let (Interpretation(updatedEnv, ({ Amount = amount }: Line))) =
        generatePostingLine env' transaction

    Assert.Equal(None, amount)
    Assert.Equal(0.0, Map.find "remainder" updatedEnv.Variables)

[<Fact>]
let ``Posting lines: with amount`` () =
    let env' =
        { env with
            Variables = Map.add "remainder" 20.00 env.Variables }

    let transaction =
        { Account = AccountLiteral [ "Expenses"; "Food" ]
          Amount = Some <| Amount(Commodity "€", 5.00)
          Tag = None }

    let (Interpretation(updatedEnv, ({ Amount = amount }: Line))) =
        generatePostingLine env' transaction

    Assert.Equal(Some("€", 5.0), amount)
    Assert.Equal(25.0, Map.find "remainder" updatedEnv.Variables)

[<Fact>]
let ``Posting lines: with amount expression`` () =
    let env' =
        { env with
            Variables = Map.add "remainder" 10.00 env.Variables }

    let transaction =
        { Account = AccountLiteral [ "Expenses"; "Food" ]
          Amount = Some <| AmountExpression(Commodity "€", ExprNum 20.00)
          Tag = None }

    let (Interpretation(updatedEnv, ({ Amount = amount }: Line))) =
        generatePostingLine env' transaction

    Assert.Equal(Some("€", 20.0), amount)
    Assert.Equal(30.0, Map.find "remainder" updatedEnv.Variables)

[<Fact>]
let ``Posting: multiple lines`` () =
    let env' =
        { env with
            Variables = Map.add "remainder" 0.00 env.Variables
            EnvVars = Map.add "account:default" "Default" env.EnvVars
        }

    let transactions =
        [ trx (AccountLiteral [ "Expenses"; "Food" ], Some <| AmountExpression(Commodity "€", ExprNum 20.00))
          trx (AccountVariable "account:default", None) ]

    let (Interpretation(_, lines)) = generatePosting env' transactions
    Assert.Collection(lines, 
        (fun l -> Assert.Equal(line(["Expenses"; "Food"], Some ("€", float 20)), l)),
        (fun l -> Assert.Equal(line(["Default"], None), l))
    )

[<Fact>]
let ``Posting: updates remainder between lines`` () =
    let env' =
        { env with
            Variables = Map.ofList [ ("remainder", 0.00) ] }

    let transactions =
        [ trx (AccountLiteral [ "Expenses"; "Food" ], Some <| Amount(Commodity "€", 50.00))
          trx (
              AccountLiteral [ "Assets"; "Receivables" ],
              Some
              <| AmountExpression(Commodity "€", Subtract(ExprNum 20.00, Variable "remainder"))
          )
          trx (
              AccountLiteral [ "Assets"; "Checking" ],
              Some
              <| AmountExpression(Commodity "€", Multiply(ExprNum -1.0, Variable "remainder"))
          ) ]

    let (Interpretation(newEnv, lines)) = generatePosting env' transactions
    Assert.Equal(3, lines.Length)
    Assert.Equal(0.0, Map.find "remainder" newEnv.Variables)
    testAmount lines.[0] (Some 50.0)
    testAmount lines.[1] (Some -30.0)
    testAmount lines.[2] (Some -20.0)

[<Fact>]
let ``Query: given a matching row, a posting is generated`` () =
    let ql =
        Query(
            Word "a payee",
            [ Filter(Column "Amount", GreaterThan, Number 0.00) ],
            Posting(
                None,
                [ trx (AccountLiteral [ "Expenses"; "Misc" ], Some <| AmountExpression(Commodity "€", Variable "total"))
                  trx (AccountLiteral [ "Assets"; "Checking" ], None) ]
            )
        )

    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let env' = { env with Row = row }
    let (Interpretation(_, entry)) = evalQuery env' ql
    Assert.NotEqual(None, entry)

    let ({ Header = Header _
           Lines = lines
           Comments = _ }) =
        Option.get entry

    Assert.Equal(2, lines.Length)

[<Fact>]
let ``Query: given a row that does not match, no posting is generated`` () =
    let ql =
        Query(
            Word "a payee",
            [ Filter(Column "Amount", GreaterThan, Number 0.00) ],
            Posting(
                None,
                [ trx (AccountLiteral [ "Expenses"; "Misc" ], Some <| AmountExpression(Commodity "€", Variable "total"))
                  trx (AccountLiteral [ "Assets"; "Checking" ], None) ]
            )
        )

    let row = Map.ofList [ ("Amount", "-10.00"); ("Date", "2019/06/01") ]
    let env' = { env with Row = row }
    let (Interpretation(_, entry)) = evalQuery env' ql
    Assert.Equal(None, entry)

[<Fact>]
let ``Queries: multiple matching queries only applies the first match`` () =
    let queries =
        [ Query(
              Word "first payee",
              [ Filter(Column "Amount", GreaterThan, Number 0.00) ],
              Posting(
                  None,
                  [ trx (AccountLiteral [ "Expenses"; "Misc" ], Some <| AmountExpression(Commodity "€", Variable "total"))
                    trx (
                        AccountLiteral [ "Assets"; "Checking" ],
                        Some
                        <| AmountExpression(Commodity "€", Multiply(ExprNum -1.0, Variable "remainder"))
                    ) ]
              )
          )
          Query(
              Word "second payee",
              [ Filter(Column "Amount", GreaterThan, Number 0.00) ],
              Posting(
                  None,
                  [ trx (
                        AccountLiteral [ "Expenses"; "Subscription" ],
                        Some <| AmountExpression(Commodity "€", Variable "total")
                    )
                    trx (AccountLiteral [ "Assets"; "Savings" ], None) ]
              )
          ) ]

    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation(_, entry)) = evalProgram { env with Row = row } queries
    Assert.NotEqual(None, entry)

    let ({ Header = Header(date, title)
           Lines = lines
           Comments = _ }) =
        Option.get entry

    Assert.Equal(new DateTime(2019, 6, 1), date)
    Assert.Equal("first payee", title)
    Assert.Equal(2, lines.Length)

[<Fact>]
let ``Queries: multiple queries only applies the match`` () =
    let queries =
        [ Query(
              Word "first payee",
              [ Filter(Column "Amount", LessThan, Number 0.00) ],
              Posting(
                  None,
                  [ trx (AccountLiteral [ "Expenses"; "Misc" ], Some <| AmountExpression(Commodity "€", Variable "total"))
                    trx (
                        AccountLiteral [ "Assets"; "Checking" ],
                        Some
                        <| AmountExpression(Commodity "€", Multiply(ExprNum -1.0, Variable "remainder"))
                    ) ]
              )
          )
          Query(
              Word "second payee",
              [ Filter(Column "Amount", GreaterThan, Number 0.00) ],
              Posting(
                  None,
                  [ trx (
                        AccountLiteral [ "Expenses"; "Subscription" ],
                        Some <| AmountExpression(Commodity "€", Variable "total")
                    )
                    trx (AccountLiteral [ "Assets"; "Savings" ], None) ]
              )
          ) ]

    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation(_, entry)) = evalProgram { env with Row = row } queries

    let ({ Header = Header(date, title)
           Lines = lines
           Comments = _ }) =
        Option.get entry

    Assert.Equal(new DateTime(2019, 6, 1), date)
    Assert.Equal("second payee", title)
    Assert.Equal(2, lines.Length)

[<Fact>]
let ``Queries: no matches`` () =
    let queries = []
    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation(_, entry)) = evalProgram { env with Row = row } queries
    Assert.Equal(None, entry)

[<Fact>]
let ``Queries: notes are added to the comments`` () =
    let queries =
        [ Query(
              Word "second payee",
              [ Filter(Column "Amount", GreaterThan, Number 0.00) ],
              Posting(
                  "this is a note" |> Some,
                  [ trx (
                        AccountLiteral [ "Expenses"; "Subscription" ],
                        Some <| AmountExpression(Commodity "€", Variable "total")
                    )
                    trx (AccountLiteral [ "Assets"; "Savings" ], None) ]
              )
          ) ]

    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation(_, entry)) = evalProgram { env with Row = row } queries

    let ({ Header = Header _
           Lines = _
           Comments = comments }) =
        Option.get entry

    Assert.Equal(1, comments.Length)
    Assert.Equal("this is a note", comments.[0])

[<Fact>]
let ``Queries: tags are added to the posting line`` () =
    let queries =
        [ Query(
              Word "second payee",
              [ Filter(Column "Amount", GreaterThan, Number 0.00) ],
              Posting(
                  "this is a note" |> Some,
                  [ { Account = AccountLiteral [ "Expenses"; "Subscription" ]
                      Amount = (Some << AmountExpression) (Commodity "€", Variable "total")
                      Tag = Some "My: FirstTag" }
                    { Account = AccountLiteral [ "Assets"; "Savings" ]
                      Amount = None
                      Tag = Some "My: OtherTag" } ]
              )
          ) ]

    let row = Map.ofList [ ("Amount", "10.00"); ("Date", "2019/06/01") ]
    let (Interpretation(_, entry)) = evalProgram { env with Row = row } queries

    let ({ Header = Header _
           Lines = lines
           Comments = _ }) =
        Option.get entry

    let containsTag expectedTag ({ Tag = tag }: Line) = Assert.Equal(expectedTag, tag)
    Assert.Collection(lines, containsTag (Some "My: FirstTag"), containsTag (Some "My: OtherTag"))
