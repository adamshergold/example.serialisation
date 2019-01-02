namespace Example.Serialisation.Binary.Tests

open Example.Serialisation

open System.IO
open Example.Serialisation.Binary
open Example.Serialisation.Binary
open Xunit
open Xunit.Abstractions

type BinaryDeserialiserShould( oh: ITestOutputHelper ) =
    
    let logger =
        let options = { Logging.Options.Default with OutputHelper = Some oh } 
        Logging.CreateLogger options

    let serde = 
        Helpers.DefaultSerde
            
    let sut (ms:System.IO.MemoryStream) =
        
        let serdeStream =
            SerdeStreamWrapper.Make( ms )

        BinaryDeserialiser.Make( serde, serdeStream, "test" )
        
    let testStream cb =
        
        let body =
            
            let ms =
                new System.IO.MemoryStream()
                
            use bw =
                new System.IO.BinaryWriter( ms, System.Text.Encoding.Default, true )
                
            cb bw
    
            bw.Close()
            
            ms.ToArray()
            
        let os =
            new System.IO.MemoryStream()

        use ow =
            new System.IO.BinaryWriter( os, System.Text.Encoding.Default, true )
            
        ow.Write( "test" )
        ow.Write( (int32) body.Length )
        ow.Write( body )
        
        ow.Close()
        
        os.Position <- 0L
        
        os
            
    [<Fact>]
    member this.``BeCreateable`` () = 

        use ts =
            testStream ( fun bw -> () )
            
        use sut = sut ts
        
        Assert.Equal( "test", sut.TypeName )
        
    [<Fact>]
    member this.``ReadInt8`` () = 
        
        use ts =
            testStream ( fun bw -> bw.Write( 12uy ) )
            
        use sut = sut ts

        Assert.Equal( 12uy, sut.ReadInt8() )
        
    [<Fact>]
    member this.``ReadInt32`` () = 
        
        use ts =
            testStream ( fun bw -> bw.Write( 12 ) )
            
        use sut = sut ts

        Assert.Equal( 12, sut.ReadInt32() )
        
    [<Fact>]
    member this.``ReadInt64`` () = 
        
        use ts =
            testStream ( fun bw -> bw.Write( 12L ) )
            
        use sut = sut ts

        Assert.Equal( 12L, sut.ReadInt64() )

    [<Fact>]
    member this.``ReadString`` () = 
        
        use ts =
            testStream ( fun bw -> bw.Write( "hello, There" ) )
            
        use sut = sut ts

        Assert.Equal( "hello, There", sut.ReadString() )
   
    [<Fact>]
    member this.``ReadBool`` () = 
        
        use ts =
            testStream ( fun bw -> bw.Write( false ) )
            
        use sut = sut ts

        Assert.Equal( false, sut.ReadBool() )

    [<Fact>]
    member this.``ReadDouble`` () = 
        
        use ts =
            testStream ( fun bw -> bw.Write( 123.456 ) )
            
        use sut = sut ts

        Assert.Equal( 123.456, sut.ReadDouble() )
            
    [<Fact>]
    member this.``ReadNull`` () = 
        
        use ts =
            testStream ( fun bw -> () )
            
        use sut = sut ts

        sut.ReadNull()
        
        Assert.True( true )
        
    [<Fact>]
    member this.``ReadDateTime`` () = 
        
        let tv = System.DateTime.UtcNow
        
        use ts =
            testStream ( fun bw -> bw.Write( tv.ToBinary() ) )
            
        use sut = sut ts

        Assert.Equal( tv, sut.ReadDateTime() )
        

                                               