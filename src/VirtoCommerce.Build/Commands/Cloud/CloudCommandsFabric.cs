using System;
using System.CommandLine;
using System.IO;
using VirtoCommerce.Build.Commands.Cloud.Actions;

namespace Commands.Cloud
{
    /// <summary>
    /// Factory for creating cloud-related commands for the System.CommandLine implementation
    /// </summary>
    public static class CloudCommandsFabric
    {
        /// <summary>
        /// Creates the main cloud command group with all subcommands
        /// </summary>
        /// <returns>Configured cloud command</returns>
        public static Command CreateCloudCommand()
        {
            var cloudCommand = new Command("cloud")
            {
                Description = "Cloud environment management commands"
            };

            // Add authentication command
            cloudCommand.Add(CreateAuthCommand());

            // Add environment management commands
            cloudCommand.Add(CreateInitCommand());
            cloudCommand.Add(CreateUpCommand());
            cloudCommand.Add(CreateDownCommand());
            cloudCommand.Add(CreateDeployCommand());
            cloudCommand.Add(CreateListCommand());
            cloudCommand.Add(CreateRestartCommand());
            cloudCommand.Add(CreateLogsCommand());
            cloudCommand.Add(CreateStatusCommand());

            // Add environment configuration commands
            cloudCommand.Add(CreateSetParameterCommand());
            cloudCommand.Add(CreateUpdateCommand());

            return cloudCommand;
        }

        /// <summary>
        /// Creates the cloud auth command
        /// </summary>
        public static Command CreateAuthCommand()
        {
            var authCommand = new Command("auth")
            {
                Description = "Authenticate with VirtoCloud"
            };

            var cloudUrl = new Option<string>("--url")
            {
                Description = "VirtoCloud URL",
                DefaultValueFactory = _ => "https://portal.virtocommerce.cloud"
            };

            var azureAdOption = new Option<bool>("--azure-ad")
            {
                Description = "Use Azure AD authentication"
            };
            var tokenOption = new Option<string>("--token")
            {
                Description = "Provide authentication token directly"
            };

            var tokenFileOption = new Option<string>("--token-file")
            {
                Description = "Path to file containing authentication token",
                DefaultValueFactory = _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vc-build", "cloud")
            };

            authCommand.Add(cloudUrl);
            authCommand.Add(azureAdOption);
            authCommand.Add(tokenOption);
            authCommand.Add(tokenFileOption);

            authCommand.SetAction(CloudAuthAction.ExecuteAsync);

            return authCommand;
        }

        /// <summary>
        /// Creates the cloud init command for creating new environments
        /// </summary>
        public static Command CreateInitCommand()
        {
            var initCommand = new Command("init")
            {
                Description = "Create a new cloud environment"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Name of the new environment",
                Required = true
            };
            var servicePlanOption = new Option<string>("--service-plan")
            {
                Description = "Service plan for the environment",
                DefaultValueFactory = _ => "F1"
            };

            var clusterNameOption = new Option<string>("--cluster-name")
            {
                Description = "Target cluster name"
            };
            var organizationOption = new Option<string>("--organization")
            {
                Description = "Organization name"
            };
            var dbProviderOption = new Option<string>("--db-provider")
            {
                Description = "Database provider"
            };
            var dbNameOption = new Option<string>("--db-name")
            {
                Description = "Database name"
            };
            var appProjectOption = new Option<string>("--app-project")
            {
                Description = "Application project name"
            };
            var cloudUrlOption = new Option<string>("--url")
            {
                Description = "VirtoCloud URL",
                DefaultValueFactory = _ => "https://portal.virtocommerce.cloud"
            };

            initCommand.Add(environmentNameOption);
            initCommand.Add(servicePlanOption);
            initCommand.Add(clusterNameOption);
            initCommand.Add(organizationOption);
            initCommand.Add(dbProviderOption);
            initCommand.Add(dbNameOption);
            initCommand.Add(appProjectOption);
            initCommand.Add(cloudUrlOption);

            initCommand.SetAction(CloudInitAction.ExecuteAsync);

            return initCommand;
        }

