using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using Metapsi.Hyperapp;
using System.Linq;
using Metapsi;
using MdsCommon.Controls;
using Metapsi.Shoelace;
using Metapsi.Html;
using System.Collections.Generic;

namespace MdsLocal
{
    public class RenderOverviewListProcesses : Metapsi.Hyperapp.HyperPage<OverviewPage>
    {
        public const string IdKillProcessPopup = "id-kill-process-popup";

        public override Var<IVNode> OnRender(LayoutBuilder b, Var<OverviewPage> model)
        {
            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Overview" }));
            b.Set(headerProps, x => x.User, b.Const<User>(new User() { Name = "Hardcoded user" }));
            b.Set(headerProps, x => x.UseSignIn, b.Const(false));

            var localSettings = b.Get(model, x => x.LocalSettings);
            var title = b.Concat(
                b.Const($"Node: "),
                b.Get(localSettings, x => x.NodeName),
                b.Const(" OS: "),
                b.Const(System.Environment.OSVersion.ToString()),
                b.Const(" infrastructure API: "),
                b.Get(localSettings, x => x.InfrastructureApiUrl));

            var view = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col space-y-4");
                },
                b.Optional(
                    b.Get(model, x => x.Warnings.Any()),
                    b =>
                    {
                        return b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("w-full flex flex-col gap-4");
                            },
                            b.Map(
                                b.Get(model, x => x.Warnings),
                                (b, w) =>
                                {
                                    return b.HtmlDiv(
                                        b =>
                                        {
                                            b.SetClass("border border-solid border-red-300 bg-red-100 text-red-300 rounded w-full p-2 mb-4 drop-shadow transition-all");
                                        },
                                        b.Text(w));
                                }));
                    }),
                b.InfoPanel(
                    b.Const(Panel.Style.Info),
                    b => b.Text(title),
                    b => b.Text(b.Get(model, x => x.OverviewText))),
                b.MdsMainPanel(b => { }, ProcessesGrid(b, model)),
                KillProcessPopup(b, model));

            return b.Layout(
                b.LocalMenu(nameof(Overview)),
                b.Render(headerProps),
                view);
        }

        private Var<IVNode> ProcessesGrid(LayoutBuilder b, Var<OverviewPage> model)
        {
            var processes = b.Get(model, x => x.Processes);

            var processesGridBuilder = MdsDefaultBuilder.DataGrid<ProcessRow>();

            processesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(ProcessRow.ServiceName), b => b.Text("Service name"));
            processesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(ProcessRow.ProjectName), b => b.Text("Project name"));
            processesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(ProcessRow.ProjectVersion), b => b.Text("Project version"));
            processesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(ProcessRow.Pid), b => b.Text("Process ID"));
            processesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(ProcessRow.UsedRam), b => b.Text("Working set (RAM, MB)"));
            processesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(ProcessRow.RunningSince), b => b.Text("Running since"));
            processesGridBuilder.DataTableBuilder.OverrideDataCell(nameof(ProcessRow.ServiceName),
                (b, row) =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-row gap-1");
                        },
                        b.Text(b.Get(row, x => x.ServiceName)),
                        b.Optional(
                            b.Get(row, x => x.Disabled),
                            b => b.Badge(b.Const("disabled"), b.Const("bg-orange-600"))),
                        b.Optional(
                            b.Get(row, x => !x.Disabled && !x.Running),
                            b => b.Badge(b.Const("not running"), b.Const("bg-red-600"))));
                });
            //processesGridBuilder.DataTableBuilder.AddTrProps(
            //    (b, process) =>
            //    {
            //        b.If(
            //            b.Get(process, x => x.Disabled),
            //            b =>
            //            {
            //                b.AddClass("bg-orange-400");
            //            },
            //            b => b.If(
            //                b.Get(process, x => x.HasError),
            //                b =>
            //                {
            //                    b.AddClass("bg-red-400");
            //                }));
            //    });

            processesGridBuilder.AddRowAction(
                (b, row) =>
                {
                    return b.Optional(
                        b.Get(row, x => x.Running),
                        b => b.RowIconAction(
                            b =>
                            {
                                b.OnClickAction(b.MakeActionDescriptor<OverviewPage, ProcessRow>(KillProcessAction, row));
                            },
                            b.Svg(Metapsi.Heroicons.Solid.ArrowPath, "w-8 h-8 text-red-500")));
                });

            var columns = DataTable.GetColumns<ProcessRow>().ToList().Except(new List<string>() { nameof(ProcessRow.Running), nameof(ProcessRow.Disabled) });
            return b.DataGrid(processesGridBuilder, processes, columns.ToArray());
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
                b.SlDialog(
                    b =>
                    {
                        b.SetId(IdKillProcessPopup);
                    },
                    b.DialogHeader("Restart process?"),
                    b.Text(b.Concat(b.Const("Are you sure you want to restart "), b.Get(model, x=>x.RestartProcess.ServiceName), b.Const("?"))),
                    b.DialogFooter(
                        "",
                        b.HtmlButton(
                            b =>
                            {
                                b.AddButtonStyle();
                                b.AddClass("bg-red-500");

                                b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<OverviewPage> model) =>
                                {
                                    b.HideDialog(b.Const(IdKillProcessPopup));
                                    var process = b.Get(model, x => x.RestartProcess);

                                    var pid = b.Get(process, x => x.Pid);

                                    b.Set(process, x => x.Pid, b.Const(string.Empty));
                                    b.Set(process, x => x.UsedRam, b.Const(string.Empty));
                                    b.Set(process, x => x.RunningSince, b.Const(string.Empty));

                                    return b.MakeStateWithEffects(
                                        b.Clone(model),
                                        b.PostRequest(
                                            Frontend.KillProcessByPid,
                                            pid,
                                            b.MakeAction((SyntaxBuilder b, Var<OverviewPage> page, Var<ApiResponse> response) =>
                                            {
                                                return b.MakeStateWithEffects(
                                                    b.Clone(model),
                                                    b.Request(
                                                        Frontend.ReloadProcesses,
                                                        b.MakeAction((SyntaxBuilder b, Var<OverviewPage> page, Var<ReloadedOverviewModel> response) =>
                                                        {
                                                            return b.Get(response, x => x.Model);
                                                        })));
                                            })));
                                }));
                            },
                            b.Text("Restart"))));
        }
    }

    public static class DialogExtensions
    {
        public static Var<IVNode> DialogHeader(this LayoutBuilder b, string label)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.AddClass("flex flex-row items-center gap-2");
                    b.SetSlot("label");
                },
                b.Text(label));
        }

        public static Var<IVNode> DialogFooter(this LayoutBuilder b, string text, params Var<IVNode>[] buttons)
        {
            return
                b.HtmlDiv(
                    b =>
                    {
                        b.SetSlot("footer");
                        b.AddClass("flex flex-row items-center justify-between");
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.AddClass("text-xs text-gray-600 text-left");
                        },
                        b.Text(text)),
                    b.HtmlDiv(
                        b =>
                        {
                            // To make the buttons smaller at end
                            b.AddClass("flex flex-row gap-2 justify-end");
                        },
                        buttons));
        }
    }
}
