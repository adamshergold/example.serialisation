namespace Example.Serialisation.Json

open Newtonsoft.Json

open Example.Serialisation
open Example.Serialisation.Binary


[<AutoOpen>]
module JsonProxyImpl = 

    let ToBase64 (v:byte[]) =
        System.Convert.ToBase64String(v).TrimEnd('=').Replace('+','-').Replace('/','_')
        
    let FromBase64 (s:string) =
        let pad (text:string) = 
            let padding = 3 - ( (text.Length+3) % 4 )
            if padding = 0 then text else text.PadRight( text.Length + padding, '=' )
        s.Replace('_','/').Replace('-','+') |> pad |> System.Convert.FromBase64String 


type JsonProxy( wrapper: ITypeWrapper ) =

    member val Wrapper = wrapper 
    
with
    static member Make( wrapper ) =
        new JsonProxy( wrapper )

    member this.Dispose () =
        ()

    interface ITypeSerialisable
        with
            member this.Type
                with get () = typeof<JsonProxy> 
                
    interface System.IDisposable
        with
            member this.Dispose () = 
                this.Dispose()
               
    interface ITypeWrapper 
        with 
            member this.ContentType = 
                this.Wrapper.ContentType
                
            member this.TypeName =
                this.Wrapper.TypeName
                
            member this.Body = 
                this.Wrapper.Body          
                    
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerialiser<JsonProxy> 
                with 
                    member this.TypeName =
                        "JsonProxy"

                    member this.Type
                        with get () = typeof<JsonProxy> 
    
                    member this.ContentType 
                        with get () = "binary" 
                                                
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =

                        use bs = 
                            BinarySerialiser.Make( serialiser, stream, this.TypeName )
                         
                        bs.Write( v.Wrapper.ContentType.IsSome )
                        if v.Wrapper.ContentType.IsSome then 
                            bs.Write( v.Wrapper.ContentType.Value )
                            
                        bs.Write( v.Wrapper.TypeName )
                       
                        bs.Write( (int32) v.Wrapper.Body.Length ) 
                        bs.Write( v.Wrapper.Body ) 
                                                       

                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, stream, this.TypeName )
                            
                        let contentType =
                            if bds.ReadBool() then Some( bds.ReadString() ) else None 
                            
                        let typeName = 
                            bds.ReadString() 
                            
                        let body = 
                            bds.ReadBytes( bds.ReadInt32() )
        
                        let wrapper = 
                            TypeWrapper.Make( contentType, typeName, body ) 
                                                                                  
                        JsonProxy.Make( wrapper ) }
                        
    static member JsonSerialiser 
        with get () =   
            { new ITypeSerialiser<JsonProxy> 
                with 
                    member this.TypeName =
                        "JsonProxy"

                    member this.Type
                        with get () = typeof<JsonProxy> 
    
                    member this.ContentType 
                        with get () = "json" 
                                                
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
                    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        if v.Wrapper.ContentType.IsSome then 
                            js.WriteProperty "ContentType"
                            js.Serialise v.Wrapper.ContentType.Value
                       
                        js.WriteProperty "TypeName"
                        js.Serialise v.Wrapper.TypeName
                        
                        js.WriteProperty "Body"
                        js.Serialise ( v.Wrapper.Body |> ToBase64 ) 
                        
                        js.WriteEndObject()
    
        
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
                    
                        use wrapper = 
                            JsonPeekReaderStreamWrapper.Make(stream)
                            
                        let reader = 
                            wrapper.Reader
                                   
                        let tokens = 
                            new System.Collections.Generic.List<ReaderItem>()
                        
                        let nesting = ref 1
                        
                        if reader.Peek().Token <> JsonToken.StartObject then 
                            failwithf "Can only use proxy for serialisation stream that is an object(like)"

                        let proxying = 
                            if reader.PeekTokenAt(1) = JsonToken.PropertyName then 
                                let propertyV = reader.PeekAt(1).Value.ToString()
                                if propertyV = "@type" then
                                    unbox<string>(reader.PeekAt(2).Value)
                                else    
                                    failwithf "First property proxy sees must be '@type'"
                            else 
                                failwithf "Invalid json structure when attempting to deserialise a proxy"                                            
                            
                        tokens.Add( reader.Read() )
                        
                        while (reader.Peek().Token <> JsonToken.EndObject) && !nesting > 0 do
                        
                            if reader.Peek().Token = JsonToken.StartObject then 
                                System.Threading.Interlocked.Increment(nesting) |> ignore
                            elif reader.Peek().Token = JsonToken.EndObject then
                                System.Threading.Interlocked.Decrement(nesting) |> ignore
                            else
                                ()
                                
                            tokens.Add( reader.Read() )
                            
                        tokens.Add( reader.Read() )   
                                 
                        let body = 
                            
                            use ms = 
                                new System.IO.MemoryStream()
                                
                            use writer =
                            
                                use sw = 
                                    new System.IO.StreamWriter( ms, System.Text.Encoding.UTF8, 1024, true )
                                    
                                new JsonTextWriter( sw )   
                                            
                            tokens |> Seq.iter ( fun item ->
                                writer.WriteToken (item.Token,item.Value) )
                                
                            writer.Flush()
                            
                            ms.ToArray() 
                                                                                                          
                        let wrapper =
                            TypeWrapper.Make( Some this.ContentType, proxying, body ) 
                                                                
                        JsonProxy.Make( wrapper ) }
                        
                        