using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static void ApplyApplicationsFilter(this BlockBuilder b, Var<EditConfigurationPage> clientModel)
        {
            b.Set(
                 clientModel,
                 x => x.FilteredApplications,
                 b.ContainingText(
                     b.Get(
                         clientModel,
                         x => x.Configuration.Applications),
                     b.Get(clientModel, x => x.ApplicationsFilterValue)));
        }
        public static Var<HyperNode> TabApplications(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {

            var configId = b.Get(clientModel, x => x.Configuration.Id);

            var addApplication = b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> state) =>
            {
                var newId = b.NewId();

                var newApp = b.NewObj<Application>(b =>
                {
                    b.Set(x => x.ConfigurationHeaderId, configId);
                    b.Set(x => x.Id, newId);
                    b.Set(x => x.Name, string.Empty);
                });
                b.Push(b.Get(clientModel, x => x.Configuration.Applications), newApp);
                b.Push(b.Get(clientModel, x => x.FilteredApplications), newApp);

                return b.GoTo(state, EditApplication, newId);
            });

            var rows = b.Get(clientModel, x => x.FilteredApplications.OrderBy(x => x.Name).ToList());
          
            var rc = b.Def((BlockBuilder b, Var<Application> app, Var<DataTable.Column> col) =>
            {
                var name = b.Get(app, x => x.Name, "(not set)");
                return b.VPadded4(b.Link(name, b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> clientModel) => b.GoTo(clientModel, EditApplication, app))));
            });

            return b.DataGrid<Application>(
                new()
                {
                    b=> b.CommandButton<EditConfigurationPage>(b=>
                    {
                        b.Set(x=>x.Label, "Add application");
                        b.Set(x => x.OnClick, addApplication);
                    }),
                      b => b.Filter<EditConfigurationPage>(
                                clientModel,
                                x => x.ApplicationsFilterValue,
                                ApplyApplicationsFilter,
                                "")
                },
                (b) =>
                {
                    b.AddColumn("AppName", "Application name");
                    b.SetRows(rows);
                    b.SetRenderCell(rc);
                },
                (b, actions, application) =>
                {
                    var applicationId = b.Get(application, x => x.Id);

                    var onCommand = b.Def((BlockBuilder b, Var<Application> application) =>
                    {
                        var removed = b.Get(clientModel, application, (x, application) => x.Configuration.Applications.Where(x => x != application).ToList());
                        b.Set(b.Get(clientModel, x => x.Configuration), x => x.Applications, removed);
                    });

                    var removeIcon = Icon.Remove;

                    var isInUse = b.Get(clientModel, applicationId, (x, applicationId) => x.Configuration.InfrastructureServices.Any(x => x.ApplicationId == applicationId));
                    b.If(b.Not(isInUse), b =>
                    {
                        b.Modify(actions, b => b.Commands,
                            b =>
                            {
                                b.Add(b =>
                                {
                                    b.Set(x => x.IconHtml, removeIcon);
                                    b.Set(x => x.OnCommand, onCommand);
                                });
                            });
                    });
                });
        }
    }
}
