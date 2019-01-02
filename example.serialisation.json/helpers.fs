namespace Example.Serialisation.Json

open Example.Serialisation
 
module Helpers = 

    let ToJson (serialiser:ISerde) (v:obj) = 
        match v with 
        | :? ITypeSerialisable as s ->
            use msw = new System.IO.MemoryStream()
            use stream = SerdeStreamWrapper.Make(msw)
            serialiser.Serialise (Some "json") stream v 
            System.Text.Encoding.UTF8.GetString(msw.ToArray())
        | _ ->
            failwithf "[%O] does not implement ITypeSerialisable" v

    let FromJson<'T> (serialiser:ISerde) (json:string) =
        use msr = new System.IO.MemoryStream( System.Text.Encoding.UTF8.GetBytes(json) )
        use reader = SerdeStreamWrapper.Make( msr ) 
        serialiser.DeserialiseT<'T> (Some "json") reader
        