using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static class Project
    {
        public class List : MixedHyperPage<MdsInfrastructure.ListProjectsPage, MdsInfrastructure.ListProjectsPage>
        {
            public override ListProjectsPage ExtractClientModel(ListProjectsPage serverModel)
            {
                return serverModel;
            }

            public override Var<HyperNode> OnRender(BlockBuilder b, ListProjectsPage serverModel, Var<ListProjectsPage> clientModel)
            {
                return b.Layout(
                    b.InfraMenu(nameof(Routes.Project), serverModel.User.IsSignedIn()),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Projects" },
                        User = serverModel.User
                    })), b.Render(clientModel));
            }
        }

        //public static async Task<ProjectVersion> SaveVersionEnabled(CommandContext commandContext, ProjectVersion version)
        //{
        //    await commandContext.Do(Api.SaveVersionEnabled, version);
        //    return version;
        //}

        public static Var<string> TargetEnvironment(this BlockBuilder b, Var<string> target)
        {
            return b.If(b.AreEqual(target, b.Const("linux-x64")), b => b.Const("Linux"), b => b.Const("Windows"));
        }

        public static Var<HyperNode> Render(this BlockBuilder b, Var<ListProjectsPage> clientModel)
        {
            var view = b.Div();
            b.Add(view, b.ValidationPanel(clientModel));

            b.Add(view, b.SidePanel<ListProjectsPage>(
                clientModel,
                b =>
                {
                    b.Log("REndering side panel");

                    var project = b.Get(clientModel, x => x.SelectedProject);
                    var projectId = b.Get(project, x => x.Id);
                    var versions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).OrderByDescending(x => x.VersionTag).ToList());

                    var container = b.Div("flex flex-col space-y-4");

                    b.Add(container, b.Text((b.Get(project, x => x.Name))));

                    b.Foreach(versions, (b, version) =>
                    {
                        var versionId = b.Get(version, x => x.Id);
                        var isInUse = b.Get(clientModel, versionId, (x, versionId) => x.InfrastructureServices.Any(x => x.ProjectVersionId == versionId));

                        var versionBuilds = b.Get(version, x => x.Binaries);
                        var buildDescriptions = b.Get(versionBuilds, b.Def<string, string>(TargetEnvironment), (version, getTarget) => version.Select(x => "Build " + x.BuildNumber + " " + getTarget(x.Target)).ToList());

                        var checkboxText = b.Get(
                            version,
                            b.Def<string, System.Collections.Generic.List<string>, string>(Native.JoinStrings),
                            buildDescriptions,
                            (version, join, buildDescriptions) => version.VersionTag + "(" + join(", ", buildDescriptions) + ")");

                        var toggleContainer = b.Add(container, b.Div("flex flex-row"));

                        b.Add(toggleContainer,
                            b.Toggle(
                                b.Get(version, x => x.Enabled),
                                b.MakeAction((BlockBuilder b, Var<ListProjectsPage> state, Var<bool> isChecked) =>
                                {
                                    var initialVersion = b.Clone(version);
                                    b.ShowLoading(state);
                                    b.Set(version, x => x.Enabled, isChecked);
                                    return b.AsyncResult(
                                        b.Clone(state),
                                        b.CallApi<ListProjectsPage, ProjectVersion>(
                                            Backend.SaveVersionEnabled,
                                            version,
                                            (BlockBuilder b, Var<ListProjectsPage> state) => b.HideLoading(state),
                                            (BlockBuilder b, Var<ListProjectsPage> state, Var<ApiError> error) =>
                                            {
                                                var allVersions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).ToList());
                                                var versionReference = b.Get(allVersions, b.Get(initialVersion, x => x.Id), (allVersions, versionid) => allVersions.Single(x => x.Id == versionid));
                                                b.Set(versionReference, x => x.Enabled, b.Get(initialVersion, x => x.Enabled));
                                                b.HideLoading(state);
                                                return b.Clone(state);
                                            }));

                                }),
                                    //b.AsyncCommand<ListProjectsPage, bool, ProjectVersion>(
                                    //    b.Def((BlockBuilder b, Var<ListProjectsPage> clientModel, Var<bool> isChecked) =>
                                    //    {
                                    //        b.ShowLoading();
                                    //        b.Set(version, x => x.Enabled, isChecked);
                                    //        return clientModel;
                                    //    }),
                                    //    b.Def((BlockBuilder b, Var<ListProjectsPage> clientModel, Var<bool> isChecked, Var<System.Action<ProjectVersion>> onDone) =>
                                    //    {
                                    //        b.CallApi(Api.SaveVersionEnabled, version, b.Def(b => { b.Call(onDone, version); }));
                                    //        //throw new System.NotImplementedException("API calls are different now");
                                    //        //b.PostData(b.Url(SaveVersionEnabled), version, onDone, b.Def((BlockBuilder b, Var<object> error) =>
                                    //        //{
                                    //        //    b.HideLoading();
                                    //        //    b.SetValidationMessage(b.AsString(error));
                                    //        //    b.Call(onDone, version);
                                    //        //}));
                                    //    }),
                                    //    b.Def((BlockBuilder b, Var<ListProjectsPage> clientModel, Var<ProjectVersion> newData) =>
                                    //    {
                                    //        b.HideLoading();
                                    //        return b.Clone(clientModel);
                                    //    })),
                                    checkboxText,
                                    checkboxText,
                                    b =>
                                    {
                                        b.Set(x => x.Enabled, b.Not(isInUse));
                                    }));

                        b.If(isInUse, b =>
                        {
                            b.Add(toggleContainer, b.AddClass(b.Badge(b.Const("in use")), "bg-green-500"));
                        });
                    });

                    return container;
                }));

            var renderCell = b.RenderCell<MdsCommon.Project>((b, row, col) =>
            {
                var projectId = b.Get(row, x => x.Id);
                var versions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).Count());

                return b.VPadded4(b.If(b.AreEqual(b.Get(col, x => x.Name), b.Const(nameof(MdsCommon.Project.Name))),
                    b => b.Link<ListProjectsPage>(b.Get(row, x => x.Name), b.MakeAction((BlockBuilder b, Var<ListProjectsPage> state) =>
                    {
                        b.Log("show side panel", state);
                        b.Set(state, x => x.SelectedProject, row);
                        return b.ShowSidePanel(state);
                    })),
                    b => b.Text(b.AsString(versions))));
            });

            var projectRows = b.Get(clientModel, x => x.ProjectsList.OrderBy(x => x.Name).ToList());

            var props = b.NewObj<DataTable.Props<MdsCommon.Project>>(b =>
            {

                b.AddColumn(nameof(MdsCommon.Project.Name), "Project");
                b.AddColumn("versions", "Versions");
                b.SetRows(projectRows);
                b.SetRenderCell<MdsCommon.Project>(renderCell);
            });
            var dataTable = b.Add(view, b.DataTable(props));
            b.AddClass(dataTable, "drop-shadow");
            return view;
        }
    }
}