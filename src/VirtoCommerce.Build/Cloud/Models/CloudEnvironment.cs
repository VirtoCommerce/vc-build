using System;
using System.Collections.Generic;

namespace Cloud.Models;

public class CloudEnvironment
{
    public IDictionary<string, string> Labels { get; set; }

    public string AppProjectId { get; set; }
    public string MetadataName { get; set; }

    public string TenantId { get; set; }

    public string Id { get; set; }

    public string Name { get; set; }

    public string Status { get; set; }

    public string SyncStatus { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public string[] Urls { get; set; }
    public Helm Helm { get; set; }

}
