namespace Example.Serialisation

module Helpers = 
        
    let Serialise (s:ISerde) (contentType:string option) (v:obj) =
     
        use msw = 
            new System.IO.MemoryStream()
            
        use writer = 
            SerdeStreamWrapper.Make( msw )
                        
        v |> s.Serialise contentType writer 
        
        msw.ToArray()


    let Deserialise (s:ISerde) (contentType:string option) (typeName:string) (bs:byte[]) =
     
        use msr = 
            new System.IO.MemoryStream( bs )
            
        use reader = 
            SerdeStreamWrapper.Make( msr )
                        
        s.Deserialise contentType typeName reader 
      
      
    let DeserialiseT<'T when 'T :> ITypeSerialisable> (s:ISerde) (contentType:string option) (bs:byte[]) =
     
        use msr = 
            new System.IO.MemoryStream( bs ) 
            
        use reader = 
            SerdeStreamWrapper.Make( msr ) 
                
        s.DeserialiseT<'T> contentType reader 
        
                      
    let Wrap (serialiser:ISerde) (ts:ITypeSerialisable) (contentTypes:seq<string>) = 
    
        let contentType =
         
            let picker (ct:string) = 
                serialiser.TryLookupBySystemType (Some ct,ts.Type) |> Option.map ( fun _ -> ct )
                 
            match contentTypes |> Seq.tryPick picker with 
            | None -> 
                failwithf "Unable to find a serialiser for '%O' for any of '%s'" (ts.Type) (contentTypes |> String.concat ",")
            | Some contentType -> 
                contentType
                
        let typeName = 
            serialiser.TypeName (Some contentType) (ts.Type) 
                                
        let body = 
            Serialise serialiser (Some contentType) ts 
                                                  
        TypeWrapper.Make( Some contentType, typeName, body )   
        
    let Unwrap (serialiser:ISerde) (tw:ITypeWrapper) = 
    
        Deserialise serialiser tw.ContentType tw.TypeName tw.Body          