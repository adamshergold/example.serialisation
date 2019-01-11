namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type All = {
    TheSerialisable : ITypeSerialisable
}
with
    static member Make( _TheSerialisable ) =
        {
            TheSerialisable = _TheSerialisable
        }

    interface ITypeSerialisable

module private All_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerde<All>
            with
                member this.TypeName =
                    "Example.All"

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    bs.Write( v.TheSerialisable ) 
                    
                member this.Deserialise (serde:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, s, this.TypeName )
                    
                    let theSerialisable = 
                        bds.ReadSerialisable()
                        
                    All.Make( theSerialisable ) }
        
    let JSON_Serialiser = 
        { new ITypeSerde<All>
            with
                member this.TypeName =
                    "Example.All"

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteProperty "TheSerialisable"
                    js.Serialise v.TheSerialisable
                    
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "TheSerialisable" ( jds.ReadSerialisable )

                    jds.Deserialise()

                    let result =
                        {
                            TheSerialisable = jds.Handlers.TryItem<_>( "TheSerialisable" ).Value
                        }

                    result }

type All with

    static member Binary_Serialiser = All_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = All_Serialisers.JSON_Serialiser
