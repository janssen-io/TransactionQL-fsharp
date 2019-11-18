module QLParser
    open FParsec
    open AST

    let curry2 f a b = f (a, b)
    let curry3 f a b c = f (a, b, c)

    let pword =
        let islower x = List.contains x ['a' .. 'z']
        let isupper x = List.contains x ['A' .. 'Z']
        let isdigit x = List.contains x ['0' .. '9']
        many1SatisfyL (fun c -> islower c || isupper c || isdigit c) "word (a-Z0-9)"

    let qnumber = pfloat |>> Number

    let regexEscape =
        anyOf @"{}[]/()?*+\"
        |>> function
              | '/' -> "/"
              | c   -> "\\" + string c

    let stringEscape =
        anyOf  "\"\\bfnrt"
        |>> function
              | 'b' -> "\b"
              | 'f' -> "\u000C"
              | 'n' -> "\n"
              | 'r' -> "\r"
              | 't' -> "\t"
              | c   -> string c // every other char is mapped to itself

    let escapedBetween (betweenChar:char) escape =
        let escapedCharSnippet = pstring "\\" >>. escape
        let normalCharSnippet  = manySatisfy (fun c -> c <> betweenChar && c <> '\\')

        let surrounder = betweenChar.ToString()
        between (pstring surrounder) (pstring surrounder)
                (stringsSepBy normalCharSnippet escapedCharSnippet)

    let stringLiteral = escapedBetween '"' stringEscape
    let qstring = stringLiteral <?> "String literal" |>> String

    let regexLiteral : Parser<string, unit> = escapedBetween '/' regexEscape
    let qregex = regexLiteral <?> "Regular expression" |>> RegExp

    let qboolop =
        let qequals = stringReturn "=" EqualTo
        let qnotEqual = stringReturn "/=" NotEqualTo
        let qlte = stringReturn "<=" LessThanOrEqualTo
        let qgte = stringReturn ">=" GreaterThanOrEqualTo
        let qlt = stringReturn "<" LessThan
        let qgt = stringReturn ">" GreaterThan
        let qcontains = stringReturn "contains" Contains
        let qmatches = stringReturn "matches" Matches
        choice [ qnotEqual; qequals; qgte; qgt; qlte; qlt; qcontains; qmatches]

    let qexpression =
        let opp = OperatorPrecedenceParser<Expression, unit, unit> ()
        let ws = spaces
        let paren, parenRef = createParserForwardedToRef ()
        let qexprNum = pfloat .>> ws |>> ExprNum
        let qvariable = pword .>> ws |>> Variable

        opp.TermParser <- choice [paren; qexprNum; qvariable]
        opp.AddOperator(InfixOperator("+", ws, 1, Associativity.Left, curry2 Add))
        opp.AddOperator(InfixOperator("-", ws, 1, Associativity.Left, curry2 Subtract))
        opp.AddOperator(InfixOperator("*", ws, 2, Associativity.Left, curry2 Multiply))
        opp.AddOperator(InfixOperator("/", ws, 2, Associativity.Left, curry2 Divide))

        parenRef := between (pchar '(') (pchar ')') opp.ExpressionParser .>> ws

        opp.ExpressionParser
        |> between (pchar '{') (pchar '}')

    let qaccount =
        sepBy1 (pword) (pstring ":") |>> Account

    let qcommodity =
        choice [stringLiteral; pword] |>> Commodity

    let qamount =
        let pamount = (pfloat |>> ExprNum) <|> qexpression
        pipe2 (qcommodity .>> spaces1) pamount (fun commodity amount ->
            match amount with
            | ExprNum f -> Amount (commodity, f)
            | expression -> AmountExpression (commodity, expression)
            )

    let qtransaction =
        let withExpr =
            pipe2 (qaccount .>> spaces1) qamount
                (fun account amount -> Trx (account, Some amount))
        let withoutExpr = qaccount |>> (fun a -> Trx (a, None))
        //qaccount .>>. opt (attempt qamount) |>> fun (acc, amOpt) -> Trx (acc, amOpt) // <-- why doesn't this als work?
        (attempt withExpr) <|> withoutExpr

    let qposting =
        let transaction = spaces >>. qtransaction .>> newline
        pstringCI "posting"
        >>. spaces
        >>. pchar '{' .>> spaces
        >>. manyTill transaction (attempt (spaces >>. pchar '}')) |>> Posting

    let qcolumnIdentifier =
        (anyOf ['A' .. 'Z'])
        .>>. pword
        |>> fun (first, rest) -> Column (string first + rest)

    let qfilter =
        let pcolumn = qcolumnIdentifier .>> spaces
        let poperator = qboolop .>> spaces
        let patom = choice [qnumber;qstring;qregex]
        pipe3 pcolumn poperator patom
        <| (fun col op atom -> Filter (col, op, atom))

    let qpayee : Parser<Payee, unit> =
        let isNewline = fun c -> List.contains c ["\n"; "\r"]
        (pchar '#' .>> spaces1)
        >>. manySatisfy (not << isNewline << string)
        |>> Payee

    let qquery =
        let payee = qpayee .>> newline
        let filters = many (between spaces spaces1 qfilter)
        let posting = between spaces spaces qposting
        pipe3 payee filters posting (curry3 Query)

    let qprogram =
        many (qquery .>> spaces) |>> Program

