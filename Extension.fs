namespace PolyglotExtension.Commands

open System.Threading.Tasks
open Microsoft.AspNetCore.Html
open Microsoft.DotNet.Interactive

type FableKernelExtension() =

    interface IKernelExtension with 
 
        member this.OnLoadAsync(kernel:Kernel) : Task = 
            
            // run code on  extension load
            PolyglotFacade.send  "javascript" StaticCode.pyodideAndTurfLoader

            // run custom code on demand (via magic command)
            LoadPackage.code |> kernel.AddDirective
            FablePython.code |> kernel.AddDirective

            match KernelInvocationContext.Current.HandlingKernel with   
            | null -> ()
            | x -> 

                HtmlString($@"<code>Polyglot extension -> .net/js/python playground. </code>")
                |> KernelInvocationContext.Current.Display 
                |> ignore

            Task.CompletedTask