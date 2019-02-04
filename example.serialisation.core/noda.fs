namespace Example.Serialisation

open NodaTime

module Noda =
    
    let private localDatePattern =
        NodaTime.Text.LocalDatePattern.Create( "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture )
    
    let LocalDateToString (dt:LocalDate) =
        localDatePattern.Format(dt)
        
    let LocalDateFromString (dt:string) =
        let pr = localDatePattern.Parse(dt)
        if pr.Success then pr.Value else failwithf "Unable to parse NodaTime LocalDate from string '%s'" dt 
        
    let private localDateTimePattern =
        NodaTime.Text.LocalDateTimePattern.Create( "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture )
    
    let LocalDateTimeToString (lt:LocalDateTime) =
        localDateTimePattern.Format(lt)
        
    let LocalDateTimeFromString (lt:string) =
        let pr = localDateTimePattern.Parse(lt)
        if pr.Success then
            pr.Value
        else
            failwithf "Unable to parse NodaTime LocalDateTime from string '%s'" lt 

    let private resolver =
        NodaTime.TimeZones.Resolvers.LenientResolver
        
    let private tzProvider =
        NodaTime.DateTimeZoneProviders.Tzdb
        
    let private zonedDateTimePattern =
        let templateZDT = ZonedDateTime()
        NodaTime.Text.ZonedDateTimePattern.Create( "F", System.Globalization.CultureInfo.InvariantCulture, resolver, tzProvider, templateZDT )
    
    let ZonedDateTimeToString (zt:ZonedDateTime) =
        zonedDateTimePattern.Format(zt)
        
    let ZonedDateTimeFromString (zt:string) =
        let pr = zonedDateTimePattern.Parse(zt)
        if pr.Success then pr.Value else failwithf "Unable to parse NodaTime ZonedDateTime from string '%s'" zt 
        
        
        
