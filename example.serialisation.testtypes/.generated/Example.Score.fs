namespace Klarity.Serialisation.TestTypes.Example

open Klarity.Types.Framework
open Klarity.Serialisation
open Klarity.Serialisation.Json

type Score = {
    Mark : double
    Pass : bool
}
with
    static member Make( _Mark, _Pass ) =
        {
            Mark = _Mark
            Pass = _Pass
        }

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<Score>

module private Score_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Score>
            with
                member this.TypeName =
                    "Example.Score"

                member this.Type
                    with get () = typeof<Score>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                    
                    bs.Write( v.Mark ) 
                    
                    bs.Write( v.Pass )
                     
                member this.Deserialise (serialiser:ISerialiser) (s:ISerialiserStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, s, Some this.ContentType )

                    bds.Start( this.TypeName )
                    
                    let mark = 
                        bds.ReadDouble()
                        
                    let pass = 
                        bds.ReadBoolean()
                    
                    Score.Make( mark, pass ) }
                    
    let JSON_Serialiser = 
        { new ITypeSerialiser<Score>
            with
                member this.TypeName =
                    "Example.Score"

                member this.Type
                    with get () = typeof<Score>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    js.WriteProperty "Mark"
                    js.Serialise v.Mark
                    
                    js.WriteProperty "Pass"
                    js.Serialise v.Pass
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerialiser) (stream:ISerialiserStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Mark" ( jds.ReadDouble )
                    jds.Handlers.On "Pass" ( jds.ReadBool )

                    jds.Deserialise()

                    let result =
                        {
                            Mark = jds.Handlers.TryItem<_>( "Mark" ).Value
                            Pass = jds.Handlers.TryItem<_>( "Pass" ).Value
                        }

                    result }

type Score with

    static member Binary_Serialiser = Score_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Score_Serialisers.JSON_Serialiser
