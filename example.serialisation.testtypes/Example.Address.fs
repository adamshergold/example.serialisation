namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Address = {
    Number : int32
    Street : string
    Region : Example.Serialisation.TestTypes.Example.Region
}
with
    static member Make( _Number, _Street, _Region ) =
        {
            Number = _Number
            Street = _Street
            Region = _Region
        }

    interface ITypeSerialisable

module private Address_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerde<Address>
            with
                member this.TypeName =
                    "Example.Address"

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    bs.Write( v.Number )
                    
                    bs.Write( v.Street )
                    
                    bs.Write( v.Region.ToString() )
                    
                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )

                    let _Number = 
                        bds.ReadInt32()

                    let _Street = 
                        bds.ReadString()
                        
                    let _Region = 
                        bds.ReadEnum<Example.Serialisation.TestTypes.Example.Region>()                            
                    
                    let result = 
                        {
                            Number = _Number
                            Street = _Street
                            Region = _Region 
                        }
                    
                    result }
                    
    let JSON_Serialiser = 
        { new ITypeSerde<Address>
            with
                member this.TypeName =
                    "Example.Address"

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteProperty "Number"
                    js.Serialise v.Number
                    
                    js.WriteProperty "Street"
                    js.Serialise v.Street
                    
                    js.WriteProperty "Region"
                    js.Serialise (v.Region.ToString())
                    
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Number" ( jds.ReadInt32 )
                    jds.Handlers.On "Street" ( jds.ReadString )
                    jds.Handlers.On "Region" ( jds.ReadEnum<Example.Serialisation.TestTypes.Example.Region> )

                    jds.Deserialise()

                    let result =
                        {
                            Number = jds.Handlers.TryItem<_>( "Number" ).Value
                            Street = jds.Handlers.TryItem<_>( "Street" ).Value
                            Region = jds.Handlers.TryItem<_>( "Region" ).Value
                        }

                    result }

type Address with

    static member Binary_Serialiser = Address_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Address_Serialisers.JSON_Serialiser
    
    