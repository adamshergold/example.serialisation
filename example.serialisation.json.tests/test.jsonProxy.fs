namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation.Json
open Example.Serialisation


type JsonProxyShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    [<Fact>]
    member this.``BeCreateable`` () =
        
        use sut =
            
            let wrapper =
                TypeWrapper.Make( Some "json", "test", Array.empty )
                
            JsonProxy.Make( wrapper )
        
        Assert.True( true )


    [<Fact>]
    member this.``SerialiseDeserialise`` () =
        
        let wrapper =
            let json = "{ \"@type\" : \"foo\", \"name\" : \"john\" }"
            let body = json |> System.Text.Encoding.UTF8.GetBytes
            TypeWrapper.Make( Some "json", "foo", body )
            
        use sut =
            JsonProxy.Make( wrapper )
        
        let serde =
            Serde.Make( SerdeOptions.Default )
            
        use ms =
            new System.IO.MemoryStream()
            
        use outStream =
            SerdeStreamWrapper.Make( ms )
            
        sut |> JsonProxy.BinarySerialiser.Serialise serde outStream
        
        let serialised =
            ms.ToArray()
            
        use inMemStream =
            new System.IO.MemoryStream( serialised )
            
        use inStream =
            SerdeStreamWrapper.Make( inMemStream  )

        let rt =
            JsonProxy.BinarySerialiser.Deserialise serde inStream
        
        Assert.Equal( rt.Wrapper.TypeName, sut.Wrapper.TypeName )
        Assert.Equal( rt.Wrapper.ContentType, sut.Wrapper.ContentType )
        Assert.Equal( rt.Wrapper.Body.Length, sut.Wrapper.Body.Length )






