using Metapsi.Hyperapp;
using Metapsi.Syntax;
using M = MdsInfrastructure.Docs;
using MdsCommon;
using Metapsi.Ui;
using System.Collections.Generic;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static class Docs
    {
        public class Service : MixedHyperPage<M.ServicePage, M.ServicePage>
        {
            public override M.ServicePage ExtractClientModel(M.ServicePage serverModel)
            {
                return serverModel;
            }

            public override Var<IVNode> OnRender(LayoutBuilder b, M.ServicePage serverModel, Var<M.ServicePage> clientModel)
            {
                b.AddModuleStylesheet();

                return b.Layout(b.InfraMenu(nameof(Routes.Status), serverModel.User.IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Docs", Entity = serverModel.ServiceSummary.ServiceName },
                    User = serverModel.User
                })), b.Render(serverModel.InfrastructureSummary, serverModel.ServiceSummary)).As<IVNode>();
            }
        }

        public class RedisMap : MixedHyperPage<M.RedisMap, M.RedisMap>
        {
            public override M.RedisMap ExtractClientModel(M.RedisMap serverModel)
            {
                return serverModel;
            }

            public override Var<IVNode> OnRender(LayoutBuilder b, M.RedisMap serverModel, Var<M.RedisMap> clientModel)
            {
                b.AddModuleStylesheet();

                return b.Layout(
                    b.InfraMenu(nameof(Routes.Project),
                    serverModel.User.IsSignedIn()),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Docs", Entity = serverModel.ServiceSummary.ServiceName },
                        User = serverModel.User
                    })), b.RenderRedisMap(serverModel.InfrastructureSummary, serverModel.ServiceSummary)).As<IVNode>();
            }
        }

        public class Connection
        {
            public string FromId { get; set; }
            public string ToId { get; set; }
            public string FromSide { get; set; } = "right";
            public string ToSide { get; set; } = "left";
        }

        public static void AddConnector(this LayoutBuilder b,
            string fromId,
            string toId,
            string fromSide = "right",
            string toSide = "left")
        {
            b.AddScript("leader-line.min.js");

            b.AddSubscription(
                "leaderLineSub",
                (SyntaxBuilder b, Var<object> state) =>
                b.Listen(b.Const("afterRender"), b.MakeAction((SyntaxBuilder b, Var<object> state, Var<object> _payload) =>
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

        public static Var<HyperNode> ServiceLink(this LayoutBuilder b,
            string serviceName,
            string controlId)
        {
            var container = b.Div("py-8 px-4 font-semibold");
            var control = b.Add(container, b.Link(
                b.Url<Routes.Docs.Service, string>(b.Const(serviceName)),
                b.TextNode(b.Const(serviceName))));
            b.SetAttr(control, Html.id, controlId);
            return container;
        }


        public static Var<HyperNode> ExternalApiUrl(this LayoutBuilder b,
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


        public static Var<HyperNode> ApiUrl(this LayoutBuilder b,
            string serviceName,
            string url,
            string controlId)
        {
            var container = b.Div("flex flex-col px-4");
            //var icon = b.Add(container, b.Div("text-yellow-300"));
            //b.SetInnerHtml(icon, b.FromFile("inline/antenna.svg"));
            var semiBold = b.Add(container, b.Div("font-semibold"));
            b.Add(semiBold, b.Link(
                b.Url<Routes.Docs.Service, string>(b.Const(serviceName)),
                b.TextNode(b.Const(serviceName))));
            b.Add(container, b.Text(url));
            b.SetAttr(container, Html.id, controlId);
            return container;
        }

        public static Var<HyperNode> RedisChannel(this LayoutBuilder b,
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

        public static Var<HyperNode> RedisQueue(this LayoutBuilder b,
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
        public static Var<HyperNode> Render(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            var view = b.Div("flex flex-col space-y-4 text-gray-800");
            b.Add(view, b.RenderServiceCard(summary, currentService));
            b.Add(view, b.RenderServiceMap(summary, currentService));
            b.Add(view, b.RenderRedisMapCard(summary, currentService));
            return view;
        }

        public static Var<HyperNode> RenderServiceCard(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
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
                if (parameter.ParameterType.ToLower() == "dbconnectionstring")
                {
                    parameter.DeployedValue = MdsCommon.Parameter.ParseConnectionString(parameter.DeployedValue).ToString();
                }

                if (parameter.ParameterName.ToLower().Contains("password"))
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

        public static Var<HyperNode> RenderServiceMap(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
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

                if (inputService == null)
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

                var serverService = summary.GetServer(accessedUrl);
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

        public static Var<HyperNode> RenderRedisMapCard(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            var container = b.Div("h-96 w-full rounded shadow bg-white");
            b.Add(container, b.RenderRedisMap(summary, currentService));
            return container;
        }



        public class Graph
        {
            public List<Node> nodes { get; set; } = new List<Node>();
            public List<Edge> edges { get; set; } = new List<Edge>();
        }

        public class NodeData
        {
            public string id { get; set; }
            public int FontWeight { get; set; } = 600;
            public string NodeColor { get; set; } = "#666";
        }

        public class Position
        {
            public int x { get; set; }
            public int y { get; set; }
        }

        public class Node
        {
            public NodeData data { get; set; }
            public Position position { get; set; }
        }

        public class EdgeData
        {
            public string source { get; set; }
            public string target { get; set; }
        }

        public class Edge
        {
            public EdgeData data { get; set; }
        }


        public static Var<HyperNode> RenderRedisMap(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary serviceSummary)
        {
            b.AddScript("cytoscape.min.js");
            b.AddSubscription<object>(
                "initCy",
                (SyntaxBuilder b, Var<object> state) =>
                b.Listen<object, object>(
                    b.Const("afterRender"),
                    b.MakeAction((SyntaxBuilder b, Var<object> state, Var<object> _noPayload) =>
                    {
                        b.CallExternal("cy", "updateCy");
                        return state;
                    })));

            var isMaximized = b.CallExternal<bool>("cy", "getMaximized");

            var container = b.Div("");

            b.If(isMaximized, b =>
            {
                b.AddClass(container, "fixed w-screen h-screen top-0 left-0 bg-white z-50");
            },
            b =>
            {
                b.AddClass(container, "h-full w-full relative bg-white z-10");
            });

            var microBar = b.Add(container, b.Div("absolute right-1 top-1 z-30 flex flex-row space-x-1 items-stretch opacity-50 hover:opacity-100 transition text-sky-600"));

            var onPlus = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
            {
                b.CallExternal("cy", "plusZoom");
                return b.Clone(state);
            });

            var plus = b.NewObj<CommandButton.Props<InfrastructureSummary>>(b =>
            {
                //b.Set(x => x.ColorClass, "bg-white");
                b.Set(x => x.SvgIcon, MdsCommon.Icon.Plus);
                b.Set(x => x.OnClick, onPlus);
            });

            b.AddClass(b.Add(microBar, b.CommandButton(plus)), "bg-white");


            var onMinus = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
            {
                b.CallExternal("cy", "minusZoom");
                return b.Clone(state);
            });

            var minus = b.NewObj<CommandButton.Props<InfrastructureSummary>>(b =>
            {
                //b.Set(x => x.ColorClass, "bg-white");
                b.Set(x => x.SvgIcon, MdsCommon.Icon.Minus);
                b.Set(x => x.OnClick, onMinus);
            });

            b.AddClass(b.Add(microBar, b.CommandButton(minus)), "bg-white");

            b.If(isMaximized, b =>
            {
                var onMinimize = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
                {
                    b.CallExternal("cy", "minimize");
                    return b.Clone(state);
                });

                var minimize = b.NewObj<CommandButton.Props<InfrastructureSummary>>(b =>
                {
                    //b.Set(x => x.ColorClass, "bg-white");
                    b.Set(x => x.SvgIcon, MdsCommon.Icon.Minimize);
                    b.Set(x => x.OnClick, onMinimize);
                });

                b.AddClass(b.Add(microBar, b.CommandButton(minimize)), "bg-white");
            },
            b =>
            {
                var onMaximize = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
                {
                    b.CallExternal("cy", "maximize");
                    return b.Clone(state);
                });

                var maximize = b.NewObj<CommandButton.Props<InfrastructureSummary>>(b =>
                {
                    //b.Set(x => x.ColorClass, "bg-white");
                    b.Set(x => x.SvgIcon, MdsCommon.Icon.Maximize);
                    b.Set(x => x.OnClick, onMaximize);
                });

                b.AddClass(b.Add(microBar, b.CommandButton(maximize)), "bg-white");
            });

            var cy = b.Add(container, b.Div("h-full w-full absolute"));
            b.SetAttr(cy, Html.id, "cy");

            //b.ModuleBuilder.AddImport("import {setGraph, plusZoom, minusZoom, maximize, minimize, getMaximized } from '/cy.js'");

            var graph = RedisGraph(summary, serviceSummary);

            b.CallExternal("cy", "setGraph", b.Const(graph));

            return container;
        }

        public class ServiceColumn
        {
            public List<string> Services { get; set; } = new List<string>();
        }

        public static List<ServiceSummary> GetDownstreamServices(InfrastructureSummary summary, ServiceSummary currentService)
        {
            List<ServiceSummary> outputServices = new();
            foreach (var outputQueue in currentService.OutputQueues)
            {
                outputServices.AddRange(summary.GetReaderServices(outputQueue));
            }

            foreach (var outputChannel in currentService.OutputChannels)
            {
                outputServices.AddRange(summary.GetListenerServices(outputChannel));
            }

            return outputServices.Distinct().ToList();
        }

        public static List<ServiceSummary> GetUpstreamServices(InfrastructureSummary summary, ServiceSummary currentService)
        {
            List<ServiceSummary> inputServices = new();
            foreach (var inputQueue in currentService.InputQueues)
            {
                inputServices.AddRange(summary.GetWriterServices(inputQueue));
            }

            foreach (var inputChannel in currentService.InputChannels)
            {
                inputServices.AddRange(summary.GetNotificationServices(inputChannel));
            }

            return inputServices.Distinct().ToList();
        }

        public static bool AddEdge(this Graph graph, string from, string to)
        {
            if (!graph.edges.Any(x => x.data.source == from && x.data.target == to))
            {
                graph.edges.Add(new Edge()
                {
                    data = new EdgeData()
                    {
                        source = from,
                        target = to
                    }
                });
                return true;
            }
            return false;
        }


        const int hSpace = 700;
        const int vSpace = 100;

        public static bool AddNode(this Graph graph, string name, int column, int row, bool selected = false)
        {
            int xOffset = new System.Random().Next(10, 50);
            int x = column * hSpace;
            int yOffset = new System.Random().Next(20);

            if (column % 2 == 0)
                yOffset = yOffset * -1;

            int y = row * vSpace + yOffset;

            if (!graph.nodes.Any(x => x.data.id == name))
            {
                graph.nodes.Add(new Node()
                {
                    data = new NodeData()
                    {
                        id = name,
                        FontWeight = selected ? 800 : 500,
                        NodeColor = selected ? "#0ea5e9" : "#666"
                    },
                    position = new Position()
                    {
                        x = x + xOffset,
                        y = y + yOffset
                    }
                });
                return true;
            }
            return false;
        }

        public static string ProjectLabel(this ServiceSummary service)
        {
            return $"{service.Project} {service.Version}";
        }

        public static Graph RedisGraph(InfrastructureSummary summary, ServiceSummary service)
        {
            Graph graph = new Graph();

            graph.AddNode(service.ServiceName, 0, 0, true);

            List<ServiceSummary> alreadyAdded = new List<ServiceSummary>();
            alreadyAdded.Add(service);

            List<ServiceSummary> currentServicesColumn = new List<ServiceSummary>();
            currentServicesColumn.Add(service);

            int column = 0;

            // Add input
            while (true)
            {
                int row = 0;
                column -= 1;
                List<ServiceSummary> inputServices = new List<ServiceSummary>();
                foreach (var currentService in currentServicesColumn)
                {
                    foreach (var inputChannel in currentService.InputChannels)
                    {
                        foreach (var inputService in summary.GetNotificationServices(inputChannel).OrderBy(x => x.ServiceName))
                        {
                            if (!alreadyAdded.Contains(inputService))
                            {
                                inputServices.Add(inputService);

                                if (graph.AddNode(inputService.ServiceName, column, row))
                                    row++;
                            }

                            graph.AddEdge(inputService.ServiceName, currentService.ServiceName);
                        }
                    }

                    foreach (var inputQueue in currentService.InputQueues)
                    {
                        foreach (var inputService in summary.GetWriterServices(inputQueue).OrderBy(x => x.ServiceName))
                        {
                            if (!alreadyAdded.Contains(inputService))
                            {
                                inputServices.Add(inputService);

                                if (graph.AddNode(inputService.ServiceName, column, row))
                                    row++;
                            }

                            graph.AddEdge(inputService.ServiceName, currentService.ServiceName);
                        }
                    }
                }


                if (!inputServices.Any())
                    break;

                alreadyAdded.AddRange(inputServices);
                currentServicesColumn = inputServices;
            }

            currentServicesColumn.Clear();
            currentServicesColumn.Add(service);

            column = 0;

            // Add output
            while (true)
            {
                int row = 0;
                column += 1;
                List<ServiceSummary> outputServices = new List<ServiceSummary>();
                foreach (var currentService in currentServicesColumn)
                {
                    foreach (var outputChannel in currentService.OutputChannels)
                    {
                        foreach (var outputService in summary.GetListenerServices(outputChannel).OrderBy(x => x.ServiceName))
                        {
                            if (!alreadyAdded.Contains(outputService))
                            {
                                outputServices.Add(outputService);

                                if (graph.AddNode(outputService.ServiceName, column, row))
                                    row++;
                            }

                            graph.AddEdge(currentService.ServiceName, outputService.ServiceName);
                        }
                    }

                    foreach (var outputQueue in currentService.OutputQueues)
                    {
                        foreach (var outputService in summary.GetReaderServices(outputQueue).OrderBy(x => x.ServiceName))
                        {
                            if (!alreadyAdded.Contains(outputService))
                            {
                                outputServices.Add(outputService);

                                if (graph.AddNode(outputService.ServiceName, column, row))
                                    row++;
                            }

                            graph.AddEdge(currentService.ServiceName, outputService.ServiceName);
                        }
                    }
                }

                if (!outputServices.Any())
                    break;

                alreadyAdded.AddRange(outputServices);
                currentServicesColumn = outputServices;
            }

            return graph;
        }

        public static Graph RedisGraph(InfrastructureSummary summary)
        {
            Graph graph = new Graph();

            //List<List<ServiceSummary>> columns = new List<List<ServiceSummary>>();

            List<string> remainingServiceNames = summary.ServiceReferences.Select(x => x.ServiceName).ToList();

            List<ServiceSummary> currentColumnServices = summary.ServiceReferences.Where(x => GetUpstreamServices(summary, x).Count == 0).ToList();

            List<int> maxIndexes = new List<int>();

            int NextRowIndex(int column)
            {
                while (maxIndexes.Count() <= column)
                {
                    maxIndexes.Add(0);
                }

                maxIndexes[column]++;
                return maxIndexes[column];
            }

            int columnIndex = 2;
            while (remainingServiceNames.Any())
            {
                int serviceRowIndex = 0;

                foreach (var service in currentColumnServices.OrderBy(x => x.ServiceName))
                {
                    if (graph.AddNode(service.ProjectLabel(), columnIndex * 500, serviceRowIndex * 100))
                        serviceRowIndex++;

                    foreach (var inputChannel in service.InputChannels.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x))
                    {
                        foreach (var inputService in summary.GetNotificationServices(inputChannel))
                        {
                            graph.AddNode(inputService.ProjectLabel(), (columnIndex - 1) * 500, NextRowIndex(columnIndex - 1) * 100);
                            graph.AddEdge(inputService.ProjectLabel(), service.ProjectLabel());
                        }
                    }

                    foreach (var inputQueue in service.InputQueues.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x))
                    {
                        foreach (var inputService in summary.GetWriterServices(inputQueue))
                        {
                            graph.AddNode(inputService.ProjectLabel(), (columnIndex - 1) * 500, NextRowIndex(columnIndex - 1) * 100);
                            graph.AddEdge(inputService.ProjectLabel(), service.ProjectLabel());
                        }
                    }

                    foreach (var outputChannel in service.OutputChannels.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x))
                    {
                        foreach (var outputService in summary.GetListenerServices(outputChannel))
                        {
                            graph.AddNode(outputService.ProjectLabel(), (columnIndex + 1) * 500, NextRowIndex(columnIndex + 1) * 100);
                            graph.AddEdge(service.ProjectLabel(), outputService.ProjectLabel());
                        }
                    }

                    foreach (var outputQueue in service.OutputQueues.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x))
                    {
                        foreach (var outputService in summary.GetReaderServices(outputQueue))
                        {
                            graph.AddNode(outputService.ProjectLabel(), (columnIndex + 1) * 500, NextRowIndex(columnIndex + 1) * 100);
                            graph.AddEdge(service.ProjectLabel(), outputService.ProjectLabel());
                        }
                    }
                }

                columnIndex += 2;

                remainingServiceNames.RemoveAll(x => currentColumnServices.Select(x => x.ServiceName).Contains(x));
                currentColumnServices = currentColumnServices.SelectMany(x => GetDownstreamServices(summary, x)).Where(x => remainingServiceNames.Contains(x.ServiceName)).ToList();
            }

            return graph;
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
