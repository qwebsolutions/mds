﻿using Metapsi;
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

    public static string GetConfigurationCellValue(ConfigurationHeadersList dataModel, InfrastructureConfiguration configurationHeader, string fieldName)
    {
        if (fieldName == "ServicesCount")
        {
            return dataModel.Services.Where(x => x.ConfigurationHeaderId == configurationHeader.Id).Count().ToString();
        }

        if (fieldName == nameof(InfrastructureConfiguration.Name))
            return configurationHeader.Name;

        return string.Empty;
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
        foreach (var serviceName in infrastructureConfiguration.InfrastructureServices.Where(x => x.Enabled).Select(x => x.ServiceName))
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
            SnapshotTimestamp = DateTime.UtcNow
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

        string hash = GetHash(snapshot);

        var existingSnapshot = await commandContext.Do(Backend.LoadServiceSnapshotByHash, hash);

        // Does not already exist, so return this one
        if (existingSnapshot == null)
        {
            snapshot.Hash = hash;
            return snapshot;
        }
        else
        {
            // Return the already existing one, preserving the original ID
            return existingSnapshot;
        }
    }

    public static string GetHash(MdsCommon.ServiceConfigurationSnapshot serviceSnapshot)
    {
        // Cannot serialize the data structure, as the IDs will be always different
        // Cannot set IDs to Guid.Empty, as the collection will not accept duplicate keys
        // So, just add as most data as possible to a string builder
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();


        // Set fields that are always different to fixed values
        var serializedService = serviceSnapshot.Clone();
        serializedService.Id = Guid.Empty;
        serializedService.SnapshotTimestamp = DateTime.MinValue;
        serializedService.ServiceConfigurationSnapshotParameters.Clear();

        // Application name is not relevant in deployment
        serializedService.ApplicationName = String.Empty;

        stringBuilder.AppendLine(Metapsi.Serialize.ToJson(serializedService));

        foreach (var parameter in serviceSnapshot.ServiceConfigurationSnapshotParameters.OrderBy(x => x.ParameterName))
        {
            var serializedParameter = parameter.Clone();
            serializedParameter.Id = Guid.Empty;
            serializedParameter.ServiceConfigurationSnapshotId = Guid.Empty;
            serializedParameter.ParameterTypeId = Guid.Empty; // If parameter type changes, the deployment is not affected
            serializedParameter.ConfiguredValue = String.Empty; // If configured value changes, the deployed value could still be the same

            stringBuilder.AppendLine(Metapsi.Serialize.ToJson(serializedParameter));
        }
        using (var hash = System.Security.Cryptography.MD5.Create())
        {
            string builderValue = stringBuilder.ToString();
            var hashBytes = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(builderValue));
            return System.Convert.ToBase64String(hashBytes);
        }
    }

}