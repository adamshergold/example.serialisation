namespace Example.Serialisation.Core.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.TestTypes
open Example.Serialisation.TestTypes.Extensions 

type AnyShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = { 
            Logging.Options.Default 
                with 
                    OutputHelper = Some oh
                    Level = "trace" 
        }
            
        Logging.CreateLogger options

    [<Fact>]
    member this.``Createable-String`` () =
        
        let v = "Hello"
        
        let sut = Any.Make( v )
            
        Assert.Equal( sut.Value, v )            