namespace Example.Serialisation.Binary.Tests

open Example.Serialisation

open Example.Serialisation.Binary
open Xunit
open Xunit.Abstractions

type BinaryDeserialiserShould( oh: ITestOutputHelper ) =
    
    let logger =
        let options = { Logging.Options.Default with OutputHelper = Some oh } 
        Logging.CreateLogger options
        
    [<Fact>]
    member this.``BeCreateable`` () = 
        
        let serde = 
            Helpers.DefaultSerde
            
        use ms = 
            new System.IO.MemoryStream()
            
        use serdeStream =
            SerdeStreamWrapper.Make( ms )
            
        use sut =
            BinaryDeserialiser.Make( serde, serdeStream, "test" )
            
        Assert.Equal( "test", sut.TypeName )            