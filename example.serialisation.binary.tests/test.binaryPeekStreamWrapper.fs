namespace Example.Serialisation.Binary.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.Binary
open Example.Serialisation.Binary
open Example.Serialisation.TestTypes 

type PeekStreamWrapperShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    let sut (s:System.IO.Stream) =
       
        let ss = 
            SerdeStreamWrapper.Make( s )
        
        BinaryPeekStreamWrapper.Make( ss ) 
    
    let testStream cb =
        
        let ms =
            new System.IO.MemoryStream()
            
        use bw =
            new System.IO.BinaryWriter( ms, System.Text.Encoding.Default, true )
            
        cb bw

        bw.Close()
        
        ms.Position <- 0L

        ms
        
    [<Fact>]
    member this.``BeCreateable`` () = 
    
        use ms =
            new System.IO.MemoryStream()
            
        use sut = sut( ms )
        
        Assert.Same( ms, (sut :> ISerdeStream).Stream )
        
        Assert.True( true )                    

    
    [<Fact>]
    member this.``PeekAndReadString`` () = 
    
        let tv = "Hello!"
       
        use ts =
            testStream ( fun bw -> bw.Write( tv ) )
        
        use sut = sut( ts )

        Assert.Equal( tv, sut.PeekString() )
        Assert.Equal( tv, sut.ReadString() )

        
    [<Fact>]
    member this.``ReadInt8`` () = 
    
        let tv = 6uy
       
        use ts =
            testStream ( fun bw -> bw.Write( tv ) )
        
        use sut = sut( ts )

        Assert.Equal( tv, sut.ReadInt8() )        

        
    [<Fact>]
    member this.``ReadInt64`` () = 
    
        let tv = (int64) 123
       
        use ts =
            testStream ( fun bw -> bw.Write( tv ) )
        
        use sut = sut( ts )

        Assert.Equal( tv, sut.ReadInt64() )        
        
        
    [<Fact>]
    member this.``ReadDouble`` () = 
    
        let tv = 123.456
        
        use ts =
            testStream ( fun bw -> bw.Write( tv ) )
        
        use sut = sut( ts )

        Assert.Equal( tv, sut.ReadDouble() )        

    [<Fact>]
    member this.``ReadBool`` () = 
    
        let tv = false
        
        use ts =
            testStream ( fun bw -> bw.Write( tv ) )
        
        use sut = sut( ts )

        Assert.Equal( tv, sut.ReadBool() )        
        
    [<Fact>]
    member this.``ReadBytes`` () = 
    
        let tv = Array.ofList [ 1uy; 2uy; 3uy ]
        
        use ts =
            testStream ( fun bw -> bw.Write( tv ) )
        
        use sut = sut( ts )

        let vs =
            sut.ReadBytes( 3 )
            
        Assert.Equal( [ 1uy; 2uy; 3uy ], vs )      
        