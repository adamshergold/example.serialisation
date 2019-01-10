namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Example.Serialisation.Json
open Newtonsoft.Json
open Xunit
open Xunit.Abstractions 

type PropertyHandlerShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    [<Fact>]
    member this.``BeCreateable`` () =
        
        let sut =
            let tr = new System.IO.StringReader("")
            let reader = PeekReader.Make( 0, new JsonTextReader(tr))
            PropertyHandler.Make( reader, System.StringComparison.Ordinal )
        
        Assert.False( sut.Has "foo" )
            






