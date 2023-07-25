using Metapsi;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M = MdsInfrastructure;

namespace MdsInfrastructure.Flow
{
    public static class Docs
    {
        public class SummaryInput
        {
            public string InfrastructureName { get; set; }
            public InfrastructureConfiguration InfrastructureConfiguration { get; set; }
            public List<InfrastructureNode> InfrastructureNodes { get; set; }
            public List<ParameterType> ParameterTypes { get; set; }
            public List<NoteType> NoteTypes { get; set; }
            public M.Deployment CurrentDeployment { get; set; }
        }

        public class Service : Metapsi.Http.Get<Routes.Docs.Service, string>
        {
            public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, string serviceName)
            {
                SummaryInput summaryInput = new SummaryInput()
                {
                    CurrentDeployment = await commandContext.Do(Api.LoadCurrentDeployment),
                    InfrastructureConfiguration = await commandContext.Do(Api.LoadCurrentConfiguration),
                    InfrastructureName = await commandContext.Do(Api.GetInfrastructureName),
                    InfrastructureNodes = await commandContext.Do(Api.LoadAllNodes),
                    ParameterTypes = await commandContext.Do(Api.GetAllParameterTypes),
                    NoteTypes = await commandContext.Do(Api.GetAllNoteTypes)
                };

                var summary = GetSummary(summaryInput);
                var currentService = summary.ServiceReferences.Single(x => x.ServiceName == serviceName);

                return Page.Result(new M.Docs.ServicePage()
                {
                    InfrastructureSummary = summary,
                    ServiceSummary = currentService,
                    User = httpContext.User()
                });
            }
        }

