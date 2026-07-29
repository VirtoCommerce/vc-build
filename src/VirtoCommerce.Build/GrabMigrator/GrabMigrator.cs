using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Nuke.Common;
using Serilog;
using VirtoCommerce.Platform.Core.Common;

namespace GrabMigrator
{
    /// <summary>
    ///     Grab-migrator target implementation
    /// </summary>
    internal static partial class GrabMigrator
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
                            sqlStatements = EnableGrabMode(configFilePath, config);
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

            if (config.ConnectionStringsRefs.TryGetValue(module, out var value))
            {
                foreach (var moduleConnStringKey in value)
                {
                    connectionString = connectionStrings.TryGetValue(moduleConnStringKey, out var connString) ? connString : string.Empty;

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        return connectionString;
                    }
                }
            }

            return connectionString;
        }

        private static Dictionary<string, List<string>> EnableGrabMode(string configFilePath, Config config)
        {
            OutBox("Grab mode");

            Out("Refresh connection strings references...");
            config.ConnectionStringsRefs = new Dictionary<string, List<string>>();

            foreach (var migrationDirectory in config.MigrationDirectories)
            {
                Out($"Looking in {migrationDirectory}...");
                GrabConnectionStringsRefsFromModules(config.ConnectionStringsRefs, migrationDirectory);
            }

            File.WriteAllText(configFilePath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

            var connectionStrings = ResolveConnectionStrings(config);

            Out("Looking for migrations in migration directories recursively...");
            var sqlStatements = GrabSqlStatements(config, connectionStrings);

            if (connectionStrings.Count > 0)
            {
                WriteDatabaseGrouping(config, sqlStatements, connectionStrings);
            }
            else
            {
                Out("Warning! PlatformConfigFile not set or not found - skipping per-database grouping report.");
            }

            return sqlStatements;
        }

        private static Dictionary<string, string> ResolveConnectionStrings(Config config)
        {
            if (string.IsNullOrEmpty(config.PlatformConfigFile) || !File.Exists(config.PlatformConfigFile))
            {
                return new Dictionary<string, string>();
            }

            Out("Read platform config file...");
            return GrabConnectionStrings(config.PlatformConfigFile);
        }

        private static void WriteDatabaseGrouping(Config config, Dictionary<string, List<string>> sqlStatements, Dictionary<string, string> connectionStrings)
        {
            Out("Building per-database combined scripts and mapping report...");

            var entries = new List<(string Module, string Server, string Database, List<string> Statements)>();

            foreach (var (module, statements) in sqlStatements)
            {
                try
                {
                    var connectionString = GetConnectionString(config, connectionStrings, module).EmptyToNull() ?? connectionStrings.GetValueOrDefault("VirtoCommerce");

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        continue;
                    }

                    var builder = new SqlConnectionStringBuilder(connectionString);
                    entries.Add((module, builder.DataSource, builder.InitialCatalog, statements));
                }
                catch (Exception exc)
                {
                    Out($"Warning! Could not resolve target database for module {module}: {exc.Message}");
                }
            }

            var statementsDir = new DirectoryInfo(config.StatementsDirectory).FullName;

            foreach (var group in entries.GroupBy(e => (e.Server, e.Database)))
            {
                var label = SanitizeFileName(string.IsNullOrEmpty(group.Key.Database) ? "database" : group.Key.Database);
                var combinedPath = Path.Combine(statementsDir, $"_combined.{label}.sql");
                var combined = string.Join($"{Environment.NewLine}GO{Environment.NewLine}", group.SelectMany(g => g.Statements));
                File.WriteAllText(combinedPath, combined);
            }

            var sb = new StringBuilder();
            sb.AppendLine("# Migration grab - database mapping");
            sb.AppendLine();
            sb.AppendLine("| Module | Server | Database |");
            sb.AppendLine("|---|---|---|");

            foreach (var entry in entries.OrderBy(e => e.Module, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"| {entry.Module} | {(string.IsNullOrEmpty(entry.Server) ? "(unknown)" : entry.Server)} | {(string.IsNullOrEmpty(entry.Database) ? "(unknown)" : entry.Database)} |");
            }

            File.WriteAllText(Path.Combine(statementsDir, "_databases.md"), sb.ToString());

            var jsonModel = entries
                .OrderBy(e => e.Module, StringComparer.OrdinalIgnoreCase)
                .Select(e => new { module = e.Module, server = e.Server, database = e.Database })
                .ToList();
            File.WriteAllText(Path.Combine(statementsDir, "_databases.json"), JsonSerializer.Serialize(jsonModel, new JsonSerializerOptions { WriteIndented = true }));
        }

        private static string SanitizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
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
            var connKeyRegex = ConnectionKeyRegex();
            var moduleRegex = ModuleRegex();
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

        private static Dictionary<string, List<string>> GrabSqlStatements(Config config, Dictionary<string, string> connectionStrings)
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var migrationDirectory in config.MigrationDirectories)
            {
                GrabSqlStatementsWithEFTool(result, migrationDirectory, config, connectionStrings);
            }

            return result;
        }

        private static void GrabSqlStatementsWithEFTool(Dictionary<string, List<string>> sqlStatements, string migrationDirectory, Config config, Dictionary<string, string> connectionStrings)
        {
            Directory.CreateDirectory(config.StatementsDirectory);
            var moduleRegex = ModuleMigrationsRegex();
            var migrationNameRegex = MigrationNameRegex();
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

            ProcessMigrations(sqlStatements, migrationDirectory, config, moduleRegex, migrationNameRegex, migrationFiles, connectionStrings);
        }

        private static void ProcessMigrations(Dictionary<string, List<string>> sqlStatements, string migrationDirectory, Config config, Regex moduleRegex, Regex migrationNameRegex, string[] migrationFiles, Dictionary<string, string> connectionStrings)
        {
            Out($"Found {migrationFiles.Length} migrations in directory {migrationDirectory}");

            foreach (var migrationFile in migrationFiles)
            {
                var moduleName = moduleRegex.Match(migrationFile).Groups["module"].Value;

                if (moduleName.EndsWith(".Data"))
                {
                    var moduleRegexData = ModuleDataRegex();
                    moduleName = moduleRegexData.Match(moduleName).Groups["module"].Value;
                }

                try
                {
                    // Set migrations range to extract. Leave it empty for all migrations
                    var migrationName = ResolveMigrationRange(config, migrationNameRegex, migrationFile, moduleName, connectionStrings);
                    var statementsFilePath = Path.Combine(new DirectoryInfo(config.StatementsDirectory).FullName, $"{moduleName}.sql");

                    Out($"Extract migrations for module {moduleName}...");

                    RunEfMigrationsScript(config, migrationFile, moduleName, migrationName, statementsFilePath);

                    sqlStatements.Add(moduleName, SplitStatements(File.ReadAllText(statementsFilePath)));

                    Out("OK.");
                }
                catch (Exception exc)
                {
                    Out($"Warning! Failed to grab migrations for module {moduleName}: {exc.Message}");
                }
            }
        }

        private static string ResolveMigrationRange(Config config, Regex migrationNameRegex, string migrationFile, string moduleName, Dictionary<string, string> connectionStrings)
        {
            if (config.GrabMode == GrabMode.V2V3)
            {
                return $"0 {migrationNameRegex.Match(File.ReadAllText(migrationFile)).Groups["migration"].Value}";
            }

            if (config.PendingOnly && connectionStrings.Count > 0)
            {
                var lastApplied = TryGetLastAppliedMigration(config, connectionStrings, moduleName);

                if (lastApplied != null)
                {
                    return lastApplied;
                }

                Out($"Warning! Could not determine applied migrations for module {moduleName}; falling back to a full script.");
            }

            return string.Empty;
        }

        private static string TryGetLastAppliedMigration(Config config, Dictionary<string, string> connectionStrings, string moduleName)
        {
            var connectionString = GetConnectionString(config, connectionStrings, moduleName).EmptyToNull() ?? connectionStrings.GetValueOrDefault("VirtoCommerce");

            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT TOP 1 [MigrationId] FROM [__EFMigrationsHistory] ORDER BY [MigrationId] DESC";
                return command.ExecuteScalar() as string;
            }
            catch (Exception exc)
            {
                Out($"Warning! Could not read migration history for module {moduleName}: {exc.Message}");
                return null;
            }
        }

        private static void RunEfMigrationsScript(Config config, string migrationFile, string moduleName, string migrationName, string statementsFilePath)
        {
            var fileInfo = new FileInfo(migrationFile);

            // Idempotent (self-guarding) script, unless a precise pending-only range was already resolved
            var idempotentArg = config.Idempotent && !config.PendingOnly ? "-i" : string.Empty;

            var contextArg = config.ContextNames != null && config.ContextNames.TryGetValue(moduleName, out var contextName) && !string.IsNullOrEmpty(contextName)
                ? $"--context {contextName}"
                : string.Empty;

            var efTool = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = fileInfo.Directory?.Parent?.Parent?.FullName ?? string.Empty,
                FileName = "dotnet",
                Arguments = $"ef migrations script {migrationName} -o {statementsFilePath} {idempotentArg} {(config.VerboseEFTool ? "-v" : "")} {contextArg}",
            });

            efTool?.WaitForExit();

            if (efTool == null || efTool.ExitCode != 0)
            {
                throw new InvalidOperationException($"dotnet-ef failed for module {moduleName} (exit code {efTool?.ExitCode.ToString() ?? "n/a"}).");
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
            var statementsSplitRegex = StatementsSplitRegex();

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

        [GeneratedRegex(@"\.GetConnectionString\(""(?<connkey>((?!GetConnectionString)[\w.])*)""\)", RegexOptions.Singleline)]
        private static partial Regex ConnectionKeyRegex();
        [GeneratedRegex(@"[\\\w^\.-]*\\(?<module>.+)\.Web")]
        private static partial Regex ModuleRegex();
        [GeneratedRegex(@"[\\\w^\.-]*\\(?<module>.+)\\Migrations")]
        private static partial Regex ModuleMigrationsRegex();
        [GeneratedRegex(@"\[Migration\(""(?<migration>.+)""\)\]")]
        private static partial Regex MigrationNameRegex();
        [GeneratedRegex(@"(?<module>.+)\.Data")]
        private static partial Regex ModuleDataRegex();
        [GeneratedRegex(@"(?<statement>((?!\s*GO\s*).)+)\s*GO\s*", RegexOptions.Singleline)]
        private static partial Regex StatementsSplitRegex();
    }
}
