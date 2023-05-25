using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static class Overview
    {
        public partial class OverviewPage
        {
            public LocalSettings LocalSettings { get; set; } = new();
            public FullLocalStatus FullLocalStatus { get; set; } = new();
            public RecordCollection<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; } = new RecordCollection<MdsLocal.RunningServiceProcess>();
            public List<string> Warnings { get; set; } = new List<string>();
        }

        public static async Task<OverviewPage> Load(CommandContext commandContext)
        {
            return new OverviewPage()
            {
                LocalSettings = await commandContext.Do(MdsLocalApplication.GetLocalSettings),
                Warnings = await commandContext.Do(MdsLocalApplication.GetWarnings),
                FullLocalStatus = await commandContext.Do(MdsLocalApplication.GetFullLocalStatus),
                ServiceProcesses = await commandContext.Do(MdsLocalApplication.GetRunningProcesses)
            };
        }

        public static async Task<IResponse> ListProcesses(CommandContext commandContext, HttpContext requestData)
        {
            var dataModel = await Load(commandContext);
            return Page.Response(dataModel, (b, clientModel) => b.Layout(b.LocalMenu(nameof(Overview)), b.Render(b.Const(new Header.Props()
            {
                Main = new Header.Title() { Entity = "Processes" },
                User = requestData.User()
            })), b.Render(dataModel)));
        }

        public class ProcessRow
        {
            public string ServiceName { get; set; }
            public string ProjectName { get; set; }
            public string ProjectVersionTag { get; set; }
            public string Pid { get; set; }
            public string UsedRamMB { get; set; }
            public string RunningStatus { get; set; }
            public bool HasError { get; set; } = false;
        }

        public static Var<HyperNode> Render(this BlockBuilder b, OverviewPage dataModel)
        {
            List<ProcessRow> rows = new List<ProcessRow>();
            foreach(var serviceSnapshot in dataModel.FullLocalStatus.LocalServiceSnapshots)
            {
                var serviceProcess = dataModel.ServiceProcesses.SingleOrDefault(x => x.ServiceName == serviceSnapshot.ServiceName);

                var pid = "Not running";
                var runningStatus = "Not running";
                string usedRamMb = "0";

                if (serviceProcess != null)
                {
                    TimeSpan running = DateTime.UtcNow - serviceProcess.StartTimestampUtc;
                    TimeSpan rounded = TimeSpan.FromSeconds((int)running.TotalSeconds);
                    runningStatus = $"{serviceProcess.StartTimestampUtc} ({rounded.ToString("c")})";

                    pid = serviceProcess.Pid.ToString();
                    usedRamMb = serviceProcess.UsedRamMB.ToString();
                }

                rows.Add(new ProcessRow()
                {
                    ServiceName = serviceSnapshot.ServiceName,
                    ProjectName = serviceSnapshot.ProjectName,
                    ProjectVersionTag = serviceSnapshot.ProjectVersionTag,
                    RunningStatus = runningStatus,
                    Pid = pid,
                    UsedRamMB = usedRamMb,
                    HasError = serviceProcess == null
                });
            }

            var clientRows = b.Const(rows.ToList());

            var view = b.Div("flex flex-col space-y-4");

            if (dataModel.Warnings.Any())
            {
                foreach (string warning in dataModel.Warnings)
                {
                    var warningContainer = b.Add(view, b.Div("border border-solid border-red-300 bg-red-100 text-red-300 rounded w-full p-2 mb-4 drop-shadow transition-all"));
                    b.Add(warningContainer, b.Text(warning));
                }
            }

            LocalSettings localSettings = dataModel.LocalSettings;
            string title = $"Node: {localSettings.NodeName}, OS: {System.Environment.OSVersion}, infrastructure API: {localSettings.InfrastructureApiUrl}";

            string overviewText = $"Total services: 0";

            var lastChange = dataModel.FullLocalStatus.SyncResults.Where(x => x.ResultCode == SyncStatusCodes.Changed).OrderByDescending(x => x.Timestamp).FirstOrDefault();

            if (lastChange != null)
            {
                overviewText = $"Local services: {dataModel.FullLocalStatus.LocalServiceSnapshots.Count()}, last change: {lastChange.Timestamp.ToString("G")}";
            }

            b.Add(view, b.InfoPanel(Panel.Style.Info, title, overviewText));

            var rc = b.Def((BlockBuilder b, Var<ProcessRow> serviceSnapshot, Var<DataTable.Column> col) =>
            {
                Var<string> serviceName = b.Get(serviceSnapshot, x => x.ServiceName);
                Var<string> columnName = b.Get(col, x => x.Name);

                return b.VPadded4(b.Text(b.GetProperty<string>(serviceSnapshot, columnName)));
            });

            if (dataModel.FullLocalStatus.LocalServiceSnapshots.Any())
            {
                var props = b.NewObj<DataTable.Props<ProcessRow>>(b =>
                {
                    b.AddColumn(nameof(ProcessRow.ServiceName), "Service name");
                    b.AddColumn(nameof(ProcessRow.ProjectName), "Project name");
                    b.AddColumn(nameof(ProcessRow.ProjectVersionTag), "Project version");
                    b.AddColumn(nameof(ProcessRow.Pid), "Process ID");
                    b.AddColumn(nameof(ProcessRow.UsedRamMB), "Working set (RAM, MB)");
                    b.AddColumn(nameof(ProcessRow.RunningStatus), "Running since");
                    b.SetRows(clientRows);
                    b.SetRenderCell(rc);
                });

                b.Set(props, x => x.CreateRow, b.Def((BlockBuilder b, Var<ProcessRow> row) =>
                {
                    Var<ProcessRow> processRow = row.As<ProcessRow>();
                    return b.If(b.Get(processRow, x => x.HasError), b => b.Node("tr", "bg-red-500"), b => b.Node("tr"));
                }));

                b.Add(view, b.DataTable(props));
            }

            return view;
        }
    }
}

