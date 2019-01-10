namespace Example.Serialisation.Core.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.TestTypes
open Example.Serialisation.TestTypes.Extensions 

type SerdeShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = { 
            Logging.Options.Default 
                with 
                    OutputHelper = Some oh
                    Level = "trace" 
        }
            
        Logging.CreateLogger options

    let sut () =
    
        let sut = 
            let options = {
                SerdeOptions.Default 
                    with Logger = Some logger }
                    
            Serde.Make( options ) 
            
        sut.TryRegister Example.Serialisation.Json.Serialisers.AnyJsonSerialiser |> ignore
        sut.TryRegister Example.Serialisation.Binary.Serialisers.AnyBinarySerialiser |> ignore
        
        sut
        
      
    [<Fact>]
    member this.``BeCreateable`` () =
    
        let sut = 
            Serde.Make()
            
        Assert.Equal( 0, sut.Items |> Seq.length )
        
        
    [<Fact>]
    member this.``RegisterByReflection`` () =
    
        let sut = 
            Serde.Make( )
            
        let nItems =             
            sut.TryRegisterAssembly typeof<Example.Serialisation.TestTypes.Example.Person>.Assembly
            
        Assert.True( nItems > 0 )

        Assert.Equal( nItems, sut.Items |> Seq.length )                    

    [<Fact>]
    member this.``TryLookupByTypeName`` () =
    
        let sut = 
            Serde.Make( )
            
        let nItems =             
            sut.TryRegisterAssembly typeof<Example.Serialisation.TestTypes.Example.Person>.Assembly
            
        Assert.True( nItems > 0 )

        Assert.Equal( nItems, sut.Items |> Seq.length )                    
                    
          
        
        
        
        