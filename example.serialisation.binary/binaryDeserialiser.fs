namespace Example.Serialisation.Binary

open Microsoft.Extensions.Logging 

open Example.Serialisation

type BinaryDeserialiser( serialiser: ISerde, ss: ISerdeStream, typeName: string ) = 

    let wrapper = 
        BinaryPeekStreamWrapper.Make( ss ) 
        
    do 
        let inlineTypeName = 
            wrapper.ReadString()
    
        let nBytes = 
            wrapper.ReadInt32() 
          
        if inlineTypeName <> typeName then 
            failwithf "Attempting to deserialise '%s' but saw '%s'" inlineTypeName typeName 
 
                
    member val TypeName = typeName 
            
    member val ContentType = "binary"
        
    static member Make( serialiser, ss, typeName ) = 
        new BinaryDeserialiser( serialiser, ss, typeName )

    member this.ReadInt8 () =
        wrapper.ReadInt8()
        
    member this.ReadString () = 
        wrapper.ReadString()
        
    member this.ReadInt32 () =
        wrapper.ReadInt32() 

    member this.ReadInt64 () =
        wrapper.ReadInt64() 
         
    member this.ReadDateTime () = 
        System.DateTime.FromBinary( wrapper.ReadInt64() ) 
                
    member this.ReadBool () = 
        wrapper.ReadBool() 
                        
    member this.ReadDouble () = 
        wrapper.ReadDouble() 
           
    member this.ReadBytes (n:int32) = 
        wrapper.ReadBytes( n ) 

    member this.ReadAny() =
        match serialiser.Deserialise this.ContentType "Any" (wrapper :> ISerdeStream) with
        | :? Any as av -> av
        | _ as v -> failwithf "Unable to binary serialise Any - saw '%O'" (v.GetType())
        
    member this.ReadNull () = 
        ()
                           
    member this.ReadEnum<'T> () =
        let strV = wrapper.ReadString()
        try  
            System.Enum.Parse( typeof<'T>, strV ) :?> 'T
        with 
        | _ as ex ->
            failwithf "Failed to parse '%s' as enum of type '%O'" strV (typeof<'T>)
                                                
    member this.InferType () = 

        let inlineTypeName = 
            wrapper.PeekString()
                  
        match serialiser.TryLookupByTypeName (this.ContentType, inlineTypeName) with
        | Some serialiser -> 
            inlineTypeName
        | None ->
            BinaryProxy.TypeName
          
    member this.ReadSerialisable () =
        match box(this.ReadRecord None) with
        | :? ITypeSerialisable as ts -> 
            match ts with 
            | :? ITypeWrapper as tw ->
                if tw.TypeName.IsSome then 
                    match serialiser.TryLookupByTypeName (tw.ContentType,tw.TypeName.Value) with 
                    | Some typeSerialiser ->
                        use ms = 
                            new System.IO.MemoryStream( tw.Body ) 
                            
                        use stream =
                            SerdeStreamWrapper.Make( ms )
                            
                        match serialiser.Deserialise tw.ContentType tw.TypeName.Value stream with 
                        | :? ITypeSerialisable as ts ->
                            ts
                        | _ -> 
                            failwithf "Deserialised item did not implement ITypeSerialisable"
                    | None ->    
                        ts
                else
                    ts
            | _ -> 
                ts 
        | _ -> 
            failwithf "Deserialised item did not implement ITypeSerialisable"

    member this.ReadRecord (rt:string option) =
     
        let inlineTypeName =
            this.InferType() 
            
        if rt.IsSome && rt.Value <> inlineTypeName then 
            failwithf "Attempted to deserialise '%s' but saw '%s'" (rt.Value) inlineTypeName 
                        
        serialiser.Deserialise this.ContentType inlineTypeName (wrapper :> ISerdeStream) 
                   
    member this.ReadRecord<'T> () =
     
        let inlineTypeName =
            this.InferType() 
            
        match serialiser.Deserialise this.ContentType inlineTypeName (wrapper :> ISerdeStream) with 
        | :? 'T as rt -> rt
        | _ as v -> failwithf "Unexpected type when deserialising record! Saw [%O] but expected [%O]" (v.GetType()) (typeof<'T>)            
        
    member this.ReadInterface<'T> () =
     
        let inlineTypeName = 
            this.InferType()
                           
        match serialiser.Deserialise this.ContentType inlineTypeName (wrapper :> ISerdeStream) with 
        | :? 'T as rt -> rt
        | _ as v -> failwithf "Unexpected type when deserialising interface! Saw [%O] but expected [%O]" (v.GetType()) (typeof<'T>)
        
    member this.ReadUnion<'T> () =

        let inlineTypeName = 
            this.InferType()
                           
        match serialiser.Deserialise this.ContentType inlineTypeName (wrapper :> ISerdeStream) with 
        | :? 'T as rt -> rt
        | _ as v -> failwithf "Unexpected type when deserialising union! Saw [%O] but expected [%O]" (v.GetType()) (typeof<'T>)
                                                      
    member this.ReadArray<'T> (cb:unit->'T) =
    
        let nItems = 
            wrapper.ReadInt32()
        
        let results =   
            [| 0 .. nItems-1 |]
            |> Array.map ( fun _ ->
                cb() )    
                
        results 

    member this.ReadMap<'T> (cb:unit->'T) =
    
        let nItems = 
            wrapper.ReadInt32()
        
        let result =   
            [| 0 .. nItems-1 |]
            |> Array.map ( fun _ ->
                let k = wrapper.ReadString()
                let v = cb()
                k, v ) 
            |> Map.ofSeq    
                
        result

    member this.ReadSet<'T when 'T : comparison > (cb:unit->'T) =
    
        let nItems = 
            wrapper.ReadInt32()
        
        let results =   
            [| 0 .. nItems-1 |]
            |> Array.map ( fun _ ->
                cb() )
            |> Set.ofSeq    
                
        results 
                                                             
    member this.Dispose () = 
        ()
                
    interface System.IDisposable 
        with 
            member this.Dispose () = 
                this.Dispose() 
   