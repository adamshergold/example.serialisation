namespace Example.Serialisation

type TypeWrapper( contentType: string option, typeName: string, body: byte[] ) = 

    member val ContentType = contentType 
    
    member val TypeName = typeName 
    
    member val Body = body 
    
    static member Make( contentType, typeName, body ) = 
        new TypeWrapper( contentType, typeName, body ) :> ITypeWrapper 
        
    override this.ToString () = 
        sprintf "TypeWrapper(%s,%s,%d)"
            (match this.ContentType with | Some v -> v | None -> "-")
            this.TypeName
            this.Body.Length 

    interface ITypeWrapper
        with 
            member this.ContentType = this.ContentType 
            
            member this.TypeName = this.TypeName 
            
            member this.Body = this.Body 
           
    interface ITypeSerialisable
        with 
            member this.Type with get () = typeof<TypeWrapper>

              