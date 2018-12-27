namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type UnionOfPersons = 
    | Nobody 
    | Persons of Example.Serialisation.TestTypes.Example.Person[] 
with
    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<UnionOfPersons>

module private UnionOfPersons_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<UnionOfPersons>
            with
                member this.TypeName =
                    "Example.UnionOfPersons"

                member this.Type
                    with get () = typeof<UnionOfPersons>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                    
                    match v with 
                    | Nobody -> 
                        bs.Write( "Nobody" )
                    | Persons(v) ->
                        bs.Write( "Persons" )
                        bs.Write( (int32) v.Length ) 
                        v |> Seq.iter ( fun item -> bs.Write( item ) )
                        
                member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, s, this.TypeName )
                    
                    match bds.ReadString() with
                    | _ as v when v = "Nobody" ->
                        UnionOfPersons.Nobody 
                    | _ as v when v = "Persons" ->
                        let v =
                            bds.ReadArray<Example.Serialisation.TestTypes.Example.Person>( fun () -> bds.ReadRecord<_>() )
                        UnionOfPersons.Persons(v)
                    | _ as v ->
                        failwithf "Unexpected union case seen when deserialising UnionOfPersons: '%s'" v } 
                    
    let JSON_Serialiser =
        { new ITypeSerialiser<UnionOfPersons>
            with
                member this.TypeName =
                    "Example.UnionOfPersons"

                member this.Type
                    with get () = typeof<UnionOfPersons>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    match v with
                    | Nobody ->
                        js.WriteProperty "Nobody"
                        js.WriteNull()
                    | Persons(v) ->
                        js.WriteProperty "Persons"
                        js.WriteStartArray()
                        v |> Seq.iter js.Serialise
                        js.WriteEndArray()

                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Nobody" ( jds.ReadNull )
                    jds.Handlers.On "Persons" ( jds.ReadArray<Example.Serialisation.TestTypes.Example.Person>( jds.ReadRecord "Example.Person" ) )

                    jds.Deserialise()

                    let result =
                        if jds.Handlers.Has "Nobody" then
                            UnionOfPersons.Nobody
                        else if jds.Handlers.Has "Persons" then
                            UnionOfPersons.Persons( jds.Handlers.TryItem<_>( "Persons" ).Value )
                            else
                                failwithf "Unable to determine union case when deserialising [%s]" this.TypeName

                    result }

type UnionOfPersons with

    static member Binary_Serialiser = UnionOfPersons_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = UnionOfPersons_Serialisers.JSON_Serialiser
