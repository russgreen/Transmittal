// Copyright 2003-2023 by Autodesk, Inc.
// This code is taken from the RevitLookup project and is licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Transmittal.Library.DTO;

[Serializable]
public sealed class GutHubResponse
{
    [JsonPropertyName("html_url")] public string Url { get; set; }
    [JsonPropertyName("tag_name")] public string TagName { get; set; }
    [JsonPropertyName("draft")] public bool Draft { get; set; }
    [JsonPropertyName("prerelease")] public bool PreRelease { get; set; }
    [JsonPropertyName("published_at")] public DateTimeOffset PublishedDate { get; set; }
    [JsonPropertyName("assets")] public List<GutHubResponseAsset> Assets { get; set; }
}

[Serializable]
public sealed class GutHubResponseAsset
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; }
}
