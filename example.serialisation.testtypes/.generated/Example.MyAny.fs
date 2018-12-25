namespace Klarity.Serialisation.TestTypes.Example

open Klarity.Types.Framework
open Klarity.Serialisation
open Klarity.Serialisation.Json

type MyAny = {
    BitsAndBobs : Map<string,Any>
}
with
    static member Make( _BitsAndBobs ) =
        {
            BitsAndBobs = _BitsAndBobs
        }

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<MyAny>

module private MyAny_Serialisers = 

    let JSON_Serialiser = 
        { new ITypeSerialiser<MyAny>
            with
                member this.TypeName =
                    "Example.MyAny"

                member this.Type
                    with get () = typeof<MyAny>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    js.WriteProperty "BitsAndBobs"
                    js.WriteStartObject()
                    v.BitsAndBobs
                    |> Map.iter ( fun k v ->
                        js.WriteProperty (k.ToString())
                        js.Serialise v
                    )
                    js.WriteEndObject()
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerialiser) (stream:ISerialiserStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "BitsAndBobs" ( jds.ReadMap<Any>( jds.ReadAny ) )

                    jds.Deserialise()

                    let result =
                        {
                            BitsAndBobs = jds.Handlers.TryItem<_>( "BitsAndBobs" ).Value
                        }

                    result }

type MyAny with

    static member JSON_Serialiser = MyAny_Serialisers.JSON_Serialiser
