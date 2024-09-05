namespace TransactionQL.Shared.Extensions

module Extensions =
    open System.Runtime.CompilerServices

    [<Extension>]
    let Or(this: option<'a>, value: 'a) =
        Option.defaultValue value this

    [<Extension>]
    let HasValue(this: option<'a>) =
        Option.isSome this

