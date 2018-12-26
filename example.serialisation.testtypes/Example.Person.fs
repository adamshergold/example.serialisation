namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Json

type Person = {
    Name : string
    Address : Example.Serialisation.TestTypes.Example.Address
    Phone : Example.Serialisation.TestTypes.Example.Phone option
    Scores : Map<string,Example.Serialisation.TestTypes.Example.Score>
    Pets : Example.Serialisation.TestTypes.Example.IPet[] option
    Ethnicity : Example.Serialisation.TestTypes.Example.Ethnicity
    Status : Example.Serialisation.TestTypes.Example.Status
    Hobbies : Set<Example.Serialisation.TestTypes.Example.Hobby>
}
with
    static member Make( _Name, _Address, _Phone, _Scores, _Pets, _Ethnicity, _Status, _Hobbies ) =
        {
            Name = _Name
            Address = _Address
            Phone = _Phone
            Scores = _Scores
            Pets = _Pets
            Ethnicity = _Ethnicity
            Status = _Status
            Hobbies = _Hobbies
        }

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<Person>

module private Person_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerialiser<Person>
            with
                member this.TypeName =
                    "Example.Person"

                member this.Type
                    with get () = typeof<Person>

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serialiser, stream, this.TypeName )
                    
                    bs.Write( v.Name )
                     
                    bs.Write( v.Address )
                    
                    bs.Write( v.Phone.IsSome ) 
                    if v.Phone.IsSome then 
                        bs.Write( v.Phone.Value )
                        
                    bs.Write( (int32) v.Scores.Count )
                    v.Scores
                    |> Map.iter ( fun k v ->
                        bs.Write( k ) 
                        bs.Write( v ) )                      

                    bs.Write( v.Pets.IsSome ) 
                    if v.Pets.IsSome then 
                        bs.Write( (int32) v.Pets.Value.Length )
                        v.Pets.Value |> Seq.iter ( fun v -> bs.Write( v ) )
                        
                    bs.Write( v.Ethnicity )
                    
                    bs.Write( v.Status ) 
                    
                    bs.Write( (int32) v.Hobbies.Count )
                    v.Hobbies |> Seq.iter ( fun v -> bs.Write( v ) )
                                                                 
                        
                member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serialiser, s, this.TypeName )

                    let name = 
                        bds.ReadString()
                        
                    let address = 
                        bds.ReadRecord<_>()
                        
                    let phone = 
                        if bds.ReadBool() then 
                            Some( bds.ReadRecord<Example.Serialisation.TestTypes.Example.Phone>() ) 
                        else 
                            None 
                    
                    let scores =
                        bds.ReadMap<Example.Serialisation.TestTypes.Example.Score>( fun () -> bds.ReadRecord<_>() )
                        
                    let pets =
                        if bds.ReadBool() then  
                            let v = bds.ReadArray<Example.Serialisation.TestTypes.Example.IPet>( fun () -> bds.ReadInterface<_>() )
                            Some v 
                        else
                            None
                            
                    let ethnicity = 
                        bds.ReadUnion<Example.Serialisation.TestTypes.Example.Ethnicity>()
                        
                    let status = 
                        bds.ReadUnion<Example.Serialisation.TestTypes.Example.Status>()
                        
                    let hobbies = 
                        bds.ReadSet<Example.Serialisation.TestTypes.Example.Hobby>( fun () -> bds.ReadEnum<_>() )
                                                    
                    Person.Make( name, address, phone, scores, pets, ethnicity, status, hobbies ) }                                                            
                    
    let JSON_Serialiser = 
        { new ITypeSerialiser<Person>
            with
                member this.TypeName =
                    "Example.Person"

                member this.Type
                    with get () = typeof<Person>

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
                    
                    js.WriteProperty "Address"
                    js.Serialise v.Address
                    
                    if v.Phone.IsSome then
                        js.WriteProperty "Phone"
                        js.Serialise v.Phone.Value
                    
                    js.WriteProperty "Scores"
                    js.WriteStartObject()
                    v.Scores
                    |> Map.iter ( fun k v ->
                        js.WriteProperty (k.ToString())
                        js.Serialise v
                    )
                    js.WriteEndObject()
                    
                    if v.Pets.IsSome then
                        js.WriteProperty "Pets"
                        js.WriteStartArray()
                        v.Pets.Value |> Seq.iter js.Serialise
                        js.WriteEndArray()
                    
                    js.WriteProperty "Ethnicity"
                    js.Serialise v.Ethnicity
                    
                    js.WriteProperty "Status"
                    js.Serialise v.Status
                    
                    js.WriteProperty "Hobbies"
                    js.WriteStartArray()
                    v.Hobbies |> Seq.iter js.Serialise
                    js.WriteEndArray()
                    
                    js.WriteEndObject()

                member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Name" ( jds.ReadString )
                    jds.Handlers.On "Address" ( jds.ReadRecord "Example.Address" )
                    jds.Handlers.On "Phone" ( jds.ReadRecord "Example.Phone" )
                    jds.Handlers.On "Scores" ( jds.ReadMap<Example.Serialisation.TestTypes.Example.Score>( jds.ReadRecord "Example.Score" ) )
                    jds.Handlers.On "Pets" ( jds.ReadArray<Example.Serialisation.TestTypes.Example.IPet>( jds.ReadInterface "Example.IPet" ) )
                    jds.Handlers.On "Ethnicity" ( jds.ReadUnion "Example.Ethnicity" )
                    jds.Handlers.On "Status" ( jds.ReadUnion "Example.Status" )
                    jds.Handlers.On "Hobbies" ( jds.ReadSet<Example.Serialisation.TestTypes.Example.Hobby>( jds.ReadEnum<Example.Serialisation.TestTypes.Example.Hobby> ) )

                    jds.Deserialise()

                    let result =
                        {
                            Name = jds.Handlers.TryItem<_>( "Name" ).Value
                            Address = jds.Handlers.TryItem<_>( "Address" ).Value
                            Phone = jds.Handlers.TryItem<_>( "Phone" )
                            Scores = jds.Handlers.TryItem<_>( "Scores" ).Value
                            Pets = jds.Handlers.TryItem<_>( "Pets" )
                            Ethnicity = jds.Handlers.TryItem<_>( "Ethnicity" ).Value
                            Status = jds.Handlers.TryItem<_>( "Status" ).Value
                            Hobbies = jds.Handlers.TryItem<_>( "Hobbies" ).Value
                        }

                    result }

type Person with

    static member Binary_Serialiser = Person_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Person_Serialisers.JSON_Serialiser
