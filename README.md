## Issue Downloader

[![Build Status](https://dev.azure.com/wk-j/issue-downloader/_apis/build/status/wk-j.issue-downloader?branchName=master)](https://dev.azure.com/wk-j/issue-downloader/_build/latest?definitionId=54&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/wk.IssueDownloader.svg)](https://www.nuget.org/packages/wk.IssueDownloader)

## Installation

```bash
dotnet tool install -g wk.IssueDownloader
```

## Usage

```bash
export GITHUB_TOKEN=xyz

wk-issue-downloader <IssueNumber>
wk-issue-downloader 1
open resource/001
```