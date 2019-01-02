namespace Example.Serialisation.Binary.Tests 

open Example.Serialisation 

module Helpers = 
    
    let Serde () =
    
        let options =   
            SerdeOptions.Default
         
        let serde = 
            Serde.Make( options )
            
        serde                 
        
    let DefaultSerde = 
        Serde() 
                
