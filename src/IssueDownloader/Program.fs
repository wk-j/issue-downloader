open System
open LibGit2Sharp
open Octokit
open System.IO
open System.Text.RegularExpressions

let split k s = (s:String).Split (k:String)
let replace k s = (s:String).Replace(k, String.Empty)

let getRemotes (repo: LibGit2Sharp.Repository) =
    repo.Network.Remotes |> Seq.map(fun x -> x.Url)

let downloadLink token (id:int) (link: string) (fullPath: string) =
    printfn " > download %A" link

    use client = new System.Net.Http.HttpClient()
    client.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("token", token)
    // client.DefaultRequestHeaders.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3.raw"))

    let data =
        client.GetAsync(link)
        |> Async.AwaitTask
        |> Async.RunSynchronously
    let stream =
        data.Content.ReadAsStreamAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let mode = FileMode.OpenOrCreate
    let access = FileAccess.Write

    if File.Exists fullPath then File.Delete fullPath

    use fileStream = new FileStream(fullPath,  mode, access)
    stream.CopyTo(fileStream)

let createPath (id:int) (link: string) =
    let path = Path.Combine("resource", id.ToString("d3"))
    if (Directory.Exists path |> not) then Directory.CreateDirectory path |> ignore

    let uri = Uri(link);
    let fileName = System.IO.Path.GetFileName(uri.AbsolutePath);
    let cleanName = Uri.UnescapeDataString(fileName)

    let fullPath = Path.Combine(path, cleanName)
    fullPath

let downloadLinks token (id:int) (urls: string array)  =
    for item in urls do
        let fullPath = createPath id item
        downloadLink token id item fullPath

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
    client.Credentials <- tokenAuth

    let issue =
        client.Issue.Get(user, repo, id)
        |> Async.AwaitTask
        |> Async.RunSynchronously

    issue.Title, issue.Body

let clean (title: string) =
    let name = Uri.UnescapeDataString(title)
    String.Join("_", name.ToLower().Split(Path.GetInvalidFileNameChars())).Replace(" ", "-")

let writeBody (id: int) title content =
    let name = clean (title)
    let fullPath = createPath id ("http://google.com/" + name + ".md")
    if File.Exists fullPath then File.Delete fullPath
    File.WriteAllText(fullPath, content)

[<EntryPoint>]
let main argv =
    let issueId = argv.[0] |> Int32.Parse
    let repo = new LibGit2Sharp.Repository(".")
    let data = getUser repo
    let token = System.Environment.GetEnvironmentVariable("GITHUB_TOKEN")

    match data with
    | Some (user, repo) ->
        let issueTitle, issueBody = getIssue token user repo issueId
        let links = extractUrls issueBody |> Seq.toArray
        downloadLinks token issueId links
        writeBody issueId issueTitle issueBody
    | _ ->
        printfn "Invalid remote"
    0