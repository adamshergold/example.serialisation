namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation.Json
open Example.Serialisation


type JsonTextWriterStreamWrapper( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    [<Fact>]
    member this.``BeCreateable`` () =
        
        use sut =
            use ms = new System.IO.MemoryStream() 
            let input = SerdeStreamWrapper.Make( ms )
            JsonTextWriterStreamWrapper.Make( input )
        
        Assert.True( true )







