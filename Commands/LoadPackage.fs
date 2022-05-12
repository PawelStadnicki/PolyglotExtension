namespace PolyglotExtension.Commands

open System.Threading.Tasks
open System.CommandLine.NamingConventionBinder
open System.CommandLine
open Microsoft.DotNet.Interactive


type LoadPackageParams (name, import, context) =
        member val Context: KernelInvocationContext = context
        member val Name: string = name
        member val Import: string = import

module LoadPackage =
    let code =
        let cmd = Command("#!loadPackage")

        cmd.AddOption(new Option<string>("--name", "name of the package, that is available from pyodide"))
        cmd.AddOption(new Option<string>("--import", "name of the package, that is not available from pyodide"))

        cmd.Handler <- 
            fun (opt:LoadPackageParams) ->

                if(System.String.IsNullOrWhiteSpace opt.Import) then 
                    $"""await pyodide.loadPackage(['{opt.Name}'], m => console.log(m), e => console.log(e), r => console.log(r));"""
                    |> PolyglotFacade.sendJs
                    
                else
                    $"""
                    await pyodide.runPythonAsync(`
                    import micropip
                    micropip.install('{opt.Name}') 
                    package_list = micropip.list()
                    "{opt.Name}" in package_list
                    `);"""
                    |> PolyglotFacade.sendJs

                Task.CompletedTask
            |> CommandHandler.Create
        cmd