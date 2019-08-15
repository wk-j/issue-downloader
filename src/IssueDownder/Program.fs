// Learn more about F# at http://fsharp.org

open System
open LibGit2Sharp

let split k s = (s:String).Split (k:String)
let replace k s = (s:String).Replace(k, String.Empty)

let getRemotes (repo: Repository) =
    repo.Network.Remotes |> Seq.map(fun x -> x.Url)

let getUser (repo: Repository) =
    let remote = getRemotes repo |> Seq.head
    let tokens =
        remote
        |> replace "https://github.com/"
        |> replace ".git"
        |> split "/"
        |> Seq.toList

    match tokens with
    | [user; repo] -> Some (user, repo)
    | _ -> None

[<EntryPoint>]
let main argv =

    let repo = new LibGit2Sharp.Repository(".")
    let data = getUser repo
    match data with
    | Some (user, repo) ->
        printfn "%A %A" user repo
    | _ ->
        printfn "Invalid remote"

    0 // return an integer exit code
