namespace Example.Serialisation.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.Tests
open Example.Serialisation.TestTypes

type HelpersShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    let serialiser () =
        let options = { SerdeOptions.Default with Logger = Some logger }
        let serialiser = Serde.Make( options )
        let nRegistered = serialiser.TryRegisterAssembly typeof<Example.Person>.Assembly
        logger.LogInformation("Registered {Number} types", nRegistered )
        serialiser
        
    [<Fact>]
    member this.``SerialiseDeserialise`` () = 
        
        let serialiser =   
            serialiser() 
            
        let p = 
            Example.Serialisation.TestTypes.Extensions.Person.Examples.[0]
                       
        let bytes = 
            p |> Helpers.Serialise serialiser (Some "json") 
        
        Assert.True( bytes.Length > 0  )
        
        let p' = 
            Helpers.DeserialiseT<_> serialiser (Some "json") bytes 
         
        Assert.Equal( p, p' )
         
//        use wrapper =
//            SerialiserStreamWrapper.Make( new System.IO.MemoryStream( bytes ) )
//              
//        let rt = serialiser.DeserialiseT<Klarity.Serialisation.TestTypes.Example.Person> (Some "json") wrapper
//        
//        Assert.True( true )       
        
    
        
        