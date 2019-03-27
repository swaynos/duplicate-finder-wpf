using Newtonsoft.Json;
using System.Collections.Generic;

namespace FileHashRepository.Model
{
    internal class ScanResult
    {
        [JsonProperty("files")]
        internal List<ScannedFile> Files { get; set; }

        [JsonProperty("locations")]
        internal List<ScannedLocation> Locations { get; set; }
    }
}
