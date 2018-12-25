namespace Example.Serialisation.Json

open Example.Serialisation
 
module Helpers = 

    let ToJson (serialiser:ISerde) (v:obj) = 
        match v with 
        | :? ITypeSerialisable as s ->
            use msw = new System.IO.MemoryStream()
            let stream = SerialiserStreamWrapper.Make(msw)
            serialiser.Serialise (Some "json") stream v 
            System.Text.Encoding.UTF8.GetString(msw.ToArray())
        | _ ->
            failwithf "[%O] does not implement ITypeSerialisable" v

    let FromJson<'T> (serialiser:ISerde) (json:string) =
        let msr = new System.IO.MemoryStream( System.Text.Encoding.UTF8.GetBytes(json) )
        let reader = SerialiserStreamWrapper.Make(msr) 
        serialiser.DeserialiseT<'T> (Some "json") reader
        