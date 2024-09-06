namespace TransactionQL.Shared

module Types =
    open System.Runtime.InteropServices
    open System.Runtime.CompilerServices

    type Either<'a, 'b> =
        | Left of 'a
        | Right of 'b

    type Either<'a, 'b> with

        member x.TryGetLeft([<Out>] left: byref<'a>) =
            match x with
            | Left value ->
                left <- value
                true
            | _ -> false

        member x.TryGetRight([<Out>] right: byref<'b>) =
            match x with
            | Right value ->
                right <- value
                true
            | _ -> false

    [<Extension>]
    type OptionExtensions() =
        [<Extension>]
        static member HasValue(this: option<'a>) =
            match this with
            | Some _ -> true
            | None -> false

        static member TryGetValue(this: option<'a>, [<Out>] value: byref<'a>) =
            match this with
            | Some v ->
                value <- v
                true
            | None -> false
