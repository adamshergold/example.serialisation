namespace Example.Serialisation

type SerialiserStreamWrapper( s: System.IO.Stream ) = 

    member val Stream = s
    
    static member Make( s ) = 
        new SerialiserStreamWrapper( s ) :> ISerdeStream
    
    member this.Dispose () = 
        () 
                    
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose()
                          
    interface ISerdeStream
        with 
            member this.Stream = this.Stream        



    