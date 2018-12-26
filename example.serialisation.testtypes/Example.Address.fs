namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
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
        with
            member this.Type
                with get () = typeof<Address>

module private Address_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Address>
            with
                member this.TypeName =
                    "Example.Address"

                member this.Type
                    with get () = typeof<Address>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                    
                    bs.Write( v.Number )
                    
                    bs.Write( v.Street )
                    
                    bs.Write( v.Region.ToString() )
                    
                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, stream, this.TypeName )

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
        { new ITypeSerialiser<Address>
            with
                member this.TypeName =
                    "Example.Address"

                member this.Type
                    with get () = typeof<Address>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    js.WriteProperty "Number"
                    js.Serialise v.Number
                    
                    js.WriteProperty "Street"
                    js.Serialise v.Street
                    
                    js.WriteProperty "Region"
                    js.Serialise (v.Region.ToString())
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

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
    
    