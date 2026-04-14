# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CoveoVerifierTool is a .NET Framework 4.7.2 console application that verifies whether Sitecore content items (hotels, excursions) are indexed in Coveo and checks that their GA Content Node fields are fully populated. It queries the Coveo search REST API and prints a color-coded table showing each item's name, Sitecore item ID, and any missing GA metadata fields.

## Build Command

`MSBuild` is not on PATH. Use the full path via PowerShell:

```powershell
powershell.exe -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe' 'C:\Projects\CoveoVerifierTool\CoveoVerifierTool.csproj' /p:Configuration=Release /nologo /v:minimal"
```

Output: `bin\Release\CoveoVerifierTool.exe`. No NuGet restore needed — only BCL assemblies are used (`System.Net.Http`, `System.Configuration`, `System.Web.Extensions`).

## Running the Tool

IDs are passed as comma-separated values after each flag:

```
CoveoVerifierTool.exe --hotels 29,30 --excursions 11650,11651
CoveoVerifierTool.exe --hotels 29
CoveoVerifierTool.exe --excursions 11650
```

## Configuration

`App.config` must have both keys set:

```xml
<appSettings>
  <add key="Coveo:BaseUrl" value="https://platform.cloud.coveo.com/rest/search/v2" />
  <add key="Coveo:AuthToken" value="YOUR_BEARER_TOKEN" />
</appSettings>
```

`CoveoService` throws `InvalidOperationException` at startup if either key is missing.

## Architecture

**Data flow:**
```
Program.Main() → parse CLI args (comma-separated IDs per flag)
  → CoveoService (reads App.config, holds single HttpClient)
  → For each ID: BuildHotelRequest() / BuildExcursionRequest() → VerifyAsync()
  → HTTP POST to Coveo API → deserialize CoveoResponse
  → Parse fgahotelinfo50416 / fgaez120xcursioninfo50416 from raw field → collect missing keys
  → PrintTable() → PrintSummary()
```

**Key classes:**

| File | Responsibility |
|------|----------------|
| `Program.cs` | CLI parsing, `PrintTable()`, `PrintSummary()`, `GetName()`/`GetMissing()` helpers |
| `Services/CoveoService.cs` | `VerifyResult` class, `HttpClient` wrapper, query builders, `VerifyAsync()` |
| `Models/CoveoRequest.cs` | Request DTO with `ToJson()` serialization |
| `Models/CoveoResponse.cs` | Response DTOs (`CoveoResponse`, `CoveoResult` with `raw` dictionary) |
| `Models/GaHotelInfo.cs` | Hotel GA metadata model + `DeserializeGaHotelInfo()` + `GetMissingKeys()` |
| `Models/GaExcursionInfo.cs` | Excursion GA metadata model + `DeserializeGaExcursionInfo()` + `GetMissingKeys()` |

**`VerifyResult`** (defined in `CoveoService.cs`) carries: `SriggLeId`, `Label`, `Title`, `Exists`, `Count`, `ItemId`, `ItemPath`, `HotelInfo`, `ExcursionInfo`, `MissingHotelKeys`, `MissingExcursionKeys`.

**Coveo query details:**
- Hotels: template ID `d801ccec-bb25-4621-a1db-ea4eaf32120e`, requests fields `fhotelinfo50416` + `fgahotelinfo50416`
- Excursions: template ID `44002bda-145b-49a9-a8f4-36b568362c76`, requests fields `fexcursioninfo50416` + `fgaez120xcursioninfo50416`
- Both filter by `flanguage50416`, content path, template, and SriggLE ID
- 17 common Coveo metadata fields excluded via `CoveoService.CommonExclusions`
- Item GUIDs extracted from Coveo URIs via regex in `ExtractItemId()`

**GA field parsing (`GaHotelInfo` / `GaExcursionInfo`):**
- The raw Coveo fields (`fgahotelinfo50416`, `fgaez120xcursioninfo50416`) are JSON strings containing an array of `{"name": "...", "value": ...}` pairs
- `DeserializeGa*Info(string json)` parses the array with `JavaScriptSerializer`, builds a `Dictionary<string, object>` keyed by name, maps it to the typed model, and returns `Tuple<Model, Dictionary>`
- `GetMissingKeys(dict)` checks each expected key: missing = absent from dict, null value, or empty string
- If the raw field is absent from the Coveo result entirely, all expected keys are reported as missing

**Output:** A single unified table (`# | SriggleId | Name | Item ID | Missing Keys`) is printed for hotels then excursions. The `Missing Keys` column width is computed dynamically so all keys are always shown untruncated. A `SUMMARY` block follows.

## No Tests

There is no test project. Verification is done by running the tool manually against the Coveo API.
