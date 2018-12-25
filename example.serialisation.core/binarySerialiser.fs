namespace Example.Serialisation

open Microsoft.Extensions.Logging 

type BinarySerialiser( serialiser: ISerde, ss: ISerdeStream, typeName: string ) = 

    let ms = 
        new System.IO.MemoryStream()

    let streamWrapper = 
        SerialiserStreamWrapper.Make( ms ) 
        
    let bw = Serde.BinaryWriter( ms )
    
    //member val ContentType = contentType 
    
    static member Make( serialiser, ss, typeName ) = 
        new BinarySerialiser( serialiser, ss, typeName ) 
        
    member this.Write (v:obj) = 
        match v with 
        | :? string as v -> bw.Write(v)
        | :? int32 as v -> bw.Write(v)
        | :? int64 as v -> bw.Write(v)
        | :? bool as v -> bw.Write(v)
        | :? double as v -> bw.Write(v)
        | :? System.DateTime as v -> bw.Write(v.ToBinary())
        | :? ITypeSerialisable as v -> serialiser.Serialise (Some "binary") streamWrapper v
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
                