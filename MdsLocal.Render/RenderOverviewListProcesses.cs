using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using Metapsi.Hyperapp;
using System.Linq;
using Metapsi;
using MdsCommon.Controls;
using MdsCommon.HtmlControls;
using Metapsi.Shoelace;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MdsLocal
{
    public class RenderOverviewListProcesses : Metapsi.Hyperapp.HyperPage<OverviewPage>
    {
        public const string IdKillProcessPopup = "id-kill-process-popup";

        public override Var<IVNode> OnRender(LayoutBuilder b, Var<OverviewPage> model)
        {
            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Infrastructure events" }));
            b.Set(headerProps, x => x.User, b.Const<User>(new User() { Name = "Hardcoded user" }));
            b.Set(headerProps, x => x.UseSignIn, b.Const(false));

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

            var processes = b.Get(model, x => x.Processes);

            b.OnModel(
                model,
                (bParent, context) =>
                {
                    var b = new LayoutBuilder(bParent);

                    var dataGrid = b.DataGrid<ProcessRow>(
                        b =>
                        {
                            b.OnTable(b =>
                            {
                                b.FillFrom(processes, exceptColumns: new()
                                {
                                    nameof(ProcessRow.HasError)
                                });

                                b.SetCommonStyle();
                            });

                            b.AddHoverRowAction<OverviewPage, ProcessRow>(KillProcessAction, Metapsi.Heroicons.Solid.ArrowPath, (b, data, props) =>
                            {
                                b.AddClass(props, "w-8 h-8 text-red-500");
                            }, 
                            visible: (b, row) =>
                            {
                                return b.Get(row, x => x.Pid != "Not running");
                            });
                        });

                    b.Add(view, b.AddClass(dataGrid, "p-4 bg-white"));
                });

            //var rc = b.Def((LayoutBuilder b, Var<ProcessRow> serviceSnapshot, Var<DataTable.Column> col) =>
            //{
            //    Var<string> serviceName = b.Get(serviceSnapshot, x => x.ServiceName);
            //    Var<string> columnName = b.Get(col, x => x.Name);

            //    return b.VPadded4(b.Text(b.GetProperty<string>(serviceSnapshot, columnName)));
            //});

            //b.If(
            //    b.Get(model, model => model.FullLocalStatus.LocalServiceSnapshots.Any()),
            //    b =>
            //    {
            //        var props = b.NewObj<DataTable.Props<ProcessRow>>(b =>
            //        {
            //            b.AddColumn(nameof(ProcessRow.ServiceName), "Service name");
            //            b.AddColumn(nameof(ProcessRow.ProjectName), "Project name");
            //            b.AddColumn(nameof(ProcessRow.ProjectVersion), "Project version");
            //            b.AddColumn(nameof(ProcessRow.Pid), "Process ID");
            //            b.AddColumn(nameof(ProcessRow.UsedRam), "Working set (RAM, MB)");
            //            b.AddColumn(nameof(ProcessRow.RunningSince), "Running since");
            //            b.SetRows(clientRows);
            //            b.SetRenderCell(rc);
            //        });

            //        b.Set(props, x => x.CreateRow, b.Def((LayoutBuilder b, Var<ProcessRow> row) =>
            //        {
            //            Var<ProcessRow> processRow = row.As<ProcessRow>();
            //            return b.If(b.Get(processRow, x => x.HasError), b => b.Node("tr", "bg-red-500"), b => b.Node("tr"));
            //        }));

            //        b.Add(view, b.DataTable(props));
            //    });


            b.Add(view, KillProcessPopup(b, model));

            return b.Layout(
                b.LocalMenu(nameof(Overview)),
                b.Render(headerProps),
                view).As<IVNode>();
        }

        private Var<OverviewPage> KillProcessAction(SyntaxBuilder b, Var<OverviewPage> model, Var<ProcessRow> processData)
        {
            b.Set(model, x => x.RestartProcess, processData);
            b.ShowDialog(b.Const(IdKillProcessPopup));
            return b.Clone(model);
        }


        public static Var<IVNode> KillProcessPopup(
            LayoutBuilder b,
            Var<OverviewPage> model)
        {
            return
                b.SlNode(
                    "sl-dialog",
                    (b, props) =>
                    {
                        b.SetDynamic(props, Html.id, b.Const(IdKillProcessPopup));
                    },
                    b.DialogHeader("Restart process?"),
                    b.T(b.Concat(b.Const("Are you sure you want to restart "), b.Get(model, x=>x.RestartProcess.ServiceName), b.Const("?"))),
                    b.DialogFooter(
                        "The process will be forcefully killed and then automatically restarted",
                        b.H(
                            "button",
                            (b, props) =>
                            {
                                b.AddButtonStyle(props);
                                b.AddClass(props, "bg-red-500");

                                b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<OverviewPage> model) =>
                                {
                                    b.HideDialog(b.Const(IdKillProcessPopup));
                                    var process = b.Get(model, x => x.RestartProcess);

                                    var pid = b.Get(process, x => x.Pid);

                                    b.Set(process, x => x.Pid, b.Const(string.Empty));
                                    b.Set(process, x => x.UsedRam, b.Const(string.Empty));
                                    b.Set(process, x => x.RunningSince, b.Const(string.Empty));

                                    return b.MakeStateWithEffects(
                                        b.Clone(model),
                                        b.MakeEffect(
                                            b.Def(
                                                b.Request(
                                                    Frontend.KillProcessByPid,
                                                    pid,
                                                    b.MakeAction((SyntaxBuilder b, Var<OverviewPage> page, Var<ApiResponse> response) =>
                                                    {
                                                        return b.MakeStateWithEffects(
                                                            b.Clone(model),
                                                            b.MakeEffect(
                                                                b.Def(
                                                                    b.Request(Frontend.ReloadProcesses,
                                                                    b.MakeAction((SyntaxBuilder b, Var<OverviewPage> page, Var<ReloadedOverviewModel> response) =>
                                                                    {
                                                                        return b.Get(response, x => x.Model);
                                                                    })))));
                                                    })))));
                                }));
                            },
                            b.T("Restart"))));
        }
    }

    public static class DialogExtensions
    {
        public static Var<IVNode> DialogHeader(this LayoutBuilder b, string label)
        {
            return b.H(
                "div",
                (b, props) =>
                {
                    b.AddClass(props, "flex flex-row items-center gap-2");
                    b.SetDynamic(props, DynamicProperty.String("slot"), b.Const("label"));
                },
                b.T(label));
        }

        public static Var<IVNode> DialogFooter(this LayoutBuilder b, string text, params Var<IVNode>[] buttons)
        {
            return
                b.H(
                    "div",
                    (b, props) =>
                    {
                        b.SetDynamic(props, DynamicProperty.String("slot"), b.Const("footer"));
                        b.AddClass(props, "flex flex-row items-center justify-between");
                    },
                    b.H(
                        "div",
                        (b, props) =>
                        {
                            b.AddClass(props, "text-xs text-gray-600 text-left");
                        },
                        b.T(text)),
                    b.H(
                        "div",
                        (b, props) =>
                        {
                            // To make the buttons smaller at end
                            b.AddClass(props, "flex flex-row gap-2 justify-end");
                        },
                        buttons));
        }
    }
}
