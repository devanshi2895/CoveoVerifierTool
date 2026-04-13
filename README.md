# CoveoVerifierTool

A .NET Framework 4.7.2 console application that verifies whether Sitecore content items exist in a Coveo index. Accepts multiple sriggle IDs via command-line arguments and displays results in a formatted table.

## Requirements

- .NET Framework 4.7.2
- MSBuild (Visual Studio 2019 / 2022) or the .NET Framework build tools
- A valid Coveo platform API bearer token

## Project Structure

```
CoveoVerifierTool/
├── App.config                  # Base URL and auth token settings
├── CoveoVerifierTool.csproj    # Classic .csproj (no SDK-style, no NuGet)
├── Program.cs                  # Entry point, CLI parsing, table output
├── Models/
│   ├── CoveoRequest.cs         # Request DTO with ToJson()
│   └── CoveoResponse.cs        # Response DTO (totalCount, results)
└── Services/
    └── CoveoService.cs         # HTTP client, request builders, VerifyAsync
```

## Configuration

Edit `App.config` before running:

```xml
<appSettings>
  <add key="Coveo:BaseUrl"   value="https://platform.cloud.coveo.com/rest/search/v2" />
  <add key="Coveo:AuthToken" value="YOUR_BEARER_TOKEN_HERE" />
</appSettings>
```

## Build

```bat
MSBuild CoveoVerifierTool.csproj /p:Configuration=Release
```

The compiled binary will be at `bin\Release\CoveoVerifierTool.exe`.

## Usage

```
CoveoVerifierTool.exe --hotels <id1,id2,...> --excursions <id1,id2,...>
```

Both flags are optional — supply one or both:

```bat
REM Check a single hotel
CoveoVerifierTool.exe --hotels 29

REM Check multiple excursions
CoveoVerifierTool.exe --excursions 11650,11651,11652

REM Check both types at once
CoveoVerifierTool.exe --hotels 29,30 --excursions 11650,11651
```

Running with no arguments prints the usage hint and exits without making any API calls.

## Output

For each ID the tool prints a progress line while the request is in flight, then produces a colour-coded results table:

```
+-----------+-----------+-------+--------------------------------------+-----------------------------+
| SriggLeId | Label     | Count | Item ID                              | Title                       |
+-----------+-----------+-------+--------------------------------------+-----------------------------+
| 29        | HOTEL     | 1     | 67C2B893-63BD-4100-9658-E2B8C5E2AD20 | Yas Island Hotel            |
| 30        | HOTEL     | 0     |                                      |                             |
| 11650     | EXCURSION | 1     | A1B2C3D4-0000-1111-2222-333344445555 | Desert Safari Excursion     |
+-----------+-----------+-------+--------------------------------------+-----------------------------+
```

- **Green** row — data found in Coveo
- **Red** row — no results returned

### Columns

| Column    | Description                                              |
|-----------|----------------------------------------------------------|
| SriggLeId | The sriggle ID passed on the command line                |
| Label     | Content type: `HOTEL` or `EXCURSION`                     |
| Count     | Total number of matching records in the index            |
| Item ID   | Sitecore item GUID extracted from the Coveo result URI   |
| Title     | Title of the first matching result (blank if not found)  |

## Dependencies

No NuGet packages. Only BCL assemblies are used:

| Assembly              | Usage                          |
|-----------------------|--------------------------------|
| `System.Net.Http`     | `HttpClient` for REST calls    |
| `System.Configuration`| `ConfigurationManager`/App.config |
| `System.Web.Extensions` | `JavaScriptSerializer` for JSON |
