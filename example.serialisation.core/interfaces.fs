namespace Example.Serialisation

// Serde is short for 'SerialiserDeserialiser'

// We don't simply use a plain stream for the Serde to read/write since we often need
// the ability to 'peek' ahead into the stream (before reading) to decide what
// actions to take.  As such we'll typically wrap an underlying stream in some concrete
// implementation that has some custom behaviour.  We still expose the underlying stream though.

type ISerdeStream =
    inherit System.IDisposable
    abstract Stream : System.IO.Stream with get


// Marker interface for a type to be useable by a Serde
// TBD: Could we now actually ditch this I wonder?

type ITypeSerialisable = interface end
    
// Each type that can be serialised/deserialised must have an associated
// 'TypeSerde' (i.e. a type-specific implementation) and we have a weakly-typed
// interface that defines TypeName and ContentType.
//
// When a type is serialised we do not use the System.Type name to identify it
// since that is very bound to the platform.  Instead we allow the author to give
// a friendly name to the serialised type.
//
// We also wish to support multiple serialisation formats (e.g. json, binary, messagepack)
// so there may be several TypeSerdes for a given type that differ in this value.    

type ITypeSerde =
    abstract TypeName : string with get
    abstract ContentType : string with get

and ITypeSerde<'Source> = 
    inherit ITypeSerde
    abstract Serialise   : ISerde -> ISerdeStream -> 'Source ->  unit
    abstract Deserialise : ISerde -> ISerdeStream -> 'Source


// The Serde interface itself.  There are methods to register TypeSerdes, look them up and
// do the serialisation itself        

and ISerde =
    // Try to register a TypeSerde (we use obj as input as this will typically be called by reflection)
    abstract TryRegister           : obj -> ITypeSerde option
    
    // Register all the TypeSerdes in a given assembly (returns the number registered)
    abstract TryRegisterAssembly   : System.Reflection.Assembly -> int
    
    // Enumerate the registered Serdes (more for debugging)
    abstract Items                 : seq<ITypeSerde> with get
    
    // Given a contentType/typeName see if there is a TypeSerde
    abstract TrySerdeByTypeName    : contentType:string * typeName:string -> ITypeSerde option
    
    // Do the same lookup but by System.Type
    abstract TrySerdeBySystemType  : contentType:string * System.Type -> ITypeSerde option
    
    // Given a content-type, a stream and an object we attempt to serialiser it
    abstract Serialise             : contentType:string -> ISerdeStream -> obj -> unit
    
    // Two deserialise methods: one where we explicitly state the typeName we expect (this may throw if
    // the actual type in the stream is different).  The second uses a generic type parameter to indicate
    // the type we expect to receive
    abstract Deserialise           : contentType:string -> typeName:string -> ISerdeStream -> obj
    abstract DeserialiseT<'T>      : contentType:string -> ISerdeStream -> 'T
    
    // Expose the options the Serde is running with
    abstract Options               : SerdeOptions with get
    
// Sometimes if we have a stream with a type but we have no registered TypeSerde we can still extract
// it into a convenient wrapper (perhaps to pass-on) and the TypeWrapper is an interface that allows
// access to what we know about the type.  Clearly the 'content' we can't make sense of so is simply
// the byte[] read from the stream.
  
type ITypeWrapper =
    inherit ITypeSerialisable
    abstract ContentType : string with get
    abstract TypeName : string option with get
    abstract Body : byte[] with get
