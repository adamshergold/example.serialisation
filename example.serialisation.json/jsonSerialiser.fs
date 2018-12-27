namespace Example.Serialisation.Json

open Example.Serialisation
open Newtonsoft.Json 

type JsonSerialiser( serialiser: ISerde, ss: ISerdeStream, contentType:string option ) = 

    let wrapper = 
        JsonTextWriterStreamWrapper.Make(ss) 
    
    let writer = 
        wrapper.Writer 
        
    do
        writer.Formatting <- Newtonsoft.Json.Formatting.Indented
         
    
    static member Make( serialiser, ss, contentType ) = 
        new JsonSerialiser( serialiser, ss, Some contentType )
        
    member this.Dispose () =
        wrapper.Dispose()

    member this.WriteProperty propertyName = 
        writer.WritePropertyName propertyName
    
    member this.WriteToken (token:JsonToken,value:obj) =
        writer.WriteToken( token, value )
        
    member this.WriteValue (v:obj) = 
        writer.WriteValue v 

    member this.WriteNull () = 
        writer.WriteNull()
                  
    member this.WriteStartObject () = 
        writer.WriteStartObject()

    member this.WriteEndObject () = 
        writer.WriteEndObject()
                          
    member this.WriteStartArray() = 
        writer.WriteStartArray()
        
    member this.WriteEndArray () = 
        writer.WriteEndArray() 
       
    member this.Serialise (v:obj) = 
        
        let implements (want:System.Type) (t:System.Type) = 
            t.GetInterfaces() |> Seq.exists ( fun it -> it = want )
            
        let normaliseType (t:System.Type) = 
            if Microsoft.FSharp.Reflection.FSharpType.IsUnion t then
                let cases = 
                    Microsoft.FSharp.Reflection.FSharpType.GetUnionCases t
                if cases.Length = 0 then t else cases.[0].DeclaringType 
            else 
                t

        if v = null then 
            writer.WriteNull()
        else 
            match v with 
            | :? string ->
                writer.WriteValue(v)    
            | :? bool ->
                 writer.WriteValue(v) 
            | :? double ->
                writer.WriteValue(v)
            | :? int32 ->
                writer.WriteValue(v)    
            | :? int64 -> 
                writer.WriteValue(v)
            | :? array<byte> as v -> 
                writer.WriteValue(System.Text.Encoding.UTF8.GetString(v))                
            | :? System.IConvertible as ic ->
                writer.WriteValue (ic.ToString())
            | :? ITypeSerialisable as v ->
                serialiser.Serialise contentType wrapper v
            | _ -> 
                failwithf "Do not know how to serialise [%O]" (v.GetType())
                                                                      
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose()
                