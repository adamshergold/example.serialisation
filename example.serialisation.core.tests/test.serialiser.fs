namespace Example.Serialisation.Core.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.TestTypes
open Example.Serialisation.TestTypes.Extensions 

type SerialiserShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = { 
            Logging.Options.Default 
                with 
                    OutputHelper = Some oh
                    Level = "trace" 
        }
            
        Logging.CreateLogger options

    let roundTrip (serialiser:ISerde) (typeName:string) (v:obj) = 
    
        use msw = 
            new System.IO.MemoryStream()
                    
        use writeStream = 
            SerdeStreamWrapper.Make( msw )            
                    
        v |> serialiser.Serialise None writeStream

        use msr = 
            new System.IO.MemoryStream( msw.ToArray() )
            
        use readStream = 
            SerdeStreamWrapper.Make( msr )            
        
        serialiser.Deserialise None typeName readStream 

    let sut = 
    
        let sut = 
            let options = {
                SerdeOptions.Default 
                    with Logger = Some logger }
                    
            Serde.Make( options ) 
            
        sut.TryRegisterAssembly (typeof<Example.Serialisation.Binary.BinaryProxy>.Assembly) |> ignore            
        sut.TryRegisterAssembly (typeof<Example.Serialisation.TestTypes.Example.Person>.Assembly) |> ignore
        sut.TryRegister Example.Serialisation.Json.Serialisers.AnyJsonSerialiser |> ignore
        sut.TryRegister Example.Serialisation.Binary.Serialisers.AnyBinarySerialiser |> ignore
    
        fun () -> sut 
            
        
    static member Examples
        with get () =
        
            let addresses (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.Address.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.Address"); box(v) |] )

            let phones (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.Phone.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.Phone"); box(v) |] )

            let dogs (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.Dog.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.Dog"); box(v) |] )

            let ethnicities (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.Ethnicity.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.Ethnicity"); box(v) |] )

            let scores (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.Score.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.Score"); box(v) |] )

            let persons (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.Person.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.Person"); box(v) |] )

            let unionOfPersons (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.UnionOfPersons.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.UnionOfPersons"); box(v) |] )

            let myAnys (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.MyAny.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.MyAny"); box(v) |] )

            seq {
                yield! (addresses "json")
                yield! (addresses "binary")
            
                yield! (phones "json")
                yield! (phones "binary")

                yield! (scores "json")
                yield! (scores "binary")

                yield! (dogs "json")
                yield! (dogs "binary")

                yield! (ethnicities "json")
                yield! (ethnicities "binary")

                yield! (persons "json")
                yield! (persons "binary")

                yield! (unionOfPersons "json")
                yield! (unionOfPersons "binary")

                yield! (myAnys "json")
                yield! (myAnys "binary")
            }
                             
    [<Fact>]
    member this.``RegisterByReflection`` () =
    
        let sut = 
            Serde.Make( )
            
        let nItems =             
            sut.TryRegisterAssembly typeof<Example.Serialisation.TestTypes.Example.Person>.Assembly
            
        Assert.True( nItems > 0 )

        Assert.Equal( nItems, sut.Items |> Seq.length )                    

                    
    [<Theory>]
    [<MemberData("Examples")>]
    member this.``RoundTrip`` (contentType:string) (typeName:string) (v:obj) = 
    
        let sut = sut()

        let bytes = 
            Helpers.Serialise sut (Some contentType) v
          
        logger.LogInformation( "{ContentType} nBytes {Bytes}", contentType, bytes.Length )
                    
        let rt = 
            Helpers.Deserialise sut (Some contentType) typeName bytes 
                    
        Assert.Equal( v, rt )            
        
        
        
        