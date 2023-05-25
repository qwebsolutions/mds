using Metapsi;
using Metapsi.Hyperapp;
using System.Threading.Tasks;
using MdsCommon;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{




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

    public static partial class Docs
    {
        public static async Task<IResponse> RedisMap(CommandContext commandContext, HttpContext requestData)
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
            var serviceName = requestData.EntityId();
            var serviceSummary = summary.ServiceReferences.Single(x => x.ServiceName == serviceName);

            return Page.Response(
                new object(), (b, clientModel) => b.Layout(b.InfraMenu(nameof(Projects), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Docs", Entity = serviceName },
                    User = requestData.User()
                })), b.RenderRedisMap(summary, serviceSummary)));
        }

        public static Var<HyperNode> RenderRedisMap(this BlockBuilder b, InfrastructureSummary summary, ServiceSummary serviceSummary)
        {
            b.AddScript("cytoscape.min.js");
            b.AddSubscription<object>(
                "initCy",
                (BlockBuilder b, Var<object> state) =>
                b.Listen<object, object>(
                    b.Const("afterRender"),
                    b.MakeAction((BlockBuilder b, Var<object> state, Var<object> _noPayload) =>
                    {
                        b.CallExternal("cy", "updateCy");
                        return state;
                    })));

            var isMaximized = b.CallExternal<bool>("cy", "getMaximized");

            var container = b.Div("");

            b.If(isMaximized, b =>
            {
                b.Log("render maximized");
                b.AddClass(container, "fixed w-screen h-screen top-0 left-0 bg-white z-50");
            },
            b =>
            {
                b.AddClass(container, "h-full w-full relative bg-white z-10");
            });

            var microBar = b.Add(container, b.Div("absolute right-1 top-1 z-30 flex flex-row space-x-1 items-stretch opacity-50 hover:opacity-100 transition text-sky-600"));

            var onPlus = b.MakeAction((BlockBuilder b, Var<InfrastructureSummary> state) =>
            {
                b.Log("+");
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


            var onMinus = b.MakeAction((BlockBuilder b, Var<InfrastructureSummary> state) =>
            {
                b.Log("-");
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
                var onMinimize = b.MakeAction((BlockBuilder b, Var<InfrastructureSummary> state) =>
                {
                    b.Log("minimize");
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
                var onMaximize = b.MakeAction((BlockBuilder b, Var<InfrastructureSummary> state) =>
                {
                    b.Log("maximize");
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
                        foreach (var outputService in summary.GetListenerServices(outputChannel).OrderBy(x=>x.ServiceName))
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
}
