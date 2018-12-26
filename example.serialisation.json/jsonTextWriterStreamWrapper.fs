namespace Example.Serialisation.Json

open Example.Serialisation
open Newtonsoft.Json 

type JsonTextWriterStreamWrapper( ss: ISerdeStream, writer: JsonTextWriter, owner: bool ) = 
        
    member val Writer = writer 
            
    static member Make( input : ISerdeStream ) =
    
        match input with 
        | :? JsonTextWriterStreamWrapper as sw -> 
            sw.Child()
             
        | _ ->
            use writer =
                use sw = 
                    new System.IO.StreamWriter(input.Stream, System.Text.Encoding.UTF8, 1024, true )
                new JsonTextWriter( sw )   
                
            new JsonTextWriterStreamWrapper( input, writer, true ) 

    member this.Child () = 
        new JsonTextWriterStreamWrapper( ss, this.Writer, false )
        
    member this.Dispose () =
        if owner then  
            writer.Close()
            
    interface System.IDisposable 
        with 
            member this.Dispose () = 
                this.Dispose() 
                        
    interface ISerdeStream 
        with 
            member this.Stream = ss.Stream