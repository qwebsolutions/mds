using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;
using Metapsi.Shoelace;
using Metapsi.Dom;

namespace MdsInfrastructure.Render
{
    public static class Project
    {
        const string DeleteSelectedDialogId = "id-delete-selected-dialog";

        public class List : MixedHyperPage<MdsInfrastructure.ListProjectsPage, MdsInfrastructure.ListProjectsPage>
        {
            public override ListProjectsPage ExtractClientModel(ListProjectsPage serverModel)
            {
                return serverModel;
            }

            public override Var<IVNode> OnRender(LayoutBuilder b, ListProjectsPage serverModel, Var<ListProjectsPage> clientModel)
            {
                b.AddModuleStylesheet();
                var headerProps = b.GetHeaderProps(
                    b.Const("Projects"),
                    b.Const(string.Empty),
                    b.Get(clientModel, x => x.User));

                return b.Layout(
                    b.InfraMenu(nameof(Routes.Project), serverModel.User.IsSignedIn()),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Projects" },
                        User = serverModel.User
                    })), 
                    b.Render(clientModel));
            }
        }

        public static Var<string> TargetEnvironment(this SyntaxBuilder b, Var<string> target)
        {
            return b.If(b.AreEqual(target, b.Const("linux-x64")), b => b.Const("Linux"), b => b.Const("Windows"));
        }

        public static Var<IVNode> Render(this LayoutBuilder b, Var<ListProjectsPage> clientModel)
        {
            var sidePanel = b.SidePanel(
                b =>
                {
                    var project = b.Get(clientModel, x => x.SelectedProject);
                    var projectId = b.Get(project, x => x.Id);
                    var versions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).OrderByDescending(x => x.VersionTag).ToList());

                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col space-y-4");
                        },
                        b.TextSpan(b.Get(project, x => x.Name)),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-col gap-4");
                            },
                            b.Map(versions, (b, version) =>
                            {
                                var versionId = b.Get(version, x => x.Id);
                                var isInUse = b.Get(clientModel, versionId, (x, versionId) => x.InfrastructureServices.Any(x => x.ProjectVersionId == versionId));

                                var versionBuilds = b.Get(version, x => x.Binaries);
                                var buildDescriptions = b.Get(versionBuilds, b.Def<SyntaxBuilder, string, string>(TargetEnvironment), (version, getTarget) => version.Select(x => "Build " + x.BuildNumber + " " + getTarget(x.Target)).ToList());

                                var checkboxText = b.Get(
                                    version,
                                    b.Def<SyntaxBuilder, string, System.Collections.Generic.List<string>, string>(Core.JoinStrings),
                                    buildDescriptions,
                                    (version, join, buildDescriptions) => version.VersionTag + "(" + join(", ", buildDescriptions) + ")");

                                return b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("flex flex-row");
                                    },
                                    b.Toggle(
                                        b.Get(version, x => x.Enabled),
                                        b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> state, Var<bool> isChecked) =>
                                        {
                                            var initialVersion = b.Clone(version);
                                            b.ShowLoading(state);
                                            b.Set(version, x => x.Enabled, isChecked);
                                            return b.MakeStateWithEffects(
                                                b.Clone(state),
                                                b.MakeEffect(
                                                    b.Def(
                                                        b.CallApi<ListProjectsPage, ProjectVersion>(
                                                            Backend.SaveVersionEnabled,
                                                            version,
                                                            (SyntaxBuilder b, Var<ListProjectsPage> state) => b.HideLoading(state),
                                                            (SyntaxBuilder b, Var<ListProjectsPage> state, Var<ApiError> error) =>
                                                            {
                                                                var allVersions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).ToList());
                                                                var versionReference = b.Get(allVersions, b.Get(initialVersion, x => x.Id), (allVersions, versionid) => allVersions.Single(x => x.Id == versionid));
                                                                b.Set(versionReference, x => x.Enabled, b.Get(initialVersion, x => x.Enabled));
                                                                b.HideLoading(state);
                                                                return b.Clone(state);
                                                            }))));

                                        }),
                                            checkboxText,
                                            checkboxText,
                                            b =>
                                            {
                                                b.Set(x => x.Enabled, b.Not(isInUse));
                                            }),
                                    b.Optional(
                                        isInUse,
                                        b =>
                                        {
                                            return b.Badge(b.Const("in use"), b.Const("bg-green-500"));
                                        })
                                    );
                            })));
                });

            var projectTableBuilder = MdsDefaultBuilder.DataTable<MdsCommon.Project>();
            projectTableBuilder.OverrideDataCell(
                "Project name",
                (b, project) =>
                {
                    return b.HtmlA(
                        b =>
                        {
                            b.SetClass("underline text-sky-500");
                            b.SetHref(b.Const("javascript:void(0);"));
                            b.OnClickAction((SyntaxBuilder b, Var<ListProjectsPage> state) =>
                            {
                                b.Set(state, x => x.SelectedProject, project);
                                b.ShowSidePanel();
                                return b.Clone(state);
                            });
                        },
                        b.TextSpan(b.Get(project, x => x.Name)));
                });
            projectTableBuilder.OverrideDataCell(
                "Versions",
                (b, project) =>
                {
                    var projectId = b.Get(project, x => x.Id);
                    var versions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).Count());
                    return b.TextSpan(b.AsString(versions));
                });


            var projectsTab = b.NewObj<TabControl.TabPair>();
            var projectsTabHeader = b.Text("Infrastructure projects");
            var projectsTabContent = b.DataTable(
                projectTableBuilder,
                b.Get(clientModel, x => x.ProjectsList),
                "Project name",
                "Versions");
            b.Set(projectsTab, x => x.TabHeader, projectsTabHeader);
            b.Set(projectsTab, x => x.TabContent, projectsTabContent);

            var repositoryTab = b.NewObj<TabControl.TabPair>();
            var repositoryTabHeader = b.Text("Repository");
            var repositoryTabContent = b.RepositoryTab(clientModel);

            b.Set(repositoryTab, x => x.TabHeader, repositoryTabHeader);
            b.Set(repositoryTab, x => x.TabContent, repositoryTabContent);

            var tabs = b.Tabs(
                b.HtmlDiv(),
                projectsTab,
                repositoryTab);

            return b.HtmlDiv(
                b =>
                {
                    // The side panel must not be inside the main panel
                    // because it's FIXED position gets broken by...
                    // ... the parent DROP SHADOW!
                },
                sidePanel,
                b.ValidationPanel(clientModel),
                b.RemoveSelectedDialog(clientModel),
                tabs);
        }

        public static Var<IVNode> RemoveSelectedDialog(this LayoutBuilder b, Var<ListProjectsPage> model)
        {
            var selectedCount = b.Get(model, x => x.Binaries.Where(x => x.Selected).Count());

            var titleQuestion = b.If(
                b.AreEqual(selectedCount, b.Const(1)),
                b => b.Const("Are you sure you want to remove the selected build?"),
                b => b.Concat(b.Const("Are you sure you want to delete "), b.AsString(selectedCount), b.Const(" builds?")));

            return b.SlDialog(
                   b =>
                   {
                       b.SetId(DeleteSelectedDialogId);
                   },
                   b.HtmlDiv(
                       b =>
                       {
                           b.SetSlot(SlDialog.Slot.Label);
                           b.SetClass("text-sm");
                       },
                       b.Text(titleQuestion)),
                   b.HtmlDiv(
                       b =>
                       {
                           b.SetClass("flex flex-col gap-4");
                       },
                       b.HtmlDiv(
                           b =>
                           {
                               b.SetClass("flex flex-row gap-2");
                           },
                           b.SlIcon(
                               b =>
                               {
                                   b.SetClass("text-orange-500");
                                   b.SetName("exclamation-triangle");
                               }),
                           b.HtmlSpanText(
                               b =>
                               {
                                   b.SetClass("text-xs");
                               },
                               "This operation will remove the builds for ALL infrastructures connected to this repository"))),
                   b.HtmlButton(
                        b =>
                        {
                            b.SetSlot(SlDialog.Slot.Footer);
                            b.SetClass("rounded bg-red-600 text-white px-4 py-2");
                        },
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass(" flex flex-row items-center gap-1");
                            },
                            b.SlIcon(b => b.SetName("trash")),
                            b.Text("Yes, delete!")))
                   );
        }

        public static Var<IVNode> RepositoryTab(this LayoutBuilder b, Var<ListProjectsPage> model)
        {
            return b.HtmlDiv(
               b =>
               {
                   b.SetClass("flex flex-col gap-8 w-full");
               },
               b.HtmlDiv(
                   b =>
                   {
                       b.SetClass("flex flex-row gap-2 justify-end items-center");
                   },
                   b.SlDropdown(
                       b =>
                       {
                       },
                       b.SlIconButton(
                           b =>
                            {
                                b.SetSlot(SlDropdown.Slot.Trigger);
                                b.SetName("gear");
                                b.SetLabel("binaries-options");
                            }),
                       b.SlMenu(
                           b=>
                           {
                               b.OnSlSelect((SyntaxBuilder b, Var<ListProjectsPage> model, Var<SlSelectEventArgs> args) =>
                               {
                                   return b.Switch(
                                       b.Get(args, x => x.item.value),
                                       b => model,
                                       ("select-filtered", b =>
                                       {
                                           var filtered = b.FilterList(b.Get(model, x => x.Binaries), b.Get(model, x => x.SearchKeyword));
                                           b.Foreach(filtered, (b, item) => b.Set(item, x => x.Selected, true));
                                           return b.Clone(model);
                                       }),
                                       ("remove-selected", b=>
                                       {
                                           b.Set(model, x => x.SearchKeyword, b.Const(string.Empty));
                                           b.ShowDialog(b.Const(DeleteSelectedDialogId));
                                           return b.Clone(model);
                                       })
                                   );
                               });
                           },
                           b.SlMenuItem(
                               b =>
                               {
                                   b.SetValue("select-filtered");
                                   b.If(
                                       b.Not(
                                           b.HasValue(b.Get(model, x => x.SearchKeyword))),
                                       b => b.SetDisabled());
                               },
                               b.SlIcon(
                                   b=>
                                   {
                                       b.SetSlot(SlMenuItem.Slot.Prefix);
                                       b.SetName(b.Const("check2-square"));
                                   }),
                                b.Text("Select filtered items")),
                           b.SlMenuItem(
                               b =>
                               {
                                   b.SetValue("remove-selected");
                                   b.If(
                                       b.Not(b.Get(model, x => x.Binaries.Any(x => x.Selected))),
                                       b => b.SetDisabled());
                               },
                               b.SlIcon(
                                   b =>
                                   {
                                       b.SetSlot(SlMenuItem.Slot.Prefix);
                                       b.SetName(b.Const("trash"));
                                   }),
                                b.Text("Delete selected"))
                           )),
                   b.Filter(model, x => x.SearchKeyword)),
               b.BinariesRepositoryGrid(model));
        }

        public static Var<IVNode> BinariesRepositoryGrid(this LayoutBuilder b, Var<ListProjectsPage> model)
        {
            var tableBuilder = MdsDefaultBuilder.DataTable<BinariesRepositoryEntry>();
            tableBuilder.OverrideHeaderCell(
                nameof(BinariesRepositoryEntry.Selected),
                b =>
                {
                    return b.HtmlDiv();
                });

            tableBuilder.OverrideDataCell(
                nameof(BinariesRepositoryEntry.Selected),
                (b, entry) =>
                {
                    return b.HtmlDiv(
                        b=>
                        {
                            b.SetClass("flex flex-row items-center justify-center accent-sky-600");
                        },
                        b.HtmlCheckbox(
                            b =>
                            {
                                b.SetChecked(b.Get(entry, x => x.Selected));
                                b.OnClickAction((SyntaxBuilder b, Var<ListProjectsPage> model, Var<DomEvent> args) =>
                                {
                                    var isChecked = b.GetProperty<bool>(b.Get(args, x => x.target), "checked");
                                    b.Set(entry, x => x.Selected, isChecked);
                                    return b.Clone(model);
                                });
                            }));
                });

            var filtered = b.FilterList(b.Get(model, x => x.Binaries), b.Get(model, x => x.SearchKeyword));
            var sorted = b.Get(filtered, x => x.OrderByDescending(x => x.BuildNumber).ToList());

            return b.DataTable(tableBuilder, sorted);

            //eventsTableBuilder.OverrideHeaderCell(nameof(InfrastructureEvent.ShortDescription), b => b.Text("Description"));

            //eventsTableBuilder.OverrideDataCell(
            //    nameof(InfrastructureEvent.Timestamp),
            //    (b, row) =>
            //    {
            //        var date = b.Get(row, x => x.Timestamp);
            //        var dateStringLocale = b.ItalianFormat(date);

            //        return b.Link(
            //            dateStringLocale,
            //            b.MakeAction<ListInfrastructureEventsPage>(
            //            (b, clientModel) =>
            //            {
            //                b.Set(clientModel, x => x.SelectedEvent, row);
            //                b.ShowSidePanel();
            //                return b.Clone(clientModel);
            //            }));
            //    });
            //eventsTableBuilder.OverrideDataCell(
            //    nameof(InfrastructureEvent.Criticality),
            //    (b, row) =>
            //    {
            //        var criticality = b.Get(row, x => x.Criticality);

            //        return b.HtmlSpan(
            //            b =>
            //            {
            //            },
            //            b.TextSpan(criticality),
            //            b.AlertBadge(criticality));
            //    });
        }
    }
}