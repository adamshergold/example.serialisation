namespace Example.Serialisation.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.TestTypes 

type PeekStreamWrapperShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    let sut () =
       
        let ms = 
            new System.IO.MemoryStream() 
            
        let ss = 
            SerialiserStreamWrapper.Make( ms )
        
        BinaryPeekStreamWrapper.Make( ss ) 
        
    [<Fact>]
    member this.``BeCreateable`` () = 
    
        let sut = sut()
                    
        Assert.True( true )                    

        
        