        /// <summary>
        /// Creates the cloud up command for creating and deploying to new environments
        /// </summary>
        public static Command CreateUpCommand()
        {
            var upCommand = new Command("up")
            {
                Description = "Create a new environment and deploy custom Docker image"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Name of the new environment",
                Required = true
            };
            var dockerUsernameOption = new Option<string>("--docker-username")
            {
                Description = "Docker Hub username",
                Required = true
            };
            var dockerPasswordOption = new Option<string>("--docker-password")
            {
                Description = "Docker registry password",
                Required = true
            };
            var servicePlanOption = new Option<string>("--service-plan")
            {
                Description = "Service plan for the environment",
                DefaultValueFactory = _ => "F1"
            };

            var dockerRegistryUrlOption = new Option<string>("--docker-registry-url")
            {
                Description = "Docker registry URL"
            };
            var dockerImageNameOption = new Option<string>("--docker-image-name")
            {
                Description = "Custom Docker image name"
            };
            var dockerImageTagOption = new Option<string>("--docker-image-tag")
            {
                Description = "Docker image tag"
            };
            var clusterNameOption = new Option<string>("--cluster-name")
            {
                Description = "Target cluster name"
            };
            var dbProviderOption = new Option<string>("--db-provider")
            {
                Description = "Database provider"
            };
            var dbNameOption = new Option<string>("--db-name")
            {
                Description = "Database name"
            };

            upCommand.Add(environmentNameOption);
            upCommand.Add(dockerUsernameOption);
            upCommand.Add(dockerPasswordOption);
            upCommand.Add(servicePlanOption);
            upCommand.Add(dockerRegistryUrlOption);
            upCommand.Add(dockerImageNameOption);
            upCommand.Add(dockerImageTagOption);
            upCommand.Add(clusterNameOption);
            upCommand.Add(dbProviderOption);
            upCommand.Add(dbNameOption);

            upCommand.SetAction(CloudUpAction.ExecuteAsync);

            return upCommand;
        }

        /// <summary>
        /// Creates the cloud deploy command for deploying to existing environments
        /// </summary>
        public static Command CreateDeployCommand()
        {
            var deployCommand = new Command("deploy")
            {
                Description = "Deploy custom Docker image to existing environment"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Target environment name",
                Required = true
            };
            var dockerUsernameOption = new Option<string>("--docker-username")
            {
                Description = "Docker Hub username",
                Required = true
            };
            var dockerPasswordOption = new Option<string>("--docker-password")
            {
                Description = "Docker registry password",
                Required = true
            };
            var dockerRegistryUrlOption = new Option<string>("--docker-registry-url")
            {
                Description = "Docker registry URL"
            };
            var dockerImageNameOption = new Option<string>("--docker-image-name")
            {
                Description = "Custom Docker image name"
            };
            var dockerImageTagOption = new Option<string>("--docker-image-tag")
            {
                Description = "Docker image tag"
            };
            var organizationOption = new Option<string>("--organization")
            {
                Description = "Organization name"
            };

            deployCommand.Add(environmentNameOption);
            deployCommand.Add(dockerUsernameOption);
            deployCommand.Add(dockerPasswordOption);
            deployCommand.Add(dockerRegistryUrlOption);
            deployCommand.Add(dockerImageNameOption);
            deployCommand.Add(dockerImageTagOption);
            deployCommand.Add(organizationOption);

            deployCommand.SetAction(CloudDeployAction.ExecuteAsync);

            return deployCommand;
        }

        /// <summary>
        /// Creates the cloud down command for deleting environments
        /// </summary>
        public static Command CreateDownCommand()
        {
            var downCommand = new Command("down")
            {
                Description = "Delete cloud environment"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Environment name to delete",
                Required = true
            };
            var organizationOption = new Option<string>("--organization")
            {
                Description = "Organization name"
            };

            downCommand.Add(environmentNameOption);
            downCommand.Add(organizationOption);

            downCommand.SetAction(CloudDownAction.ExecuteAsync);

            return downCommand;
        }

