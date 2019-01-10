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
    abstract ContentType : string with get
    abstract TypeName : string option with get
    abstract Body : byte[] with get
        
and ISerde =
    abstract TryRegister           : obj -> ITypeSerialiser option
    abstract TryRegisterAssembly   : System.Reflection.Assembly -> int
    abstract Items                 : seq<ITypeSerialiser> with get
    
    abstract TryLookupByTypeName   : contentType:string * typeName:string -> ITypeSerialiser option
    abstract TryLookupBySystemType : contentType:string * System.Type -> ITypeSerialiser option
    
    abstract Serialise             : contentType:string -> ISerdeStream -> obj -> unit
    
    abstract Deserialise           : contentType:string -> typeName:string -> ISerdeStream -> obj
    abstract DeserialiseT<'T>      : contentType:string -> ISerdeStream -> 'T
    
    abstract Options               : SerdeOptions with get
    
