using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

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

            public override Var<IVNode> OnRender(LayoutBuilder b, ListProjectsPage serverModel, Var<ListProjectsPage> clientModel)
            {
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

        //public static async Task<ProjectVersion> SaveVersionEnabled(CommandContext commandContext, ProjectVersion version)
        //{
        //    await commandContext.Do(Api.SaveVersionEnabled, version);
        //    return version;
        //}

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

            return b.HtmlDiv(
                b =>
                {
                    // The side panel must not be inside the main panel
                    // because it's FIXED position gets broken by...
                    // ... the parent DROP SHADOW!
                },
                sidePanel,
                b.MdsMainPanel(
                    b =>
                    {
                    },
                    b.ValidationPanel(clientModel),
                    b.DataTable(
                        projectTableBuilder,
                        b.Get(clientModel, x => x.ProjectsList),
                        "Project name",
                        "Versions")));

            //var renderCell = b.RenderCell<MdsCommon.Project>((b, row, col) =>
            //{
            //    var projectId = b.Get(row, x => x.Id);
            //    var versions = b.Get(clientModel, projectId, (x, projectId) => x.ProjectsList.SelectMany(x => x.Versions).Where(x => x.ProjectId == projectId).Count());

            //    return b.VPadded4(
            //        b.If(
            //            b.AreEqual(
            //                b.Get(col, x => x.Name),
            //                b.Const(nameof(MdsCommon.Project.Name))),
            //            b =>
            //            b.HtmlA(
            //                b =>
            //                {
            //                    b.SetClass("underline text-sky-500");
            //                    b.SetHref(b.Const("javascript:void(0);"));
            //                    b.OnClickAction((SyntaxBuilder b, Var<ListProjectsPage> state) =>
            //                    {
            //                        b.Set(state, x => x.SelectedProject, row);
            //                        return b.ShowSidePanel(state);
            //                    });
            //                },
            //                b.TextSpan(b.Get(row, x => x.Name))),
            //            b => b.TextSpan(b.AsString(versions))));
            //});

            //var projectRows = b.Get(clientModel, x => x.ProjectsList.OrderBy(x => x.Name).ToList());

            //var props = b.NewObj<DataTable.Props<MdsCommon.Project>>(b =>
            //{

            //    b.AddColumn(nameof(MdsCommon.Project.Name), "Project");
            //    b.AddColumn("versions", "Versions");
            //    b.SetRows(projectRows);
            //    b.SetRenderCell<MdsCommon.Project>(renderCell);
            //});

            //return b.HtmlDiv(
            //    b => { },
            //    b.ValidationPanel(clientModel),
            //    sidePanel,
            //    b.DataTable(props, b =>
            //    {
            //        b.AddClass("drop-shadow");
            //    }));

        }
    }
}