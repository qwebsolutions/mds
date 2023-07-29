using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using Metapsi.Hyperapp;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using Metapsi;

namespace MdsLocal
{
    public class RenderOverviewListProcesses : Metapsi.Hyperapp.HyperPage<OverviewPage>
    {
        public override Var<HyperNode> OnRender(BlockBuilder b, Var<OverviewPage> model)
        {
            b.AddStylesheet("metapsi.hyperapp.css");

            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Infrastructure events" }));
            //b.Set(headerProps, x => x.User, b.Get(model, x => x.User));
            b.Set(headerProps, x => x.User, b.Const<User>(new User() { Name = "Hardcoded user" }));

            var clientRows = b.Get(model, x => x.Processes);

            var view = b.Div("flex flex-col space-y-4");

            b.If(
                b.Get(model, x => x.Warnings.Any()),
                b =>
                {
                    b.Foreach(
                        b.Get(model, x => x.Warnings),
                        (b, w) =>
                        {
                            var warningContainer = b.Add(view, b.Div("border border-solid border-red-300 bg-red-100 text-red-300 rounded w-full p-2 mb-4 drop-shadow transition-all"));
                            b.Add(warningContainer, b.Text(w));
                        });
                });

            var localSettings = b.Get(model, x => x.LocalSettings);
            var title = b.Concat(
                b.Const($"Node: "),
                b.Get(localSettings, x => x.NodeName),
                b.Const(" OS: "),
                b.Const(System.Environment.OSVersion.ToString()),
                b.Const(" infrastructure API: "),
                b.Get(localSettings, x => x.InfrastructureApiUrl));

            b.Add(view, b.InfoPanel(
                b.Const(Panel.Style.Info),
                b => b.Text(title),
                b => b.Text(b.Get(model, x => x.OverviewText))));

            var rc = b.Def((BlockBuilder b, Var<ProcessRow> serviceSnapshot, Var<DataTable.Column> col) =>
            {
                Var<string> serviceName = b.Get(serviceSnapshot, x => x.ServiceName);
                Var<string> columnName = b.Get(col, x => x.Name);

                return b.VPadded4(b.Text(b.GetProperty<string>(serviceSnapshot, columnName)));
            });

            b.If(
                b.Get(model, model => model.FullLocalStatus.LocalServiceSnapshots.Any()),
                b =>
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
                });

            return view;
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
                new (){Code = nameof(MdsCommon.Routes.EventsLog) , Label = "Events log", Href = Route.Path<MdsCommon.Routes.EventsLog.List>() }
            };

            return b.Menu(b.Const(new Metapsi.Hyperapp.Menu.Props()
            {
                ActiveCode = selectedCode,
                Entries = menuEntries.ToList()
            }));
        }
    }

    //public class PageDetails
    //{
    //    public string Version { get; set; } = string.Empty;
    //    public string PageTitle { get; set; } = string.Empty;
    //    public List<LinkTag> LinkTags { get; set; } = new();
    //    public List<string> ScriptPaths { get; set; } = new();
    //    public string Favicon { get; set; } = string.Empty;
    //    public object InitialModel { get; set; }
    //    public string PageScript { get; set; }
    //    public string ModuleScriptPath { get; set; } = string.Empty;
    //    public string BodyCss { get; set; } = string.Empty;
    //}


    //public abstract class PageBuilder<TModel> : IPageBuilder<TModel>
    //{
    //    private static Module module = null;

    //    public Module GetModule()
    //    {
    //        if (module == null)
    //        {
    //            module = Page.BuildMain<TModel>(Render, Init);
    //        }
    //        return module;
    //    }

    //    public virtual Var<HyperType.StateWithEffects> Init(BlockBuilder b, Var<TModel> model)
    //    {
    //        return b.MakeStateWithEffects(model);
    //    }

    //    public abstract Var<HyperNode> Render(BlockBuilder b, Var<TModel> model);
    //}

    public static partial class ListProcesses
    {
        //public static string RenderListProcessesBuilder(OverviewPage serverModel)
        //{
        //    return BuildExternalScriptPageContent(new PageDetails()
        //    {
        //        InitialModel = serverModel,
        //        PageTitle = "Overview",
        //        PageScript = Metapsi.JavaScript.PrettyBuilder.Generate(
        //            BuildMain<OverviewPage>((BlockBuilder b, Var<OverviewPage> page) => b.Text("Built!")),
        //            string.Empty)
        //    });
        //}


        

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

    public class RenderSyncHistory : HyperPage<SyncHistory.DataModel>
    {
        public override Var<HyperNode> OnRender(BlockBuilder b, Var<SyncHistory.DataModel> dataModel)
        {

            b.AddStylesheet("metapsi.hyperapp.css");

            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Sync history" }));
            b.Set(headerProps, x => x.User, b.Get(dataModel, x => x.User));

            return b.Layout(
                b.LocalMenu(nameof(SyncHistory)),
                b.Render(headerProps), Render2(b, dataModel));
        }

        public static Var<HyperNode> Render2(BlockBuilder b, Var<SyncHistory.DataModel> dataModel)
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
