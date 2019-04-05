using Newtonsoft.Json;
using System.Collections.Generic;

namespace FileHashRepository.Model
{
    internal class ScanResult
    {
        [JsonProperty("files")]
        internal IEnumerable<ScannedFile> Files { get; set; }

        [JsonProperty("locations")]
        internal IEnumerable<ScannedLocation> Locations { get; set; }
    }
}
