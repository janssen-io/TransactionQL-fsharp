namespace TransactionQL.Shared

module Types =
    open System.Runtime.InteropServices

    type Either<'a, 'b> = 
        | Left of 'a
        | Right of 'b
    type Either<'a, 'b> with
        member x.TryGetLeft([<Out>] left:byref<'a>) =
          match x with
          | Left value -> left <- value; true
          | _ -> false
        member x.TryGetRight([<Out>] right:byref<'b>) =
          match x with
          | Right value -> right <- value; true
          | _ -> false
                         
