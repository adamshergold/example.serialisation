namespace Example.Serialisation.Json

open System

module Utilities = 

    let LookupStringComparer =
        
        let items = Map.ofList [
            StringComparison.Ordinal,           StringComparer.Ordinal
            StringComparison.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase
            StringComparison.CurrentCulture,    StringComparer.CurrentCulture
            ]
        
        fun sc ->
            match items.TryFind sc with
            | Some cmp -> cmp
            | None -> failwithf "Unable to find string comparer for comparison [%O]" sc

