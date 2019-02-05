namespace Example.Serialisation.Core.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.TestTypes
open Example.Serialisation.TestTypes.Extensions 

type ExamplesShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = { 
            Logging.Options.Default 
                with 
                    OutputHelper = Some oh
                    Level = "trace" 
        }
            
        Logging.CreateLogger options

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

            let myEmpties (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.Empty.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.Empty"); box(v) |] )

            let myAlls (ct:string) = 
                Example.Serialisation.TestTypes.Extensions.All.Examples
                |> Seq.map ( fun v ->
                    [| box(ct); box("Example.All"); box(v) |] )
                        
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

                yield! (myEmpties "json")
                yield! (myEmpties "binary")

                yield! (myAlls "json")
                yield! (myAlls "binary")

            }
                             
    [<Theory>]
    [<MemberData("Examples")>]
    member this.``RoundTrip`` (contentType:string) (typeName:string) (v:obj) = 
    
        let sut = sut()

        let bytes = 
            Helpers.Serialise sut contentType v
          
        logger.LogInformation( "{ContentType} nBytes {Bytes}", contentType, bytes.Length )
                    
        if contentType = "json" then
            logger.LogInformation( "{JSON}", System.Text.Encoding.UTF8.GetString bytes )
            
        let rt = 
            Helpers.Deserialise sut contentType typeName bytes 
                    
        Assert.Equal( v, rt )            
        
        
        
        