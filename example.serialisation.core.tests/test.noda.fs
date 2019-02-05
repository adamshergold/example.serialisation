namespace Example.Serialisation.Core.Tests

open Microsoft.Extensions.Logging

open Xunit
open Xunit.Abstractions 

open NodaTime

open Example.Serialisation
open Example.Serialisation.Core.Tests
//open Example.Serialisation.TestTypes

type NodaShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
            
        Logging.CreateLogger options
        
    [<Fact>]
    member this.``LocalDateRoundTrips`` () = 
        
        let ld =
            new LocalDate( 2019, 01, 01 )
            
        let s = Noda.LocalDateToString ld
        
        let ldRT = s |> Noda.LocalDateFromString
        
        Assert.Equal( ldRT, ld )
        
    [<Fact>]
    member this.``LocalDateTimeRoundTrips`` () = 
        
        let ldt =
            //new LocalDateTime( 2019, 01, 01, 23, 59, 59, 0 )
            new LocalDateTime()
            
        let s = Noda.LocalDateTimeToString ldt
        
        logger.LogInformation( "{NodaTime}", s )
        
        let ldtRT = s |> Noda.LocalDateTimeFromString
        
        Assert.Equal( ldtRT, ldt )
        
    [<Fact>]
    member this.``ZonedDateTimeRoundTrips`` () = 
        
        let ldt =
            new LocalDateTime( 2019, 01, 01, 23, 59, 59, 0 )
        
        let tzp = NodaTime.DateTimeZoneProviders.Tzdb
        
        let tz = tzp.Item("Europe/London")
        
        let offset = NodaTime.Offset()
        
        let zdt = ZonedDateTime( ldt, tz, offset )
        
        let s = Noda.ZonedDateTimeToString zdt
        
        let zdtRT = s |> Noda.ZonedDateTimeFromString
        
        Assert.Equal( zdtRT, zdt )                   