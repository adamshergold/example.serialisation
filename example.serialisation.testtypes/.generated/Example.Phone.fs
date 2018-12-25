namespace Klarity.Serialisation.TestTypes.Example

open Klarity.Types.Framework
open Klarity.Serialisation
open Klarity.Serialisation.Json

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

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                    
                    bs.Write( v.Code.IsSome ) 
                    if v.Code.IsSome then 
                        bs.Write( v.Code.Value )
                        
                    bs.Write( (int32) v.Digits.Length )
                    v.Digits |> Seq.iter bs.Write
                        
                member this.Deserialise (serialiser:ISerialiser) (stream:ISerialiserStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, stream, Some this.ContentType )

                    bds.Start( this.TypeName )
                    
                    let _Code = 
                        if bds.ReadBoolean() then 
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

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    if v.Code.IsSome then
                        js.WriteProperty "Code"
                        js.Serialise v.Code.Value
                    
                    js.WriteProperty "Digits"
                    js.WriteStartArray()
                    v.Digits |> Seq.iter js.Serialise
                    js.WriteEndArray()
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerialiser) (stream:ISerialiserStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

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

