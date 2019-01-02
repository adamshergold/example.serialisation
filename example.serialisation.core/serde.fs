namespace Example.Serialisation

open Microsoft.Extensions.Logging

[<AutoOpen>]
module SerdeImpl = 

    type CacheItem = {
        Serialiser        : ISerde
        LocalType         : System.Type
        TypeSerialiser    : ITypeSerialiser
        SerialiseMethod   : System.Reflection.MethodInfo
        DeserialiseMethod : System.Reflection.MethodInfo 
    }
    with 
        member this.Handles (t:System.Type) = 
            t = this.LocalType
            
        member this.Deserialise (s:ISerdeStream) = 
            this.DeserialiseMethod.Invoke( this.TypeSerialiser, [| this.Serialiser; s |] )
            
        member this.Serialise (v:obj) (s:ISerdeStream) = 
            this.SerialiseMethod.Invoke( this.TypeSerialiser, [| this.Serialiser; s; v |] ) |> ignore
            
        static member Make( serialiser, lt, ts, sm, dm ) = 
            { Serialiser = serialiser; LocalType = lt; TypeSerialiser = ts; SerialiseMethod = sm; DeserialiseMethod = dm  }
    
    type ContentTypeItems( typeName: string ) = 
    
        let items = 
            new System.Collections.Generic.LinkedList<CacheItem>()
        
        static member Make( localType ) = 
            new ContentTypeItems( localType )
            
        member this.TypeName 
            with get () = typeName 
                        
        member this.Items 
            with get () = items |> Seq.cast<CacheItem>
                        
        member this.Add (ci:CacheItem) = 
            match items |> Seq.tryFind ( fun i -> i.TypeSerialiser.ContentType = ci.TypeSerialiser.ContentType ) with 
            | Some item -> ()
            | None -> items.AddFirst( ci ) |> ignore
            
        member this.TryForContentType (ct:string option) = 
            if ct.IsSome then
                items |> Seq.tryFind ( fun i -> i.TypeSerialiser.ContentType = ct.Value )
            else 
                Some this.Default   

        member this.Default 
            with get () = if items.Count = 1 then items.First.Value else failwithf "Unable to extract default as [%d] items" items.Count                            
            
type SerdeOptions = {
    Logger : ILogger option
    TypeWrapperFallback : bool
    FallbackDeserialisers : Map<string option,string>
}
with 
    static member Default = {
        Logger = None
        TypeWrapperFallback = false 
        FallbackDeserialisers = Map.empty
    }
             
