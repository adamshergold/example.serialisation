namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Phone = {
    Code : string option
    Digits : int32[]
}
with
    static member Make( _Code, _Digits ) =
        {
            Code = _Code
            Digits = _Digits
        }

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<Phone>

module private Phone_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Phone>
            with
                member this.TypeName =
                    "Example.Phone"

                member this.Type
                    with get () = typeof<Phone>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    bs.Write( v.Code.IsSome ) 
                    if v.Code.IsSome then 
                        bs.Write( v.Code.Value )
                        
                    bs.Write( (int32) v.Digits.Length )
                    v.Digits |> Seq.iter bs.Write
                        
                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )

                    let _Code = 
                        if bds.ReadBool() then 
                            Some( bds.ReadString() ) 
                        else 
                            None
                        
                    let _Digits = 
                        bds.ReadArray<int32>( bds.ReadInt32 )
                    
                    let result = 
                        {
                            Code = _Code
                            Digits = _Digits 
                        }
                    
                    result }
                                                  
    let JSON_Serialiser = 
        { new ITypeSerialiser<Phone>
            with
                member this.TypeName =
                    "Example.Phone"

                member this.Type
                    with get () = typeof<Phone>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    if v.Code.IsSome then
                        js.WriteProperty "Code"
                        js.Serialise v.Code.Value
                    
                    js.WriteProperty "Digits"
                    js.WriteStartArray()
                    v.Digits |> Seq.iter js.Serialise
                    js.WriteEndArray()
                    
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Code" ( jds.ReadString )
                    jds.Handlers.On "Digits" ( jds.ReadArray<int32>( jds.ReadInt32 ) )

                    jds.Deserialise()

                    let result =
                        {
                            Code = jds.Handlers.TryItem<_>( "Code" )
                            Digits = jds.Handlers.TryItem<_>( "Digits" ).Value
                        }

                    result }

type Phone with

    static member Binary_Serialiser = Phone_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Phone_Serialisers.JSON_Serialiser

