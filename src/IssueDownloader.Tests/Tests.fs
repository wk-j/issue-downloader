module Tests

open System
open Xunit
open System.Text.RegularExpressions

[<Fact>]
let ``My test`` () =

    let code =
        """
        [data-migrate.txt](https://github.com/bcircle/lion-migration/files/3496298/data-migrate.txt)
        [data-migrate.xlsx](https://github.com/bcircle/lion-migration/files/3504556/data-migrate.xlsx)

        Model alfresco
        [ISO.zip](https://github.com/bcircle/lion-migration/files/3496319/ISO.zip)

        Consult : Gif

        ![code](https://user-images.githubusercontent.com/860704/63093104-1a5bdd00-bf8e-11e9-82ae-fa82bb5b0280.png)
        """

    let pattern = "\((?<link>[\S]+)\)"
    let reg = Regex(pattern)
    let matchs = reg.Matches(code) |> Seq.map(fun x -> x.Groups.["link"].Value)

    ()

[<Fact>]
let unes() =
    let myUrl = "my.aspx?val=%2Fxyz2F"
    let decodeUrl = System.Uri.UnescapeDataString(myUrl)
    Assert.Equal("my.aspx?val=/xyz2F", decodeUrl)
