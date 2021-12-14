using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ArgoApplication
    {
        public string Name { get; set; }
        //[YamlMember(Alias = "imagetag", ApplyNamingConventions = false)]
        public string ImageTag { get; set; }
        public string Tier { get; set; }
        public Dictionary<string, string> Config { get; set; }
        public Dictionary<string, string> SecretConfig { get; set; }


    }
}