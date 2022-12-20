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

    public CloudEnvironment()
    {

    }
    public CloudEnvironment(string appProjectId, string metadataName, string tenantId, string id, string name, string status, string syncStatus, DateTime created, DateTime updated, string[] urls, Helm helm, IDictionary<string, string> labels)
    {
        MetadataName = metadataName;
        TenantId = tenantId;
        Id = id;
        Name = name;
        Status = status;
        SyncStatus = syncStatus;
        Created = created;
        Updated = updated;
        Urls = urls;
        Labels = labels;
        AppProjectId = appProjectId;
        Helm = helm;
    }
}
