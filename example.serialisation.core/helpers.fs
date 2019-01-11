namespace Example.Serialisation

module Helpers = 
        
    let Serialise (s:ISerde) (contentType:string) (v:obj) =
     
        use msw = 
            new System.IO.MemoryStream()
            
        use writer = 
            SerdeStreamWrapper.Make( msw )
                        
        v |> s.Serialise contentType writer 
        
        msw.ToArray()


    let Deserialise (s:ISerde) (contentType:string) (typeName:string) (bs:byte[]) =
     
        use msr = 
            new System.IO.MemoryStream( bs )
            
        use reader = 
            SerdeStreamWrapper.Make( msr )
                        
        s.Deserialise contentType typeName reader 
      
      
    let DeserialiseT<'T when 'T :> ITypeSerialisable> (s:ISerde) (contentType:string) (bs:byte[]) =
     
        use msr = 
            new System.IO.MemoryStream( bs ) 
            
        use reader = 
            SerdeStreamWrapper.Make( msr ) 
                
        s.DeserialiseT<'T> contentType reader 
        
                      
    let Wrap (serde:ISerde) (ts:ITypeSerialisable) (contentTypes:seq<string>) = 
    
        let actualType =
            ts.GetType()
            
        let contentType, typeName =
         
            let picker (ct:string) = 
                serde.TrySerdeBySystemType (ct,actualType) |> Option.map ( fun serde -> serde, ct )
                 
            match contentTypes |> Seq.tryPick picker with 
            | None -> 
                failwithf "Unable to find a serialiser for '%O' for any of '%s'" actualType (contentTypes |> String.concat ",")
            | Some (ts,contentType) -> 
                contentType, ts.TypeName
                
        let body = 
            Serialise serde contentType ts 
                                                  
        TypeWrapper.Make( contentType, Some typeName, body )   
        
    let Unwrap (serde:ISerde) (tw:ITypeWrapper) = 
    
        if tw.TypeName.IsSome then 
            Deserialise serde tw.ContentType tw.TypeName.Value tw.Body
        else
            failwithf "Unable to unwrap TypeWrapper that has no TypeName specified!"