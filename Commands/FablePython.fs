namespace PolyglotExtension.Commands

open System
open System.CommandLine.NamingConventionBinder
open System.CommandLine
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Threading.Tasks

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Commands

type FablePythonParams (context) =
        member val Context: KernelInvocationContext = context

module FablePython =

    let display value =
        KernelInvocationContext.Current.DisplayAs(value, "text/plain") |> ignore

    let code =
        
        let commandName = "#!fable.python"

        let sourceFsFileName = "fsharp.fsx"
        let sourceFsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "fsharp.fsx")
        let targetPyFilePath = Path.Combine(Directory.GetCurrentDirectory(), "fsharp.py")

        let cmd = Command(commandName)
    
        cmd.Handler <- 
            fun (opt:FablePythonParams) ->

                let command = opt.Context.Command :?> SubmitCode
            
                let stdOutBuffer = StringBuilder()
                let stdErrBuffer = StringBuilder()

                // get rid of magic command line, take only F# code

                let lines = 
                    Regex.Split(command.Code, "\r\n|\r|\n") 
                    |> Array.filter ( fun x -> not (x.StartsWith(commandName)))

                let code = String.Join(Environment.NewLine, lines)

                // run fable-py (must be globally installed) 

                async {

                    do! File.WriteAllTextAsync(sourceFsFilePath, code) |> Async.AwaitTask

                    let! fable = 
                        CliWrap.Cli
                            .Wrap($"fable-py")
                            .WithArguments(sourceFsFileName)
                            .WithStandardOutputPipe(CliWrap.PipeTarget.ToStringBuilder(stdOutBuffer))
                            .WithStandardErrorPipe(CliWrap.PipeTarget.ToStringBuilder(stdErrBuffer))
                            .WithWorkingDirectory(Directory.GetCurrentDirectory())
                            .ExecuteAsync().Task 
                            |> Async.AwaitTask 

                    //display result 

                    let! content = File.ReadAllTextAsync(targetPyFilePath) |> Async.AwaitTask
                    PolyglotFacade.send  "javascript" $"""console.log("From python: " + python(`{content}`));"""
                } |> Async.RunSynchronously
                

                // premature command completion will prevent F# result to be displayed in VS Code (as we only want python result)
                // is there a better way to achieve it ?
                opt.Context.Complete(opt.Context.Command)

                Task.CompletedTask
            |> CommandHandler.Create
        cmd