namespace Example.Serialisation

open Microsoft.Extensions.Logging

type SerdeOptions = {
    Logger : ILogger option

    // Allow specified content types to have fallback deserialisers (e.g. proxies)
    // map: contentType -> serialiser
    FallbackDeserialisers : Map<string,string>
        
    // If no deserialiser or fallbacks are found then, if set, the stream will be consumed (entirely)
    // into a TypeWrapper 
    TypeWrapperFallback : bool
    
    // The name of the type propery (e.g. '@type') 
    TypeProperty : string
    
    // Allowable type properties (if empty then only allowable one is as above)
    AllowableTypeProperties : Set<string> option
    
    // String comparison mode for properties
    PropertyNameStringComparison : System.StringComparison
    
    // Strict inline type-check: if we have conciously chosen to deserialise 'T' then if the inline type information
    // (if it exists) is different then what do we do?  Fail immediately or try and proceed?
    StrictInlineTypeCheck : bool 

}
with 
    static member Default = {
        Logger = None
        FallbackDeserialisers = Map.empty
        TypeWrapperFallback = false 
        TypeProperty = "@type"
        AllowableTypeProperties = None
        PropertyNameStringComparison = System.StringComparison.OrdinalIgnoreCase
        StrictInlineTypeCheck = true
    }
