using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Nuke.Common;
using Serilog;
using VirtoCommerce.Platform.Core.Common;

namespace GrabMigrator
{
    /// <summary>
    ///     Grab-migrator target implementation
    /// </summary>
    internal static class GrabMigrator
    {
        public static void Do(string configFilePath)
        {
            OutBox("VirtoCommerce EF-migration grabbing and applying tool.");

            if (!string.IsNullOrEmpty(configFilePath))
            {
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        Out("Read config file...");

                        var config = (Config)JsonSerializer.Deserialize(File.ReadAllText(configFilePath), typeof(Config));

                        Dictionary<string, List<string>> sqlStatements = null;

                        if (config?.Grab == true)
                        {
                            sqlStatements = EnableGrabMode(configFilePath, config, sqlStatements);
                        }

                        if (config?.Apply == true)
                        {
                            EnableApplyMode(config, sqlStatements);
                        }

                        OutBox("Complete!");
                    }
                    catch (Exception exc)
                    {
                        Fail($"An exception occurred: {exc}");
                    }
                }
                else
                {
                    Fail($"Configuration file {configFilePath} not found!");
                }
            }
            else
            {
                Out("Usage:");
                Out("vc-build GrabMigrator --grab-migrator-config <ConfigFile>");
                Fail("Configuration file required!");
            }
        }

        private static void EnableApplyMode(Config config, Dictionary<string, List<string>> sqlStatements)
        {
            OutBox("Apply mode");

            sqlStatements ??= ReadSavedStatements(config.StatementsDirectory);

            Out("Read platform config file...");

            var connectionStrings = GrabConnectionStrings(config.PlatformConfigFile);

            foreach (var module in config.ApplyingOrder)
            {
                OutBox($"Applying scripts for module: {module}...");

                if (!sqlStatements.ContainsKey(module))
                {
                    Out($"Warning! There is no SQL expressions for module: {module}");
                    continue;
                }
                var connectionString = GetConnectionString(config, connectionStrings, module);

                // Fallback connection string key is always "VirtoCommerce"
                connectionString = connectionString.EmptyToNull() ?? connectionStrings["VirtoCommerce"];

                using var connection = (IDbConnection)new SqlConnection(connectionString);
                // One connection and transaction per each module
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var commandText in sqlStatements[module])
                    {
                        Out($"Run SQL statement:{Environment.NewLine}{commandText}");
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandTimeout = config.CommandTimeout;
                        command.CommandText = commandText;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Out($"Successfully applied for module: {module}!");
                }
                catch
                {
                    transaction.Rollback();
                    Out($"Statement not executed. Transaction for module {module} rolled back.");
                    throw;
                }
            }
        }

        private static string GetConnectionString(Config config, Dictionary<string, string> connectionStrings, string module)
        {
            var connectionString = string.Empty;

            if (config.ConnectionStringsRefs.ContainsKey(module))
            {
                foreach (var moduleConnStringKey in config.ConnectionStringsRefs[module])
                {
                    connectionString = connectionStrings.ContainsKey(moduleConnStringKey) ? connectionStrings[moduleConnStringKey] : string.Empty;

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        return connectionString;
                    }
                }
            }

            return connectionString;
        }

        private static Dictionary<string, List<string>> EnableGrabMode(string configFilePath, Config config, Dictionary<string, List<string>> sqlStatements)
        {
            sqlStatements ??= new Dictionary<string, List<string>>();
            OutBox("Grab mode");

            Out("Refresh connection strings references...");
            config.ConnectionStringsRefs = new Dictionary<string, List<string>>();

            foreach (var migrationDirectory in config.MigrationDirectories)
            {
                Out($"Looking in {migrationDirectory}...");
                GrabConnectionStringsRefsFromModules(config.ConnectionStringsRefs, migrationDirectory);
            }

            File.WriteAllText(configFilePath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

            Out("Looking for migrations in migration directories recursively...");
            return GrabSqlStatements(config);
        }

        private static Dictionary<string, string> GrabConnectionStrings(string platformConfigFile)
        {
            var result = new Dictionary<string, string>();

            var platformConfigJson = JsonDocument.Parse(File.ReadAllText(platformConfigFile), new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });

            foreach (var property in platformConfigJson.RootElement.GetProperty("ConnectionStrings").EnumerateObject())
            {
                result.Add(property.Name, property.Value.ToString());
            }

            return result;
        }

        private static void GrabConnectionStringsRefsFromModules(Dictionary<string, List<string>> refs, string migrationDirectory)
        {
            var connKeyRegex = new Regex(@"\.GetConnectionString\(""(?<connkey>((?!GetConnectionString)[\w.])*)""\)", RegexOptions.Singleline);
            var moduleRegex = new Regex(@"[\\\w^\.-]*\\(?<module>.+)\.Web");
            var moduleFiles = Directory.GetFiles(migrationDirectory, "Module.cs", SearchOption.AllDirectories);

            foreach (var moduleFile in moduleFiles)
            {
                Out($"Parse file {moduleFile}...");
                var moduleName = moduleRegex.Match(moduleFile).Groups["module"].Value;
                var content = File.ReadAllText(moduleFile);
                var matches = connKeyRegex.Matches(content);
                var listRefs = new List<string>();

                foreach (Match match in matches)
                {
                    listRefs.Add(match.Groups["connkey"].Value);
                }

                if (listRefs.Count > 0)
                {
                    refs.Add(moduleName, listRefs);
                }
            }
        }

        private static Dictionary<string, List<string>> GrabSqlStatements(Config config)
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var migrationDirectory in config.MigrationDirectories)
            {
                GrabSqlStatementsWithEFTool(result, migrationDirectory, config);
            }

            return result;
        }

        private static void GrabSqlStatementsWithEFTool(Dictionary<string, List<string>> sqlStatements, string migrationDirectory, Config config)
        {
            Directory.CreateDirectory(config.StatementsDirectory);
            var moduleRegex = new Regex(@"[\\\w^\.-]*\\(?<module>.+)\\Migrations");
            var migrationNameRegex = new Regex(@"\[Migration\(""(?<migration>.+)""\)\]");
            string[] migrationFiles;

            if (config.GrabMode == GrabMode.V2V3)
            {
                // Look for upgrade migrations
                migrationFiles = Directory.GetFiles(migrationDirectory, "20000*2.Designer.cs", SearchOption.AllDirectories);
            }
            else
            {
                // look for at least one migration
                migrationFiles = Directory.GetFiles(migrationDirectory, "2*.Designer.cs", SearchOption.AllDirectories);
                migrationFiles = migrationFiles.GroupBy(x => new FileInfo(x).Directory?.FullName).Select(x => x.FirstOrDefault()).ToArray();
            }

            ProcessMigrations(sqlStatements, migrationDirectory, config, moduleRegex, migrationNameRegex, migrationFiles);
        }

        private static void ProcessMigrations(Dictionary<string, List<string>> sqlStatements, string migrationDirectory, Config config, Regex moduleRegex, Regex migrationNameRegex, string[] migrationFiles)
        {
            Out($"Found {migrationFiles.Length} migrations in directory {migrationDirectory}");

            foreach (var migrationFile in migrationFiles)
            {
                var moduleName = moduleRegex.Match(migrationFile).Groups["module"].Value;

                if (moduleName.EndsWith(".Data"))
                {
                    var moduleRegexData = new Regex(@"(?<module>.+)\.Data");
                    moduleName = moduleRegexData.Match(moduleName).Groups["module"].Value;
                }

                // Set migrations range to extract. Leave it empty for all migrations
                var migrationName = config.GrabMode == GrabMode.V2V3
                    ? $"0 {migrationNameRegex.Match(File.ReadAllText(migrationFile)).Groups["migration"].Value}"
                    : string.Empty;

                var statementsFilePath = Path.Combine(new DirectoryInfo(config.StatementsDirectory).FullName, $"{moduleName}.sql");

                Out($"Extract migrations for module {moduleName}...");

                // Run dotnet-ef to extract migrations in idempotent manner
                var fileInfo = new FileInfo(migrationFile);

                var efTool = Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = fileInfo.Directory?.Parent?.FullName ?? string.Empty,
                    FileName = "dotnet",
                    Arguments = $"ef migrations script {migrationName} -o {statementsFilePath} -i {(config.VerboseEFTool ? "-v" : "")}",
                });

                efTool?.WaitForExit();

                sqlStatements.Add(moduleName, SplitStatements(File.ReadAllText(statementsFilePath)));

                Out("OK.");
            }
        }

        private static Dictionary<string, List<string>> ReadSavedStatements(string statementsDirectory)
        {
            var result = new Dictionary<string, List<string>>();
            var migrationFiles = Directory.GetFiles(statementsDirectory, "*.sql");

            foreach (var migrationFile in migrationFiles)
            {
                var migrationFileInfo = new FileInfo(migrationFile);
                var moduleName = migrationFileInfo.Name.Replace(migrationFileInfo.Extension, string.Empty);
                result.Add(moduleName, SplitStatements(File.ReadAllText(migrationFile)));
            }

            return result;
        }

        private static List<string> SplitStatements(string statements)
        {
            var statementsSplitRegex = new Regex(@"(?<statement>((?!\s*GO\s*).)+)\s*GO\s*", RegexOptions.Singleline);

            var statementsMatches = statementsSplitRegex.Matches(statements);

            var result = new List<string>();

            foreach (Match statement in statementsMatches)
            {
                result.Add(statement.Groups["statement"].Value);
            }

            return result;
        }

        private static void Fail(string text)
        {
            Assert.Fail($"{DateTime.Now}: {text}");
        }

        private static void Out(string text)
        {
            Log.Information($"{DateTime.Now}: {text}");
        }

        private static void OutBox(string text)
        {
            Out(new string('=', text.Length));
            Out(text);
            Out(new string('=', text.Length));
        }
    }
}
