using System;
namespace ArgoCD.Client.Models
{
    public partial class V1Time
    {
        public static implicit operator V1Time(DateTime dateTime) => new V1Time(seconds: new DateTimeOffset(dateTime).ToUnixTimeSeconds().ToString());
    }
}

