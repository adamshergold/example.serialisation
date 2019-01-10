namespace Example.Serialisation.Json

open Example.Serialisation
 
module Helpers = 
            
    let ToJson (serde:ISerde) (v:obj) = 
        match v with 
        | :? ITypeSerialisable as s ->
            use msw = new System.IO.MemoryStream()
            use stream = SerdeStreamWrapper.Make(msw)
            serde.Serialise "json" stream v 
            System.Text.Encoding.UTF8.GetString(msw.ToArray())
        | _ ->
            failwithf "[%O] does not implement ITypeSerialisable" v

    let FromJson<'T> (serde:ISerde) (json:string) =
        use msr = new System.IO.MemoryStream( System.Text.Encoding.UTF8.GetBytes(json) )
        use reader = SerdeStreamWrapper.Make( msr ) 
        serde.DeserialiseT<'T> "json" reader
        