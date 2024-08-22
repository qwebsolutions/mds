using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;
using Metapsi.Shoelace;
using Metapsi.Dom;
using System.Collections.Generic;

namespace MdsInfrastructure.Render
{
    public static class Project
    {
        const string DeleteSelectedDialogId = "id-delete-selected-dialog";

        public class List
        {
            public static Var<IVNode> Render(LayoutBuilder b, ListProjectsPage serverModel, Var<ListProjectsPage> clientModel)
            {
                b.AddModuleStylesheet();
                b.AddScript(typeof(Project).Assembly, "MdsInfrastructure.Render.js", "module");
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
                                                b.PostJson(
                                                    b.GetApiUrl(Frontend.ToggleVersionEnabled),
                                                    b.NewObj<ToggleVersionEnabledInput>(
                                                        b =>
                                                        {
                                                            b.Set(x => x.VersionId, b.Get(version, x => x.Id));
                                                            b.Set(x => x.IsEnabled, isChecked);
                                                        }),
                                                    b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> state) => b.HideLoading(state)),
                                                    b.MakeAction(
                                                        (SyntaxBuilder b, Var<ListProjectsPage> state, Var<ClientSideException> error) =>
                                                        {
                                                            var allVersions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).ToList());
                                                            var versionReference = b.Get(allVersions, b.Get(initialVersion, x => x.Id), (allVersions, versionid) => allVersions.Single(x => x.Id == versionid));
                                                            b.Set(versionReference, x => x.Enabled, b.Get(initialVersion, x => x.Enabled));
                                                            b.HideLoading(state);
                                                            return b.Clone(state);
                                                        })));

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
            var selectedCount = b.Get(model, x => x.ToDeleteBinaries.Count());

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
                               b.SetClass("flex flex-row gap-2 items-center");
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
                               "This repository might be shared by multiple infrastructures. Please make sure the selected builds are not used by ANY of them!")),
                       b.HtmlDiv(
                           b => b.SetClass("flex flex-col"),
                           b.SlProgressBar(
                               b =>
                               {
                                   var toDeleteCount = b.Get(model, x => x.ToDeleteBinaries.Count());
                                   var removedCount = b.Get(model, x => x.ToDeleteBinaries.Count(x => x.Removed));
                                   var value = b.Get(selectedCount, removedCount, (selectedCount, removedCount) => (removedCount / selectedCount) * 100);

                                   b.SetValue(value);
                                   b.AddStyle("--height", "1px");
                               }),
                           b.HtmlDiv(
                               b =>
                               {
                                   b.SetClass("flex flex-col gap-1 max-h-72 p-4 overflow-auto");
                               },
                               b.Map(
                                   b.Get(model, x => x.ToDeleteBinaries.ToList()),
                                   (b, item) =>
                                   {
                                       return b.HtmlDiv(
                                           b => b.SetClass("flex flex-row gap-2 items-center font-mono text-xs"),
                                           b.Text(
                                               b.Concat(
                                                   b.Get(item, x => x.ProjectName),
                                                   b.Const(" "),
                                                   b.Get(item, x => x.ProjectVersion),
                                                   b.Const(" "),
                                                   b.Get(item, x => x.Target))),
                                           b.Optional(b.Get(item, x => x.Removed), b => b.SlIcon(b => b.SetName("check"))));
                                   })
                               ))),
                   b.SlAlert(
                       b =>
                       {
                           b.SetVariantDanger();
                           b.If(
                               b.HasValue(
                                   b.Get(model, x => x.DeleteError)),
                               b =>
                               {
                                   b.SetOpen();
                               });
                       },
                       b.SlIcon(
                           b =>
                           {
                               b.SetSlot(SlAlert.Slot.Icon);
                               b.SetName("exclamation-octagon");
                           }),
                       b.HtmlStrong(b.Text(b.Get(model, x => x.DeleteError)))),
                   b.HtmlButton(
                        b =>
                        {
                            b.SetSlot(SlDialog.Slot.Footer);
                            b.SetClass("rounded bg-red-600 text-white px-4 py-2 disabled:opacity-50");
                            b.If(
                                b.Get(model, x => x.IsLoading),
                                b =>
                                {
                                    b.SetDisabled();
                                });
                            b.OnClickAction<ListProjectsPage, HtmlButton>(OnDeleteSelectedAction);
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

        public static Var<HyperType.StateWithEffects> OnDeleteSelectedAction(SyntaxBuilder b, Var<ListProjectsPage> model)
        {
            return b.MakeStateWithEffects(
                b.ShowLoading(b.Clone(model)),
                b.MakeEffect(
                    (SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
                    {
                        b.AsyncForeach(
                            b.Get(model, x=>x.ToDeleteBinaries),
                            (b, item, next) =>
                            {
                                b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> model) =>
                                {
                                    b.Log("Delete error", b.Get(model, x => x.DeleteError));
                                    // Skip fetch if already in error, but move next in async foreach to avoid the iteration never being completed
                                    b.If(
                                        b.HasValue(b.Get(model, x => x.DeleteError)),
                                        b =>
                                        {
                                            b.Log("Delete error", b.Get(model, x => x.DeleteError));
                                            b.Call(next);
                                        },
                                        b =>
                                        {
                                            b.Log("Call delete for item", item);
                                            b.CallDelete(item, dispatch, next);
                                        });
                                    return model;
                                }));
                            },
                            b =>
                            {
                                b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> model) =>
                                {
                                    return b.MakeStateWithEffects(model, b.RefreshModelOnEnd());
                                }));
                            });
                    }));
        }

        public static void CallDelete(this SyntaxBuilder b, Var<BinariesRepositoryEntry> item, Var<HyperType.Dispatcher> dispatch, Var<System.Action> moveNext)
        {
            var removeRequest = b.NewObj<RemoveBuildsRequest>();
            b.Push(b.Get(removeRequest, x => x.ToRemove), item);
            b.PostJson(
                b.GetApiUrl(Frontend.RemoveBuilds),
                removeRequest,
                b.Def((SyntaxBuilder b, Var<RemoveBuildsResponse> response) =>
                {
                    b.Dispatch(dispatch, b.MakeRemoveSuccessAction(response, moveNext));
                }),
                b.Def((SyntaxBuilder b, Var<ClientSideException> error) =>
                {
                    b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> model) =>
                    {
                        b.Set(model, x => x.DeleteError, b.Get(error, x => x.message));
                        return b.MakeStateWithEffects(
                            b.Clone(b.HideLoading(model)),
                            b.MakeEffect(moveNext));
                    }));
                }));
        }

        public static Var<BinariesRepositoryEntry> SameBinary(
            this SyntaxBuilder b,
            Var<List<BinariesRepositoryEntry>> inList,
            Var<BinariesRepositoryEntry> asThisOne)
        {
            return b.Get(inList, asThisOne, (inList, asThisOne) => inList.SingleOrDefault(x => x.ProjectName == asThisOne.ProjectName && x.ProjectVersion == asThisOne.ProjectVersion && x.Target == asThisOne.Target));
        }

        public static Var<HyperType.Action<ListProjectsPage>> MakeRemoveSuccessAction(
            this SyntaxBuilder b,
            Var<RemoveBuildsResponse> result,
            Var<System.Action> moveNext)
        {
            // If the result contains error, dispatch, move next

            return b.If(
                b.HasValue(
                    b.Get(result, x => x.ErrorMessage)),
                b => b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> model) =>
                {
                    b.Set(model, x => x.DeleteError, b.Get(result, x => x.ErrorMessage));
                    return b.MakeStateWithEffects(
                        b.Clone(model),
                        b => b.Call(moveNext));
                }),
                b => b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> model) =>
                {
                    b.MarkRemovedInModel(model, result);
                    return b.MakeStateWithEffects(
                        b.Clone(model),
                        b.MakeEffect(moveNext));
                }));
        }

        public static Var<HyperType.Effect> RefreshModelOnEnd(this SyntaxBuilder b)
        {
            return b.MakeEffect((SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
            {
                b.GetJson(
                    b.GetApiUrl(Frontend.ReloadListProjectsPageModel),
                     b.Def((SyntaxBuilder b, Var<ReloadListProjectsPageModel> response) =>
                     {
                         b.If(
                             b.HasValue(
                                 b.Get(response, x => x.ErrorMessage)),
                             b =>
                             {
                                 // Logical error

                                 b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> model) =>
                                 {
                                     b.Set(model, x => x.DeleteError, b.Get(response, x => x.ErrorMessage));
                                     return b.Clone(model);
                                 }));
                             },
                             b =>
                             {
                                 b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> prevModel) =>
                                 {
                                     var newModel = b.Get(response, x => x.Model);
                                     b.Set(newModel, x => x.IsLoading, b.Get(prevModel, x => x.IsLoading));
                                     b.Set(newModel, x => x.DeleteError, b.Get(prevModel, x => x.DeleteError));
                                     b.Set(newModel, x => x.ToDeleteBinaries, b.Get(prevModel, x => x.ToDeleteBinaries));
                                     
                                     b.Foreach(
                                         b.Get(newModel, x => x.Binaries),
                                         (b, item) =>
                                         {
                                             var toDelete = b.SameBinary(b.Get(prevModel, x => x.ToDeleteBinaries), item);
                                             b.If(
                                                 b.HasObject(toDelete),
                                                 b =>
                                                 {
                                                     b.Set(item, x => x.Selected, true);
                                                 });
                                         });
                                     return b.If(
                                         b.Not(
                                             b.HasValue(b.Get(newModel, x => x.DeleteError))),
                                         b =>
                                         {
                                             b.HideDialog(b.Const(DeleteSelectedDialogId));
                                             return b.HideLoading(newModel);
                                         },
                                         b =>
                                         {
                                             return newModel;
                                         });
                                 }));
                             });
                     }),
                     b.Def((SyntaxBuilder b, Var<ClientSideException> error) =>
                     {
                         // Actual error, network or something

                         b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<ListProjectsPage> model) =>
                         {
                             b.Set(model, x => x.DeleteError, b.Get(error, x => x.message));
                             return b.Clone(model);
                         }));
                     }));
            });
        }

        public static void MarkRemovedInModel(this SyntaxBuilder b, Var<ListProjectsPage> model, Var<RemoveBuildsResponse> result)
        {
            b.Foreach(
                b.Get(result, x => x.Removed),
                (b, item) =>
                {
                    var removedInModel = b.SameBinary(b.Get(model, x => x.ToDeleteBinaries), item);

                    b.If(
                        b.HasObject(removedInModel),
                        b =>
                        {
                            b.Set(removedInModel, x => x.Removed, true);
                        });
                });
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
                                   var value = b.GetProperty<string>(b.Get(args, x => x.item), "value");
                                   return b.Switch(
                                       value,
                                       b => model,
                                       ("select-filtered", b =>
                                       {
                                           var filtered = b.FilterList(b.Get(model, x => x.Binaries.Where(x => !x.IsInUse).ToList()), b.Get(model, x => x.SearchKeyword));
                                           b.Foreach(filtered, (b, item) => b.Set(item, x => x.Selected, true));
                                           return b.Clone(model);
                                       }),
                                       ("remove-selected", b=>
                                       {
                                           b.Set(model, x => x.SearchKeyword, b.Const(string.Empty));

                                           var selected = b.Get(b.AllSortedBinaries(model), x => x.Where(x => x.Selected).ToList());
                                           var toDelete = b.NewCollection<BinariesRepositoryEntry>();
                                           b.Foreach(selected, (b, item) =>
                                           {
                                               b.Push(toDelete, b.Clone(item));
                                           });
                                           b.Set(model, x => x.ToDeleteBinaries, toDelete);
                                           b.Set(model, x => x.DeleteError, string.Empty);
                                           b.Set(model, x => x.IsLoading, false);

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

        public static Var<List<BinariesRepositoryEntry>> AllSortedBinaries(this SyntaxBuilder b, Var<ListProjectsPage> model)
        {
            return b.Get(model, x => x.Binaries.OrderByDescending(x => x.BuildNumber).ToList());
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
                        b =>
                        {
                            b.SetClass("flex flex-row items-center justify-center accent-sky-600");
                        },
                        b.HtmlCheckbox(
                            b =>
                            {
                                b.SetChecked(b.Get(entry, x => x.Selected));
                                b.If(
                                    b.Get(entry, x=>x.IsInUse),
                                    b =>
                                    {
                                        b.SetAttribute("disabled");
                                    });
                                b.OnClickAction((SyntaxBuilder b, Var<ListProjectsPage> model, Var<DomEvent> args) =>
                                {
                                    var isChecked = b.GetProperty<bool>(b.Get(args, x => x.target), "checked");
                                    b.Set(entry, x => x.Selected, isChecked);
                                    return b.Clone(model);
                                });
                            }));
                });

            tableBuilder.OverrideDataCell(
                nameof(BinariesRepositoryEntry.ProjectVersion),
                (b, entry) =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-row gap-2");
                        },
                        b.Text(b.Get(entry, x => x.ProjectVersion)),
                        b.Optional(
                            b.Get(entry, x => x.IsInUse),
                            b => b.Badge(b.Const("in use"), b.Const("bg-green-500"))));
                });

            var filtered = b.FilterList(b.Get(model, x => x.Binaries), b.Get(model, x => x.SearchKeyword));
            var sorted = b.Get(filtered, x => x.OrderByDescending(x => x.BuildNumber).ToList());

            return b.DataTable(tableBuilder, sorted,
                nameof(BinariesRepositoryEntry.Selected),
                nameof(BinariesRepositoryEntry.ProjectName),
                nameof(BinariesRepositoryEntry.ProjectVersion),
                nameof(BinariesRepositoryEntry.Target),
                nameof(BinariesRepositoryEntry.BuildNumber));

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

        public static void AsyncForeach<TSyntaxBuilder, T>(
            this TSyntaxBuilder b,
            Var<List<T>> collection, 
            System.Action<TSyntaxBuilder, Var<T>, Var<System.Action>> action,
            System.Action<TSyntaxBuilder> whenDone)
            where TSyntaxBuilder : SyntaxBuilder, new()
        {
            b.CallExternal("MdsInfrastructure.Render", "AsyncForeach", collection, b.Def(action), b.Def(whenDone));
        }

        //public static void AsyncActions<TSyntaxBuilder>(
        //    this SyntaxBuilder b,
        //    params System.Action<TSyntaxBuilder, Var<System.Action>>[] actions)
        //    where TSyntaxBuilder : SyntaxBuilder, new()
        //{
        //    b.CallExternal("MdsInfrastructure.Render", "AsyncActions", b.List(actions.Select(x => b.Def(x))));
        //}
    }
}