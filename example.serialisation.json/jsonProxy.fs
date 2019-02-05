namespace Example.Serialisation.Json

open Newtonsoft.Json

open System
open Example.Serialisation
open Example.Serialisation.Binary


[<AutoOpen>]
module private JsonProxyImpl = 

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
            { new ITypeSerde<JsonProxy> 
                with 
                    member this.TypeName =
                        "JsonProxy"

                    member this.ContentType 
                        with get () = "binary" 
                                                
                    member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =

                        use bs = 
                            BinarySerialiser.Make( serde, stream, this.TypeName )
                         
                        bs.Write( v.Wrapper.ContentType )
                            
                        bs.Write( v.Wrapper.TypeName.IsSome )
                        if v.Wrapper.TypeName.IsSome then 
                            bs.Write( v.Wrapper.TypeName.Value )
                       
                        bs.Write( (int32) v.Wrapper.Body.Length ) 
                        bs.Write( v.Wrapper.Body ) 
                                                       

                    member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serde, stream, this.TypeName )
                            
                        let contentType =
                            bds.ReadString()  
                            
                        let typeName = 
                            if bds.ReadBoolean() then Some( bds.ReadString() ) else None 
                            
                        let body = 
                            bds.ReadBytes( bds.ReadInt32() )
        
                        let wrapper = 
                            TypeWrapper.Make( contentType, typeName, body ) 
                                                                                  
                        JsonProxy.Make( wrapper ) }
                        
    static member JsonSerialiser 
        with get () =   
            { new ITypeSerde<JsonProxy> 
                with 
                    member this.TypeName =
                        "JsonProxy"

//                    member this.Type
//                        with get () = typeof<JsonProxy> 
    
                    member this.ContentType 
                        with get () = "json" 
                                                
                    member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
                    
                        use js =
                            JsonSerialiser.Make( serde, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty serde.Options.TypeProperty
                        js.WriteValue this.TypeName
    
                        js.WriteProperty "ContentType"
                        js.Serialise v.Wrapper.ContentType
                       
                        if v.Wrapper.TypeName.IsSome then
                            js.WriteProperty "TypeName"
                            js.Serialise v.Wrapper.TypeName.Value
                        
                        js.WriteProperty "Body"
                        js.Serialise ( v.Wrapper.Body |> ToBase64 ) 
                        
                        js.WriteEndObject()
    
        
                    member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                    
                        use wrapper = 
                            JsonPeekReaderStreamWrapper.Make(stream)
                            
                        let reader = 
                            wrapper.Reader
                                   
                        let tokens = 
                            new System.Collections.Generic.List<ReaderItem>()
                        
                        if reader.Peek().Token <> JsonToken.StartObject then 
                            failwithf "Can only use proxy for serialisation stream that is an object(like)"

                        let proxying =
                            
                            if reader.PeekTokenAt(1) = JsonToken.PropertyName then
                                
                                let propertyV =
                                    reader.PeekAt(1).Value.ToString()
                                
                                if  ( serde.Options.AllowableTypeProperties.IsNone && propertyV.Equals( serde.Options.TypeProperty, StringComparison.Ordinal ) )
                                 || ( serde.Options.AllowableTypeProperties.IsSome && serde.Options.AllowableTypeProperties.Value.Contains( propertyV ) ) then     
                                    Some <| unbox<string>( reader.PeekAt(2).Value )
                                else    
                                    None
                            else 
                                failwithf "Invalid json structure when attempting to deserialise a proxy"                                            
                            
                        let wrapper =
                            
                            if proxying.IsSome && proxying.Value.Equals( "JsonProxy", System.StringComparison.Ordinal ) then
                                
                                use jds =
                                    JsonDeserialiser.Make( serde, wrapper, this.ContentType, this.TypeName )
            
                                jds.Handlers.On "ContentType" ( jds.ReadString )
                                jds.Handlers.On "TypeName" ( jds.ReadString )
                                jds.Handlers.On "Body" ( jds.ReadString )
            
                                jds.Deserialise()

                                let contentType =
                                    jds.Handlers.TryItem<string>( "ContentType").Value
                                    
                                let typeName =
                                    jds.Handlers.TryItem<string>( "TypeName" )
                                    
                                let bodyBase64 =
                                    jds.Handlers.TryItem<string>( "Body" ).Value
                                    
                                let body =
                                    bodyBase64 |> FromBase64
                                
                                TypeWrapper.Make( contentType, typeName, body )
                                
                            else
                                let nesting = ref 1
                                
                                tokens.Add( reader.Read() )
                                
                                while !nesting > 0 do
                                    
                                    if reader.Peek().Token = JsonToken.StartObject then 
                                        System.Threading.Interlocked.Increment(nesting) |> ignore
                                    elif reader.Peek().Token = JsonToken.EndObject then
                                        System.Threading.Interlocked.Decrement(nesting) |> ignore
                                    else
                                        ()
                                        
                                    tokens.Add( reader.Read() )
                                    
                                let body = 
                                    
                                    use ms = 
                                        new System.IO.MemoryStream()
                                        
                                    use writer =
                                    
                                        use sw = 
                                            // Must do this to avoid BOM marker at start of stream
                                            new System.IO.StreamWriter( ms, new System.Text.UTF8Encoding( false ), 1024, true )
                                            
                                        new JsonTextWriter( sw )   
                                                    
                                    tokens |> Seq.iter ( fun item ->
                                        writer.WriteToken (item.Token,item.Value) )
                                        
                                    writer.Flush()
                                    
                                    ms.ToArray() 
                                                                                                              
                                TypeWrapper.Make( this.ContentType, proxying, body ) 
                                                                
                        JsonProxy.Make( wrapper ) }
                        
                        