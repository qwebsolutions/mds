using MdsCommon;
using MdsCommon.Controls;
using MdsCommon.HtmlControls;
using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> TabApplications(
           LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            //var container = b.Div("w-full h-full");

            var configId = b.Get(clientModel, x => x.Configuration.Id);

            var onRemoveCommand = (SyntaxBuilder b, Var<EditConfigurationPage> model, Var<Application> application) =>
            {
                var removed = b.Get(model, application, (x, application) => x.Configuration.Applications.Where(x => x != application).ToList());
                b.Set(b.Get(model, x => x.Configuration), x => x.Applications, removed);
                return b.Clone(model);
            };

            var allApplications = b.Get(clientModel, x => x.Configuration.Applications.OrderBy(x => x.Name).ToList());
            var filteredApplications = b.FilterList<Application>(allApplications, b.Get(clientModel, x => x.ApplicationsFilter));

            var gridBuilder = MdsDefaultBuilder.DataGrid<Application>();

            gridBuilder.DataTableBuilder.CreateDataCell = (b, application, column) =>
            {
                return b.RenderApplicationNameCell(application);
            };

            gridBuilder.CreateToolbarActions = b =>
            {
                return b.HtmlDiv(b =>
                {
                    b.AddClass("flex flex-row items-center justify-between");
                },
                b.HtmlButton(
                    b =>
                    {
                        b.AddPrimaryButtonStyle();
                        b.OnClickAction<EditConfigurationPage, HtmlButton>(OnAddApplication);
                    },
                    b.T("Add application")),
                b.Filter(clientModel, x => x.ApplicationsFilter)
                );
            };

            gridBuilder.CreateRowActions = (b, row) =>
            {
                return b.HtmlButton(
                    b =>
                    {
                        b.SetClass("flex rounded bg-gray-200 w-10 h-10 p-1 cursor-pointer justify-center items-center opacity-50 hover:opacity-100");
                        b.SetInnerHtml(b.Const(Icon.Remove));
                        b.OnClickAction(
                            b.MakeActionDescriptor(
                                b.MakeAction(onRemoveCommand),
                                row));
                    });
            };

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("w-full h-full");
                },
                b.DataGrid(
                    gridBuilder,
                    filteredApplications,
                    b.Const(new System.Collections.Generic.List<string>() { nameof(Application.Name) })));

            //b.OnModel(
            //    clientModel,
            //    (bParent, context) =>
            //    {
            //        var b = new LayoutBuilder(bParent);

            //        b.Add(container, b.DataGrid<Application>(
            //            b =>
            //            {
            //                b.OnTable(b =>
            //                {
            //                    b.FillFrom(filteredApplications, exceptColumns: new()
            //                    {
            //                        nameof(Application.Id),
            //                        nameof(Application.ConfigurationHeaderId),
            //                    });

            //                    b.SetCommonStyle();

            //                    b.OverrideColumnCell(
            //                        nameof(Application.Name),
            //                        (b, data) => b.RenderApplicationNameCell(b.Get(data, x => x.Row)));

            //                });

            //                b.AddHoverRowAction(onRemoveCommand, Icon.Remove, (b, data, props) =>
            //                {
            //                    b.AddClass(props, "text-red-500");
            //                },
            //                visible: (b, row) =>
            //                {
            //                    var isInUse = b.Get(
            //                        clientModel,
            //                        b.Get(row, x => x.Id),
            //                        (x, applicationId) =>
            //                        x.Configuration.InfrastructureServices.Any(x => x.ApplicationId == applicationId));
            //                    return b.Not(isInUse);
            //                });

            //                b.AddToolbarChild(AddApplicationButton);

            //                b.AddToolbarChild(
            //                    b => b.Filter(
            //                        b =>
            //                        {
            //                            b.BindFilter(context, x => x.ApplicationsFilter);
            //                        }),
            //                    HorizontalPlacement.Right);

            //            }));
            //    });

            //return container;
        }

        public static Var<IVNode> RenderApplicationNameCell(this LayoutBuilder b, Var<Application> row)
        {
            var applicationName = b.Get(row, x => x.Name);

            var goToApplication = (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                b.Set(clientModel, x => x.EditApplicationId, b.Get(row, x => x.Id));
                return b.EditView<EditConfigurationPage>(clientModel, EditApplication);
            };

            return b.HtmlSpan(
                b =>
                {

                },
                b.Link<EditConfigurationPage>(b.WithDefault(applicationName), b.MakeAction<EditConfigurationPage>(goToApplication)));
        }

        public static Var<EditConfigurationPage> OnAddApplication(SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var configHeaderId = b.Get(clientModel, x => x.Configuration.Id);

            var newId = b.NewId();

            var newApp = b.NewObj<Application>(b =>
            {
                b.Set(x => x.ConfigurationHeaderId, configHeaderId);
                b.Set(x => x.Id, newId);
                b.Set(x => x.Name, string.Empty);
            });
            b.Push(b.Get(clientModel, x => x.Configuration.Applications), newApp);
            b.Set(clientModel, x => x.EditApplicationId, newId);
            return b.EditView<EditConfigurationPage>(clientModel, EditApplication);
        }

        //public static Var<IVNode> AddApplicationButton(this LayoutBuilder b)
        //{
        //    return b.Render(ControlDefinition.New<object>(
        //        "button",
        //        (b, data, props) =>
        //        {
        //            b.AddPrimaryButtonStyle(props);
        //            b.OnClickAction<EditConfigurationPage>(props, OnAddApplication);
        //        },
        //        (b, data) => b.TextSpan("Add application")),
        //        b.Const(new object()));

        //}
    }
}
