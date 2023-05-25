using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;
using MdsCommon;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class Docs
    {
        public class SummaryInput
        {
            public string InfrastructureName { get; set; }
            public InfrastructureConfiguration InfrastructureConfiguration { get; set; }
            public List<InfrastructureNode> InfrastructureNodes { get; set; }
            public List<ParameterType> ParameterTypes { get; set; }
            public List<NoteType> NoteTypes { get; set; }
            public Deployment CurrentDeployment { get; set; }
        }

        public static async Task<IResponse> Service(CommandContext commandContext, HttpContext requestData)
        {
            //var serviceId = Guid.Parse(requestData.Parameters.Single());
            var serviceName = requestData.EntityId();

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

            return Page.Response(
                new object(), (b, clientModel) => b.Layout(b.InfraMenu(nameof(Status), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Docs", Entity = currentService.ServiceName },
                    User = requestData.User()
                })), b.Render(summary, currentService)));
        }

        public class Connection
        {
            public string FromId { get; set; }
            public string ToId { get; set; }
            public string FromSide { get; set; } = "right";
            public string ToSide { get; set; } = "left";
        }

        public static void AddConnector(this BlockBuilder b,
            string fromId,
            string toId,
            string fromSide = "right",
            string toSide = "left")
        {
            b.AddScript("leader-line.min.js");

            b.AddSubscription(
                "leaderLineSub",
                (BlockBuilder b, Var<object> state) =>
                b.Listen(b.Const("afterRender"), b.MakeAction((BlockBuilder b, Var<object> state, Var<object> _payload) =>
                {
                    b.CallExternal<object>("connect", "update");
                    return state;
                })));

            var connections = b.CallExternal<List<Connection>>("connect", "getConnectors");
            var c = b.Const(new Connection() { FromId = fromId, ToId = toId, FromSide = fromSide, ToSide = toSide });
            b.Push(connections, c);
        }

        public class ControlsMapping
        {
            public string Key { get; set; }
            public string ControlId { get; set; }
        }

        public static Var<HyperNode> ServiceLink(this BlockBuilder b,
            string serviceName,
            string controlId)
        {
            var container = b.Div("py-8 px-4 font-semibold");
            var control = b.Add(container, b.Link(b.Const(serviceName), Service, b.Const(serviceName)));
            b.SetAttr(control, Html.id, controlId);
            return container;
        }


        public static Var<HyperNode> ExternalApiUrl(this BlockBuilder b,
            string url,
            string controlId)
        {
            var container = b.Div("flex flex-col px-4");
            //var icon = b.Add(container, b.Div("text-yellow-300"));
            //b.SetInnerHtml(icon, b.FromFile("inline/antenna.svg"));
            b.Add(container, b.Text(url));
            b.SetAttr(container, Html.id, controlId);
            return container;
        }


        public static Var<HyperNode> ApiUrl(this BlockBuilder b,
            string serviceName,
            string url,
            string controlId)
        {
            var container = b.Div("flex flex-col px-4");
            //var icon = b.Add(container, b.Div("text-yellow-300"));
            //b.SetInnerHtml(icon, b.FromFile("inline/antenna.svg"));
            var semiBold = b.Add(container, b.Div("font-semibold"));
            b.Add(semiBold, b.Link(b.Const(serviceName), Service, b.Const(serviceName)));
            b.Add(container, b.Text(url));
            b.SetAttr(container, Html.id, controlId);
            return container;
        }

        public static Var<HyperNode> RedisChannel(this BlockBuilder b,
            string channelName,
            string controlId)
        {
            var container = b.Div("rounded shadow p-2 relative");
            //var icon = b.Add(container, b.Div("text-red-200 absolute right-0 -top-6"));
            //b.SetInnerHtml(icon, b.FromFile("inline/antenna.svg"));
            var control = b.Add(container, b.Text(channelName));
            b.SetAttr(container, Html.id, controlId);
            return container;
        }

        public static Var<HyperNode> RedisQueue(this BlockBuilder b,
            string channelName,
            string controlId)
        {
            var container = b.Div("rounded shadow p-2 relative");
            //var icon = b.Add(container, b.Div("text-red-200 absolute right-0 -top-6"));
            //b.SetInnerHtml(icon, b.FromFile("inline/queue.svg"));
            var control = b.Add(container, b.Text(channelName));
            b.SetAttr(container, Html.id, controlId);
            return container;
        }
        public static Var<HyperNode> Render(this BlockBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            var view = b.Div("flex flex-col space-y-4 text-gray-800");
            b.Add(view, b.RenderServiceCard(summary, currentService));
            b.Add(view, b.RenderServiceMap(summary, currentService));
            b.Add(view, b.RenderRedisMapCard(summary, currentService));
            return view;
        }

        public static Var<HyperNode> RenderServiceCard(this BlockBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            var card = b.Div("w-full rounded flex flex-row p-4 space-x-8 text-gray-700 shadow bg-white");

            var left = b.Add(card, b.Div("flex flex-col flex-1"));

            b.Add(left, b.Bold(currentService.ServiceName));
            var summaryNote = currentService.ServiceDescription;
            if (!string.IsNullOrEmpty(summaryNote))
            {
                var summaryNoteText = b.Add(left, b.Text(summaryNote));
                b.AddClass(summaryNoteText, "font-thin text-gray-500");
            }

            foreach (var parameter in currentService.ServiceParameters)
            {
                if(parameter.ParameterType.ToLower() == "dbconnectionstring")
                {
                    parameter.DeployedValue = MdsCommon.Parameter.ParseConnectionString(parameter.DeployedValue).ToString();
                }

                if(parameter.ParameterName.ToLower().Contains("password"))
                {
                    parameter.DeployedValue = "*****";
                }

                var paramContainer = b.Add(left, b.Div("flex flex-col pt-2 text-gray-700 text-sm"));
                var paramLine = b.Add(paramContainer, b.Div("flex flex-row space-x-2"));
                var paramName = b.Add(paramLine, b.Text(parameter.ParameterName));
                b.AddClass(paramName, "font-semibold");
                b.Add(paramLine, b.Text(parameter.DeployedValue));

                //var paramComment = currentService.Notes.SingleOrDefault(x => x.Code.ToLower() == "parameter" && x.Reference == parameter.ParameterName);

                if (!string.IsNullOrEmpty(parameter.ParameterComment))
                {
                    var doc = b.Add(paramContainer, b.Text(parameter.ParameterComment));
                    b.AddClass(doc, "text-gray-500");
                }
                else
                {
                    var doc = b.Add(paramContainer, b.Text(parameter.ParameterTypeDescription));
                    b.AddClass(doc, "text-gray-500");

                }
            }

            var divToIgnoreSize = b.Add(card, b.Div());

            var right = b.Add(divToIgnoreSize, b.Div("flex flex-col items-start space-y-2 bg-gray-50 rounded flex-initial px-8 py-4 text-sm text-gray-500"));

            var nodeLine = b.Add(right, b.Div("flex flex-row space-x-2 items-center font-semibold"));
            b.Add(nodeLine, b.Svg(Icon.Computer, "w-3 h-3 text-gray-400"));
            b.Add(nodeLine, b.Text(currentService.NodeName));

            var projectLine = b.Add(right, b.Div("flex flex-row space-x-2 items-center font-semibold"));
            b.Add(projectLine, b.Svg(Icon.DocumentText, "w-3 h-3 text-gray-400"));
            b.Add(projectLine, b.Text($"{currentService.ProjectLabel()}"));

            return card;
        }

        public static Var<HyperNode> RenderServiceMap(this BlockBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            List<ControlsMapping> controlsMappings = new List<ControlsMapping>();

            //b.Module.Imports.Add("import {connectors} from '/connect.js'");

            var serviceMapRoot = b.Div("h-full w-full p-2 text-sm flex flex-col space-y-8 rounded shadow bg-white");

            var httpRowContainer = b.Add(serviceMapRoot, b.Div("flex flex-row items-center"));

            var httpLabel = b.Add(httpRowContainer, b.Text("HTTP"));
            b.AddClass(httpLabel, "text-lg font-semibold text-gray-300 vertical-text");

            var httpServicesContainer = b.Add(httpRowContainer, b.Div("flex w-full justify-center"));
            var httpArea = b.Add(httpServicesContainer, b.Div("flex flex-row items-center"));

            var redisRow = b.Add(serviceMapRoot, b.Div("flex flex-row items-center"));

            var redisLabel = b.Add(redisRow, b.Text("REDIS"));
            b.AddClass(redisLabel, "text-lg font-semibold text-gray-300 vertical-text");

            //var redisAreas = b.Add(redisRow, b.Div("flex flex-row items-center"));

            var redisInputArea = b.Add(redisRow, b.Div("flex-1 flex flex-row"));
            var inputServicesArea = b.Add(redisInputArea, b.Div(""));
            var inputRedisArea1 = b.Add(redisInputArea, b.Div("w-full flex items-center justify-end"));
            var inputRedisArea = b.Add(inputRedisArea1, b.Div(""));

            var serviceNameDiv = b.Add(redisRow, b.Div("flex-none p-16 relative"));
            var portsArea = b.Add(serviceNameDiv, b.Div("flex flex-row justify-center absolute -translate-y-5 translate-x-12 space-x-2 font-thin text-xs"));
            var serviceText = b.Add(serviceNameDiv, b.Bold(b.Const(currentService.ServiceName)));
            var serviceNameControlId = "ctrl_currentService";
            b.SetAttr(serviceText, Html.id, serviceNameControlId);

            var redisOutputArea = b.Add(redisRow, b.Div("flex-1 flex flex-row"));
            var outputRedisArea_ = b.Add(redisOutputArea, b.Div("w-full flex items-center justify-start"));
            var outputRedisArea = b.Add(outputRedisArea_, b.Div());
            var outputServicesArea = b.Add(redisOutputArea, b.Div(""));


            List<ControlsMapping> inputServices = new();
            string addInputService(string serviceName)
            {
                var inputService = inputServices.SingleOrDefault(x => x.Key == serviceName);

                if(inputService == null)
                {
                    inputService = new ControlsMapping()
                    {
                        Key = serviceName,
                        ControlId = "inputService_" + inputServices.Count
                    };
                    inputServices.Add(inputService);
                    var control = b.Add(inputServicesArea, b.ServiceLink(serviceName, inputService.ControlId));
                }
                return inputService.ControlId;
            }

            List<ControlsMapping> outputServices = new();
            string addOutputService(string serviceName)
            {
                var outputService = outputServices.SingleOrDefault(x => x.Key == serviceName);
                if (outputService == null)
                {
                    outputService = new ControlsMapping()
                    {
                        Key = serviceName,
                        ControlId = "outputService_" + outputServices.Count
                    };
                    outputServices.Add(outputService);
                    var control = b.Add(outputServicesArea, b.ServiceLink(serviceName, outputService.ControlId));
                }

                return outputService.ControlId;
            }

            List<ControlsMapping> httpServices = new();
            string addHttpService(string serviceName)
            {
                var httpService = httpServices.SingleOrDefault(x => x.Key == serviceName);
                if (httpService == null)
                {
                    httpService = new ControlsMapping()
                    {
                        Key = serviceName,
                        ControlId = "httpService_" + httpServices.Count
                    };
                    httpServices.Add(httpService);
                    b.Add(httpArea, b.ServiceLink(serviceName, httpService.ControlId));
                }

                return httpService.ControlId;
            }

            int redisIndex = 0;

            string getRedisControlId(string key)
            {
                redisIndex++;
                return "redis_" + redisIndex;
            }

            foreach (var inputChannel in currentService.InputChannels)
            {
                var controlId = getRedisControlId(inputChannel);
                b.Add(inputRedisArea, b.RedisChannel(inputChannel, controlId));
                b.AddConnector(controlId, serviceNameControlId);

                foreach (var inputService in summary.GetNotificationServices(inputChannel))
                {
                    var serviceControlId = addInputService(inputService.ServiceName);
                    b.AddConnector(serviceControlId, controlId);
                }
            }


            foreach (var inputQueue in currentService.InputQueues)
            {
                var controlId = getRedisControlId(inputQueue);
                b.Add(inputRedisArea, b.RedisQueue(inputQueue, controlId));
                b.AddConnector(controlId, serviceNameControlId);

                foreach (var inputService in summary.GetWriterServices(inputQueue))
                {
                    var serviceControlId = addInputService(inputService.ServiceName);
                    b.AddConnector(serviceControlId, controlId);
                }
            }

            foreach (var outputChannel in currentService.OutputChannels)
            {
                var controlId = getRedisControlId(outputChannel);
                b.Add(outputRedisArea, b.RedisChannel(outputChannel, controlId));
                b.AddConnector(serviceNameControlId, controlId);

                foreach (var listenerService in summary.GetListenerServices(outputChannel))
                {
                    var serviceControlId = addOutputService(listenerService.ServiceName);
                    b.AddConnector(controlId, serviceControlId);
                }
            }

            foreach (var outputQueue in currentService.OutputQueues)
            {
                var controlId = getRedisControlId(outputQueue);
                b.Add(outputRedisArea, b.RedisQueue(outputQueue, controlId));
                b.AddConnector(serviceNameControlId, controlId);

                foreach (var notificationService in summary.GetReaderServices(outputQueue))
                {
                    var serviceControlId = addOutputService(notificationService.ServiceName);
                    b.AddConnector(controlId, serviceControlId);
                }
            }

            foreach (var accessedUrl in currentService.AccessedUrls)
            {
                redisIndex++;
                var controlId = "server_" + redisIndex;

                var serverService = GetServer(summary, accessedUrl);
                if (string.IsNullOrEmpty(serverService))
                {
                    // external
                    b.Add(httpArea, b.ExternalApiUrl(accessedUrl, controlId));
                }
                else
                {
                    b.Add(httpArea, b.ApiUrl(serverService, accessedUrl, controlId));
                }

                b.AddConnector(serviceNameControlId, controlId, "top", "bottom");
            }

            foreach (var port in currentService.ListeningPorts)
            {
                var controlId = "port_" + port;
                var portControl = b.Add(portsArea, b.Text(port.ToString()));
                b.SetAttr(portControl, Html.id, controlId);

                foreach (var service in summary.GetClientServices(currentService.MachineIp, port))
                {
                    var httpServiceId = addHttpService(service.ServiceName);
                    b.AddConnector(httpServiceId, controlId, "bottom", "top");
                }
            }

            var dbRow = b.Add(serviceMapRoot, b.Div("flex flex-row items-center"));
            var dbLabel = b.Add(dbRow, b.Text("DB"));
            b.AddClass(dbLabel, "text-lg font-semibold text-gray-300 vertical-text");
            var dbArea = b.Add(dbRow, b.Div("w-full flex flex-row items-center justify-center"));



            foreach (var dbConnection in currentService.DbConnections)
            {
                redisIndex++;
                var controlId = "dbConnection_" + redisIndex;

                var dbConn = MdsCommon.Parameter.ParseConnectionString(dbConnection);

                string dbServer = dbConn.Server;
                string dbCatalog = dbConn.Db;

                //var segments = dbConnection.Split(";", StringSplitOptions.RemoveEmptyEntries);

                //foreach (var segment in segments)
                //{
                //    if (dbServerKeys.Any(x => segment.ToLower().Contains(x)))
                //    {
                //        var keyValue = segment.Split("=", StringSplitOptions.RemoveEmptyEntries);

                //        if (keyValue.Count() == 2)
                //        {
                //            dbServer = keyValue.Last();
                //        }
                //    }
                //    else
                //    {
                //        if (dbCatalogKeys.Any(x => segment.ToLower().Contains(x)))
                //        {
                //            var keyValue = segment.Split("=", StringSplitOptions.RemoveEmptyEntries);

                //            if (keyValue.Count() == 2)
                //            {
                //                dbCatalog = keyValue.Last();
                //            }
                //        }
                //    };
                //}

                if (!string.IsNullOrEmpty(dbServer) && !string.IsNullOrEmpty(dbCatalog))
                {
                    var dbControl = b.Add(dbArea, b.Div("flex flex-col px-8"));
                    b.Add(dbControl, b.Text(dbServer));
                    b.Add(dbControl, b.Text(dbCatalog));
                    b.SetAttr(dbControl, Html.id, controlId);
                    b.AddConnector(serviceNameControlId, controlId, "bottom", "top");
                }
                else
                {
                    var dbControl = b.Add(dbArea, b.Text(dbConnection));
                    b.SetAttr(dbControl, Html.id, controlId);
                    b.AddConnector(serviceNameControlId, controlId, "bottom", "top");
                }
            }

            return serviceMapRoot;
        }

        public static Var<HyperNode> RenderRedisMapCard(this BlockBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            var container = b.Div("h-96 w-full rounded shadow bg-white");
            b.Add(container, b.RenderRedisMap(summary, currentService));
            return container;
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

        public static List<ServiceSummary> GetWriterServices(this InfrastructureSummary summary, string queueName)
        {
            return summary.ServiceReferences.Where(x => x.OutputQueues.Contains(queueName)).ToList();
        }

        public static List<ServiceSummary> GetReaderServices(this InfrastructureSummary summary, string queueName)
        {
            return summary.ServiceReferences.Where(x => x.InputQueues.Contains(queueName)).ToList();
        }

        public static List<ServiceSummary> GetNotificationServices(this InfrastructureSummary summary, string channelName)
        {
            return summary.ServiceReferences.Where(x => x.OutputChannels.Contains(channelName)).ToList();
        }

        public static List<ServiceSummary> GetListenerServices(this InfrastructureSummary summary, string channelName)
        {
            return summary.ServiceReferences.Where(x => x.InputChannels.Contains(channelName)).ToList();
        }

        public static List<ServiceSummary> GetClientServices(this InfrastructureSummary summary, string machineIp, int port)
        {
            List<ServiceSummary> clientServices = new();

            foreach (var service in summary.ServiceReferences)
            {
                foreach (string url in service.AccessedUrls)
                {
                    var urlData = ParseUrl(url);
                    if(urlData.Machine == machineIp && urlData.Port == port)
                    {
                        clientServices.Add(service);
                    }
                }
            }

            return clientServices;
        }

        public static string GetServer(this InfrastructureSummary summary, string url)
        {
            var urlData = ParseUrl(url);

            foreach (var service in summary.ServiceReferences)
            {
                if (service.MachineIp == urlData.Machine)
                {
                    if (service.ListeningPorts.Contains(urlData.Port))
                        return service.ServiceName;
                }
            }

            return String.Empty;
        }

        public class UrlData
        {
            public string Machine { get; set; }
            public int Port { get; set; } = 80;
        }

        public static UrlData ParseUrl(string url)
        {
            UrlData urlData = new UrlData();

            if (url.Replace("http://", string.Empty).Replace("https://", string.Empty).Contains(":"))
            {
                urlData.Port = Int32.Parse(url.Split(':').Last().Split('/', '?').First());
            }

            urlData.Machine = url.Replace("http://", string.Empty).Split(new char[] { ':', '/' }).First();

            return urlData;
        }
    }

    public static class connect
    {
        public static List<object> getConnectors()
        {
            return new List<object>();
        }
    }
}

