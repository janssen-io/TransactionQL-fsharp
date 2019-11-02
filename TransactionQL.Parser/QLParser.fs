module QLParser
    open FParsec
    open AST

    let str s = pstring s
    let pword s = 
        let islower x = List.contains x ['a' .. 'z']
        let isupper x = List.contains x ['A' .. 'Z']
        let isdigit x = List.contains x ['0' .. '9']
        many1Satisfy (fun c -> islower c || isupper c || isdigit c) s

    let qword s = pword |>> Word <| s
    let qnumber s = pfloat |>> Number <| s

    let regexEscape s =
        anyOf @"{}[]/()?*+\"
        |>> function
              | '/' -> "/"
              | c   -> "\\" + string c
        <| s

    let stringEscape s =
        anyOf  "\"\\bfnrt"
        |>> function
              | 'b' -> "\b"
              | 'f' -> "\u000C"
              | 'n' -> "\n"
              | 'r' -> "\r"
              | 't' -> "\t"
              | c   -> string c // every other char is mapped to itself
        <| s

    let escapedBetween (betweenChar:char) escape s =
        let escapedCharSnippet = str "\\" >>. escape
        let normalCharSnippet  = manySatisfy (fun c -> c <> betweenChar && c <> '\\')
    
        let surrounder = betweenChar.ToString()
        between (str surrounder) (str surrounder)
                (stringsSepBy normalCharSnippet escapedCharSnippet)
                s

    let stringLiteral s = escapedBetween '"' stringEscape s
    let qstring s = stringLiteral |>> String <| s

    let regexLiteral s = escapedBetween '/' regexEscape s
    let qregex s = regexLiteral |>> Regex <| s

    let qarithop s = 
        let qadd s = stringReturn "+" Add s
        let qsubstract s = stringReturn "-" Subtract s
        let qmultiply s = stringReturn "*" Multiply s
        let qdivide s = stringReturn "/" Divide s
        choice [qadd; qsubstract; qmultiply; qdivide] s

    let qboolop s =
        let qequals s = stringReturn "=" Equals s
        let qnotEqual s = stringReturn "/=" NotEquals s
        let qlte s = stringReturn "<=" LessThanOrEqualTo s
        let qgte s = stringReturn ">=" GreaterThanOrEqualTo s
        let qlt s = stringReturn "<" LessThan s
        let qgt s = stringReturn ">" GreaterThan s
        let qcontains s = stringReturn "contains" Substring s
        let qmatches s = stringReturn "matches" Matches s
        choice [ qnotEqual; qequals; qgte; qgt; qlte; qlt; qcontains; qmatches; ] s;

    // TODO: ignore whitespace
    let qexpression =
        let exprBuilder op lh rh = Expression (lh, op ,rh)
        let opp = OperatorPrecedenceParser<Expression, unit, unit> ()
        let ws = spaces
        let qexprNum = pfloat |>> ExprNum
        let qexprWord = pword |>> ExprWord

        opp.TermParser <- choice [qexprNum; qexprWord]
        opp.AddOperator(InfixOperator("+", ws, 1, Associativity.Left, exprBuilder Add))
        opp.AddOperator(InfixOperator("-", ws, 1, Associativity.Left, exprBuilder Subtract))
        opp.AddOperator(InfixOperator("*", ws, 2, Associativity.Left, exprBuilder Multiply))
        opp.AddOperator(InfixOperator("/", ws, 2, Associativity.Left, exprBuilder Divide))

        opp.ExpressionParser
        |> between (pchar '{') (pchar '}')

    let qaccount s =
        sepBy1 (pword) (pstring ":") |>> Account <| s

    let qcommodity s =
        choice [stringLiteral; pword] |>> Commodity <| s

    // TODO: how to handle ws more gracefully
    let qtransaction = 
        pipe5 qaccount spaces1 qcommodity spaces1 qexpression (fun a _ c _ e -> (a, c, e))




