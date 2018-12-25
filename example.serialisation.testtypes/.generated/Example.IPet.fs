namespace Klarity.Serialisation.TestTypes.Example

open Klarity.Types.Framework

type IPet =
    abstract Name : string with get
    abstract NickName : string option with get

