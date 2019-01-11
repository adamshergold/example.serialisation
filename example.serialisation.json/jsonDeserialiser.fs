namespace Example.Serialisation.Json

open Newtonsoft.Json

open System
open Example.Serialisation
             
type JsonDeserialiser( serde: ISerde, stream: ISerdeStream, contentType : string, typeName: string ) = 

    let wrapper = 
        JsonPeekReaderStreamWrapper.Make(stream) 
    
    let reader = 
        wrapper.Reader 

    let handlers = 
        PropertyHandler.Make( reader, serde.Options.PropertyNameStringComparison )
    
    member val Handlers = handlers 
                  
    member val Reader = reader 
                      
    static member Make( serialiser, stream, contentType, typeName ) = 
        new JsonDeserialiser( serialiser, stream, contentType, typeName )

    member this.Dispose () = 
        wrapper.Dispose()

    member this.ReadString () = 
        System.Convert.ToString( reader.Read().Value ) :> obj

    member this.ReadInt32 () = 
        System.Convert.ToInt32( reader.Read().Value ) :> obj

    member this.ReadInt64 () = 
        System.Convert.ToInt64( reader.Read().Value ) :> obj

    member this.ReadDouble () = 
        System.Convert.ToDouble( reader.Read().Value ) :> obj
         
    member this.ReadBool () = 
        System.Convert.ToBoolean( reader.Read().Value ) :> obj
                     
    member this.ReadEnum<'T> () =    
        let strV = reader.Read().Value.ToString()
        try  
            box <| System.Enum.Parse( typeof<'T>, strV ) 
        with 
        | _ as ex ->
            failwithf "Failed to parse '%s' as enum of type '%O'" strV (typeof<'T>)

    member this.ReadNull () = 
        reader.ReadToken JsonToken.Null
        null :> obj

    member this.InferType () =
        if  reader.PeekTokenAt(0) = JsonToken.StartObject
         && reader.PeekTokenAt(1) = JsonToken.PropertyName then
                
            let propertyV =
                reader.PeekAt(1).Value.ToString()
            
            if  ( serde.Options.AllowableTypeProperties.IsNone && propertyV.Equals( serde.Options.TypeProperty, System.StringComparison.Ordinal ) )
             || ( serde.Options.AllowableTypeProperties.IsSome && serde.Options.AllowableTypeProperties.Value.Contains( propertyV ) ) then
                
                let inlineTypeName =
                    unbox<string>(reader.PeekAt(2).Value)
                    
                match serde.TrySerdeByTypeName (contentType,inlineTypeName) with 
                | Some _ ->
                    Some inlineTypeName
                | None ->
                    Some "JsonProxy"
                
            else if propertyV = "@any" then
                Some "Any"
            else
                None
        else 
            None

    member this.ReadAny = 
        fun () ->   
            serde.Deserialise contentType "Any" wrapper 
            
    member this.ReadSerialisable = 
        fun () ->   
            this.ReadAnyRecord()
                
    member this.ReadRecord (expectedTypeName:string) = 
        fun () ->
            match this.InferType() with 
            | Some typeName -> 
                serde.Deserialise contentType typeName wrapper
            | None ->
                failwithf "Unable to infer type name when attempting to ReadRecord(%s)" typeName
                                  
    member this.ReadInterface (ifaceName:string) = 

        fun () ->
            if reader.PeekTokenAt(0) = JsonToken.StartObject &&
               reader.PeekTokenAt(1) = JsonToken.PropertyName then 
                let inlineTypeName = unbox<string>(reader.PeekAt(2).Value)
                serde.Deserialise contentType inlineTypeName wrapper 
            else 
                failwithf "Unexpected token stream for an interface!"

    member this.ReadAnyUnion = 
        fun () ->
            match this.InferType() with 
            | Some typeName -> 
                serde.Deserialise contentType typeName wrapper
            | None ->
                failwithf "Unable to infer type name when attempting to ReadUnion" 

    member this.ReadAnyRecord = 
        fun () ->
            match this.InferType() with 
            | Some typeName -> 
                serde.Deserialise contentType typeName wrapper
            | None ->
                failwithf "Unable to infer type name when attempting to ReadRecord" 
                            
    member this.ReadUnion (expectedTypeName:string) = 
        fun () ->
            match this.InferType() with 
            | Some typeName -> 
                serde.Deserialise contentType typeName wrapper
            | None ->
                failwithf "Unable to infer type name when attempting to ReadUnion" 

    member this.ReadSet<'T when 'T:comparison> (cb:unit -> obj) =

        fun () ->
            if reader.Read().Token <> JsonToken.StartArray then
                failwith "Wasn't an array"
            
            let vs = new System.Collections.Generic.List<'T>()
            
            while reader.PeekAt(0).Token <> JsonToken.EndArray do
                vs.Add( cb() :?> 'T )
                
            reader.Read() |> ignore
            
            vs |> Set.ofSeq :> obj 
                                                                          
    member this.ReadArray<'T> (cb:unit -> obj) =
    
        fun () ->
            if reader.Read().Token <> JsonToken.StartArray then
                failwith "Wasn't an array"
            
            let vs = new System.Collections.Generic.List<'T>()
            
            while reader.PeekAt(0).Token <> JsonToken.EndArray do
                vs.Add( cb() :?> 'T )
                
            reader.Read() |> ignore
            
            vs.ToArray() :> obj

    member this.ReadMap<'T> (cb:unit -> obj) = 
    
        fun () ->
            reader.ReadToken JsonToken.StartObject
            
            let vs = new System.Collections.Generic.Dictionary<string,'T>()
            
            while reader.PeekAt(0).Token <> JsonToken.EndObject do
                let key = reader.ReadProperty()
                vs.Add( key, cb() :?> 'T )
                
            reader.ReadToken JsonToken.EndObject
            
            vs |> Seq.map ( fun kvp -> kvp.Key, kvp.Value ) |> Map.ofSeq |> box 

    member this.Deserialise () = 

        reader.ReadToken JsonToken.StartObject 

        if serde.Options.StrictInlineTypeCheck then
            
            if  ( reader.PeekTokenAt(0) = JsonToken.PropertyName )
             && ( reader.PeekAt(0).Value.ToString().Equals( serde.Options.TypeProperty, StringComparison.Ordinal ) ) then

                let _ =
                    reader.ReadProperty()
                
                let inlineTypeName =
                    unbox<string>( reader.ReadString() )

                if inlineTypeName <> typeName then  
                    failwithf "Attempted to deserialise [%s] but expecting [%s]" inlineTypeName typeName
            else
                failwithf "Strict in-line type checking expects '%s' to be first property" serde.Options.TypeProperty
          
        while reader.PeekAt(0).Token <> JsonToken.EndObject do
            let propertyName = reader.ReadProperty()        
            handlers.Handle propertyName 

        reader.ReadToken JsonToken.EndObject
                                 
        
    interface System.IDisposable
        with    
            member this.Dispose () = 
                this.Dispose() 
