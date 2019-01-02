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
        with
            member this.Type
                with get () = typeof<Dog>

module private Dog_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Dog>
            with
                member this.TypeName =
                    "Example.Dog"

                member this.Type
                    with get () = typeof<Dog>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                    
                    bs.Write( v.Name )
                    
                    bs.Write( v.NickName.IsSome ) 
                    if v.NickName.IsSome then 
                        bs.Write( v.NickName.Value )
                        
                    bs.Write( v.Breed )                        
                        
                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, stream, this.TypeName )

                    let _Name = 
                        bds.ReadString()
                        
                    let _NickName =
                        if bds.ReadBool() then 
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
        { new ITypeSerialiser<Dog>
            with
                member this.TypeName =
                    "Example.Dog"

                member this.Type
                    with get () = typeof<Dog>

                member this.ContentType
                    with get () = "json"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serialiser, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty "@type"
                    js.WriteValue this.TypeName

                    js.WriteProperty "Name"
                    js.Serialise v.Name
                    
                    if v.NickName.IsSome then
                        js.WriteProperty "NickName"
                        js.Serialise v.NickName.Value
                    
                    js.WriteProperty "Breed"
                    js.Serialise v.Breed
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

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
