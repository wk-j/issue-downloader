open System
open LibGit2Sharp
open Octokit

let split k s = (s:String).Split (k:String)
let replace k s = (s:String).Replace(k, String.Empty)

let getRemotes (repo: LibGit2Sharp.Repository) =
    repo.Network.Remotes |> Seq.map(fun x -> x.Url)

let getUser (repo: LibGit2Sharp.Repository) =
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

let getIssue user repo id =
    let token = System.Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    let client = GitHubClient(ProductHeaderValue("my-cool-app"));
    let tokenAuth = Credentials(token)
    let issue =
        client.Issue.Get(user, repo, id)
        |> Async.AwaitTask
        |> Async.RunSynchronously

    printfn "%A" (issue.Body)

[<EntryPoint>]
let main argv =
    let repo = new LibGit2Sharp.Repository(".")
    let data = getUser repo

    match data with
    | Some (user, repo) ->
        printfn "%A %A" user repo
        getIssue user repo 1
    | _ ->
        printfn "Invalid remote"

    0