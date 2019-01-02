namespace Example.Serialisation.Json.Tests

open Microsoft.Extensions.Logging 

open Example.Serialisation.Json
open Newtonsoft.Json
open Xunit
open Xunit.Abstractions 

type PeekReaderShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    let sut count (s:string) =
        
        let reader =
            let sr = new System.IO.StringReader(s)
            new JsonTextReader( sr )
            
        PeekReader.Make( count, reader )

                
    [<Fact>]
    member this.``BeCreateable`` () =
        
        let sut =
            sut 0 "{}"
        
        Assert.Equal( 0, sut.Count )
            
    [<Fact>]
    member this.``BePeekable`` () =
        
        let sut =
            sut 3 "{\"$type\" : \"foo\" }"
        
        let item =
            sut.PeekAt(0)
            
        Assert.Equal( item.Token, JsonToken.StartObject )
        
                    

                  

        
        