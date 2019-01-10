namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Ethnicity = 
    | Earthian 
    | SolarSystem of string 
with
    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<Ethnicity>

module private Ethnicity_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Ethnicity>
            with
                member this.TypeName =
                    "Example.Ethnicity"

                member this.Type
                    with get () = typeof<Ethnicity>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    match v with 
                    | Earthian -> 
                        bs.Write( "Earthian" )
                    | SolarSystem(v) ->
                        bs.Write( "SolarSystem" )
                        bs.Write( v )
                        
                member this.Deserialise (serde:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, s, this.TypeName )

                    match bds.ReadString() with
                    | _ as v when v = "Earthian" ->
                        Ethnicity.Earthian 
                    | _ as v when v = "SolarSystem" ->
                        let v =
                            bds.ReadString()
                        Ethnicity.SolarSystem(v)
                    | _ as v ->
                        failwithf "Unexpected union case seen when deserialising Ethnicity: '%s'" v } 
                        
    let JSON_Serialiser =
        { new ITypeSerialiser<Ethnicity>
            with
                member this.TypeName =
                    "Example.Ethnicity"

                member this.Type
                    with get () = typeof<Ethnicity>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    match v with
                    | Earthian ->
                        js.WriteProperty "Earthian"
                        js.WriteNull()
                    | SolarSystem(v) ->
                        js.WriteProperty "SolarSystem"
                        js.Serialise v

                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Earthian" ( jds.ReadNull )
                    jds.Handlers.On "SolarSystem" ( jds.ReadString )

                    jds.Deserialise()

                    let result =
                        if jds.Handlers.Has "Earthian" then
                            Ethnicity.Earthian
                        else if jds.Handlers.Has "SolarSystem" then
                            Ethnicity.SolarSystem( jds.Handlers.TryItem<_>( "SolarSystem" ).Value )
                            else
                                failwithf "Unable to determine union case when deserialising [%s]" this.TypeName

                    result }

type Ethnicity with

    static member Binary_Serialiser = Ethnicity_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Ethnicity_Serialisers.JSON_Serialiser
