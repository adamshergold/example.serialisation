namespace Example.Serialisation

type ISerdeStream =
    inherit System.IDisposable
    abstract Stream : System.IO.Stream with get
    
type ITypeSerialisable =
    abstract Type : System.Type with get 
        
type ITypeSerialiser =
    inherit ITypeSerialisable
    abstract TypeName : string with get
    abstract ContentType : string with get

and ITypeSerialiser<'Source> = 
    inherit ITypeSerialiser
    abstract Serialise   : ISerde -> ISerdeStream -> 'Source ->  unit
    abstract Deserialise : ISerde -> ISerdeStream -> 'Source
    
and ITypeWrapper =
    inherit ITypeSerialisable
    abstract ContentType : string option with get
    abstract TypeName : string with get
    abstract Body : byte[] with get
        
and ISerde =
    abstract TryRegister           : obj -> ITypeSerialiser option
    abstract TryRegisterAssembly   : System.Reflection.Assembly -> int
    abstract Items                 : seq<ITypeSerialiser> with get
    abstract TryLookupByTypeName   : string option * string -> ITypeSerialiser option
    abstract TryLookupBySystemType : string option * System.Type -> ITypeSerialiser option
    abstract TypeName              : contentType:string option -> System.Type -> string
    abstract SystemType            : contentType:string option -> typeName:string -> System.Type
    abstract Serialise             : contentType:string option -> ISerdeStream -> obj -> unit
    abstract Deserialise           : contentType:string option -> typeName:string -> ISerdeStream -> obj
    abstract Extract               : contentType:string option -> typeName:string -> ISerdeStream -> ITypeWrapper
    abstract DeserialiseT<'T>      : contentType:string option -> ISerdeStream -> 'T
