using Metapsi.Hyperapp;
using Metapsi.Syntax;
using M = MdsInfrastructure.Docs;
using MdsCommon;
using Metapsi.Ui;
using System.Collections.Generic;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;
using System.ComponentModel;
using System;

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

                b.AddScript(typeof(Docs).Assembly, "cy.js", "module");
                b.AddScript(typeof(Docs).Assembly, "cytoscape.min.js");
                b.AddScript(typeof(Docs).Assembly, "connect.js", "module");
                b.AddScript(typeof(Docs).Assembly, "leader-line.min.js");

                var headerProps = b.GetHeaderProps(
                    b.Const("Docs"),
                    b.Const(serverModel.ServiceSummary.ServiceName),
                    b.Get(clientModel, x => x.User));

                return b.Layout(
                    b.InfraMenu(
                        nameof(Routes.Status),
                        serverModel.User.IsSignedIn()),
                    b.Render(headerProps),
                    b.Render(serverModel.InfrastructureSummary, serverModel.ServiceSummary)).As<IVNode>();
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

        public static Var<IVNode> ServiceLink(this LayoutBuilder b,
            string serviceName,
            string controlId)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("py-8 px-4 font-semibold");
                },
                b.HtmlA(
                    b =>
                    {
                        b.SetId(controlId);
                        b.SetClass("underline text-sky-500");
                        b.SetHref(b.Url<Routes.Docs.Service, string>(b.Const(serviceName)));
                    },
                    b.TextSpan(serviceName)));
        }


        public static Var<IVNode> ExternalApiUrl(this LayoutBuilder b,
            string url,
            string controlId)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetId(controlId);
                    b.SetClass("flex flex-col px-4");
                },
                b.TextSpan(url));
        }


        public static Var<IVNode> ApiUrl(this LayoutBuilder b,
            string serviceName,
            string url,
            string controlId)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetId(controlId);
                    b.SetClass("flex flex-col px-4");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("font-semibold");
                    },
                    b.HtmlA(
                        b =>
                        {
                            b.SetHref(b.Url<Routes.Docs.Service, string>(b.Const(serviceName)));
                            b.UnderlineBlue();
                        },
                        b.TextSpan(serviceName))),
                b.TextSpan(url));
        }

        public static Var<IVNode> RedisChannel(this LayoutBuilder b,
            string channelName,
            string controlId)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetId(controlId);
                    b.SetClass("rounded shadow p-2 relative");
                },
                b.TextSpan(channelName));
        }

        public static Var<IVNode> RedisQueue(this LayoutBuilder b,
            string channelName,
            string controlId)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetId(controlId);
                    b.SetClass("rounded shadow p-2 relative");
                },
                b.TextSpan(channelName));
        }

        public static Var<IVNode> Render(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col space-y-4 text-gray-800");
                },
                b.RenderServiceCard(summary, currentService),
                b.RenderServiceMap(summary, currentService),
                b.RenderRedisMapCard(summary, currentService));
        }

        public static Var<IVNode> RenderServiceCard(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            List<Var<IVNode>> leftNodes = new();

            var summaryNote = currentService.ServiceDescription;
            if (!string.IsNullOrEmpty(summaryNote))
            {
                leftNodes.Add(b.StyledSpan("font-thin text-gray-500", b.TextSpan(summaryNote)));
            }

            leftNodes.Add(b.Bold(currentService.ServiceName));

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

                leftNodes.Add(
                    b.StyledDiv(
                        "flex flex-col pt-2 text-gray-700 text-sm",
                        b.StyledDiv(
                            "flex flex-row space-x-2",
                            b.StyledSpan(
                                "font-semibold",
                                b.TextSpan(parameter.ParameterName)),
                            b.StyledSpan("", b.TextSpan(parameter.DeployedValue))),
                        b.StyledSpan(
                            "text-gray-500",
                            !string.IsNullOrEmpty(parameter.ParameterComment)
                            ? b.TextSpan(parameter.ParameterComment)
                            : b.TextSpan(parameter.ParameterTypeDescription))));
            }

            var right = b.StyledDiv(
                "flex flex-col items-start space-y-2 bg-gray-50 rounded flex-initial px-8 py-4 text-sm text-gray-500",
                b.StyledDiv(
                    "flex flex-row space-x-2 items-center font-semibold",
                    b.Svg(Icon.Computer, "w-3 h-3 text-gray-400"),
                    b.StyledSpan("", b.TextSpan(currentService.NodeName))),
                b.StyledDiv(
                    "flex flex-row space-x-2 items-center font-semibold",
                    b.Svg(Icon.DocumentText, "w-3 h-3 text-gray-400"),
                    b.StyledSpan("", b.TextSpan(currentService.ProjectLabel()))));

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("w-full rounded flex flex-row p-4 space-x-8 text-gray-700 shadow bg-white");
                },
                b.StyledDiv(
                    "flex flex-col flex-1", 
                    leftNodes.ToArray()),
                b.HtmlDiv(b => { /*divToIgnoreSize*/}, right));
        }

        public static Var<IVNode> RenderServiceMap(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            List<ControlsMapping> controlsMappings = new List<ControlsMapping>();

            List<Var<IVNode>> httpAreaNodes = new();
            List<Var<IVNode>> inputServicesAreaNodes = new();
            List<Var<IVNode>> inputRedisAreaNodes = new();
            List<Var<IVNode>> portsAreaNodes = new();
            List<Var<IVNode>> outputRedisAreaNodes = new();
            List<Var<IVNode>> outputServicesAreaNodes = new();
            List<Var<IVNode>> dbAreaNodes = new();

            var serviceNameControlId = "ctrl_currentService";

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
                    inputServicesAreaNodes.Add(b.ServiceLink(serviceName, inputService.ControlId));
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
                    outputServicesAreaNodes.Add(b.ServiceLink(serviceName, outputService.ControlId));
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
                    httpAreaNodes.Add(b.ServiceLink(serviceName, httpService.ControlId));
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
                inputRedisAreaNodes.Add(b.RedisChannel(inputChannel, controlId));
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
                inputRedisAreaNodes.Add(b.RedisQueue(inputQueue, controlId));
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
                outputRedisAreaNodes.Add(b.RedisChannel(outputChannel, controlId));
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
                outputRedisAreaNodes.Add(b.RedisQueue(outputQueue, controlId));
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
                    httpAreaNodes.Add(b.ExternalApiUrl(accessedUrl, controlId));
                }
                else
                {
                    httpAreaNodes.Add(b.ApiUrl(serverService, accessedUrl, controlId));
                }

                b.AddConnector(serviceNameControlId, controlId, "top", "bottom");
            }

            foreach (var port in currentService.ListeningPorts)
            {
                var controlId = "port_" + port;
                portsAreaNodes.Add(
                    b.HtmlSpan(b =>
                    {
                        b.SetId(controlId);
                    }, b.Text(port.ToString())));

                foreach (var service in summary.GetClientServices(currentService.MachineIp, port))
                {
                    var httpServiceId = addHttpService(service.ServiceName);
                    b.AddConnector(httpServiceId, controlId, "bottom", "top");
                }
            }


            foreach (var dbConnection in currentService.DbConnections)
            {
                redisIndex++;
                var controlId = "dbConnection_" + redisIndex;

                var dbConn = MdsCommon.Parameter.ParseConnectionString(dbConnection);

                string dbServer = dbConn.Server;
                string dbCatalog = dbConn.Db;

                if (!string.IsNullOrEmpty(dbServer) && !string.IsNullOrEmpty(dbCatalog))
                {
                    dbAreaNodes.Add(
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetId(controlId);
                                b.SetClass("flex flex-col px-8");
                            },
                            b.TextSpan(dbServer),
                            b.TextSpan(dbCatalog)));

                    b.AddConnector(serviceNameControlId, controlId, "bottom", "top");
                }
                else
                {
                    dbAreaNodes.Add(
                        b.HtmlSpan(b =>
                        {
                            b.SetId(controlId);
                        },
                        b.TextSpan(dbConnection)));

                    b.AddConnector(serviceNameControlId, controlId, "bottom", "top");
                }
            }

            var serviceMapRoot = b.StyledDiv(
                "h-full w-full p-2 text-sm flex flex-col space-y-8 rounded shadow bg-white",
                b.StyledDiv( // httpRowContainer
                    "flex flex-row items-center",
                    b.StyledSpan("text-lg font-semibold text-gray-300 vertical-text", b.TextSpan("HTTP")),
                    b.StyledDiv(// httpServicesContainer
                        "flex w-full justify-center",
                        b.StyledDiv( // httpArea
                            "flex flex-row items-center",
                            httpAreaNodes.ToArray()))),
                b.StyledDiv( // redisRow
                    "flex flex-row items-center",
                    b.StyledSpan("text-lg font-semibold text-gray-300 vertical-text", b.TextSpan("REDIS")), // redisLabel
                    b.HtmlDiv(
                        b=>
                        {
                            //redisInputArea
                            b.SetId("inputServicesArea");
                            b.SetClass("flex-1 flex flex-row");
                        },
                        b.HtmlDiv(b => { }, inputServicesAreaNodes.ToArray()),
                        b.StyledDiv(
                            "w-full flex items-center justify-end",
                            b.HtmlDiv(b =>
                            {
                                b.SetId("inputRedisArea");
                            },
                            inputRedisAreaNodes.ToArray()))
                        ),
                    b.StyledDiv( // serviceNameDiv
                        "flex-none p-16 relative",
                        b.StyledDiv( // portsArea
                            "flex flex-row justify-center absolute -translate-y-5 translate-x-12 space-x-2 font-thin text-xs",
                            portsAreaNodes.ToArray()),
                        b.HtmlSpan(
                            b =>
                            {
                                b.SetId(serviceNameControlId);
                            },
                            b.Bold(currentService.ServiceName))),
                    b.StyledDiv( // redisOutputArea
                        "flex-1 flex flex-row",
                        b.StyledDiv(
                            "w-full flex items-center justify-start",
                            b.HtmlDiv(b => { }, outputRedisAreaNodes.ToArray())),
                        b.HtmlDiv(b => { }, outputServicesAreaNodes.ToArray()))),
                b.StyledDiv( // dbRow
                    "flex flex-row items-center",
                    b.StyledSpan( // dbLabel
                        "text-lg font-semibold text-gray-300 vertical-text",
                        b.TextSpan("DB")),
                    b.StyledDiv( // dbArea
                        "w-full flex flex-row items-center justify-center",
                        dbAreaNodes.ToArray())));

            return serviceMapRoot;
        }

        public static Var<IVNode> RenderRedisMapCard(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary currentService)
        {
            return b.StyledDiv(
                "h-96 w-full rounded shadow bg-white",
                b.RenderRedisMap(summary, currentService));
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

        private static Var<IVNode> IconCommandButton(
            this LayoutBuilder b,
            string icon,
            Action<PropsBuilder<HtmlButton>> setProps)
        {
            var button = b.HtmlButton(
                b =>
                {
                    b.AddClass("rounded p-1 shadow");
                    setProps(b);
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row space-x-2 items-center");
                    },
                    b.Svg(icon, "h-5 w-5")));

            return button;
        }

        public static Var<IVNode> RenderRedisMap(this LayoutBuilder b, InfrastructureSummary summary, ServiceSummary serviceSummary)
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

            var onPlus = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
            {
                b.CallExternal("cy", "plusZoom");
                return b.Clone(state);
            });


            var onMinus = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
            {
                b.CallExternal("cy", "minusZoom");
                return b.Clone(state);
            });

            var container =
                b.HtmlDiv(
                    b =>
                    {
                        b.If(isMaximized, b =>
                        {
                            b.AddClass("fixed w-screen h-screen top-0 left-0 bg-white z-50");
                        },
                        b =>
                        {
                            b.AddClass("h-full w-full relative bg-white z-10");
                        });
                    },
                    b.HtmlDiv( // microBar
                        b =>
                        {
                            b.SetClass("absolute right-1 top-1 z-30 flex flex-row space-x-1 items-stretch opacity-50 hover:opacity-100 transition text-sky-600");
                        },
                        b.IconCommandButton(
                            Icon.Plus,
                            b =>
                            {
                                b.AddClass("bg-white");
                                b.OnClickAction(onPlus);
                            }),
                        b.IconCommandButton(
                            Icon.Minus,
                            b =>
                            {
                                b.AddClass("bg-white");
                                b.OnClickAction(onMinus);
                            }),
                        b.If(
                            isMaximized,
                            b =>
                            {
                                var onMinimize = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
                                {
                                    b.CallExternal("cy", "minimize");
                                    return b.Clone(state);
                                });

                                return b.IconCommandButton(Icon.Minimize,
                                    b =>
                                    {
                                        b.AddClass("bg-white");
                                        b.OnClickAction(onMinimize);
                                    });
                            },
                            b =>
                            {
                                var onMaximize = b.MakeAction((SyntaxBuilder b, Var<InfrastructureSummary> state) =>
                                {
                                    b.CallExternal("cy", "maximize");
                                    return b.Clone(state);
                                });

                                return b.IconCommandButton(
                                    Icon.Maximize,
                                    b =>
                                    {
                                        b.AddClass("bg-white");
                                        b.OnClickAction(onMaximize);
                                    });
                            })),
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetId("cy");
                            b.SetClass("h-full w-full absolute");
                        }));

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
