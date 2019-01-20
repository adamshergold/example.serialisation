namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Married = {
    Spouse : string
}
with
    static member Make( _Spouse ) =
        {
            Spouse = _Spouse
        }

    interface ITypeSerialisable

module private Married_Serdes = 

    let Binary_Serde = 
        { new ITypeSerde<Married>
            with
                member this.TypeName =
                    "Example.Married"

                member this.ContentType =
                    "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde, stream, this.TypeName )
                    
                    bs.Write( v.Spouse )
                        
                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )

                    let _Spouse = 
                        bds.ReadString()

                    let result = 
                        {
                            Spouse = _Spouse
                        }
                    
                    result }
                    
    let JSON_Serde = 
        { new ITypeSerde<Married>
            with
                member this.TypeName =
                    "Example.Married"

                member this.ContentType =
                    "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteProperty "Spouse"
                    js.Serialise v.Spouse
                    
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Spouse" ( jds.ReadString )

                    jds.Deserialise()

                    let result =
                        {
                            Spouse = jds.Handlers.TryItem<_>( "Spouse" ).Value
                        }

                    result }

type Married with

    static member Binary_Serde = Married_Serdes.Binary_Serde
    
    static member JSON_Serde = Married_Serdes.JSON_Serde
