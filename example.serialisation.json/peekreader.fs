namespace Example.Serialisation.Json

open Newtonsoft.Json 


type ReaderItem = 
    {
        Token : JsonToken
        Value : obj
    }

type PeekReader( count: int, reader: JsonTextReader ) as this =
    let mutable items = new System.Collections.Generic.Queue<ReaderItem>()
    
    let mutable drained = false 
    
    let rec generator () = 
        seq {
            if reader.Read() then
                yield { Token = reader.TokenType; Value = reader.Value }
                yield! generator() }   

    let check () =
        lock this ( fun _ ->
            if not drained then
                let nSpaces = count - items.Count
                if nSpaces > 0 then
                    generator() |> Seq.truncate nSpaces |> Seq.iter items.Enqueue
                    if items.Count < count then drained <- true )  
                    
    member val Count = count

with
    static member Make( count, reader ) =
        new PeekReader( count, reader )
        
    member this.Dispose () = 
        () 
        
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose() 
                
//    member this.IsEnd () =
//        check()
//        items.Count = 0 
        
    member this.Peek () = 
        check()
        items.Peek() 
        
    member this.PeekTokenAt (idx:int) = 
        this.PeekAt(idx).Token
            
    member this.PeekAt (idx:int) = 
        check()
        if idx > items.Count then
            failwithf "Unable to peek for [%d] - only [%d] items available" idx items.Count
        System.Linq.Enumerable.ElementAt(items,idx)
        
    member this.Read () = 
        lock this ( fun _ ->
            check()
            items.Dequeue() )
    
    member this.ReadProperty () = 
        let item = this.Read()
        if item.Token <> JsonToken.PropertyName then
            failwithf "Expecting to read property but saw [%O]" item.Token 
        else
            unbox<string>( item.Value )
    
    member this.ReadToken (token:JsonToken) = 
        let item = this.Read()
        if item.Token <> token then
            failwithf "Expecting to read [%O] but saw [%O]" token item.Token
            
    member this.ReadString () = 
        let item = this.Read()
        if item.Token <> JsonToken.String then
            failwithf "Expecting to read 'string' but saw [%O]" item.Token
        unbox<string>( item.Value )
        
    member this.Skip () =
        
        if  this.Peek().Token = JsonToken.StartObject
         || this.Peek().Token = JsonToken.StartArray then 
            
            let openingToken =
                this.Peek().Token
                
            let closingToken =
                if openingToken = JsonToken.StartObject then JsonToken.EndObject else JsonToken.EndArray
                
            this.Read() |> ignore
            
            let nesting = ref 1
            
            while !nesting > 0 do 
            
                if this.Peek().Token = openingToken then 
                    System.Threading.Interlocked.Increment(nesting) |> ignore
                elif this.Peek().Token = closingToken then
                    System.Threading.Interlocked.Decrement(nesting) |> ignore
                else
                    ()
                                        
                this.Read() |> ignore
            
        else
            this.Read() |> ignore