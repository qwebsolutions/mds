using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;
using Metapsi.SignalR;

namespace MdsInfrastructure.Render
{
    public static class InfrastructureStatus
    {
        public static void Render(this HtmlBuilder b, MdsInfrastructure.InfrastructureStatus serverModel)
        {
            b.BodyAppend(b.DeploymentToasts());
            b.BodyAppend(
                b.Hyperapp(
                    (SyntaxBuilder b) => b.MakeInit(
                        b.MakeStateWithEffects(
                            b.Const(serverModel),
                            b.SignalRConnect())),
                    (LayoutBuilder b, Var<MdsInfrastructure.InfrastructureStatus> model) =>
                    {
                        return OnRender(b, serverModel, model);
                    },
                    (b, model) => b.Listen(b.MakeAction((SyntaxBuilder b, Var<MdsInfrastructure.InfrastructureStatus> model, Var<DeploymentEvent.DeploymentStart> e) =>
                    {
                        b.ShowDeploymentToast(MdsCommon.Controls.Controls.IdDeploymentStartedToast);
                        return model;
                    })),
                    (b, model) => b.Listen(b.MakeAction((SyntaxBuilder b, Var<MdsInfrastructure.InfrastructureStatus> model, Var<DeploymentEvent.DeploymentComplete> e) =>
                    {
                        b.ShowDeploymentToast(MdsCommon.Controls.Controls.IdDeploymentSuccessToast);
                        return b.MakeStateWithEffects(
                            model);
                    })),
                    (b, model) => b.Listen(b.MakeAction((SyntaxBuilder b, Var<MdsInfrastructure.InfrastructureStatus> model, Var<RefreshInfrastructureStatusModel> e) =>
                    {
                        return b.MakeStateWithEffects(model, b.RefreshModelEffect<MdsInfrastructure.InfrastructureStatus>());
                    }))));
        }

        public static Var<IVNode> OnRender(LayoutBuilder b, MdsInfrastructure.InfrastructureStatus serverModel, Var<MdsInfrastructure.InfrastructureStatus> clientModel)
        {
            b.AddModuleStylesheet();

            var headerProps = b.GetHeaderProps(b.Const("Infrastructure status"), b.Const(string.Empty), b.Get(clientModel, x => x.User));

            return b.Layout(
                b.InfraMenu(nameof(Routes.Status), serverModel.User.IsSignedIn()),
                b.Render(headerProps),
                RenderContent(b, serverModel, clientModel));
        }

        public static Var<string> FormatDistanceAgo(this SyntaxBuilder b, Var<string> date, Var<string> language)
        {
            StaticFiles.Add(typeof(MdsInfrastructure.Render.InfrastructureStatus).Assembly, "dateFns.js");
            var parsedDate = b.ParseDate(date);
            b.Log("FormatDistanceAgo language", language);
            return b.Concat(b.CallExternal<string>("dateFns", "formatDistanceToNow", parsedDate), b.Const(" ago"));
        }

        public static Var<IVNode> RenderContent(LayoutBuilder b, MdsInfrastructure.InfrastructureStatus serverModel, Var<MdsInfrastructure.InfrastructureStatus> clientModel)
        {

            if (!string.IsNullOrEmpty(serverModel.InfrastructureStatusData.SchemaValidationMessage))
            {
                return b.StyledDiv("text-red-500", b.Bold(serverModel.InfrastructureStatusData.SchemaValidationMessage));
            }

            if (serverModel.InfrastructureStatusData.Deployment == null)
            {
                return b.TextSpan("No deployment yet! The infrastructure is not running any service!");
            }

            var totalServices = b.Get(b.GetDeployedServices(clientModel), x => x.Count());
            var totalNodes = b.Get(b.GetDeployedServices(clientModel), x => x.Select(x => x.NodeName).Distinct().Count());

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col space-y-4");
                },
                b.InfoPanel(
                    Panel.Style.Info,
                    b.Concat(b.Const("Last deployment: "), b.Get(clientModel, x => x.InfrastructureStatusData.Deployment.ConfigurationName)),
                    b.Concat(b.ItalianFormat(b.Get(clientModel, x => x.InfrastructureStatusData.Deployment.Timestamp)), b.Const(" total services "), b.AsString(totalServices), b.Const(" total infrastructure nodes "), b.AsString(totalNodes))),
                b.PanelsContainer(
                    4,
                    b.Map(
                        b.Get(clientModel, x => x.NodePanels),
                        (b, panelData) =>
                        b.HtmlA(
                            b =>
                            {
                                b.SetHref(b.Url<Routes.Status.Node, string>(b.Get(panelData, x => x.NodeName)));
                            },
                            b.NodePanel(panelData)))),
                b.PanelsContainer(
                    4,
                    b.Map(
                        b.Get(clientModel, x => x.ApplicationPanels),
                        (b, panelData) =>
                        b.HtmlA(
                            b =>
                            {
                                b.SetHref(b.Url<Routes.Status.Application, string>(b.Get(panelData, x => x.ApplicationName)));
                            },
                            b.ApplicationPanel(panelData)))));
        }
    }
}
