namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Json

type Status = 
    | Single 
    | Married of string 
with
    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<Status>

module private Status_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Status>
            with
                member this.TypeName =
                    "Example.Status"

                member this.Type
                    with get () = typeof<Status>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                    
                    match v with 
                    | Single -> 
                        bs.Write( "Single" )
                    | Married(v) ->
                        bs.Write( "Married" )
                        bs.Write(v)
                        
                member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, s, Some this.ContentType )

                    bds.Start( this.TypeName )
                    
                    match bds.ReadString() with
                    | _ as v when v = "Single" ->
                        Status.Single 
                    | _ as v when v = "Married" ->
                        let v =
                            bds.ReadString()
                        Status.Married(v)
                    | _ as v ->
                        failwithf "Unexpected union case seen when deserialising Status: '%s'" v } 
                        
    let JSON_Serialiser =
        { new ITypeSerialiser<Status>
            with
                member this.TypeName =
                    "Example.Status"

                member this.Type
                    with get () = typeof<Status>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    match v with
                    | Single ->
                        js.WriteProperty "Single"
                        js.WriteNull()
                    | Married(v) ->
                        js.WriteProperty "Married"
                        js.Serialise v

                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Single" ( jds.ReadNull )
                    jds.Handlers.On "Married" ( jds.ReadString )

                    jds.Deserialise()

                    let result =
                        if jds.Handlers.Has "Single" then
                            Status.Single
                        else if jds.Handlers.Has "Married" then
                            Status.Married( jds.Handlers.TryItem<_>( "Married" ).Value )
                            else
                                failwithf "Unable to determine union case when deserialising [%s]" this.TypeName

                    result }

type Status with

    static member Binary_Serialiser = Status_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Status_Serialisers.JSON_Serialiser
