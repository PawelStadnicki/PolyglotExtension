module PolyglotFacade

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Commands

let send lang code =
    KernelInvocationContext.Current.HandlingKernel.ParentKernel.SendAsync(SubmitCode(code, lang))  
    |> Async.AwaitTask 
    |> ignore

let sendJs code =
    send "javascript" code