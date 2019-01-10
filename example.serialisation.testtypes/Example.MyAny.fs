namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type MyAny = {
    BitsAndBobs : Map<string,Any>
}
with
    static member Make( _BitsAndBobs ) =
        {
            BitsAndBobs = _BitsAndBobs
        }

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<MyAny>

module private MyAny_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<MyAny>
            with
                member this.TypeName =
                    "Example.MyAny"

                member this.Type
                    with get () = typeof<MyAny>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    bs.Write( (int32) v.BitsAndBobs.Count )
                    v.BitsAndBobs
                    |> Map.iter ( fun k v ->
                        bs.Write( k ) 
                        bs.Write( v ) )                                          

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use bds =
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )

                    let _BitsAndBobs =
                        bds.ReadMap<Example.Serialisation.Any>( bds.ReadAny )
                    
                    let result = {
                        BitsAndBobs = _BitsAndBobs
                        }
                    
                    result }
        
    let JSON_Serialiser = 
        { new ITypeSerialiser<MyAny>
            with
                member this.TypeName =
                    "Example.MyAny"

                member this.Type
                    with get () = typeof<MyAny>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteProperty "BitsAndBobs"
                    js.WriteStartObject()
                    v.BitsAndBobs
                    |> Map.iter ( fun k v ->
                        js.WriteProperty (k.ToString())
                        js.Serialise v
                    )
                    js.WriteEndObject()
                    
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "BitsAndBobs" ( jds.ReadMap<Any>( jds.ReadAny ) )

                    jds.Deserialise()

                    let result =
                        {
                            BitsAndBobs = jds.Handlers.TryItem<_>( "BitsAndBobs" ).Value
                        }

                    result }

type MyAny with

    static member Binary_Serialiser = MyAny_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = MyAny_Serialisers.JSON_Serialiser
