namespace TransactionQL.Shared.Disposables

module Disposables =
    open System
    open System.Globalization

    let private createDisposable f =
        {
            new IDisposable with
                member x.Dispose() = f()
        }

    let changeCulture (culture:string) =
        let current = CultureInfo.CurrentCulture
        CultureInfo.CurrentCulture <- CultureInfo.GetCultureInfo(culture)
        createDisposable(fun () -> CultureInfo.CurrentCulture <- current)

