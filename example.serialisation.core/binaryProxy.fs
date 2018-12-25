namespace Example.Serialisation

type BinaryProxy( wrapper: ITypeWrapper ) =

    static member TypeName = "BinaryProxy"
    
    member val Wrapper = wrapper 
    
    static member Make( wrapper ) = 
        new BinaryProxy( wrapper ) 
        
    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<BinaryProxy> 
        
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerialiser<BinaryProxy> 
                with 
                    member this.TypeName =
                        BinaryProxy.TypeName

                    member this.Type 
                        with get () = typeof<BinaryProxy> 
                        
                    member this.ContentType = 
                        "binary" 
                                                    
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:BinaryProxy) =

                        use bw = 
                            Serde.BinaryWriter( s.Stream )
                        
                        bw.Write( v.Wrapper.ContentType.IsSome ) 
                        if v.Wrapper.ContentType.IsSome then 
                            bw.Write( v.Wrapper.ContentType.Value )
                            
                        bw.Write( v.Wrapper.TypeName )
                        
                        bw.Write( (int32) v.Wrapper.Body.Length ) 
                        bw.Write( v.Wrapper.Body )
                        
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use br = 
                            Serde.BinaryReader( s.Stream ) 
                        
                        let contentType = 
                            if br.ReadBoolean() then Some( br.ReadString() ) else None 
                            
                        let typeName = 
                            br.ReadString()
                            
                        let body = 
                            br.ReadBytes( br.ReadInt32() )
                            
                        let wrapper = 
                            TypeWrapper.Make( contentType, typeName, body ) 
                                
                        BinaryProxy.Make( wrapper ) }
                                                  