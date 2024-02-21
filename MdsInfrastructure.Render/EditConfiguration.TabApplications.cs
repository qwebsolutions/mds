using MdsCommon;
using MdsCommon.Controls;
using MdsCommon.HtmlControls;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> TabApplications(
           LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var container = b.Div("w-full h-full");

            var configId = b.Get(clientModel, x => x.Configuration.Id);

            var onRemoveCommand = (SyntaxBuilder b, Var<EditConfigurationPage> model, Var<Application> application) =>
            {
                var removed = b.Get(model, application, (x, application) => x.Configuration.Applications.Where(x => x != application).ToList());
                b.Set(b.Get(model, x => x.Configuration), x => x.Applications, removed);
                return b.Clone(model);
            };

            var allApplications = b.Get(clientModel, x => x.Configuration.Applications.OrderBy(x => x.Name).ToList());
            var filteredApplications = b.FilterList<Application>(allApplications, b.Get(clientModel, x => x.ApplicationsFilter));

            b.OnModel(
                clientModel,
                (bParent, context) =>
                {
                    var b = new LayoutBuilder(bParent);

                    b.Add(container, b.DataGrid<Application>(
                        b =>
                        {
                            b.OnTable(b =>
                            {
                                b.FillFrom(filteredApplications, exceptColumns: new()
                                {
                                    nameof(Application.Id),
                                    nameof(Application.ConfigurationHeaderId),
                                });

                                b.SetCommonStyle();

                                b.OverrideColumnCell(
                                    nameof(Application.Name),
                                    (b, data) => b.RenderApplicationNameCell(b.Get(data, x => x.Row)));

                            });

                            b.AddHoverRowAction(onRemoveCommand, Icon.Remove, (b, data, props) =>
                            {
                                b.AddClass(props, "text-red-500");
                            },
                            visible: (b, row) =>
                            {
                                var isInUse = b.Get(
                                    clientModel,
                                    b.Get(row, x => x.Id),
                                    (x, applicationId) =>
                                    x.Configuration.InfrastructureServices.Any(x => x.ApplicationId == applicationId));
                                return b.Not(isInUse);
                            });

                            b.AddToolbarChild(AddApplicationButton);

                            b.AddToolbarChild(
                                b => b.Filter(
                                    b =>
                                    {
                                        b.BindFilter(context, x => x.ApplicationsFilter);
                                    }),
                                HorizontalPlacement.Right);

                        }).As<HyperNode>());
                });

            return container;
        }

        public static Var<IVNode> RenderApplicationNameCell(this LayoutBuilder b, Var<Application> row)
        {
            var applicationName = b.Get(row, x => x.Name);

            var goToApplication = (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                b.Set(clientModel, x => x.EditApplicationId, b.Get(row, x => x.Id));
                return b.EditView<EditConfigurationPage>(clientModel, EditApplication);
            };

            var container = b.Span();
            b.Add(container, b.Link<EditConfigurationPage>(b.WithDefault(applicationName), b.MakeAction<EditConfigurationPage>(goToApplication)));
            return container.As<IVNode>();
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

        public static Var<IVNode> AddApplicationButton(this LayoutBuilder b)
        {
            return b.Render(ControlDefinition.New<object>(
                "button",
                (b, data, props) =>
                {
                    b.AddPrimaryButtonStyle(props);
                    b.OnClickAction<EditConfigurationPage>(props, OnAddApplication);
                },
                (b, data) => b.T("Add application")),
                b.Const(new object()));

        }
    }
}