        /// <summary>
        /// Creates the cloud list command for listing environments
        /// </summary>
        public static Command CreateListCommand()
        {
            var listCommand = new Command("list")
            {
                Description = "List cloud environments with statuses"
            };

            listCommand.SetAction(CloudListAction.ExecuteAsync);

            return listCommand;
        }

        /// <summary>
        /// Creates the cloud restart command for restarting environments
        /// </summary>
        public static Command CreateRestartCommand()
        {
            var restartCommand = new Command("restart")
            {
                Description = "Restart cloud environment"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Environment name to restart",
                Required = true
            };

            restartCommand.Add(environmentNameOption);

            restartCommand.SetAction(CloudRestartAction.ExecuteAsync);

            return restartCommand;
        }

        /// <summary>
        /// Creates the cloud logs command for viewing environment logs
        /// </summary>
        public static Command CreateLogsCommand()
        {
            var logsCommand = new Command("logs")
            {
                Description = "Show environment logs"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Environment name to show logs for",
                Required = true
            };
            var filterOption = new Option<string>("--filter")
            {
                Description = "Log filter"
            };
            var tailOption = new Option<int>("--tail")
            {
                Description = "Number of lines to tail"
            };
            var resourceNameOption = new Option<string>("--resource-name")
            {
                Description = "Specific resource to show logs for"
            };

            logsCommand.Add(environmentNameOption);
            logsCommand.Add(filterOption);
            logsCommand.Add(tailOption);
            logsCommand.Add(resourceNameOption);

            logsCommand.SetAction(CloudLogsAction.ExecuteAsync);

            return logsCommand;
        }

        /// <summary>
        /// Creates the cloud status command for checking environment status
        /// </summary>
        public static Command CreateStatusCommand()
        {
            var statusCommand = new Command("status")
            {
                Description = "Wait for environment health and sync status"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Environment name",
                Required = true
            };
            var healthStatusOption = new Option<string>("--health-status")
            {
                Description = "Expected health status to wait for"
            };
            var syncStatusOption = new Option<string>("--sync-status")
            {
                Description = "Expected sync status to wait for"
            };

            statusCommand.Add(environmentNameOption);
            statusCommand.Add(healthStatusOption);
            statusCommand.Add(syncStatusOption);

            statusCommand.SetAction(CloudStatusAction.ExecuteAsync);

            return statusCommand;
        }

        /// <summary>
        /// Creates the cloud set-parameter command for updating environment parameters
        /// </summary>
        public static Command CreateSetParameterCommand()
        {
            var setParamCommand = new Command("set-parameter")
            {
                Description = "Update cloud environment parameters"
            };

            var environmentNameOption = new Option<string>("--environment-name")
            {
                Description = "Environment name",
                Required = true
            };
            var helmParametersOption = new Option<string[]>("--helm-parameters")
            {
                Description = "Helm parameters in key=value format",
                Required = true
            };
            var organizationOption = new Option<string>("--organization")
            {
                Description = "Organization name"
            };

            setParamCommand.Add(environmentNameOption);
            setParamCommand.Add(helmParametersOption);
            setParamCommand.Add(organizationOption);

            setParamCommand.SetAction(CloudSetParameterAction.ExecuteAsync);

            return setParamCommand;
        }

        /// <summary>
        /// Creates the cloud update command for updating applications
        /// </summary>
        public static Command CreateUpdateCommand()
        {
            var updateCommand = new Command("update")
            {
                Description = "Update applications in the cloud"
            };

            var manifestOption = new Option<string>("--manifest")
            {
                Description = "Path to application manifest file",
                Required = true
            };
            var routesFileOption = new Option<string>("--routes-file")
            {
                Description = "Path to routes file"
            };

            updateCommand.Add(manifestOption);
            updateCommand.Add(routesFileOption);

            updateCommand.SetAction(CloudUpdateAction.ExecuteAsync);

            return updateCommand;
        }
    }
}
