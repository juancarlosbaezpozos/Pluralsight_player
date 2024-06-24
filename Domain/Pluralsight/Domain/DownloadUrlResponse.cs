using System.Collections.Generic;

namespace Pluralsight.Domain;

public class DownloadUrlResponse
{
    public string Url { get; set; }

    public List<UrlOptionResponse> RankedOptions { get; set; }
}
