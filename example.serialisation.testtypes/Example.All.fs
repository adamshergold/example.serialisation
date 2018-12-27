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
        with
            member this.Type
                with get () = typeof<All>

module private All_Serialisers = 

    let JSON_Serialiser = 
        { new ITypeSerialiser<All>
            with
                member this.TypeName =
                    "Example.All"

                member this.Type
                    with get () = typeof<All>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    js.WriteProperty "TheSerialisable"
                    js.Serialise v.TheSerialisable
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "TheSerialisable" ( jds.ReadSerialisable )

                    jds.Deserialise()

                    let result =
                        {
                            TheSerialisable = jds.Handlers.TryItem<_>( "TheSerialisable" ).Value
                        }

                    result }

type All with

    static member JSON_Serialiser = All_Serialisers.JSON_Serialiser
