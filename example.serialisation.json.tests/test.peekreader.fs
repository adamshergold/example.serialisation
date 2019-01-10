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
        
    [<Fact>]
    member this.``BeAbleToSkipScalar`` () =
        
        let sut =
            sut 3 "{ \"name\" : \"john\", \"age\" : 21 }"
            
        sut.Read() |> ignore // start object
        sut.Read() |> ignore // name
        sut.Skip()
        
        Assert.Equal( "age", unbox<string>( sut.Read().Value ) )
        
    [<Fact>]
    member this.``BeAbleToSkipObject`` () =
        
        let sut =
            sut 3 "{ \"name\" : { \"formal\" : \"Jonathan\", \"inner\" : { \"foo\" : 123.4 } }, \"age\" : 21 }"
            
        sut.Read() |> ignore // start object
        sut.Read() |> ignore // name
        sut.Skip()
        
        Assert.Equal( "age", unbox<string>(sut.Read().Value) )
                    
    [<Fact>]
    member this.``BeAbleToSkipArray`` () =
        
        let sut =
            sut 3 "{ \"name\" : [ { \"formal\" : [ 1,2,3 ] } ], \"age\" : 21 }"
            
        sut.Read() |> ignore // start object
        sut.Read() |> ignore // name
        sut.Skip()
        
        Assert.Equal( "age", unbox<string>(sut.Read().Value) )

                  

        
        