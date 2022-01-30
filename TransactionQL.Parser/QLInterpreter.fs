namespace TransactionQL.Parser

open System.Globalization
open System.Text.RegularExpressions

module QLInterpreter = 

    open AST
    open Interpretation

    type Header = Header of System.DateTime * string
    type Line = Line of Account * (Commodity * float) option
    type Entry = // Entry of Header * Line list
        { Header: Header
          Lines: Line list
          Comments: string list }

    let rec eval (env : Env) (expr : Expression)  =
        let rec eval' e =
            let arithmetic op (l, r) = op (eval' l) (eval' r)
            match e with
            | Variable var -> 
                if env.Variables.ContainsKey var then
                    Map.find var env.Variables
                else
                    failwith <| sprintf "Unknown variable '%s'" var
            | ExprNum number  -> number
            | Add (l, r)      -> arithmetic (+) (l, r)
            | Subtract (l, r) -> arithmetic (-) (l, r)
            | Multiply (l, r) -> arithmetic (*) (l, r)
            | Divide (l, r)   -> arithmetic (/) (l, r)

        eval' expr
        |> fun n -> Interpretation (env, n)

    let generatePostingLine env (Trx (Account accounts, am)) =
        let remainder = Map.find "remainder" env.Variables

        let getCommodity amount =
            match amount with
            | Amount (Commodity c, _) -> c
            | AmountExpression (Commodity c, _) -> c

        let evalAmount amount =
            match amount with
            | Amount (Commodity _, f) -> f
            | AmountExpression (Commodity _, e) -> 
                Interpretation.result (eval env e)

        let commodity = Option.map getCommodity am
        let amount = Option.map evalAmount am

        match commodity, amount with
        | Some c, Some f ->
            let vars = Map.add "remainder" (remainder + f) env.Variables
            { env with Variables = vars }, Line (Account accounts, Some (Commodity c, f))
        | _ ->
            let vars = Map.add "remainder" 0.0 env.Variables // update remainder to 0
            { env with Variables = vars }, Line (Account accounts, None)
        |> Interpretation

    let generatePosting env (Posting p) =
        let rec gen env posting acc =
            match posting with
            | [] -> Interpretation (env, acc)
            | x :: xs -> 
                let (Interpretation (newEnv, line)) = generatePostingLine env x
                gen newEnv xs (line :: acc)

        let envWithRemainder = { env with Variables = Map.add "remainder" 0.0 env.Variables }
        let (Interpretation (env', lines)) = gen envWithRemainder p []
        Interpretation (env', List.rev lines)

    let rec evalString text (column:string) op =
        match op with
        | EqualTo -> column = text
        | NotEqualTo -> not <| evalString text column EqualTo
        | Contains -> column.Contains text
        | _ -> failwith (sprintf "Operator '%A' is not supported for strings." op)

    let evalRegex regex column op =
        match op with
        | Matches -> Regex(regex, RegexOptions.IgnoreCase).IsMatch(column)
        | _ -> failwith (sprintf "Operator '%A' is not supported for regular expressions." op)

    let rec evalNumber number column op =
        let value = float column
        match op with
        | EqualTo                   -> number = value
        | NotEqualTo                -> not <| evalNumber number column EqualTo
        | GreaterThan               -> value > number
        | GreaterThanOrEqualTo      -> value >= number
        | LessThan                  -> value < number
        | LessThanOrEqualTo         -> value <= number
        | _ -> failwith (sprintf "Operator '%A' is not supported for numbers." op)

    let rec evalFilter env filter =
        match filter with
        | (Filter (Column col, op, atom)) ->
            let value = 
                if Map.containsKey col env.Row then
                    Map.find col env.Row
                else 
                    failwith <| sprintf "Invalid column: '%s'" col

            let evalType =
                match atom with
                | String text -> evalString text
                | Number number -> evalNumber number
                | RegExp regex -> evalRegex regex
            Interpretation (env, evalType value op)

        | OrGroup filters -> Interpretation.fold evalFilter (||) (Interpretation (env, false)) filters
        
    let evalQuery env (Query (Payee payee, filters, posting)) =
        let (Interpretation (envFilter, isMatch)) =
            Interpretation.fold evalFilter (&&) (Interpretation (env, true)) filters
        if not <| isMatch then
            Interpretation (envFilter, None)
        else
            let total = float <| Map.find "Amount" envFilter.Row
            let envTotal = 
                { envFilter with 
                    Variables = 
                        envFilter.Variables
                        |> Map.add "amount" total
                        |> Map.add "total" (abs total)  }
            let (Interpretation (envPosting, postingLines)) = generatePosting envTotal posting

            let date = System.DateTime.ParseExact(Map.find "Date" env.Row, env.DateFormat, CultureInfo.InvariantCulture)
            let header = Header (date, payee)
            let comments = 
                [ if Map.containsKey "Description" envFilter.Row
                  then Map.find "Description" envFilter.Row ]

            Interpretation (envPosting, Some { Header = header; Lines = postingLines; Comments = comments })


    let rec evalProgram env (Program queries) =
        match queries with
        | (q :: qs) -> 
            let (Interpretation (env', entry)) = evalQuery env q
            match entry with
            | Some entry ->
                Interpretation (env', Some entry)
            | None ->
                evalProgram env' (Program qs)
        | [] -> Interpretation (env, None)
