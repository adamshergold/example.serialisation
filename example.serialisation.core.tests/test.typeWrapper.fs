namespace Example.Serialisation.Core.Tests

open Xunit
open Xunit.Abstractions

type TypeWrapperShould( oh: ITestOutputHelper ) =
    
    let logger =
        let options = { Logging.Options.Default with OutputHelper = Some oh } 
        Logging.CreateLogger options
        
    [<Fact>]
    member this.``AllTestsPass`` () = 
        Assert.True(true)
