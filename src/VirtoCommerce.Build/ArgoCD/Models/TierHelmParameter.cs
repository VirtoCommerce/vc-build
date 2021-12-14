using System;
using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class TierHelmParameter : V1alpha1HelmParameter
    {
        public TierHelmParameter(string value) : base(false, "platform.tier", value)
        {
        }
    }
}
