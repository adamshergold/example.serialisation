namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open System
open Example.Serialisation.Json
open Example.Serialisation


type HelpersShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    [<Fact>]
    member this.``ToJsonThenFromJson`` () =
        
        let serde =
            Serde.Make( SerdeOptions.Default )

        serde.TryRegisterAssembly (typeof<Example.Serialisation.TestTypes.Example.Person>.Assembly) |> ignore
        
        let v =
            Example.Serialisation.TestTypes.Extensions.Person.Examples.[0]
            
        let json =
            Helpers.ToJson serde v
            
        logger.LogInformation( "{Json}", json )
        
        let v' =
            Helpers.FromJson serde json
            
        Assert.Equal( v', v )            






