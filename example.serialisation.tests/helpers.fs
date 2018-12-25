namespace Example.Serialisation.Tests 

open Example.Serialisation 

module Helpers = 
    
    let Serde () =
    
        let options =   
            SerdeOptions.Default
         
        let serialiser = 
            Serde.Make( options )
            
        serialiser                 
        
    let DefaultSerialiser = 
        Serde() 
                
