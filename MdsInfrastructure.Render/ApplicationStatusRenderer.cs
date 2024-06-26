﻿using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public class ApplicationStatus : MixedHyperPage<MdsInfrastructure.ApplicationStatus, MdsInfrastructure.ApplicationStatus>
    {
        public override MdsInfrastructure.ApplicationStatus ExtractClientModel(MdsInfrastructure.ApplicationStatus serverData)
        {
            return serverData;
        }

        public override Var<IVNode> OnRender(LayoutBuilder b, MdsInfrastructure.ApplicationStatus serverData, Var<MdsInfrastructure.ApplicationStatus> clientModel)
        {
            b.AddModuleStylesheet();

            return b.Layout(
                b.InfraMenu(nameof(Routes.Status), serverData.InfrastructureStatus.User.IsSignedIn()),
                b.Render(
                    b.GetHeaderProps(
                        b.Const("Application status"),
                        b.Const(serverData.ApplicationName),
                        b.Get(clientModel, x=>x.InfrastructureStatus.User))),
                Render(
                    b,
                    serverData.InfrastructureStatus,
                    serverData.ApplicationName)).As<IVNode>();
        }


        public Var<IVNode> Render(
            LayoutBuilder b,
            MdsInfrastructure.InfrastructureStatus applicationStatusPage,
            string selectedApplicationName)
        {
            var selectedApplication = applicationStatusPage.InfrastructureConfiguration.Applications.Single(x => x.Name == selectedApplicationName);

            //var applicationPanel = b.RenderApplicationPanel<MdsInfrastructure.InfrastructureStatus, MdsInfrastructure.InfrastructureStatus>(
            //    applicationStatusPage.Deployment,
            //    applicationStatusPage.HealthStatus,
            //    applicationStatusPage.InfrastructureEvents,
            //    selectedApplication.Name);

            //var servicesGroup = b.Add(page, b.PanelsContainer(4));

            //foreach (var service in applicationStatusPage.Deployment.GetDeployedServices().Where(x => x.ApplicationName == selectedApplication.Name))
            //{
            //    b.Add(servicesGroup, b.RenderServicePanel(
            //        applicationStatusPage.Deployment,
            //        applicationStatusPage.HealthStatus,
            //        service,
            //        applicationStatusPage.InfrastructureEvents));
            //}

            return b.HtmlDiv(
                b =>
                {
                    b.AddClass("flex flex-col space-y-4");
                },
                b.PanelsContainer(
                    4,
                    applicationStatusPage.Deployment.GetDeployedServices().Where(x => x.ApplicationName == selectedApplication.Name).Select(
                        service => b.RenderServicePanel(
                            applicationStatusPage.Deployment,
                            applicationStatusPage.HealthStatus,
                            service,
                            applicationStatusPage.InfrastructureEvents))
                    ));


        }
    }
}
