namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open System
open Example.Serialisation.Json
open Example.Serialisation


type UtilitiesShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    [<Fact>]
    member this.``LookupValidStringComparer`` () =
        Assert.Equal( StringComparer.Ordinal, Utilities.LookupStringComparer StringComparison.Ordinal )

    [<Fact>]
    member this.``ThrowOnInvalidStringComparer`` () =
        Assert.Throws<System.Exception>( fun _ -> Utilities.LookupStringComparer StringComparison.InvariantCultureIgnoreCase |> ignore )
            
    
    


