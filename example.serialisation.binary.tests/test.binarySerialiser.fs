namespace Example.Serialisation.Binary.Tests

open Xunit
open Xunit.Abstractions


open Example.Serialisation
open Example.Serialisation.Binary

type BinarySerialiserShould( oh: ITestOutputHelper ) =
    
    let logger =
        let options = { Logging.Options.Default with OutputHelper = Some oh } 
        Logging.CreateLogger options
        
    [<Fact>]
    member this.``BeCreateable`` () = 

        use ms =
            new System.IO.MemoryStream()

        use serdeStream =
            SerdeStreamWrapper.Make( ms )
                        
        use sut =
                
            let serde =
                Serde.Make()
                
            BinarySerialiser.Make( serde, serdeStream, "test" )
            
        Assert.True( true )
