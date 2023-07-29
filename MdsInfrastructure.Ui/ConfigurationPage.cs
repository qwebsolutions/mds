﻿using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Metapsi;
using System.Linq.Expressions;
using Metapsi.Syntax;
using Metapsi.Hyperapp;

namespace MdsInfrastructure
{
    

    public static partial class MdsInfrastructureFunctions
    {
        public static async Task<ConfigurationHeadersList> Configurations(CommandContext commandContext)
        {
            return await commandContext.Do(Api.LoadAllConfigurationHeaders);
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

        

        public static async Task<EditConfigurationPage> SaveConfiguration(CommandContext commandContext, EditConfigurationPage dataModel, Guid id)
        {
            await commandContext.Do(Api.SaveConfiguration, dataModel.Configuration);
            //dataModel.InitialConfiguration = dataModel.Configuration.Clone();
            return dataModel;
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

        public class ValidationMessage
        {
            public Guid EntityId { get; set; }
            public string Property { get; set; }
            public string Message { get; set; }

            public ValidationMessage() { }
            public ValidationMessage(Metapsi.IRecord record, string property, string message)
            {
                EntityId = record.Id;
                Property = property;
                Message = message;
            }
        }

        public static ValidationMessage ValidateConfiguration(
            InfrastructureConfiguration configuration,
            List<MdsCommon.Project> allProjects,
            List<InfrastructureNode> allNodes,
            List<NoteType> noteTypes)
        {
            HashSet<string> alreadyProcessedServiceNames = new HashSet<string>();

            foreach (var node in allNodes)
            {
                if (string.IsNullOrWhiteSpace(node.NodeName))
                {
                    return new ValidationMessage(node, nameof(node.NodeName), "Node name not set!");
                }

                if (string.IsNullOrWhiteSpace(node.MachineIp))
                {
                    return new ValidationMessage(node, nameof(node.MachineIp), "Node address not set!");
                }

                if (node.EnvironmentTypeId == Guid.Empty)
                {
                    return new ValidationMessage(node, nameof(node.EnvironmentTypeId), "Node type not set!");
                }
            }

            foreach (var service in configuration.InfrastructureServices)
            {
                string serviceName = service.ServiceName;
                if (serviceName.Any(x => !IsAllowedFilenameCharacter(x)))
                    return new ValidationMessage(service, nameof(service.ServiceName), $"Service name '{serviceName}' is not allowed. Use just alphanumeric and '_' characters");

                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return new ValidationMessage(service, nameof(service.ServiceName), "Service name not set!");
                }

                if (alreadyProcessedServiceNames.Contains(serviceName.ToLower()))
                {
                    return new ValidationMessage(service, nameof(service.ServiceName), $"Duplicate service name: {serviceName}");
                }
                else
                {
                    alreadyProcessedServiceNames.Add(serviceName.ToLower());
                }

                if (!configuration.Applications.Any(x=>x.Id == service.ApplicationId))
                {
                    return new ValidationMessage(service, nameof(service.ApplicationId), $"{service.ServiceName}: Application not selected");
                }

                if (!allProjects.Any(x=>x.Id == service.ProjectId))
                    return new ValidationMessage(service, nameof(service.ProjectId), $"{service.ServiceName}: Project not selected");

                var selectedProject = allProjects.Single(x => x.Id == service.ProjectId);
                if (!selectedProject.Versions.Any(x=>x.Id == service.ProjectVersionId))
                    return new ValidationMessage(service, nameof(service.ProjectVersionId), $"{service.ServiceName}: Project version not selected");

                if (!allNodes.Any(x=>x.Id == service.InfrastructureNodeId))
                {
                    return new ValidationMessage(service, nameof(service.InfrastructureNodeId), $"{service.ServiceName}: Deployment node not selected");
                }

                var parameters = service.InfrastructureServiceParameterDeclarations;

                HashSet<System.Guid> serviceParameterIds = parameters.Select(x => x.Id).ToHashSet();

                var unnamedParameter = parameters.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.ParameterName));

                if (unnamedParameter != null)
                {
                    return new ValidationMessage(unnamedParameter, nameof(unnamedParameter.ParameterName), $"Parameter does not have a proper name in service {serviceName}");
                }

                var repeatedParameter = parameters.GroupBy(x => x.ParameterName).FirstOrDefault(x => x.Count() > 1);
                if (repeatedParameter != null)
                {
                    return new ValidationMessage(repeatedParameter.First(), nameof(InfrastructureServiceParameterDeclaration.ParameterName), $"Duplicate parameter name: {repeatedParameter.Key}");
                }

                foreach (var parameter in parameters)
                {
                    var serviceParameterBindings = parameter.InfrastructureServiceParameterBindings;

                    var emptyParameter = serviceParameterBindings.FirstOrDefault(x => x.InfrastructureVariableId == System.Guid.Empty);

                    if (emptyParameter != null)
                    {
                        return new ValidationMessage(emptyParameter, nameof(emptyParameter.InfrastructureVariableId), $"Parameter {parameter.ParameterName} of service {serviceName} has no specified variable!");
                    }
                }
                foreach (var note in service.InfrastructureServiceNotes)
                {
                    if (note.NoteTypeId == Guid.Empty)
                    {
                        return new ValidationMessage(note, nameof(note.NoteTypeId), $"Note type not set");
                    }

                    if (string.IsNullOrEmpty(note.Note))
                    {
                        return new ValidationMessage(note, nameof(note.Note), $"Note cannot be empty");
                    }

                    var noteType = noteTypes.Single(x => x.Id == note.NoteTypeId);

                    if (noteType.Code.ToLower() == "parameter")
                    {
                        if (string.IsNullOrEmpty(note.Reference))
                            return new ValidationMessage(note, nameof(note.Reference), "Parameter reference not set");

                        var parameterId = Guid.Parse(note.Reference);
                        if (!service.InfrastructureServiceParameterDeclarations.Any(x => x.Id == parameterId))
                        {
                            return new ValidationMessage(note, nameof(note.Reference), "Parameter reference not set");
                        }
                    }
                }
            }

            if (configuration.Applications.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                return new ValidationMessage(configuration.Applications.First(x => string.IsNullOrWhiteSpace(x.Name)), nameof(Application.Name), "Application name cannot be empty");
            }

            var unnamedVariable = configuration.InfrastructureVariables.SingleOrDefault(x => string.IsNullOrWhiteSpace(x.VariableName));

            if (unnamedVariable != null)
            {
                return new ValidationMessage(unnamedVariable, nameof(unnamedVariable.VariableName), $"All variables should have a proper name");
            }

            return null;
        }

        public static bool IsAllowedFilenameCharacter(char c)
        {
            if (char.IsLetterOrDigit(c))
                return true;

            if (c == '_')
                return true;

            return false;
        }

        public static async Task<List<MdsCommon.ServiceConfigurationSnapshot>> TakeConfigurationSnapshot(
            CommandContext commandContext,
            InfrastructureConfiguration infrastructureConfiguration,
            List<MdsCommon.Project> allProjects,
            List<InfrastructureNode> allNodes)
        {
            var infrastructureName = await commandContext.Do(Api.GetInfrastructureName);
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

            var existingSnapshot = await commandContext.Do(Api.LoadServiceSnapshotByHash, hash);

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

        public static async Task<MdsCommon.ListInfrastructureEventsPage> ListInfrastructureEvents(CommandContext commandContext)
        {
            return new MdsCommon.ListInfrastructureEventsPage()
            {
                InfrastructureEvents = await commandContext.Do(MdsCommon.Api.GetAllInfrastructureEvents)
            };
        }
    }
}
