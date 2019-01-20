namespace Example.Serialisation.TestTypes.Example

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Json

type Single () = class end
with
    static member Make() =
        new Single()

    override this.GetHashCode () =
        0
        
    override this.Equals (other:obj) =
        ( this :> System.IComparable ).CompareTo( other ).Equals( 0 )
        
    interface System.IComparable
        with
            member this.CompareTo (other:obj) =
                match other with
                | :? Single -> 0
                | _ -> failwithf "Unable to compare Single to '%O'" other
                
    interface ITypeSerialisable

module private Single_Serdes = 

    let Binary_Serde = 
        { new ITypeSerde<Single>
            with
                member this.TypeName =
                    "Example.Single"

                member this.ContentType =
                    "binary"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
    
                    use bs = 
                        BinarySerialiser.Make( serde, stream, this.TypeName )
                        
                    ()    
                    
                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                                        
                    use bds = 
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )

                    let result = 
                        new Single()
                        
                    result }
                    
    let JSON_Serde = 
        { new ITypeSerde<Single>
            with
                member this.TypeName =
                    "Example.Single"

                member this.ContentType =
                    "json"

                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                    use js =
                        JsonSerialiser.Make( serde, stream, this.ContentType )

                    js.WriteStartObject()
                    js.WriteProperty serde.Options.TypeProperty
                    js.WriteValue this.TypeName

                    js.WriteEndObject()

                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =

                    use jds =
                        JsonDeserialiser.Make( serde, stream, this.ContentType, this.TypeName )

                    jds.Deserialise()

                    let result =
                        new Single()

                    result }

type Single with

    static member Binary_Serde = Single_Serdes.Binary_Serde
    
    static member JSON_Serde = Single_Serdes.JSON_Serde
