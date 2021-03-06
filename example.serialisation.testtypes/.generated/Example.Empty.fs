namespace Klarity.Serialisation.TestTypes.Example

open Klarity.Types.Framework
open Klarity.Serialisation
open Klarity.Serialisation.Json

type Empty () = class end
with
    static member Make(  ) =
        new Empty()

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<Empty>

module private Empty_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Empty>
            with
                member this.TypeName =
                    "Example.Empty"

                member this.Type
                    with get () = typeof<Empty>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                        
                    ()    
                    
                member this.Deserialise (serialiser:ISerialiser) (stream:ISerialiserStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, stream, Some this.ContentType )

                    bds.Start( this.TypeName )
                    
                    let result = 
                        new Empty()
                        
                    result }
                    
    let JSON_Serialiser = 
        { new ITypeSerialiser<Empty>
            with
                member this.TypeName =
                    "Example.Empty"

                member this.Type
                    with get () = typeof<Empty>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerialiser) (stream:ISerialiserStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )


                    jds.Deserialise()

                    let result =
                        new Empty()

                    result }

type Empty with

    static member Binary_Serialiser = Empty_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Empty_Serialisers.JSON_Serialiser
