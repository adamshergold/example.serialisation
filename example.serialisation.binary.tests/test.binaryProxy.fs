namespace Example.Serialisation.Binary.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.Binary

type BinaryProxyShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options
        
    let serialiser (vs:seq<obj>) =
        let options = { SerdeOptions.Default with Logger = Some logger }
        let s = Serde.Make( options )
        vs |> Seq.iter ( fun v -> s.TryRegister v |> ignore ) 
        s


    [<Fact>]
    member this.``BeCreateable`` () =
        
        let wrapper =
            TypeWrapper.Make( "binary", Some "test", Array.empty )
            
        let sut =
            BinaryProxy.Make( wrapper )
            
        Assert.Equal( Some "test", sut.Wrapper.TypeName )
        
        Assert.Equal( typeof<BinaryProxy>, (sut :> ITypeSerialisable).Type )
        

        
        