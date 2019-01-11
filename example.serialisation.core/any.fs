namespace Example.Serialisation

type Any = 
    | Int32 of int32
    | Int64 of int64
    | String of string
    | Double of double
    | Bool of bool
    | Record of ITypeSerialisable
    | Union of ITypeSerialisable
    | Array of Any[]
    | Map of Map<string,Any>
with
    static member Make( v: int32 ) = 
        Any.Int32( v )
         
    static member Make( v: int64 ) = 
        Any.Int64( v ) 

    static member Make( v: double ) = 
        Any.Double( v ) 
    
    static member Make( v: string ) = 
        Any.String( v ) 
     
    member this.Value 
        with get () = 
            match this with 
            | Int32(v) -> box(v)
            | Int64(v) -> box(v)
            | String(v) -> box(v)
            | Double(v) -> box(v)
            | Bool(v) -> box(v)
            | Record(v) -> box(v)
            | Union(v) -> box(v)
            | Array(v) -> box(v)
            | Map(v) -> box(v)
            
    interface ITypeSerialisable


