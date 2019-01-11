namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation.Json
open Example.Serialisation

open Example.Serialisation.TestTypes

type JsonDeserialiserShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    let sut options =
        let sut = Serde.Make( options )
        sut.TryRegisterAssembly typeof<Example.Address>.Assembly |> ignore
        sut
    
    [<Fact>]
    member this.``BeCreateable`` () =
        
        use sut =
            
            let serde =
                Serde.Make()
                
            let serdeStream =
                SerdeStreamWrapper.Make( new System.IO.MemoryStream() )
                
            JsonDeserialiser.Make( serde, serdeStream, "json", "test" )
        
        Assert.True( true )


    [<Fact>]
    member this.``CanBeCaseInsensitiveWithPropertyNames`` () =
        
        let options = {
            SerdeOptions.Default
                with
                    TypeProperty = "$type"
            }

        let sut = sut options            
            
        let json = """ { "$type" : "Example.Address", "number" : 1, "Street" : "High Street", "Region" : "North" } """
        
        logger.LogInformation( "{JSON}", json )
        
        let rt = json |> Helpers.FromJson<Example.Address> sut
        
        Assert.True( true )

    [<Fact>]
    member this.``CanDeserialiseAndIgnoreUnknownProperties`` () =
        
        let options = {
            SerdeOptions.Default
                with
                    TypeProperty = "$type"
            }

        let sut = sut options            
            
        let json = """ { "$type" : "Example.Address", "Number" : 1, "Foo" : 123, "Street" : "High Street", "Region" : "North" } """
        
        logger.LogInformation( "{JSON}", json )
        
        let rt = json |> Helpers.FromJson<Example.Address> sut
        
        Assert.True( true )
        
        
    [<Fact>]
    member this.``FailsToDeserialiseWithInlineTypeMismatchIfStrict`` () =
        
        let options = {
            SerdeOptions.Default
                with
                    StrictInlineTypeCheck = true
            }

        let sut = sut options            
            
        let json = """ { "$type" : "Example.Addr", "Number" : 1, "Street" : "High Street", "Region" : "North" } """
        
        logger.LogInformation( "{JSON}", json )
        
        let tester () =
            json |> Helpers.FromJson<Example.Address> sut |> ignore
            
        Assert.Throws<System.Exception>( tester )
        
        
    [<Fact>]
    member this.``DeserialiseIfInlineMismatchWhenInNonStrictMode`` () =
        
        let options = {
            SerdeOptions.Default
                with
                    StrictInlineTypeCheck = false
            }

        let sut = sut options            
            
        let json = """ { "@type" : "Example.Addr", "Number" : 1, "Street" : "High Street", "Region" : "North" } """
        
        logger.LogInformation( "{JSON}", json )
        
        let v =
            json |> Helpers.FromJson<Example.Address> sut 
            
        Assert.Equal( "High Street", v.Street )        
        

    [<Fact>]
    member this.``DeserialiseIfNoInlineTypePresent`` () =
        
        let options = {
            SerdeOptions.Default
                with
                    StrictInlineTypeCheck = false
            }

        let sut = sut options            
            
        let json = """ { "Number" : 1, "Street" : "High Street", "Region" : "North" } """
        
        logger.LogInformation( "{JSON}", json )
        
        let v =
            json |> Helpers.FromJson<Example.Address> sut 
            
        Assert.Equal( "High Street", v.Street )        



