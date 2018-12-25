namespace Example.Serialisation.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Serialisation
open Example.Serialisation.Tests 

type BenchmarkShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options

    let sut = 
    
        let sut = 
            Serde.Make( ) 
            
        sut.TryRegisterAssembly (typeof<Example.Serialisation.TestTypes.Example.Person>.Assembly) |> ignore 
    
        fun () -> sut 
            
    static member ContentTypes 
        with get () = 
            seq { 
                yield [| "binary" |]
                yield [| "json" |] }
                             
       
        
        
        