module QLParser
    open FParsec
    open AST

    let curry2 f a b = f (a, b)
    let curry3 f a b c = f (a, b, c)

    let str = pstring
    let pword =
        let islower x = List.contains x ['a' .. 'z']
        let isupper x = List.contains x ['A' .. 'Z']
        let isdigit x = List.contains x ['0' .. '9']
        many1Satisfy (fun c -> islower c || isupper c || isdigit c)

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
        let escapedCharSnippet = str "\\" >>. escape
        let normalCharSnippet  = manySatisfy (fun c -> c <> betweenChar && c <> '\\')

        let surrounder = betweenChar.ToString()
        between (str surrounder) (str surrounder)
                (stringsSepBy normalCharSnippet escapedCharSnippet)

    let stringLiteral = escapedBetween '"' stringEscape
    let qstring = stringLiteral |>> String

    let regexLiteral : Parser<string, unit> = escapedBetween '/' regexEscape
    let qregex = regexLiteral |>> Regex

    let qboolop =
        let qequals = stringReturn "=" Equals
        let qnotEqual = stringReturn "/=" NotEquals
        let qlte = stringReturn "<=" LessThanOrEqualTo
        let qgte = stringReturn ">=" GreaterThanOrEqualTo
        let qlt = stringReturn "<" LessThan
        let qgt = stringReturn ">" GreaterThan
        let qcontains = stringReturn "contains" Substring
        let qmatches = stringReturn "matches" Matches
        choice [ qnotEqual; qequals; qgte; qgt; qlte; qlt; qcontains; qmatches; ];

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

    let paccount =
        sepBy1 (pword) (pstring ":")

    let qaccount = paccount |>> Account

    let qcommodity =
        choice [stringLiteral; pword] |>> Commodity

    let qamount =
        let pamount = (pfloat |>> ExprNum) <|> qexpression
        pipe2 (qcommodity .>> spaces1) pamount (fun commodity amount ->
            match amount with
            | ExprNum f -> Amount (commodity, f)
            | expression -> AmountExpression (commodity, expression)
            )

    // TODO: how to handle ws more gracefully
    let qtransaction =
        let withExpr =
            pipe2 (qaccount .>> spaces1) qamount
                (fun account amount -> Trx (account, Some amount))
        let withoutExpr = qaccount |>> (fun a -> Trx (a, None))
        //qaccount .>>. opt (attempt qamount) |>> fun (acc, amOpt) -> Trx (acc, amOpt) // <-- why doesn't this als work?
        (attempt withExpr) <|> withoutExpr

    let qposting =
        let transactions = many (qtransaction .>> spaces)
        pstringCI "posting"
        >>. spaces
        >>. between
            (pchar '{' >>. spaces) (spaces .>> pchar '}')
            transactions |>> Posting

    let qcolumnIdentifier =
        (anyOf ['A' .. 'Z'])
        .>>. pword
        |>> fun (first, rest) -> Column (string first + rest)

    let qfilter =
        let pcolumn = qcolumnIdentifier .>> spaces
        let poperator = qboolop .>> spaces
        let patom = choice [qnumber;qstring;qregex]
        pipe3 pcolumn poperator patom
        <| (fun col op atom -> op (col, atom))

    let qpayee : Parser<Payee, unit> =
        let isNewline = fun c -> List.contains c ["\n"; "\r"]
        (pchar '#' .>> spaces1)
        >>. manySatisfy (not << isNewline << string)
        |>> Payee

    let qdescription =
        let payee = qpayee .>> newline
        let filters = many (between spaces spaces1 qfilter)
        let posting = between spaces spaces qposting
        pipe3 payee filters posting (curry3 Description)

