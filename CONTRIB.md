# Contributing

* Feature requests welcomed (log an issue)
* Bug reports welcomed (log an issue)
* Pull requests welcomed

## Publishing

Push the Version number in `SpotifyApi.NetCore.csproj`

```xml
<Version>1.1.2</Version>
```

Commit and push

    git commit -a -m "Packing v1.1.2"
    git push

Pack

    dotnet pack src/SpotifyApi.NetCore -c Release

Publish

    dotnet nuget push .\src\SpotifyApi.NetCore\bin\Release\SpotifyApi.NetCore.2.3.8.nupkg -k (api-key) -s https://api.nuget.org/v3/index.json

And then create a release label in Github.