namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation.TestTypes

open Example.Serialisation.Json
open Example.Serialisation


type JsonSerialiserShould( oh: ITestOutputHelper ) = 

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
                
            JsonSerialiser.Make( serde, serdeStream, "json" )
        
        Assert.True( true )

    [<Fact>]
    member this.``CanUseDifferentTypeProperties`` () =
        
        let options = {
            SerdeOptions.Default
                with TypeProperty = "_type"
            }

        let sut = sut options            
            
        let v =
            Example.Address.Make( 1, "High Street", Example.Region.North )
            
        let json = Helpers.ToJson sut v
        
        logger.LogInformation( "{JSON}", json )
                               
        Assert.Contains( "_type", json )
        
        let rt = json |> Helpers.FromJson sut
        
        Assert.Equal( v, rt )






