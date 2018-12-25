namespace Example.Serialisation.Json

open Newtonsoft.Json 

type IPropertyHandler =
    abstract On : string -> ( unit -> obj ) -> unit
    abstract Handle : string -> unit 
    abstract Has : string -> bool
    abstract TryItem<'T> : string -> 'T option


type PropertyHandler( ) =

    let cbs = 
        new System.Collections.Generic.Dictionary<string,unit->obj>()
     
    let items = 
        new System.Collections.Generic.Dictionary<string,obj>()
    
with
    static member Make() = 
        new PropertyHandler() :> IPropertyHandler
    
    member this.Dispose () = 
        cbs.Clear()
        items.Clear()
       
       
    member this.Has property = 
        items.ContainsKey property 
               
    member this.Handle property =
        match cbs.TryGetValue property with
        | false, _ ->
            ()
        | true, cb ->
            items.Add( property, cb() )
        
    member this.On (property:string) (cb:unit->obj) =
        cbs.Add( property, cb )
        
    member this.For (property:string) = 
        match cbs.TryGetValue property with
        | false, _ -> None
        | true, cb -> Some cb
        
    member this.TryItem<'T> property =
        match items.TryGetValue property with
        | false, _ ->
            None
        | true, v ->
            match v with 
            | :? 'T as t -> 
                Some t
            | _ ->
                let asT = typeof<'T> 
                failwithf "Attempted to extract property [%s] as [%O] but was [%O]" property asT.Name (v.GetType().Name)
            
    interface System.IDisposable 
        with 
            member this.Dispose () = 
                this.Dispose()
                        
    interface IPropertyHandler
        with
            member this.Handle property =
                this.Handle property
                
            member this.Has property = 
                this.Has property 
                
            member this.TryItem<'T> property = 
                this.TryItem<'T> property 
                
            member this.On property cb = 
                this.On property cb
       