        public class RedisMap : Metapsi.Http.Get<Routes.Docs.RedisMap, string>
        {
            public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, string serviceName)
            {
                SummaryInput summaryInput = new SummaryInput()
                {
                    CurrentDeployment = await commandContext.Do(Api.LoadCurrentDeployment),
                    InfrastructureConfiguration = await commandContext.Do(Api.LoadCurrentConfiguration),
                    InfrastructureName = await commandContext.Do(Api.GetInfrastructureName),
                    InfrastructureNodes = await commandContext.Do(Api.LoadAllNodes),
                    ParameterTypes = await commandContext.Do(Api.GetAllParameterTypes),
                    NoteTypes = await commandContext.Do(Api.GetAllNoteTypes)
                };

                var summary = GetSummary(summaryInput);
                var serviceSummary = summary.ServiceReferences.Single(x => x.ServiceName == serviceName);


                return Page.Result(new M.Docs.RedisMap()
                {
                    InfrastructureSummary = summary,
                    ServiceSummary = serviceSummary,
                    User = httpContext.User()
                });
            }
        }


        public static InfrastructureSummary GetSummary(SummaryInput summaryInput)
        {
            var currentInfrastructure = summaryInput.InfrastructureConfiguration;// await commandContext.Do(MdsInfrastructureApplication.LoadCurrentConfiguration);
            var allNodes = summaryInput.InfrastructureNodes; //await commandContext.Do(MdsInfrastructureApplication.LoadAllNodes);
            var allParameterTypes = summaryInput.ParameterTypes; //await commandContext.Do(MdsInfrastructureApplication.GetAllParameterTypes);
            var allNoteTypes = summaryInput.NoteTypes;// await commandContext.Do(MdsInfrastructureApplication.GetAllNoteTypes);
            var currentDeployment = summaryInput.CurrentDeployment;// await commandContext.Do(MdsInfrastructureApplication.LoadCurrentDeployment);
            var infrastructureName = summaryInput.InfrastructureName;// await commandContext.Do(MdsInfrastructureApplication.GetInfrastructureName);

            // Known parameter types
            var redisInputQueue = allParameterTypes.SingleOrDefault(x => x.Code.ToLower() == "redisinputqueue");
            var redisOutputQueue = allParameterTypes.SingleOrDefault(x => x.Code.ToLower() == "redisoutputqueue");
            var redisInputChannel = allParameterTypes.SingleOrDefault(x => x.Code.ToLower() == "redisinputchannel");
            var redisOutputChannel = allParameterTypes.SingleOrDefault(x => x.Code.ToLower() == "redisoutputchannel");
            var port = allParameterTypes.Single(x => x.Code.ToLower() == "port");
            var apiUrl = allParameterTypes.Single(x => x.Code.ToLower() == "apiaccessurl");

            // Known note types
            var paramNoteType = allNoteTypes.SingleOrDefault(x => x.Code.ToLower() == "parameter");
            var altUrl = allNoteTypes.SingleOrDefault(x => x.Code.ToLower() == "alturl");
            var serviceComment = allNoteTypes.SingleOrDefault(x => x.Code.ToLower() == "summary");

            var deployedServiceSnapshots = currentDeployment.GetDeployedServices();
            var deployedSnapshotIds = new HashSet<Guid>(deployedServiceSnapshots.Select(x => x.Id));

            InfrastructureSummary summary = new InfrastructureSummary();
            summary.InfrastructureName = infrastructureName;

            var deployedServices = currentDeployment.GetDeployedServices().OrderBy(x => x.ServiceName).ToList();

            if (currentDeployment != null && deployedServices.Any())
            {
                foreach (var serviceSnapshot in deployedServices)
                {
                    var infraService = currentInfrastructure.InfrastructureServices.SingleOrDefault(x => x.ServiceName == serviceSnapshot.ServiceName);
                    var serviceNode = allNodes.SingleOrDefault(x => x.NodeName == serviceSnapshot.NodeName, new InfrastructureNode());


                    ServiceSummary serviceSummary = new ServiceSummary()
                    {
                        ServiceName = serviceSnapshot.ServiceName,
                        NodeName = serviceSnapshot.NodeName,
                        MachineIp = serviceNode.MachineIp,
                        Project = serviceSnapshot.ProjectName,
                        Version = serviceSnapshot.ProjectVersionTag,
                        ServiceDescription = GetServiceComment(infraService, serviceComment)
                    };

                    summary.ServiceReferences.Add(serviceSummary);

                    foreach (var p in serviceSnapshot.ServiceConfigurationSnapshotParameters)
                    {
                        var serviceParameter = new ServiceParameter()
                        {
                            ParameterName = p.ParameterName,
                            ParameterComment = GetParameterComment(infraService, p.ParameterName, paramNoteType),
                            DeployedValue = p.DeployedValue,
                            ParameterType = allParameterTypes.SingleOrDefault(x => x.Id == p.ParameterTypeId, new ParameterType() { Code = String.Empty }).Code,
                            ParameterTypeDescription = allParameterTypes.SingleOrDefault(x => x.Id == p.ParameterTypeId, new ParameterType() { Code = String.Empty }).Description
                        };

                        serviceSummary.ServiceParameters.Add(serviceParameter);

                        switch (serviceParameter.ParameterType.ToLower())
                        {
                            case "redisinputqueue":
                                {
                                    serviceSummary.InputQueues.Add(serviceParameter.DeployedValue);
                                }
                                break;
                            case "redisoutputqueue":
                                {
                                    serviceSummary.OutputQueues.Add(serviceParameter.DeployedValue);
                                }
                                break;
                            case "redisinputchannel":
                                {
                                    serviceSummary.InputChannels.Add(serviceParameter.DeployedValue);
                                }
                                break;
                            case "redisoutputchannel":
                                {
                                    serviceSummary.OutputChannels.Add(serviceParameter.DeployedValue);
                                }
                                break;
                            case "port":
                                {
                                    serviceSummary.ListeningPorts.Add(Int32.Parse(serviceParameter.DeployedValue));
                                }
                                break;
                            case "apiaccessurl":
                                {
                                    if (!string.IsNullOrWhiteSpace(serviceParameter.DeployedValue))
                                    {
                                        serviceSummary.AccessedUrls.Add(serviceParameter.DeployedValue);
                                    }
                                }
                                break;
                            case "dbconnectionstring":
                                {
                                    if (!string.IsNullOrWhiteSpace(serviceParameter.DeployedValue))
                                    {
                                        //serviceParameter.DeployedValue = ParseConnectionString(serviceParameter.DeployedValue).ToString();
                                        serviceSummary.DbConnections.Add(serviceParameter.DeployedValue);
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return summary;
        }


        public static string GetParameterComment(
            InfrastructureService infrastructureService,
            string parameterName,
            NoteType paramNoteType)
        {
            if (infrastructureService == null)
                return string.Empty;

            var configParam = infrastructureService.InfrastructureServiceParameterDeclarations.SingleOrDefault(x => x.ParameterName == parameterName);

            if (configParam == null)
                return String.Empty;

            var paramNote = infrastructureService.InfrastructureServiceNotes.SingleOrDefault(x => x.NoteTypeId == paramNoteType.Id && x.Reference == configParam.Id.ToString());

            if (paramNote == null)
                return String.Empty;

            return paramNote.Note;
        }

        public static string GetServiceComment(
            InfrastructureService infrastructureService,
            NoteType noteType)
        {
            if (infrastructureService == null)
                return string.Empty;

            var note = infrastructureService.InfrastructureServiceNotes.SingleOrDefault(x => x.NoteTypeId == noteType.Id);
            if (note == null)
                return String.Empty;

            return note.Note;
        }
    }
}
