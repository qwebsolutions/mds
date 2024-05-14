using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static partial class MdsInfrastructureFunctions
{
    public static async Task<ConfigurationHeadersList> Configurations(CommandContext commandContext)
    {
        return await commandContext.Do(Backend.LoadAllConfigurationHeaders);
    }

    public static async Task<EditConfigurationPage> InitializeEditConfiguration(
        CommandContext commandContext,
        InfrastructureConfiguration configuration)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        EditConfigurationPage editConfigurationPage = new EditConfigurationPage();
        editConfigurationPage.Configuration = configuration;

        var activeInfraDeployment = await commandContext.Do(Backend.LoadCurrentDeployment);

        editConfigurationPage.LastConfigurationDeployment = await commandContext.Do(Backend.LoadLastConfigurationDeployment, editConfigurationPage.Configuration.Id);
        editConfigurationPage.LastInfrastructureDeployment = activeInfraDeployment;
        editConfigurationPage.AllProjects = await commandContext.Do(Backend.LoadAllProjects);
        //editConfigurationPage.InitialConfiguration = editConfigurationPage.Configuration.Clone();
        editConfigurationPage.EnvironmentTypes = await commandContext.Do(Backend.LoadEnvironmentTypes);
        editConfigurationPage.InfrastructureNodes = await commandContext.Do(Backend.LoadAllNodes);

        editConfigurationPage.ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes);
        editConfigurationPage.NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes);
        editConfigurationPage.LastDeployed = editConfigurationPage.LastDeploymentLabel();

        Console.WriteLine($"=== InitializeEditConfiguration : {sw.ElapsedMilliseconds} ms ===");
        return editConfigurationPage;
    }

    public static string LastDeploymentLabel(this EditConfigurationPage dataModel)
    {
        string lastDeployment = "never deployed";

        if (Metapsi.Record.IsValid(dataModel.LastConfigurationDeployment))
        {
            lastDeployment = dataModel.LastConfigurationDeployment.Timestamp.ItalianFormat();
        }

        return $"Last deployed: {lastDeployment}";
    }


    public static async Task<List<MdsCommon.ServiceConfigurationSnapshot>> TakeConfigurationSnapshot(
        CommandContext commandContext,
        InfrastructureConfiguration infrastructureConfiguration,
        List<MdsCommon.Project> allProjects,
        List<InfrastructureNode> allNodes)
    {
        var infrastructureName = await commandContext.Do(Backend.GetInfrastructureName);
        var substitutionValues = infrastructureConfiguration.GetSubstitutionValues(allNodes);
        substitutionValues["InfrastructureName"] = infrastructureName;

        List<MdsCommon.ServiceConfigurationSnapshot> configurationSnapshot = new List<MdsCommon.ServiceConfigurationSnapshot>();
        foreach (var serviceName in infrastructureConfiguration.InfrastructureServices.Select(x => x.ServiceName))
        {
            var serviceSnapshot = await TakeServiceSnapshot(
                commandContext,
                infrastructureConfiguration,
                allProjects,
                allNodes,
                serviceName,
                substitutionValues);
            configurationSnapshot.Add(serviceSnapshot);
        }

        return configurationSnapshot;
    }

    public static async Task<MdsCommon.ServiceConfigurationSnapshot> TakeServiceSnapshot(
        CommandContext commandContext,
        InfrastructureConfiguration infrastructureConfiguration,
        List<MdsCommon.Project> allProjects,
        List<InfrastructureNode> allNodes,
        string serviceName,
        Dictionary<string, string> substitutionValues)
    {
        var service = infrastructureConfiguration.InfrastructureServices.SingleOrDefault(x => x.ServiceName == serviceName);
        var infrastructureNode = allNodes.Single(x => x.Id == service.InfrastructureNodeId);
        var project = allProjects.Single(x => x.Id == service.ProjectId);

        MdsCommon.ServiceConfigurationSnapshot snapshot = new MdsCommon.ServiceConfigurationSnapshot()
        {
            ApplicationName = infrastructureConfiguration.Applications.Single(x => x.Id == service.ApplicationId).Name,
            NodeName = infrastructureNode.NodeName,
            ProjectName = project.Name,
            ProjectVersionTag = project.Versions.Single(x => x.Id == service.ProjectVersionId).VersionTag,
            ServiceName = service.ServiceName,
            SnapshotTimestamp = DateTime.UtcNow,
            Enabled = service.Enabled
        };

        foreach (var serviceParameter in service.InfrastructureServiceParameterDeclarations)
        {
            MdsCommon.ServiceConfigurationSnapshotParameter serviceConfigurationSnapshotParameter = new MdsCommon.ServiceConfigurationSnapshotParameter()
            {
                ServiceConfigurationSnapshotId = snapshot.Id,
                ParameterName = serviceParameter.ParameterName,
                ConfiguredValue = service.GetParameterValue(serviceParameter, infrastructureConfiguration.InfrastructureVariables),
                ParameterTypeId = serviceParameter.ParameterTypeId
            };

            snapshot.ServiceConfigurationSnapshotParameters.Add(serviceConfigurationSnapshotParameter);

            substitutionValues[serviceParameter.ParameterName] = serviceConfigurationSnapshotParameter.ConfiguredValue;
        }

        foreach (var serviceConfigurationSnapshotParameter in snapshot.ServiceConfigurationSnapshotParameters)
        {
            serviceConfigurationSnapshotParameter.DeployedValue = Metapsi.Mds.SubstituteVariables(serviceConfigurationSnapshotParameter.ConfiguredValue, substitutionValues);
        }

        var existingSnapshot = await commandContext.Do(Backend.LoadIdenticalSnapshot, snapshot);

        // Does not already exist, so return this one
        if (existingSnapshot == null)
        {
            return snapshot;
        }
        else
        {
            // Return the already existing one, preserving the original IDs
            return existingSnapshot;
        }
    }

    public static List<string> ValidateConfiguration(this InfrastructureConfiguration configuration, ExternalConfiguration externalConfiguration)
    {
        List<string> validationErrors = new List<string>();

        validationErrors.AddRange(Simplified.GetDuplicates(configuration.InfrastructureServices, x => x.ServiceName, "Service"));
        validationErrors.AddRange(Simplified.GetDuplicates(configuration.Applications, x => x.Name, "Application"));
        validationErrors.AddRange(Simplified.GetDuplicates(configuration.InfrastructureVariables, x => x.VariableName, "Variable"));

        foreach (var service in configuration.InfrastructureServices)
        {
            if (string.IsNullOrEmpty(service.ServiceName))
            {
                validationErrors.Add($"Empty service name!");
            }

            if (validationErrors.Any())
            {
                return validationErrors;
            }
        }
         
        foreach (var service in configuration.InfrastructureServices)
        {
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                if (service.ServiceName.Contains(c))
                {
                    validationErrors.Add($"Service {service.ServiceName} cannot contain " + c);
                }
            }


            if (!configuration.Applications.Select(x => x.Id).Contains(service.ApplicationId))
            {
                validationErrors.Add($"Service {service.ServiceName} does not use a valid application");
            }

            var serviceProject = externalConfiguration.Projects.SingleOrDefault(x => x.Id == service.ProjectId);

            if (serviceProject == null)
            {
                validationErrors.Add($"Service {service.ServiceName} does not use a valid project");
            }
            else
            {
                if (!serviceProject.Versions.Any(x => x.Id == service.ProjectVersionId))
                {
                    validationErrors.Add($"Service {service.ServiceName} does not use a valid project version");
                }
            }

            validationErrors.AddRange(Simplified.GetDuplicates(service.InfrastructureServiceParameterDeclarations, x => x.ParameterName, "Parameter"));

            foreach (var parameter in service.InfrastructureServiceParameterDeclarations)
            {
                if (string.IsNullOrEmpty(parameter.ParameterName))
                {
                    validationErrors.Add($"Service {service.ServiceName} uses an unnamed parameter");
                }

                if (parameter.InfrastructureServiceParameterBindings.Any())
                {
                    if (!configuration.InfrastructureVariables.Select(x => x.Id).Contains(parameter.InfrastructureServiceParameterBindings.First().InfrastructureVariableId))
                    {
                        validationErrors.Add($"Service {service.ServiceName} does not use a valid variable for parameter {parameter.ParameterName}");
                    }
                }
            }

            if (!externalConfiguration.Nodes.Any(x => x.Id == service.InfrastructureNodeId))
            {
                validationErrors.Add($"Service {service.ServiceName} does not use a valid node");
            }
        }

        return validationErrors;
    }
}