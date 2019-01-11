namespace Example.Serialisation.Core.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.Core.Tests
open Example.Serialisation.TestTypes

type HelpersShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    let serde () =
        
        let options =
            { SerdeOptions.Default with Logger = Some logger }
            
        let serde =
            Serde.Make( options )
            
        let nRegistered =
            serde.TryRegisterAssembly typeof<Example.Person>.Assembly
            
        serde
        
    [<Fact>]
    member this.``SerialiseThenDeserialise`` () = 
        
        let serde =   
            serde() 
            
        let p = 
            Example.Serialisation.TestTypes.Extensions.Person.Examples.[0]
                       
        let bytes = 
            p |> Helpers.Serialise serde "json" 
        
        Assert.True( bytes.Length > 0  )

        let p' =
            Helpers.Deserialise serde "json" "Example.Person" bytes
            
        Assert.Equal( p', p )
        
        
    [<Fact>]
    member this.``DeserialiseT`` () = 
        
        let serde =   
            serde() 
            
        let p = 
            Example.Serialisation.TestTypes.Extensions.Person.Examples.[0]
                       
        let bytes = 
            p |> Helpers.Serialise serde "json" 
        
        Assert.True( bytes.Length > 0  )

        let p' =
            Helpers.DeserialiseT<_> serde "json" bytes
            
        Assert.Equal( p', p )    
        
    [<Fact>]
    member this.``WrapUnwrap`` () = 
        
        let serde =   
            serde() 
            
        let p = 
            Example.Serialisation.TestTypes.Extensions.Person.Examples.[0]
                       
        let wrapped =
            Helpers.Wrap serde p (["json"])
            
        let unwrapped =
            wrapped |> Helpers.Unwrap serde
            
        Assert.Equal( unwrapped, p )            
        
        