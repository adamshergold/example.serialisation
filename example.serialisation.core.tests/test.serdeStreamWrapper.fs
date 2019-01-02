namespace Example.Serialisation.Core.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.TestTypes 

type SerdeStreamWrapperShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options
        
    [<Fact>]
    member this.``BeCreateable`` () = 
    
        use ms = 
            new System.IO.MemoryStream() 
            
        let sut = 
            SerdeStreamWrapper.Make( ms ) 
            
        Assert.True( true )                    

        
        