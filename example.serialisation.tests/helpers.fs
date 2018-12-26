namespace Example.Serialisation.Tests 

open Example.Serialisation 

module Helpers = 
    
    let Serialiser () =
    
        let options =   
            SerdeOptions.Default
         
        let serialiser = 
            Serde.Make( options )
            
        serialiser                 
        
    let DefaultSerialiser = 
        Serialiser() 
                
