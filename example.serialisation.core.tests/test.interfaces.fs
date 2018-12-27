namespace Example.Serialisation.Core.Tests

open Xunit
open Xunit.Abstractions

type InterfacesShould( oh: ITestOutputHelper ) =

    [<Fact>]
    member this.``AllTestsPass`` () = 
        Assert.True(true)
