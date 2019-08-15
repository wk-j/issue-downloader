open System
open LibGit2Sharp
open Octokit
open System.IO
open System.Text.RegularExpressions

let split k s = (s:String).Split (k:String)
let replace k s = (s:String).Replace(k, String.Empty)

let getRemotes (repo: LibGit2Sharp.Repository) =
    repo.Network.Remotes |> Seq.map(fun x -> x.Url)

let download token (urls: string array)  =

    for item in urls do
        use client = new System.Net.Http.HttpClient()
        client.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Token", token)
        let data =
            client.GetAsync(item)
            |> Async.AwaitTask
            |> Async.RunSynchronously
        let stream =
            data.Content.ReadAsStreamAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let uri = Uri(item);
        let fileName = System.IO.Path.GetFileName(uri.AbsolutePath);

        let mode = FileMode.OpenOrCreate
        let access = FileAccess.Write

        use fileStream = new FileStream(fileName,  mode, access)
        stream.CopyTo(fileStream)

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

let extractUrls code =
    let pattern = "\((?<link>[\S]+)\)"
    let reg = Regex(pattern)
    let matchs = reg.Matches(code) |> Seq.map(fun x -> x.Groups.["link"].Value)
    matchs

let getIssue token user repo id =
    let client = GitHubClient(ProductHeaderValue("my-cool-app"));
    let tokenAuth = Credentials(token)
    let issue =
        client.Issue.Get(user, repo, id)
        |> Async.AwaitTask
        |> Async.RunSynchronously

    issue.Body

[<EntryPoint>]
let main argv =
    let repo = new LibGit2Sharp.Repository(".")
    let data = getUser repo
    let token = System.Environment.GetEnvironmentVariable("GITHUB_TOKEN")

    match data with
    | Some (user, repo) ->
        let issueBody = getIssue token user repo 1
        let links = extractUrls issueBody |> Seq.toArray
        download token links
    | _ ->
        printfn "Invalid remote"

    0