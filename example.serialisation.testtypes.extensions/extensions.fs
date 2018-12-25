namespace Example.Serialisation.TestTypes

open Example.Serialisation.Json 

open Example.Serialisation.TestTypes.Example 

module Extensions =

    type Address =  
        static member Examples = 
            seq {
                yield {
                    Number = 1
                    Street = "High Street"
                    Region = Region.North
                }
            } |> Array.ofSeq
    
    
    type Phone =
        static member Examples = 
            seq {
                yield {
                    Code = Some "01892"
                    Digits = [| 5; 2; 6; 4; 0; 0 |]
                }
                yield {
                    Code = None
                    Digits = Array.empty
                }
            } |> Array.ofSeq
        
    type Score =
        static member Examples = 
            seq {
                yield {
                    Mark = 75.1
                    Pass = true 
                }
            } |> Array.ofSeq
        
    type Dog =
        static member Examples = 
            seq {
                yield {
                    Name = "Fido"
                    NickName = Some "Twig"
                    Breed = "Dachshund"
                }
            } |> Array.ofSeq
        
    type Ethnicity =
        static member Examples = 
            seq {
                yield Ethnicity.Earthian
                yield Ethnicity.SolarSystem("Foo")
            } |> Array.ofSeq
                    
    type Person = 
        static member Examples = 
            seq {
                yield {
                    Name = "John Smith"
                    Address = Address.Examples.[0]
                    Phone = Some Phone.Examples.[0]
                    Scores = Map.ofSeq [| "Maths", Score.Examples.[0] |]
                    Pets = Some [| Dog.Examples.[0] |]
                    Ethnicity = Ethnicity.Examples.[0]
                    Status = Status.Single
                    Hobbies = Set.ofSeq [ Hobby.Fishing ]
                }
            } |> Array.ofSeq
            
    type UnionOfPersons =
        static member Examples = 
            seq {
                yield UnionOfPersons.Persons( Person.Examples )
            }
            |> Array.ofSeq       
            
    type MyAny =
        static member Examples = 
            seq { 
                yield {
                    BitsAndBobs = 
                        Map.ofList [
                            "Person", Any.Record(Person.Examples.[0])
                            "Score", Any.Double(456.789)
                            "Flag", Any.Bool(true)      
                            "Vector", Any.Array( [| Any.String("Foo") |] ) 
                        ]
                }
            }    
            
    type Empty = 
        static member Examples = 
            seq { 
                yield Empty.Make()
            }            
            
    type All = 
        static member Examples = 
            seq {
                yield {
                    TheSerialisable = Address.Examples.[0]
                }
            }          