using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using Metapsi.Hyperapp;

namespace MdsLocal
{
    public static class RenderInfrastructureEventsList
    {
        public static Var<HyperNode> RenderEventsList(BlockBuilder b, Var<ListInfrastructureEventsPage> clientModel)
        {
            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Infrastructure events" }));
            b.Set(headerProps, x => x.User, b.Get(clientModel, x => x.User));

            return b.Call(
                MdsCommon.Common.Layout,
                b.LocalMenu(nameof(EventsLog)),
                b.Render(headerProps),
                b.RenderListInfrastructureEventsPage(clientModel));
        }
    }

    public static partial class MdsLocalMenu
    {
        public static Var<HyperNode> LocalMenu(
            this BlockBuilder b,
            string selectedCode)
        {
            var menuEntries = new List<Metapsi.Ui.Menu.Entry>() {
                new (){Code = nameof(Overview), Label = "Overview", Href = Route.Path<Overview.ListProcesses>()},
                new (){Code = nameof(SyncHistory), Label = "Sync history", Href = Route.Path<SyncHistory.List>()},
                new (){Code = nameof(EventsLog) , Label = "Events log", Href = Route.Path<EventsLog.List>() }
            };

            return b.Menu(b.Const(new Metapsi.Hyperapp.Menu.Props()
            {
                ActiveCode = selectedCode,
                Entries = menuEntries.ToList()
            }));
        }
    }

    public static partial class ListProcesses
    {
        public static string RenderListProcessesBuilder(OverviewPage serverModel)
        {
            return "render list processes";
        }

        public static Var<HyperNode> RenderListProcesses(BlockBuilder b, Var<OverviewPage> clientModel)
        {
            return b.Text("Render list processes");

            //return b.Layout(
            //    b.LocalMenu(nameof(Overview)), 
            //    b.Render(b.Const(new Header.Props()
            //{
            //    Main = new Header.Title() { Entity = "Processes" },
            //    User = requestData.User()
            //})), b.Render(dataModel))
        }


        //public static Var<HyperNode> Render(this BlockBuilder b, OverviewPage dataModel)
        //{
        //    List<ProcessRow> rows = new List<ProcessRow>();
        //    foreach (var serviceSnapshot in dataModel.FullLocalStatus.LocalServiceSnapshots)
        //    {
        //        var serviceProcess = dataModel.ServiceProcesses.SingleOrDefault(x => x.ServiceName == serviceSnapshot.ServiceName);

        //        var pid = "Not running";
        //        var runningStatus = "Not running";
        //        string usedRamMb = "0";

        //        if (serviceProcess != null)
        //        {
        //            TimeSpan running = DateTime.UtcNow - serviceProcess.StartTimestampUtc;
        //            TimeSpan rounded = TimeSpan.FromSeconds((int)running.TotalSeconds);
        //            runningStatus = $"{serviceProcess.StartTimestampUtc} ({rounded.ToString("c")})";

        //            pid = serviceProcess.Pid.ToString();
        //            usedRamMb = serviceProcess.UsedRamMB.ToString();
        //        }

        //        rows.Add(new ProcessRow()
        //        {
        //            ServiceName = serviceSnapshot.ServiceName,
        //            ProjectName = serviceSnapshot.ProjectName,
        //            ProjectVersionTag = serviceSnapshot.ProjectVersionTag,
        //            RunningStatus = runningStatus,
        //            Pid = pid,
        //            UsedRamMB = usedRamMb,
        //            HasError = serviceProcess == null
        //        });
        //    }

        //    var clientRows = b.Const(rows.ToList());

        //    var view = b.Div("flex flex-col space-y-4");

        //    if (dataModel.Warnings.Any())
        //    {
        //        foreach (string warning in dataModel.Warnings)
        //        {
        //            var warningContainer = b.Add(view, b.Div("border border-solid border-red-300 bg-red-100 text-red-300 rounded w-full p-2 mb-4 drop-shadow transition-all"));
        //            b.Add(warningContainer, b.Text(warning));
        //        }
        //    }

