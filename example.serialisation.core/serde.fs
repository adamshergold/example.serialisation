namespace Example.Serialisation

open System
open Microsoft.Extensions.Logging

[<AutoOpen>]
module private SerdeImpl = 

    type CacheItem = {
        Serialiser        : ISerde
        LocalType         : System.Type
        TypeSerialiser    : ITypeSerde
        SerialiseMethod   : System.Reflection.MethodInfo
        DeserialiseMethod : System.Reflection.MethodInfo 
    }
    with 
        member this.Handles (t:System.Type) = 
            t = this.LocalType
            
        member this.Deserialise (s:ISerdeStream) =
            try
                this.DeserialiseMethod.Invoke( this.TypeSerialiser, [| this.Serialiser; s |] )
            with
            | :? System.Reflection.TargetInvocationException as tie ->
                raise tie.InnerException
            | _ as ex ->
                reraise()
            
        member this.Serialise (v:obj) (s:ISerdeStream) =
            try
                this.SerialiseMethod.Invoke( this.TypeSerialiser, [| this.Serialiser; s; v |] ) |> ignore
            with
            | :? System.Reflection.TargetInvocationException as tie ->
                raise tie.InnerException
            | _ as ex ->
                reraise()
                    
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
            
        member this.TryForContentType (ct:string) = 
            items |> Seq.tryFind ( fun i -> i.TypeSerialiser.ContentType = ct )


    
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
            
    let tryLookupByTypeName (contentType:string,typeName:string) =
    
        let finder (kvp:System.Collections.Generic.KeyValuePair<System.Type,ContentTypeItems>) = 
             kvp.Value.TypeName = typeName 
             
        match itemsBySystemType |> Seq.cast |> Seq.tryFind finder with 
        | Some kvp ->
            kvp.Value.TryForContentType contentType
        | None ->
            None

    let tryLookupBySystemType (contentType:string,localType:System.Type) =
        
        let st =
            normaliseType localType
        
        match itemsBySystemType.TryGetValue( st ) with  
        | true, items -> 
            items.TryForContentType contentType 
        | false, _ -> 
            None                  

    let tryLookupTypeName (localType:System.Type) =
        None
        
    member val Options = options
    
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
            | :? ITypeSerde as serialiser ->
                
                let concreteType =
                    serialiser.GetType()
                    
                let concreteInterfaces = 
                    concreteType.GetInterfaces()
                    
                let picker (ifaceType:System.Type) = 
                    if ifaceType.IsGenericType && ifaceType.Name.StartsWith("ITypeSerde") then    
                        let ga = ifaceType.GetGenericArguments()
                        if ga.Length = 1 then Some ifaceType else None
                    else 
                        None              
            
                let genericTypeSerialiserInterface = 
                    concreteInterfaces |> Seq.tryPick picker 
                    
                if genericTypeSerialiserInterface.IsSome then
                
                    let genericSerialiser =
                        genericTypeSerialiserInterface.Value
                          
                    let sm = 
                        genericSerialiser.GetMethod("Serialise")
                    
                    let dm = 
                        genericSerialiser.GetMethod("Deserialise")
                                    
                    let serialisesType =
                        genericSerialiser.GetGenericArguments().[0]
                                                            
                    let ci = 
                        CacheItem.Make( this, serialisesType, serialiser, sm, dm) 

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
                        
                    if pt.IsGenericType && pt.Name.StartsWith("ITypeSerde") then 
                        Some pi
                    else 
                        None 
                         
                let serialisers =
                    st.GetProperties( System.Reflection.BindingFlags.Static ||| System.Reflection.BindingFlags.Public ) 
                    |> Seq.map picker
                    |> Seq.choose ( fun x -> x )
                    |> Array.ofSeq
                    
                let nRegistered =
                    serialisers 
                    |> Seq.fold ( fun acc pi ->
                        let serialiser = pi.GetValue(null,null)
                        let result = this.TryRegister serialiser
                        acc + if result.IsSome then 1 else 0 ) 0 
                    
                nRegistered )
            
        candidates |> Seq.fold ( fun acc v -> acc + v ) 0 
                    
    member this.TrySerdeBySystemType (contentType:string) (t:System.Type) =
        tryLookupBySystemType (contentType,t) |> Option.map ( fun ci -> ci.TypeSerialiser )

    member this.TrySerdeByTypeName (contentType:string) (typeName:string) =
        tryLookupByTypeName (contentType,typeName) |> Option.map ( fun ci -> ci.TypeSerialiser )

    member this.TryLookupTypeName (localType:System.Type) =
        tryLookupTypeName localType
                                
    member this.Serialise (contentType:string) (s:ISerdeStream) (v:obj) =
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
    
    member this.Deserialise (contentType:string) (typeName:string) (s:ISerdeStream) =
        
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
                    
                    TypeWrapper.Make( contentType, Some typeName, msr.ToArray() ) :> obj
                else
                    failwithf "Unable to find serialiser for type name [%s]" typeName               
                        
               
    member this.DeserialiseT<'T> (contentType:string) (s:ISerdeStream) =
        match tryLookupBySystemType (contentType, typeof<'T>) with
        | Some ci ->
            match box(ci.Deserialise s) with 
            | :? 'T as t ->
                t 
            | _ as dv ->
                failwithf "Deserialised value of [%O] was not the expected type of [%O]" dv (typeof<'T>)
        | None ->                      
            failwithf "Unable to find serialiser for system type [%O] / content type [%O]" (typeof<'T>) contentType
                    
    interface ISerde 
        with 
            member this.TryRegister v = 
                this.TryRegister v 
        
            member this.TryRegisterAssembly assy = 
                this.TryRegisterAssembly assy 
                
            member this.Items = 
                this.Items 

            member this.TrySerdeByTypeName (contentType:string,typeName:string) = 
                this.TrySerdeByTypeName contentType typeName 
                
            member this.TrySerdeBySystemType (contentType:string,localType:System.Type) = 
                this.TrySerdeBySystemType contentType localType 

            member this.Serialise contentType serialiserStream v = 
                this.Serialise contentType serialiserStream v                         
                
            member this.Deserialise contentType typeName v = 
                this.Deserialise contentType typeName v 

            member this.DeserialiseT<'T> contentType stream = 
                this.DeserialiseT<'T> contentType stream
                
            member this.Options =
                this.Options
                            
            