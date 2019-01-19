namespace Example.Serialisation.Binary

open Example.Serialisation

type BinaryPeekStreamWrapper( ss: ISerdeStream ) =

    let reader = 
        new System.IO.BinaryReader( ss.Stream, System.Text.Encoding.Default, true ) 
    
    let mutable peek : string option = None

    static member Make( ss: ISerdeStream ) =
        match ss with 
        | :? BinaryPeekStreamWrapper as psr -> psr 
        | _ -> new BinaryPeekStreamWrapper( ss )  

    member this.PeekString () = 
        lock this ( fun _ ->
            if peek.IsNone then 
                peek <- Some <| reader.ReadString()
                peek.Value
            else 
                peek.Value )
                     
    member this.ReadString () = 
        lock this ( fun _ ->
            if peek.IsSome then 
                let result = peek.Value 
                peek <- None
                result
            else 
                reader.ReadString() )
          
    member this.ReadInt8 () =
        reader.ReadByte()
            
    member this.ReadInt32 () = 
        reader.ReadInt32() 

    member this.ReadInt64 () = 
        reader.ReadInt64() 
            
    member this.ReadBool () =    
        reader.ReadBoolean() 

    member this.ReadBoolean () =    
        reader.ReadBoolean() 
            
    member this.ReadDouble () = 
        reader.ReadDouble() 
                     
    member this.ReadBytes (n:int32) = 
        reader.ReadBytes(n) 
                                                
    member this.Dispose () =
        ()
                
    interface System.IDisposable 
        with 
            member this.Dispose () = 
                this.Dispose()
                            
    interface ISerdeStream
        with 
            member this.Stream = ss.Stream         