namespace Example.Serialisation.Core.Tests

open Xunit
open Xunit.Abstractions

open Example.Serialisation

type TypeWrapperShould( oh: ITestOutputHelper ) =
    
    let logger =
        let options = { Logging.Options.Default with OutputHelper = Some oh } 
        Logging.CreateLogger options
        
    [<Fact>]
    member this.``BeCreateable`` () =
        
        let tw =
            TypeWrapper.Make( "json", Some "foo", Array.empty )
            
        Assert.True( tw.ToString().Length > 0 )
        
        Assert.Equal( typeof<TypeWrapper>, (tw :> ITypeSerialisable).Type )
                                
                            
