namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Status = 
    | Single of Single
    | Married of Married
with
    interface ITypeSerialisable

module private Status_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerde<Status>
            with
                member this.TypeName =
                    "Example.Status"

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    match v with 
                    | Single(v) -> 
                        bs.Write( "Single" )
                        bs.Write( v )
                        
                    | Married(v) ->
                        bs.Write( "Married" )
                        bs.Write(v)
                        
                member this.Deserialise (serde:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, s, this.TypeName )

                    match bds.ReadString() with
                    | _ as v when v = "Single" ->
                        
                        let _Status =
                            bds.ReadRecord<_>()
                            
                        Status.Single( _Status )
                        
                    | _ as v when v = "Married" ->
                        
                        let _Married =
                            bds.ReadRecord<_>()
                            
                        Status.Married( _Married )
                        
                    | _ as v ->
                        failwithf "Unexpected union case seen when deserialising Status: '%s'" v } 
                        
    let JSON_Serialiser =
        { new ITypeSerde<Status>
            with
                member this.TypeName =
                    "Example.Status"

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    match v with
                    | Single(v) ->
                        js.WriteProperty "Single"
                        js.Serialise v
                    | Married(v) ->
                        js.WriteProperty "Married"
                        js.Serialise v

                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Single" ( jds.ReadRecord "Example.Single" )
                    jds.Handlers.On "Married" ( jds.ReadRecord "Example.Married" )

                    jds.Deserialise()

                    let result =
                        if jds.Handlers.Has "Single" then
                            Status.Single( jds.Handlers.TryItem<_>( "Single" ).Value )
                        else if jds.Handlers.Has "Married" then
                            Status.Married( jds.Handlers.TryItem<_>( "Married" ).Value )
                        else
                            failwithf "Unable to determine union case when deserialising [%s]" this.TypeName

                    result }

type Status with

    static member Binary_Serialiser = Status_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Status_Serialisers.JSON_Serialiser