type Serde( options: SerdeOptions ) =

    let itemsBySystemType = 
        // key = (localType)
        new System.Collections.Generic.Dictionary<System.Type,ContentTypeItems>()

    let normaliseType (t:System.Type) = 
        if Microsoft.FSharp.Reflection.FSharpType.IsUnion t then
            let cases = 
                Microsoft.FSharp.Reflection.FSharpType.GetUnionCases t
            if cases.Length = 0 then t else cases.[0].DeclaringType 
        else 
            t
            
    let tryLookupByTypeName (contentType:string option,typeName:string) =
    
        let finder (kvp:System.Collections.Generic.KeyValuePair<System.Type,ContentTypeItems>) = 
             kvp.Value.TypeName = typeName 
             
        match itemsBySystemType |> Seq.cast |> Seq.tryFind finder with 
        | Some kvp ->
            kvp.Value.TryForContentType contentType
        | None ->
            None

    let tryLookupBySystemType (contentType:string option,localType:System.Type) =
        let st = normaliseType localType 
        match itemsBySystemType.TryGetValue( st ) with  
        | true, items -> 
            items.TryForContentType contentType 
        | false, _ -> 
            None                  

    static member BinaryWriter (s:System.IO.Stream) = 
        new System.IO.BinaryWriter( s, System.Text.Encoding.Default, true )

    static member BinaryReader (s:System.IO.Stream) = 
        new System.IO.BinaryReader( s, System.Text.Encoding.Default, true )
        
    static member Make( options ) = 
        new Serde( options ) :> ISerde

    static member Make() = 
        new Serde( SerdeOptions.Default ) :> ISerde
        
    member this.Items
        with get () = 
            itemsBySystemType.Values 
            |> Seq.cast<ContentTypeItems>
            |> Seq.map ( fun contentTypeItems -> contentTypeItems.Items )
            |> Seq.concat
            |> Seq.map ( fun ci -> ci.TypeSerialiser )

    member this.TryRegister (v:obj) = 
        
        lock this ( fun () ->
            match v with 
            | :? ITypeSerialiser as serialiser ->
                
                let inputType = 
                    serialiser.GetType() 
                    
                let ifaces = 
                    inputType.GetInterfaces()
                    
                let finder (it:System.Type) = 
                    if it.IsGenericType && it.Name.StartsWith("ITypeSerialiser") then    
                        let ga = it.GetGenericArguments()
                        if ga.Length = 1 then Some it else None
                    else 
                        None              
            
                let result = 
                    ifaces |> Seq.tryPick finder 
                    
                if result.IsSome then
                
                    let typedSerialiser =
                        result.Value
                          
                    let sm = 
                        typedSerialiser.GetMethod("Serialise")
                    
                    let dm = 
                        typedSerialiser.GetMethod("Deserialise")
                                    
                    let localType =     
                        serialiser.Type 
                                                            
                    let ci = 
                        CacheItem.Make( this, localType, serialiser, sm, dm) 

                    if itemsBySystemType.ContainsKey( ci.LocalType ) then 
                        itemsBySystemType.Item( ci.LocalType ).Add( ci ) 
                    else                    
                        let cti = ContentTypeItems.Make( ci.TypeSerialiser.TypeName )
                        cti.Add( ci )
                        itemsBySystemType.Add( ci.LocalType, cti )
                        
                    Some ci.TypeSerialiser  
                else
                    None
            | _ ->  
                None )

    member this.TryRegisterAssembly (assy:System.Reflection.Assembly) =
    
        let candidates = 
            assy.GetTypes() 
            |> Seq.map ( fun (st:System.Type) ->
            
                let picker (pi:System.Reflection.PropertyInfo) =
                 
                    let pt = 
                        pi.PropertyType
                        
                    if pt.IsGenericType && pt.Name.StartsWith("ITypeSerialiser") then 
                        Some pi
                    else 
                        None 
                         
                let serialisers =
                    st.GetProperties( System.Reflection.BindingFlags.Static ||| System.Reflection.BindingFlags.Public ) 
                    |> Seq.map picker
                    |> Seq.choose ( fun x -> x )
                    |> Array.ofSeq
                    
                let typeSerialisers = 
                    serialisers 
                    |> Seq.map ( fun pi ->
                        let serialiser = pi.GetValue(null,null)
                        this.TryRegister serialiser )
                    
                typeSerialisers
                |> Seq.choose ( fun tso -> tso ) 
                |> Seq.iter ( fun (ts:ITypeSerialiser) ->
                    if options.Logger.IsSome then 
                        options.Logger.Value.LogTrace( "Serialiser::TryRegisterAssembly - Added {TypeName} / {ContentType} / {SystemType}", ts.TypeName, ts.ContentType, ts.Type ) )
                                         
                serialisers |> Seq.length )                    

        candidates |> Seq.fold ( fun acc v -> acc + v ) 0 
                    
    member this.TryLooukupBySystemType (contentType:string option) (t:System.Type) = 
        let nt = normaliseType t
        match itemsBySystemType.TryGetValue( nt ) with 
        | true, items -> 
            items.TryForContentType contentType
        | false, _ -> 
            None  

    member this.TryLookup (contentType:string option,typeName:string) =
        tryLookupByTypeName (contentType,typeName) |> Option.map ( fun ci -> ci.TypeSerialiser )

    member this.TryLookup (contentType:string option,lt:System.Type) =
        tryLookupBySystemType (contentType,lt) |> Option.map ( fun ci -> ci.TypeSerialiser )

    member this.TypeName (contentType:string option) (t:System.Type) =
        match tryLookupBySystemType (contentType,t) with 
        | Some ci -> ci.TypeSerialiser.TypeName
        | None -> failwithf "Unable to find serialier for system type [%O] / content-type [%O]" t contentType
         
    member this.SystemType (contentType:string option) (tn:string) =
        match tryLookupByTypeName (contentType,tn) with 
        | Some ci -> ci.LocalType 
        | None -> failwithf "Unable to find serialier for type name [%s] / content-type [%O]" tn contentType
        
                            
    member this.Serialise (contentType:string option) (s:ISerdeStream) (v:obj) =
        let nt = normaliseType (v.GetType())
        match itemsBySystemType.TryGetValue( nt ) with 
        | true, items ->
            match items.TryForContentType contentType with 
            | Some ci ->
                ci.Serialise v s
            | None ->
                failwithf "Unable to find serialiser for system type [%O] / content type [%O]" nt contentType
        | false, _ -> 
            failwithf "Unable to find serialiser for system type [%O]" nt     

    member this.Extract (contentType:string option) (typeName:string) (s:ISerdeStream) =
    
        use msr = 
            new System.IO.MemoryStream()
            
        s.Stream.CopyTo(msr)
        
        TypeWrapper.Make( contentType, typeName, msr.ToArray() ) 
    
    member this.Deserialise (contentType:string option) (typeName:string) (s:ISerdeStream) =
        
        match tryLookupByTypeName (contentType,typeName) with 
        | Some ci ->
            ci.Deserialise s

        | None ->
        
            match options.FallbackDeserialisers.TryFind contentType with 
            | Some fallbackTypeName ->
            
                match tryLookupByTypeName (contentType,fallbackTypeName) with 
                | Some ci -> 
                    ci.Deserialise s 
                | None ->
                    failwithf "Fallback serialiser '%s' could not be found!" fallbackTypeName
                    
            | None ->
            
                if options.TypeWrapperFallback then
                 
                    use msr = 
                        new System.IO.MemoryStream()
                        
                    s.Stream.CopyTo(msr)
                    
                    TypeWrapper.Make( contentType, typeName, msr.ToArray() ) :> obj
                else
                    failwithf "Unable to find serialiser for type name [%s]" typeName               
                        
               
    member this.DeserialiseT<'T> (contentType:string option) (s:ISerdeStream) =
        match itemsBySystemType.TryGetValue( typeof<'T> ) with 
        | true, items ->
            match items.TryForContentType contentType with 
            | Some ci ->
                match box(ci.Deserialise s) with 
                | :? 'T as t -> t 
                | _ as dv -> failwithf "Deserialised value of [%O] was not the expected type of [%O]" dv (typeof<'T>)
            | None ->
                 failwithf "Unable to find serialiser for system type [%O] / content type [%O]" (typeof<'T>) contentType
        | false, _ ->                      
            failwithf "Unable to find serialiser for system type [%O] / content type [%O]" (typeof<'T>) contentType
                    
    interface ISerde 
        with 
            member this.TryRegister v = 
                this.TryRegister v 
        
            member this.TryRegisterAssembly assy = 
                this.TryRegisterAssembly assy 
                
            member this.Items = 
                this.Items 

            member this.TryLookupByTypeName (contentType:string option,typeName:string) = 
                this.TryLookup (contentType,typeName)
                
            member this.TryLookupBySystemType (contentType:string option,localType:System.Type) = 
                this.TryLookup (contentType,localType) 
                                
            member this.Serialise contentType serialiserStream v = 
                this.Serialise contentType serialiserStream v                         
                
            member this.Deserialise contentType typeName v = 
                this.Deserialise contentType typeName v 

            member this.Extract contentType typeName s = 
                this.Extract contentType typeName s 

            member this.DeserialiseT<'T> contentType stream = 
                this.DeserialiseT<'T> contentType stream

            member this.SystemType contentType tn = 
                this.SystemType contentType tn 
                
            member this.TypeName contentType st = 
                this.TypeName contentType st 
                
                
                            
            