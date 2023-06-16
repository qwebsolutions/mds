//using Metapsi.Syntax;
//using System.Linq;
//using Metapsi;
//using System;
//using System.Threading.Tasks;
//using Metapsi.Hyperapp;
//using MdsCommon;
//using Microsoft.AspNetCore.Http;

//namespace MdsInfrastructure
//{
//    public static partial class Status
//    {
//        public static async Task<IResponse> Application(CommandContext commandContext, HttpContext requestData)
//        {
//            var pageData = await LoadStatus(commandContext);

//            //var healthStatusResult = await commandContext.Do(MdsInfrastructureApplication.LoadHealthStatus);

//            var applicationName = requestData.EntityId();

//            Guid selectedApplicationId = pageData.InfrastructureConfiguration.Applications.Single(x => x.Name == applicationName).Id;

//            return Page.Response(
//                pageData,
//                (b, clientModel) => b.Layout(b.InfraMenu(nameof(Status), requestData.User().IsSignedIn()),
//                b.Render(b.Const(new Header.Props()
//                {
//                    Main = new Header.Title() { Operation = "Application status", Entity = applicationName },
//                    User = requestData.User()
//                })), b.Render(pageData, selectedApplicationId)));
//        }


//        public static Var<HyperNode> Render(
//            this BlockBuilder b, 
//            InfrastructureStatus applicationStatusPage,
//            System.Guid selectedApplicationId)
//        {
//            var page = b.Div("flex flex-col space-y-4");
//            var selectedApplication = applicationStatusPage.InfrastructureConfiguration.Applications.Single(x => x.Id == selectedApplicationId);

//            b.RenderApplicationPanel<InfrastructureStatus, InfrastructureStatus>(
//                applicationStatusPage.Deployment,
//                applicationStatusPage.HealthStatus,
//                applicationStatusPage.InfrastructureEvents,
//                selectedApplication.Name);

//            var servicesGroup = b.Add(page, b.PanelsContainer(4));

//            foreach (var service in applicationStatusPage.Deployment.GetDeployedServices().Where(x => x.ApplicationName == selectedApplication.Name))
//            {
//                b.Add(servicesGroup, b.RenderServicePanel(
//                    applicationStatusPage.Deployment,
//                    applicationStatusPage.HealthStatus,
//                    service,
//                    applicationStatusPage.InfrastructureEvents));
//            }

//            return page;
//        }
//    }
//}

