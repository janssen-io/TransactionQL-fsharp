namespace TransactionQL.Parser

module Interpretation =

    type Row = Map<string, string>

    type Env =
        { Variables: Map<string, float>
          EnvVars: Map<string, string>
          Row: Row
          DateFormat: string }

    type Interpretation<'a> = Interpretation of Env * 'a

    let result (Interpretation(_, r)) = r

    let fold
        (eval: (Env -> 'c -> Interpretation<'b>))
        (folder: ('a -> 'b -> 'a))
        (seed: Interpretation<'a>)
        (list: List<'c>)
        : Interpretation<'a> =
        List.fold
            (fun (Interpretation(currentEnv, currentResult)) currentT ->
                let (Interpretation(newEnv, newResult)) = eval currentEnv currentT
                Interpretation(newEnv, folder currentResult newResult))
            seed
            list

    let map
        (f: ('a -> 'b))
        (Interpretation(e, r): Interpretation<'a>)
        : Interpretation<'b> = Interpretation(e, f(r))
