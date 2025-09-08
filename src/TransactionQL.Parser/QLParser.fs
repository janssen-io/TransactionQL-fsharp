namespace TransactionQL.Parser

open System

module QLParser =
    open FParsec
    open AST

    let curry2 f a b = f (a, b)
    let curry3 f a b c = f (a, b, c)

    let maybe p = (attempt >> opt) p <|> preturn None
    let either p q = attempt p <|> q

    let pword =
        let islower x = List.contains x [ 'a' .. 'z' ]
        let isupper x = List.contains x [ 'A' .. 'Z' ]
        let isdigit x = List.contains x [ '0' .. '9' ]
        many1SatisfyL (fun c -> islower c || isupper c || isdigit c) "word (a-Z0-9)"

    let qnumber = pfloat |>> Number

    let regexEscape =
        anyOf @"{}[]/()?*+\"
        |>> function
            | '/' -> "/"
            | c -> "\\" + string c

    let stringEscape =
        anyOf "\"\\bfnrt"
        |>> function
            | 'b' -> "\b"
            | 'f' -> "\u000C"
            | 'n' -> "\n"
            | 'r' -> "\r"
            | 't' -> "\t"
            | c -> string c // every other char is mapped to itself

    let escapedBetween (betweenChar: char) escape =
        let escapedCharSnippet = pstring "\\" >>. escape
        let normalCharSnippet = manySatisfy (fun c -> c <> betweenChar && c <> '\\')

        let surrounder = betweenChar.ToString()
        between (pstring surrounder) (pstring surrounder) (stringsSepBy normalCharSnippet escapedCharSnippet)

    let stringLiteral = escapedBetween '"' stringEscape
    let qstring = stringLiteral <?> "String literal" |>> String

    let regexLiteral: Parser<string, unit> = escapedBetween '/' regexEscape
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
        choice [ qnotEqual; qequals; qgte; qgt; qlte; qlt; qcontains; qmatches ]

    let qexpression =
        let opp = OperatorPrecedenceParser<Expression, unit, unit>()
        let ws = spaces
        let paren, parenRef = createParserForwardedToRef ()
        let qexprNum = pfloat .>> ws |>> ExprNum
        let qvariable = pword .>> ws |>> Variable

        opp.TermParser <- choice [ paren; qexprNum; qvariable ]
        opp.AddOperator(InfixOperator("+", ws, 1, Associativity.Left, curry2 Add))
        opp.AddOperator(InfixOperator("-", ws, 1, Associativity.Left, curry2 Subtract))
        opp.AddOperator(InfixOperator("*", ws, 2, Associativity.Left, curry2 Multiply))
        opp.AddOperator(InfixOperator("/", ws, 2, Associativity.Left, curry2 Divide))

        parenRef := between (pchar '(') (pchar ')') opp.ExpressionParser .>> ws

        opp.ExpressionParser |> between (pchar '(') (pchar ')')

    let readUntil terminator =
        manySatisfy (fun c -> c <> terminator)

    let qmetadata =
        pstring "metadata" >>. spaces1 >>. readUntil ' ' .>> spaces .>> pchar '=' .>> spaces .>>. readUntil '\n'
        |>> Metadata

    let qaccount = 
        let qliteral = sepBy1 pword (pstring ":") |>> id
        let qvariable = 
            pchar '(' >>. qliteral .>> pchar ')' |>> fun n -> String.Join(":", n)
        choice [qliteral |>> AccountLiteral; qvariable |>> AccountVariable]
            

    let qcommodity = choice [ stringLiteral; pword ] |>> Commodity

    let qamount =
        let pamount = either (pfloat |>> ExprNum) qexpression

        pipe2 (qcommodity .>> spaces1) pamount (fun commodity amount ->
            match amount with
            | ExprNum f -> Amount(commodity, f)
            | expression -> AmountExpression(commodity, expression))

    let qtag: Parser<string, unit> = pstring "; " >>. readUntil '\n'

    let qtransaction =
        let maybeTag = maybe (spaces1 >>. qtag)
        let maybeAmount = maybe (spaces1 >>. qamount)

        pipe3 qaccount maybeAmount maybeTag (fun account amount tag ->
            { Account = account
              Amount = amount
              Tags = match tag with | None -> [||] | Some t -> [|t|] })

    let qnote = pstringCI "note " >>. readUntil '\n'

    let qposting =
        let transaction = spaces >>. qtransaction .>> newline
        let note = spaces >>. qnote

        pstringCI "posting" >>. spaces >>. pchar '{' .>> spaces >>. maybe note
        .>>. manyTill transaction (spaces >>? pchar '}')
        |>> (fun (note, trx) -> Posting(note, trx))

    let qcolumnIdentifier =
        attempt (
            anyOf [ 'A' .. 'Z' ] .>>. pword
            |>> fun (first, rest) -> Column(string first + rest)
        )
        <|> (anyOf [ 'A' .. 'Z' ] |>> (string >> Column))

    let qfilter: Parser<Filter, unit> =
        let pcolumn = qcolumnIdentifier .>> spaces
        let poperator = qboolop .>> spaces
        let patom = choice [ qnumber; qstring; qregex ]

        let pfilter =
            pipe3 pcolumn poperator patom <| fun col op atom -> Filter(col, op, atom)

        let por = spaces >>. pstring "or" .>> spaces
        let orGroup, orGroupRef = createParserForwardedToRef ()

        orGroupRef.Value <- 
            attempt 
            <| pipe3 pfilter por orGroup (fun f _ g -> OrGroup [ f; g ])
            <|> pfilter

        orGroup

    /// Skip zero or more literal spaces
    let pspace = skipMany (pchar ' ')

    let qcolumnToken = pchar '@' >>. qcolumnIdentifier |>> ColumnToken

    let qpayee: Parser<Payee, unit> =
        let isWhitespaceOrVar = fun c -> List.contains c [ "\n"; "\r"; "\t"; " "; "@" ]
        let ptext = manySatisfy (not << isWhitespaceOrVar << string)
        let payeeParts = either qcolumnToken (ptext |>> Word) .>>? pspace
        let pinterpolation = many1Till payeeParts newline |>> Interpolation
        pchar '#' .>> spaces1 >>. pinterpolation

    let qquery =
        let filters = many (between spaces spaces1 qfilter)
        let posting = between spaces spaces qposting
        pipe3 qpayee filters posting (curry3 Query)

    let qprogram = many qmetadata >>. many (qquery .>> spaces)

    let parse = run qprogram
