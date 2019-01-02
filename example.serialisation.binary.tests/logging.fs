namespace Example.Serialisation.Binary.Tests

open Microsoft.Extensions.Logging 

open Xunit.Abstractions 

open Serilog 

module Logging =

    type Options = {
        Level : string
        ToConsole : bool
        OutputHelper : ITestOutputHelper option
        Template : string 
    }
    with 
        static member Default = {
            Level = "debug"
            ToConsole = false
            OutputHelper = None 
            Template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {Properties}{NewLine}{Exception}"
        }

    let ParseLevel (v:string) = 
        match v.ToLower() with 
        | _ as v when v = "debug" -> Serilog.Events.LogEventLevel.Debug
        | _ -> Serilog.Events.LogEventLevel.Information 
        
    let CreateLogger (options:Options) =         

        let levelSwitch = 
            Serilog.Core.LoggingLevelSwitch() 

        levelSwitch.MinimumLevel <- options.Level |> ParseLevel
        
        let config =
            Serilog.LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)

        let config = 
            if options.OutputHelper.IsSome then  
                config.WriteTo.TestOutput(
                    options.OutputHelper.Value, 
                    levelSwitch.MinimumLevel,
                    outputTemplate = options.Template )
            else 
                config 
                            
        let lf = 
            new LoggerFactory()
                
        lf.AddSerilog( config.CreateLogger() ) |> ignore   
        
        lf.CreateLogger("Test")

