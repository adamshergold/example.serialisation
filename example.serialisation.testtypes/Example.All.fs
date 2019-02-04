namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

open NodaTime 

type All = {
    TheSerialisable : ITypeSerialisable
    TheLocalDate : LocalDate
    TheLocalDateTime : LocalDateTime
    TheZonedDateTime : ZonedDateTime
}
with
    static member Make( _TheSerialisable, _TheLocalDate, _TheLocalDateTime, _TheZonedDateTime ) =
        {
            TheSerialisable = _TheSerialisable
            TheLocalDate = _TheLocalDate
            TheLocalDateTime = _TheLocalDateTime
            TheZonedDateTime = _TheZonedDateTime
        }

    interface ITypeSerialisable

module private All_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerde<All>
            with
                member this.TypeName =
                    "Example.All"

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    bs.Write( v.TheSerialisable ) 

                    bs.Write( v.TheLocalDate )

                    bs.Write( v.TheLocalDateTime )

                    bs.Write( v.TheZonedDateTime )
                    
                member this.Deserialise (serde:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, s, this.TypeName )
                    
                    let theSerialisable = 
                        bds.ReadITypeSerialisable()
                        
                    let theLocalDate =
                        bds.ReadLocalDate()

                    let theLocalDateTime =
                        bds.ReadLocalDateTime()

                    let theZonedDateTime =
                        bds.ReadZonedDateTime()

                    All.Make( theSerialisable, theLocalDate, theLocalDateTime, theZonedDateTime ) }
        
    let JSON_Serialiser = 
        { new ITypeSerde<All>
            with
                member this.TypeName =
                    "Example.All"

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteProperty "TheSerialisable"
                    js.Serialise v.TheSerialisable

                    js.WriteProperty "TheLocalDate"
                    js.Serialise v.TheLocalDate

                    js.WriteProperty "TheLocalDateTime"
                    js.Serialise v.TheLocalDateTime

                    js.WriteProperty "TheZonedDateTime"
                    js.Serialise v.TheZonedDateTime
                                                                                
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "TheSerialisable" ( jds.ReadITypeSerialisable )
                    jds.Handlers.On "TheLocalDate" ( jds.ReadLocalDate )
                    jds.Handlers.On "TheLocalDateTime" ( jds.ReadLocalDateTime )
                    jds.Handlers.On "TheZonedDateTime" ( jds.ReadZonedDateTime )

                    jds.Deserialise()

                    let result =
                        {
                            TheSerialisable = jds.Handlers.TryItem<_>( "TheSerialisable" ).Value
                            TheLocalDate = jds.Handlers.TryItem<_>( "TheLocalDate" ).Value
                            TheLocalDateTime = jds.Handlers.TryItem<_>( "TheLocalDateTime" ).Value
                            TheZonedDateTime = jds.Handlers.TryItem<_>( "TheZonedDateTime" ).Value
                        }

                    result }

type All with

    static member Binary_Serialiser = All_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = All_Serialisers.JSON_Serialiser
