namespace TransactionQL.Shared.Extensions

open System.Runtime.InteropServices

module Extensions =
    open System.Runtime.CompilerServices

    [<Extension>]
    let Or (this : option<'a>, value : 'a) = Option.defaultValue value this

    [<Extension>]
    let HasValue (this : option<'a>) = Option.isSome this

    [<Extension>]
    let TryGetChoice1 (this : Choice<'a, 'b>, [<Out>] value : byref<'a>) =
        match this with
        | Choice1Of2 v ->
            value <- v
            true
        | _ -> false

    [<Extension>]
    let TryGetChoice2 (this : Choice<'a, 'b>, [<Out>] value : byref<'b>) =
        match this with
        | Choice2Of2 v ->
            value <- v
            true
        | _ -> false
