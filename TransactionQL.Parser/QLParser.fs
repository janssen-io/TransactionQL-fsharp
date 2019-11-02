module QLParser
    open FParsec
    open AST

    let str s = pstring s
    let islower x = List.contains x ['a' .. 'z']
    let isupper x = List.contains x ['A' .. 'Z']
    let isdigit x = List.contains x ['0' .. '9']
    let pword s = 
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

    let qadd s = stringReturn "+" Add <| s
    let qsubstract s = stringReturn "-" Subtract <| s
    let qmultiply s = stringReturn "*" Multiply <| s
    let qdivide s = stringReturn "/" Divide <| s
    let qarithop s = choice [qadd; qsubstract; qmultiply; qdivide] <| s

    let qequals s = stringReturn "=" Equals <| s
    let qnotEqual s = stringReturn "/=" NotEquals <| s
    let qgt s = stringReturn "=" GreaterThan <| s
    let qgte s = stringReturn "=" GreaterThanOrEqualTo <| s
    let qlt s = stringReturn "=" LessThan <| s
    let qlte s = stringReturn "=" LessThanOrEqualTo <| s
    let qcontains s = pstring "contains" >>. qstring |>> Substring <| s
    let qmatches s = pstring "matches" >>. qregex |>> Matches <| s

    let pboolop s = 
        ["="; "/="; "<="; ">="; "<"; ">"] 
        |> List.map pstring 
        |> choice 
        <| s

