namespace Example.Serialisation.Tests

open Xunit
open Xunit.Abstractions

open Example.Serialisation


type BinarySerialiserShould( oh: ITestOutputHelper ) =
    
    let logger =
        let options = { Logging.Options.Default with OutputHelper = Some oh } 
        Logging.CreateLogger options
        
//    [<Fact>]
//    member this.``BeAbleToSerialiseByteArray`` () = 
//        
//        let serialiser = 
//            Helpers.DefaultSerialiser 
//            
//        use ms = 
//            new System.IO.MemoryStream()
//            
//        use wrapper = 
//            SerialiserStreamWrapper.Make( ms ) 
//                                    
//        let sut = 
//            BinarySerialiser.Make( serialiser, wrapper, "", Some "binary" )
//
//        let vs = Array.init 10 ( fun _ -> (byte)0 )
//        
//        sut.Write( vs )
//
//        sut.Dispose() 
//                
//        // some padding put in by the serialiser so greater than 10                
//        Assert.True( ms.ToArray().Length > 10 )            
            
