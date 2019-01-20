namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Person = {
    Name : string
    Address : Example.Serialisation.TestTypes.Example.Address
    Phone : Example.Serialisation.TestTypes.Example.Phone option
    Scores : Map<string,Example.Serialisation.TestTypes.Example.Score>
    Pets : Example.Serialisation.TestTypes.Example.IPet[] option
    Status : Example.Serialisation.TestTypes.Example.Status
    Hobbies : Set<Example.Serialisation.TestTypes.Example.Hobby>
}
with
    static member Make( _Name, _Address, _Phone, _Scores, _Pets, _Status, _Hobbies ) =
        {
            Name = _Name
            Address = _Address
            Phone = _Phone
            Scores = _Scores
            Pets = _Pets
            Status = _Status
            Hobbies = _Hobbies
        }

    interface ITypeSerialisable


module private Person_Serialisers = 

    let Binary_Serialiser = 
        { new ITypeSerde<Person>
            with
                member this.TypeName =
                    "Example.Person"

                member this.ContentType
                    with get () = "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde,  stream, this.TypeName )
                    
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
                        
                    bs.Write( v.Status ) 
                    
                    bs.Write( (int32) v.Hobbies.Count )
                    v.Hobbies |> Seq.iter ( fun v -> bs.Write( v ) )
                                                                 
                        
                member this.Deserialise (serde:ISerde) (s:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, s, this.TypeName )

                    let name = 
                        bds.ReadString()
                        
                    let address = 
                        bds.ReadRecord<_>()
                        
                    let phone = 
                        if bds.ReadBoolean() then 
                            Some( bds.ReadRecord<_>() ) 
                        else 
                            None 
                    
                    let scores =
                        bds.ReadMap<_>( fun () -> bds.ReadRecord<_>() )
                        
                    let pets =
                        if bds.ReadBoolean() then  
                            let v = bds.ReadArray<_>( fun () -> bds.ReadInterface<_>() )
                            Some v 
                        else
                            None
                            
                    let status = 
                        bds.ReadUnion<_>()
                        
                    let hobbies = 
                        bds.ReadSet<_>( fun () -> bds.ReadEnum<_>() )
                                                    
                    Person.Make( name, address, phone, scores, pets, status, hobbies ) }                                                            
                    
    let JSON_Serialiser = 
        { new ITypeSerde<Person>
            with
                member this.TypeName =
                    "Example.Person"

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
                    
                    js.WriteProperty "Status"
                    js.Serialise v.Status
                    
                    js.WriteProperty "Hobbies"
                    js.WriteStartArray()
                    v.Hobbies |> Seq.iter js.Serialise
                    js.WriteEndArray()
                    
                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Handlers.On "Name" ( jds.ReadString )
                    jds.Handlers.On "Address" ( jds.ReadRecord "Example.Address" )
                    jds.Handlers.On "Phone" ( jds.ReadRecord "Example.Phone" )
                    jds.Handlers.On "Scores" ( jds.ReadMap<Example.Serialisation.TestTypes.Example.Score>( jds.ReadRecord "Example.Score" ) )
                    jds.Handlers.On "Pets" ( jds.ReadArray<Example.Serialisation.TestTypes.Example.IPet>( jds.ReadInterface "Example.IPet" ) )
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
                            Status = jds.Handlers.TryItem<_>( "Status" ).Value
                            Hobbies = jds.Handlers.TryItem<_>( "Hobbies" ).Value
                        }

                    result }

type Person with

    static member Binary_Serialiser = Person_Serialisers.Binary_Serialiser
    
    static member JSON_Serialiser = Person_Serialisers.JSON_Serialiser
