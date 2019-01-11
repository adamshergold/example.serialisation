namespace Example.Serialisation.Binary

open Example.Serialisation

module Serialisers =
    
    let AnyBinarySerialiser = 
        { new ITypeSerde<Any> 
            with 
                member this.TypeName =
                    "Any"

//                member this.Type 
//                    with get () = typeof<Any>

                member this.ContentType 
                    with get () = "binary" 
                                            
                member this.Serialise (serde:ISerde) (stream:ISerdeStream) v =
                
                    use bs = 
                        BinarySerialiser.Make( serde, stream, this.TypeName )

                    let rec impl (iv:Any) =                                        
                        match iv with 
                        | Int32(v) ->
                            bs.Write( (int8) 0 )
                            bs.Write( v )
                        | Int64(v) ->
                            bs.Write( (int8) 1 )
                            bs.Write( v )
                        | String(v) ->
                            bs.Write( (int8) 2 )
                            bs.Write( v )
                        | Double(v) ->
                            bs.Write( (int8) 3 )
                            bs.Write( v )
                        | Bool(v) ->
                            bs.Write( (int8) 4 )
                            bs.Write( v )
                        | Record(v) ->
                            bs.Write( (int8) 7 )
                            bs.Write( v )
                        | Union(v) ->
                            bs.Write( (int8) 8 )
                            bs.Write( v )
                        | Array(v) ->
                            bs.Write( (int8) 9 )
                            bs.Write( (int32) v.Length )
                            v |> Seq.iter impl
                        | Map(v) ->
                            bs.Write( (int8) 9 )
                            bs.Write( (int32) v.Count )
                            v
                            |> Map.iter ( fun k v ->
                                bs.Write( k )
                                impl v )
                   
                    impl v
                    
                member this.Deserialise (serde:ISerde) (stream:ISerdeStream) =
                
                    use bds = 
                        BinaryDeserialiser.Make( serde, stream, this.TypeName )
                    
                    let rec impl () =
                        match bds.ReadInt8() with
                        | 0uy ->
                            Any.Int32( bds.ReadInt32() )
                        | 1uy ->
                            Any.Int64( bds.ReadInt64() )
                        | 2uy ->
                            Any.String( bds.ReadString() )
                        | 3uy ->
                            Any.Double( bds.ReadDouble() )
                        | 4uy ->
                            Any.Bool( bds.ReadBool() )
                        | 7uy ->
                            Any.Record( bds.ReadSerialisable() )
                        | 8uy ->
                            Any.Union( bds.ReadSerialisable() )
                        | 9uy ->
                            let n =
                                bds.ReadInt32()
                                    
                            let vs =
                                [| 0 .. n-1 |]
                                |> Array.map ( fun _ -> impl() )
                                    
                            Any.Array( vs )
                        | 10uy ->
                            let n =
                                    bds.ReadInt32()
                            let vs =
                                [| 0 .. n-1 |]
                                |> Array.map ( fun _ ->
                                    let k = bds.ReadString()
                                    let v = impl()
                                    k,v )
                                |> Map.ofSeq
                                
                            Any.Map( vs )
                        | _ as v ->
                            failwithf "Unable to deserialise binary Any case '%u'" v
                    
                    let result =
                        impl()
                        
                    result }         
