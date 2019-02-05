namespace Example.Serialisation.Binary

open NodaTime

open Example.Serialisation

type BinarySerialiser( serialiser: ISerde, ss: ISerdeStream, typeName: string ) = 

    let ms = 
        new System.IO.MemoryStream()

    let streamWrapper = 
        SerdeStreamWrapper.Make( ms ) 
        
    let bw = Serde.BinaryWriter( ms )
    
    static member Make( serialiser, ss, typeName ) = 
        new BinarySerialiser( serialiser, ss, typeName ) 
        
    member this.Write (v:obj) = 
        match v with 
        | :? string as v -> bw.Write(v)
        | :? int8 as v -> bw.Write(v)
        | :? int32 as v -> bw.Write(v)
        | :? int64 as v -> bw.Write(v)
        | :? bool as v -> bw.Write(v)
        | :? double as v -> bw.Write(v)
        | :? LocalDate as v -> bw.Write( Noda.LocalDateToString v )
        | :? LocalDateTime as v -> bw.Write( Noda.LocalDateTimeToString v )
        | :? ZonedDateTime as v -> bw.Write( Noda.ZonedDateTimeToString v )
        | :? ITypeSerialisable as v -> serialiser.Serialise "binary" streamWrapper v
        | :? System.IConvertible as v -> bw.Write(v.ToString())
        | :? array<byte>  as v -> bw.Write(v)
        | _ -> failwithf "Unable to serialise object of type [%O]" (v.GetType())
        
    member this.Close () = 
        use bw = Serde.BinaryWriter( ss.Stream )
        let bytes = ms.ToArray()
        bw.Write( typeName )
        bw.Write( (int32) bytes.Length ) 
        bw.Write( bytes )
             
    member this.Dispose () = 
        this.Close()
        ms.Dispose()
        
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose()
