namespace Klarity.Serialisation.TestTypes.Example

open Klarity.Types.Framework
open Klarity.Serialisation
open Klarity.Serialisation.Json

type All = {
    TheTime : ITime
    TheTimestamp : ITimestamp
    TheObservation : Observation
    TheRevision : IRevision
    TheSerialisable : ITypeSerialisable
}
with
    static member Make( _TheTime, _TheTimestamp, _TheObservation, _TheRevision, _TheSerialisable ) =
        {
            TheTime = _TheTime
            TheTimestamp = _TheTimestamp
            TheObservation = _TheObservation
            TheRevision = _TheRevision
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

                member this.Serialise (serialiser:ISerialiser) (stream:ISerialiserStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    js.WriteProperty "TheTime"
                    js.Serialise v.TheTime
                    
                    js.WriteProperty "TheTimestamp"
                    js.Serialise v.TheTimestamp
                    
                    js.WriteProperty "TheObservation"
                    js.Serialise v.TheObservation
                    
                    js.WriteProperty "TheRevision"
                    js.Serialise v.TheRevision
                    
                    js.WriteProperty "TheSerialisable"
                    js.Serialise v.TheSerialisable
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerialiser) (stream:ISerialiserStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "TheTime" ( jds.ReadTime )
                    jds.Handlers.On "TheTimestamp" ( jds.ReadTimestamp )
                    jds.Handlers.On "TheObservation" ( jds.ReadObservation )
                    jds.Handlers.On "TheRevision" ( jds.ReadRevision )
                    jds.Handlers.On "TheSerialisable" ( jds.ReadSerialisable )

                    jds.Deserialise()

                    let result =
                        {
                            TheTime = jds.Handlers.TryItem<_>( "TheTime" ).Value
                            TheTimestamp = jds.Handlers.TryItem<_>( "TheTimestamp" ).Value
                            TheObservation = jds.Handlers.TryItem<_>( "TheObservation" ).Value
                            TheRevision = jds.Handlers.TryItem<_>( "TheRevision" ).Value
                            TheSerialisable = jds.Handlers.TryItem<_>( "TheSerialisable" ).Value
                        }

                    result }

type All with

    static member JSON_Serialiser = All_Serialisers.JSON_Serialiser
