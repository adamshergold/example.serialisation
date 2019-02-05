namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Dog = {
    Name : string
    NickName : string option
    Breed : string
}
with
    static member Make( _Name, _NickName, _Breed ) =
        {
            Name = _Name
            NickName = _NickName
            Breed = _Breed
        }

    interface Example.Serialisation.TestTypes.Example.IPet
        with
            member this.Name = this.Name
            member this.NickName = this.NickName
    
    interface ITypeSerialisable

module private Dog_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerde<Dog>
            with
                member this.TypeName =
                    "Example.Dog"

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
                    bs.Write( v.Name )
                    
                    bs.Write( v.NickName.IsSome ) 
                    if v.NickName.IsSome then 
                        bs.Write( v.NickName.Value )
                        
                    bs.Write( v.Breed )                        
                        
                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )

                    let _Name = 
                        bds.ReadString()
                        
                    let _NickName =
                        if bds.ReadBoolean() then 
                            Some( bds.ReadString() ) 
                        else 
                            None
                    
                    let _Breed = 
                        bds.ReadString()
                        
                    let result = 
                        {
                            Name = _Name
                            NickName = _NickName 
                            Breed = _Breed 
                        }
                        
                    result }
                    
    let JSON_Serialiser = 
        { new ITypeSerde<Dog>
            with
                member this.TypeName =
                    "Example.Dog"

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteProperty "Name"
                    js.Serialise v.Name
                    
                    if v.NickName.IsSome then
                        js.WriteProperty "NickName"
                        js.Serialise v.NickName.Value
                    
                    js.WriteProperty "Breed"
                    js.Serialise v.Breed
                    
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Name" ( jds.ReadString )
                    jds.Handlers.On "NickName" ( jds.ReadString )
                    jds.Handlers.On "Breed" ( jds.ReadString )

                    jds.Deserialise()

                    let result =
                        {
                            Name = jds.Handlers.TryItem<_>( "Name" ).Value
                            NickName = jds.Handlers.TryItem<_>( "NickName" )
                            Breed = jds.Handlers.TryItem<_>( "Breed" ).Value
                        }

                    result }

type Dog with

    static member Binary_Serialiser = Dog_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Dog_Serialisers.JSON_Serialiser
