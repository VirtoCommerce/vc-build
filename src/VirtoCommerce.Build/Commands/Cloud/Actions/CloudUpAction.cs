using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VirtoCommerce.Build;

namespace VirtoCommerce.Build.Commands.Cloud.Actions
{
    /// <summary>
  /// Action for cloud up command
    /// </summary>
    public static class CloudUpAction
    {
        /// <summary>
        /// Executes the cloud up command
 /// </summary>
        /// <param name="parseResult">Command line parse result</param>
        /// <returns>Task representing the operation</returns>
     public static async Task<int> ExecuteAsync(ParseResult parseResult)
{
     try
          {
 // Extract option values from ParseResult using correct API
     var environmentName = parseResult.GetValue<string>("--environment-name");
    var dockerUsername = parseResult.GetValue<string>("--docker-username");
    var dockerPassword = parseResult.GetValue<string>("--docker-password");
     var servicePlan = parseResult.GetValue<string>("--service-plan") ?? "F1";
       var dockerRegistryUrl = parseResult.GetValue<string>("--docker-registry-url");
   var dockerImageName = parseResult.GetValue<string>("--docker-image-name");
var dockerImageTag = parseResult.GetValue<string>("--docker-image-tag");
     var clusterName = parseResult.GetValue<string>("--cluster-name");
   var dbProvider = parseResult.GetValue<string>("--db-provider");
var dbName = parseResult.GetValue<string>("--db-name");

         Log.Information("Executing cloud up command for environment: {EnvironmentName}", environmentName);

      // Validate required parameters
     if (string.IsNullOrEmpty(environmentName))
      {
  Log.Error("Environment name is required for cloud up");
      Console.Error.WriteLine("Error: --environment-name is required");
 return 1;
  }
      
   if (string.IsNullOrEmpty(dockerUsername))
      {
 Log.Error("Docker username is required for cloud up");
          Console.Error.WriteLine("Error: --docker-username is required");
       return 1;
      }

    if (string.IsNullOrEmpty(dockerPassword))
    {
Log.Error("Docker password is required for cloud up");
       Console.Error.WriteLine("Error: --docker-password is required");
        return 1;
  }

  Log.Information("Docker configuration - Username: {DockerUsername}, Registry: {DockerRegistry}, Image: {DockerImage}:{DockerTag}", 
      dockerUsername, dockerRegistryUrl ?? "default", dockerImageName ?? "auto-generated", dockerImageTag ?? "auto-generated");
      Log.Information("Service plan: {ServicePlan}", servicePlan);
      
        if (!string.IsNullOrEmpty(clusterName))
         Log.Information("Target cluster: {ClusterName}", clusterName);
 if (!string.IsNullOrEmpty(dbProvider))
      Log.Information("Database provider: {DbProvider}", dbProvider);

      // Set the cloud parameters for the existing Nuke.Build infrastructure
      Build.EnvironmentName = environmentName;
          Build.DockerUsername = dockerUsername;
       Build.DockerPassword = dockerPassword;
     Build.ServicePlan = servicePlan;

      if (!string.IsNullOrEmpty(dockerRegistryUrl))
  Build.DockerRegistryUrl = dockerRegistryUrl;
      
       if (!string.IsNullOrEmpty(dockerImageName))
       Build.DockerImageName = dockerImageName;
   
     if (!string.IsNullOrEmpty(dockerImageTag))
Build.DockerImageTag = dockerImageTag;

         if (!string.IsNullOrEmpty(clusterName))
            Build.ClusterName = clusterName;
     
     if (!string.IsNullOrEmpty(dbProvider))
        Build.DbProvider = dbProvider;
         
       if (!string.IsNullOrEmpty(dbName))
 Build.DbName = dbName;

     Log.Information("Delegating to existing CloudUp target");

      // Execute the existing CloudUp target logic
      return Build.Execute<Build>(x => x.CloudUp);
     }
        catch (Exception ex)
 {
    Log.Error(ex, "Error executing cloud up command");
     Console.Error.WriteLine($"Error executing cloud up: {ex.Message}");
       return 1;
      }
}
    }
}
