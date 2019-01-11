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
        Assert.Equal( Any.Make( v ).Value, v )
        
    [<Fact>]
    member this.``Createable-Int32`` () =
        
        let v = 123
        Assert.Equal( Any.Make( v ).Value, v )
        
    [<Fact>]
    member this.``Createable-Int64`` () =
        
        let v = 123L
        Assert.Equal( Any.Make( v ).Value, v )
        
    [<Fact>]
    member this.``Createable-Double`` () =
        
        let v = 123.456
        Assert.Equal( Any.Make( v ).Value, v )                                                