namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Empty () = class end
with
    static member Make(  ) =
        new Empty()

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<Empty>

module private Empty_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Empty>
            with
                member this.TypeName =
                    "Example.Empty"

                member this.Type
                    with get () = typeof<Empty>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                        
                    ()    
                    
                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )

                    let result = 
                        new Empty()
                        
                    result }
                    
    let JSON_Serialiser = 
        { new ITypeSerialiser<Empty>
            with
                member this.TypeName =
                    "Example.Empty"

                member this.Type
                    with get () = typeof<Empty>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )


                    jds.Deserialise()

                    let result =
                        new Empty()

                    result }

type Empty with

    static member Binary_Serialiser = Empty_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Empty_Serialisers.JSON_Serialiser
