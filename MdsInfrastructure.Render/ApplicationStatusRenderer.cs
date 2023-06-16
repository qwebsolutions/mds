using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using MdsCommon;
using System.Linq;

namespace MdsInfrastructure.Render
{
    public class ApplicationStatusPage : MixedHyperPage<ApplicationStatus, ApplicationStatus>
    {
        public override ApplicationStatus ExtractDataModel(ApplicationStatus serverData)
        {
            return serverData;
        }

        public override Var<HyperNode> OnRender(BlockBuilder b, Var<ApplicationStatus> clientModel, ApplicationStatus serverData)
        {
            b.AddStylesheet("/static/tw.css");

            return b.Layout(
                b.InfraMenu(nameof(Routes.Status), serverData.InfrastructureStatus.User.IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title()
                    {
                        Operation = "Application status",
                        Entity = serverData.ApplicationName
                    },
                    User = serverData.InfrastructureStatus.User,
                })),
                Render(
                    b,
                    serverData.InfrastructureStatus,
                    serverData.ApplicationName));
        }


        public Var<HyperNode> Render(
            BlockBuilder b,
            InfrastructureStatus applicationStatusPage,
            string selectedApplicationName)
        {
            var page = b.Div("flex flex-col space-y-4");
            var selectedApplication = applicationStatusPage.InfrastructureConfiguration.Applications.Single(x => x.Name == selectedApplicationName);

            b.RenderApplicationPanel<InfrastructureStatus, InfrastructureStatus>(
                applicationStatusPage.Deployment,
                applicationStatusPage.HealthStatus,
                applicationStatusPage.InfrastructureEvents,
                selectedApplication.Name);

            var servicesGroup = b.Add(page, b.PanelsContainer(4));

            foreach (var service in applicationStatusPage.Deployment.GetDeployedServices().Where(x => x.ApplicationName == selectedApplication.Name))
            {
                b.Add(servicesGroup, b.RenderServicePanel(
                    applicationStatusPage.Deployment,
                    applicationStatusPage.HealthStatus,
                    service,
                    applicationStatusPage.InfrastructureEvents));
            }

            return page;
        }
    }
}
