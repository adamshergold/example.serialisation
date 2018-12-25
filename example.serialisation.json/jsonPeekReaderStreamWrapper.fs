namespace Example.Serialisation.Json

open Example.Serialisation 

open System
open Newtonsoft.Json 

type JsonPeekReaderStreamWrapper( ss: ISerdeStream, reader: PeekReader, owner: bool ) = 
        
    member val Reader = reader 
            
    static member Make( input : ISerdeStream ) =
    
        match input with 
        | :? JsonPeekReaderStreamWrapper as sw -> 
            sw.Child()
             
        | _ ->
        
            use tr = 
                new System.IO.StreamReader( input.Stream, System.Text.Encoding.UTF8, true, 1024, true ) 
            
            let jtr = 
                new JsonTextReader( tr ) 
            
            let reader = 
                new PeekReader( 3, jtr ) 

            new JsonPeekReaderStreamWrapper( input, reader, true ) 

    member this.Child () = 
        new JsonPeekReaderStreamWrapper( ss, this.Reader, false )
        
    member this.Dispose () =
        if owner then  
            reader.Dispose()
            
    interface System.IDisposable 
        with 
            member this.Dispose () = 
                this.Dispose() 
                        
    interface ISerdeStream 
        with 
            member this.Stream = ss.Stream