        //    LocalSettings localSettings = dataModel.LocalSettings;
        //    string title = $"Node: {localSettings.NodeName}, OS: {System.Environment.OSVersion}, infrastructure API: {localSettings.InfrastructureApiUrl}";

        //    string overviewText = $"Total services: 0";

        //    var lastChange = dataModel.FullLocalStatus.SyncResults.Where(x => x.ResultCode == SyncStatusCodes.Changed).OrderByDescending(x => x.Timestamp).FirstOrDefault();

        //    if (lastChange != null)
        //    {
        //        overviewText = $"Local services: {dataModel.FullLocalStatus.LocalServiceSnapshots.Count()}, last change: {lastChange.Timestamp.ToString("G")}";
        //    }

        //    b.Add(view, b.InfoPanel(Panel.Style.Info, title, overviewText));

        //    var rc = b.Def((BlockBuilder b, Var<ProcessRow> serviceSnapshot, Var<DataTable.Column> col) =>
        //    {
        //        Var<string> serviceName = b.Get(serviceSnapshot, x => x.ServiceName);
        //        Var<string> columnName = b.Get(col, x => x.Name);

        //        return b.VPadded4(b.Text(b.GetProperty<string>(serviceSnapshot, columnName)));
        //    });

        //    if (dataModel.FullLocalStatus.LocalServiceSnapshots.Any())
        //    {
        //        var props = b.NewObj<DataTable.Props<ProcessRow>>(b =>
        //        {
        //            b.AddColumn(nameof(ProcessRow.ServiceName), "Service name");
        //            b.AddColumn(nameof(ProcessRow.ProjectName), "Project name");
        //            b.AddColumn(nameof(ProcessRow.ProjectVersionTag), "Project version");
        //            b.AddColumn(nameof(ProcessRow.Pid), "Process ID");
        //            b.AddColumn(nameof(ProcessRow.UsedRamMB), "Working set (RAM, MB)");
        //            b.AddColumn(nameof(ProcessRow.RunningStatus), "Running since");
        //            b.SetRows(clientRows);
        //            b.SetRenderCell(rc);
        //        });

        //        b.Set(props, x => x.CreateRow, b.Def((BlockBuilder b, Var<ProcessRow> row) =>
        //        {
        //            Var<ProcessRow> processRow = row.As<ProcessRow>();
        //            return b.If(b.Get(processRow, x => x.HasError), b => b.Node("tr", "bg-red-500"), b => b.Node("tr"));
        //        }));

        //        b.Add(view, b.DataTable(props));
        //    }

        //    return view;
        //}
    }

    public static class RenderSyncHistory
    {
        public static Var<HyperNode> plm(BlockBuilder b, Var<SyncHistory.DataModel> dataModel)
        {
            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Sync history" }));
            b.Set(headerProps, x => x.User, b.Get(dataModel, x => x.User));

            return b.Layout(
                b.LocalMenu(nameof(SyncHistory)),
                b.Render(headerProps), b.Render(dataModel));
        }

        public static Var<HyperNode> Render(this BlockBuilder b, Var<SyncHistory.DataModel> dataModel)
        {
            var view = b.Div("flex flex-col");

            var clientRows = b.Get(dataModel, x => x.SyncHistory);

            var renderCell = b.Def((BlockBuilder b, Var<SyncResult> serviceSnapshot, Var<DataTable.Column> col) =>
            {
                Var<string> columnName = b.Get(col, x => x.Name);

                var cell = b.Switch<HyperNode, string>(columnName,
                    b => b.Text(b.GetProperty<string>(serviceSnapshot, columnName)),
                    (nameof(SyncResult.Timestamp), b => b.Text(b.ItalianFormat(b.Get(serviceSnapshot, x => x.Timestamp)))));

                return b.VPadded4(cell);
            });

            var props = b.NewObj<DataTable.Props<SyncResult>>(b =>
            {
                b.AddColumn(nameof(SyncResult.Timestamp));
                b.AddColumn(nameof(SyncResult.Trigger), "Sync trigger");
                b.AddColumn(nameof(SyncResult.ResultCode), "Result");
                b.SetRows(clientRows);
                b.SetRenderCell(renderCell);
            });

            b.Add(view, b.DataTable(props));

            return view;
        }
    }
}
