namespace Example.Serialisation.Json

open Newtonsoft.Json 

open Example.Serialisation 

module Serialisers =
    
    let AnyJsonSerialiser = 
            { new ITypeSerialiser<Any> 
                with 
                    member this.TypeName =
                        "Any"

                    member this.Type 
                        with get () = typeof<Any>
    
                    member this.ContentType 
                        with get () = "json" 
                                                
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
                    
                        use js = 
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
                                        
                        let typeToString () = 
                            match v with 
                            | Int32(_) -> "int32"
                            | Int64(_) -> "int64"
                            | String(_) -> "string"
                            | Double(_) -> "double"
                            | Bool(_) -> "bool"
                            | Record(_) -> "record"
                            | Union(_) -> "union"
                            | Array(_) -> "array"
                            | Map(_) -> "map"
                             
                        js.WriteStartObject()
                        
                        js.WriteProperty "@any"
                        js.WriteValue (typeToString() )
                        
                        js.WriteProperty "@value"
                        
                        match v with 
                        | Array(items) ->
                            js.WriteStartArray()
                            items |> Seq.iter ( fun v -> js.Serialise v ) 
                            js.WriteEndArray() 
                        | Map(items) ->
                            js.WriteStartObject()
                            items 
                            |> Map.toSeq
                            |> Seq.iter ( fun (mk,mv) ->
                                js.WriteProperty mk
                                js.Serialise mv ) 
                            js.WriteEndObject()
                        | Int32(v)->            
                            js.WriteValue v 
                        | Int64(v)->            
                            js.WriteValue v 
                        | String(v)->            
                            js.WriteValue v 
                        | Double(v)->            
                            js.WriteValue v 
                        | Bool(v)->            
                            js.WriteValue v 
                        | Record(v) ->
                            js.Serialise v 
                        | Union(v) ->
                            js.Serialise v 
                        
                        js.WriteEndObject()
                
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
                    
                        use jds = 
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
                        
                        let reader = 
                            jds.Reader 
                
                        reader.ReadToken JsonToken.StartObject
                        
                        let typProperty = 
                            reader.ReadProperty()
                
                        if typProperty <> "@any" then 
                            failwithf "Unexpected to see '@any' property when deserialising Any" 
                                         
                        let typ = 
                            reader.ReadString()
                            
                        let valProperty = 
                            reader.ReadProperty()
                
                        if valProperty <> "@value" then 
                            failwithf "Unexpected to see '@value' property when deserialising Any"
                            
                        let result = 
                            match typ.ToLower() with 
                            | "int32" -> 
                                Any.Int32( jds.ReadInt32() :?> int32 )
                            | "int64" ->
                                Any.Int64( jds.ReadInt64() :?> int64 )
                            | "string" ->
                                Any.String( jds.ReadString() :?> string )
                            | "double" ->
                                Any.Double( jds.ReadDouble() :?> double )
                            | "bool" ->
                                Any.Bool( jds.ReadBool() :?> bool )
                            | "record" ->
                                let v = jds.ReadAnyRecord()
                                Any.Record( v :?> ITypeSerialisable )
                            | "union" ->
                                let v = jds.ReadAnyUnion() 
                                Any.Record( v :?> ITypeSerialisable )
                            | "array" ->
                                let ra = jds.ReadArray<Any>( jds.ReadAny )
                                let vs = unbox<Any[]>( ra() )
                                Any.Array( vs )
                            | "map" ->
                                let rm = jds.ReadMap<Any>( jds.ReadAny )
                                let vs = unbox<Map<string,Any>>( rm() ) 
                                Any.Map( vs )
                            | _ ->
                                failwithf "Unable to handle type of '%s' for Any deserialisation" typ
                            
                        reader.ReadToken JsonToken.EndObject   
                        
                        result }         

                    