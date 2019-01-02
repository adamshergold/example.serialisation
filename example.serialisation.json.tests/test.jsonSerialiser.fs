namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation.Json
open Example.Serialisation


type JsonSerialiserShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    [<Fact>]
    member this.``BeCreateable`` () =
        
        use sut =
            
            let serde =
                Serde.Make()
                
            let serdeStream =
                SerdeStreamWrapper.Make( new System.IO.MemoryStream() )
                
            JsonSerialiser.Make( serde, serdeStream, "json" )
        
        Assert.True( true )







