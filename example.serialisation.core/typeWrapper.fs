namespace Example.Serialisation

type TypeWrapper( contentType: string, typeName: string option, body: byte[] ) = 

    member val ContentType = contentType 
    
    member val TypeName = typeName 
    
    member val Body = body 
    
    static member Make( contentType, typeName, body ) = 
        new TypeWrapper( contentType, typeName, body ) :> ITypeWrapper 
        
    override this.ToString () = 
        sprintf "TypeWrapper(%s,%s,%d)"
            this.ContentType
            (match this.TypeName with | Some v -> v | None -> "-")
            this.Body.Length 

    interface ITypeWrapper
        with 
            member this.ContentType = this.ContentType 
            
            member this.TypeName = this.TypeName 
            
            member this.Body = this.Body 
           
    interface ITypeSerialisable
        with 
            member this.Type with get () = typeof<TypeWrapper>

              