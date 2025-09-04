module InterpretationTests

open Xunit
open TransactionQL.Parser.Interpretation

[<Fact>]
let ``map transforms result value while preserving environment`` () =
    let env = 
        { Variables = Map.ofList [("x", 10.0)]
          EnvVars = Map.ofList [("env_var", "test")]
          Row = Map.ofList [("col", "value")]
          DateFormat = "yyyy/MM/dd" }
    
    let interpretation = Interpretation(env, 42)
    let doubled = map (fun x -> x * 2) interpretation
    
    match doubled with
    | Interpretation(resultEnv, resultValue) ->
        Assert.Equal(84, resultValue)
        Assert.True(Map.forall (fun k v -> Map.containsKey k resultEnv.Variables && Map.find k resultEnv.Variables = v) env.Variables)
        Assert.True(Map.forall (fun k v -> Map.containsKey k resultEnv.EnvVars && Map.find k resultEnv.EnvVars = v) env.EnvVars)
        Assert.True(Map.forall (fun k v -> Map.containsKey k resultEnv.Row && Map.find k resultEnv.Row = v) env.Row)
        Assert.Equal(env.DateFormat, resultEnv.DateFormat)