namespace Example.Serialisation.Json

open Example.Serialisation
//open Newtonsoft.Json 

open NodaTime

type JsonSerialiser( serialiser: ISerde, ss: ISerdeStream, contentType:string ) = 

    let wrapper = 
        JsonTextWriterStreamWrapper.Make(ss) 
    
    let writer = 
        wrapper.Writer 
        
    do
        writer.Formatting <- Newtonsoft.Json.Formatting.Indented
    
    static member Make( serialiser, ss, contentType ) = 
        new JsonSerialiser( serialiser, ss, contentType )
        
    member this.Dispose () =
        wrapper.Dispose()

    member this.WriteProperty propertyName = 
        writer.WritePropertyName propertyName
    
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
            | :? LocalDate as v ->
                writer.WriteValue( Noda.LocalDateToString v )
            | :? LocalDateTime as v ->
                writer.WriteValue( Noda.LocalDateTimeToString v )
            | :? ZonedDateTime as v ->
                writer.WriteValue( Noda.ZonedDateTimeToString v )
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
                