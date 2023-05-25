using MdsCommon;
using Metapsi;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    //    public enum DeploymentType
    //    {
    //        FromConfiguration,
    //        Rollback
    //    }

    //    public class ListDeploymentsPage : IPage
    //    {
    //        public Page Page { get; set; } = new Page();
    //        public DeploymentHistory DeploymentsHistory { get; set; }

    //    }

    //public class ViewDeploymentReportPage
    //{
    //    public Deployment Deployment { get; set; }
    //    public UpgradeReport UpgradeReport { get; set; }
    //}

    //public class ViewChangesReportPage
    //{
    //    public Deployment Deployment { get; set; }
    //    public UpgradeReport UpgradeReport { get; set; }
    //    public InfrastructureConfiguration InfrastructureConfiguration { get; set; }
    //}

    public static partial class MdsInfrastructureFunctions
    {
        //        public const string ReportType = "ReportType";
        //        public const string ShowJustDiff = "JustDiff";
        //        public const string ShowAll = "ShowAll";

        //        public static async Task<ListDeploymentsPage> Deployments(CommandContext commandContext)
        //        {
        //            var deploymentsPage = new ListDeploymentsPage();
        //            deploymentsPage.DeploymentsHistory = await commandContext.Do(MdsInfrastructureApplication.LoadDeploymentsHistory);
        //            return deploymentsPage;
        //        }

        //        public static UI.Svelte.View RenderListDeploymentsPage(ListDeploymentsPage dataModel, UI.Svelte.ViewBuilder viewBuilder)
        //        {
        //            viewBuilder.InPlaceSelectionGrid<ListDeploymentsPage, Record.Deployment, ViewDeploymentReportPage>(
        //                "grdListDeployments",
        //                Guid.Empty,
        //                dataModel,
        //                UI.Svelte.ViewBuilder.RootGroupId,
        //                (pageData, _) => pageData.DeploymentsHistory.Deployments,
        //                GetListDeploymentsColumns,
        //                GetDeploymentCellValue,
        //                string.Empty,
        //                null,
        //                async (commandContext, dataModel, id) =>
        //                {
        //                    dataModel.RegisterSelectionId<Deployment>();
        //                    var selectedDeployment = dataModel.GetSelected(dataModel.DeploymentsHistory.Deployments);

        //                    bool isActiveDeployment = dataModel.DeploymentsHistory.Deployments.OrderByDescending(x => x.Timestamp).First().Id == selectedDeployment.Id;

        //                    var deployment = await commandContext.Do(MdsInfrastructureApplication.LoadDeploymentById, selectedDeployment.Id);

        //                    ViewDeploymentReportPage viewDeploymentReportPage = new ViewDeploymentReportPage()
        //                    {
        //                        Deployment = deployment,
        //                        Page = dataModel.Page,
        //                        UpgradeReport = GetUpgradeReport(deployment, dataModel.GetString(ReportType) == ShowJustDiff)
        //                    };

        //                    viewDeploymentReportPage.SetValue("IsActive", isActiveDeployment);

        //                    return viewDeploymentReportPage;
        //                });

        //            viewBuilder.OutputView.HeaderText = "Deployments history";

        //            return viewBuilder.OutputView;
        //        }

        //        public static List<CaptionMapping> GetListDeploymentsColumns()
        //        {
        //            List<CaptionMapping> captionMappings = new List<CaptionMapping>();

        //            captionMappings.Add(new CaptionMapping()
        //            {
        //                CaptionText = "Deployment timestamp",
        //                FieldName = nameof(Record.Deployment.Timestamp)
        //            });

        //            captionMappings.Add(new CaptionMapping()
        //            {
        //                CaptionText = "Configuration name",
        //                FieldName = nameof(Record.Deployment.ConfigurationName)
        //            });

        //            return captionMappings;
        //        }

        //        public static string GetDeploymentCellValue(ListDeploymentsPage dataModel, Record.Deployment deployment, string fieldName)
        //        {
        //            if (fieldName == nameof(Record.Deployment.Timestamp))
        //                return deployment.Timestamp.ToString("G", new System.Globalization.CultureInfo("it-IT"));

        //            if (fieldName == nameof(Record.Deployment.ConfigurationName))
        //                return deployment.ConfigurationName;

        //            return string.Empty;
        //        }


    //    public static UpgradeReport GetUpgradeReport(Deployment deployment, bool justChanges)
    //{
    //    UpgradeReport upgradeReport = new UpgradeReport();

    //    //upgradeReport.ReportStatusVariables.Add(new NamedValue() { Name = "HasChanges", Value = "false" });

    //    foreach (var sameConfiguration in deployment.Transitions.Where(x => x.FromServiceConfigurationSnapshotId == x.ToServiceConfigurationSnapshotId))
    //    {
                
    //            var serviceConfiguration = deployment.GetDeployedServices().SingleOrDefault(x => x.Id == sameConfiguration.ToServiceConfigurationSnapshotId);
    //        string serviceName = serviceConfiguration.ServiceName;

    //        upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //        {
    //            ServiceName = serviceName,
    //            PropertyName = "Service",
    //            NewValue = "(service not changed)"
    //        });

    //        if (!justChanges)
    //        {
    //            upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //            {
    //                ServiceName = serviceName,
    //                PropertyName = "Application",
    //                OldValue = serviceConfiguration.ApplicationName,
    //                NewValue = serviceConfiguration.ApplicationName
    //            });

    //            upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //            {
    //                ServiceName = serviceName,
    //                PropertyName = "Node",
    //                OldValue = serviceConfiguration.NodeName,
    //                NewValue = serviceConfiguration.NodeName
    //            });

    //            upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //            {
    //                ServiceName = serviceName,
    //                PropertyName = "Project",
    //                OldValue = serviceConfiguration.ProjectName,
    //                NewValue = serviceConfiguration.ProjectName
    //            });
    //            upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //            {
    //                ServiceName = serviceName,
    //                PropertyName = "Version",
    //                OldValue = serviceConfiguration.ProjectVersionTag,
    //                NewValue = serviceConfiguration.ProjectVersionTag
    //            });

    //            foreach (var parameter in serviceConfiguration.ServiceConfigurationSnapshotParameters)
    //            {
    //                upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //                {
    //                    ServiceName = serviceName,
    //                    PropertyName = parameter.ParameterName,
    //                    OldValue = parameter.DeployedValue,
    //                    NewValue = parameter.DeployedValue
    //                });
    //            }
    //        }
    //    }

    //    foreach (var upgradeTransition in deployment.Transitions.Where(x => x.FromServiceConfigurationSnapshotId != x.ToServiceConfigurationSnapshotId))
    //    {
    //        var fromService = upgradeTransition.FromSnapshot;
    //        var toService = upgradeTransition.ToSnapshot;

    //        string currentServiceName = string.Empty;
    //        if (!fromService.IsEmpty())
    //            currentServiceName = fromService.ServiceName;
    //        else
    //            currentServiceName = toService.ServiceName;

    //        var fromExistingService = !fromService.IsEmpty();
    //        var toExistingService = !toService.IsEmpty();

    //        switch (fromExistingService, toExistingService)
    //        {
    //            case (true, true):
    //                {
    //                    upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //                    {
    //                        ServiceName = fromService.ServiceName,
    //                        PropertyName = "Service",
    //                        OldValue = "(service changed)",
    //                        NewValue = "(service changed)"
    //                    });
    //                }
    //                break;
    //            case (false, true):
    //                {
    //                    upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //                    {
    //                        ServiceName = toService.ServiceName,
    //                        PropertyName = "Service",
    //                        OldValue = "(service added)",
    //                        NewValue = "(service added)"
    //                    });
    //                }
    //                break;
    //            case (true, false):
    //                {
    //                    upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //                    {
    //                        ServiceName = fromService.ServiceName,
    //                        PropertyName = "Service",
    //                        OldValue = "(service removed)",
    //                        NewValue = "(service removed)"
    //                    });
    //                }
    //                break;
    //        }

    //        Action<Func<MdsCommon.ServiceConfigurationSnapshot, string>, string> addServicePropertyChange = (f, propertyName) =>
    //        {
    //            string previousValue = "(none)";
    //            string nextValue = "(none)";
    //            string serviceName = string.Empty;

    //            if (!fromService.IsEmpty())
    //            {
    //                previousValue = f(fromService);
    //                serviceName = fromService.ServiceName;
    //            }
    //            if (!toService.IsEmpty())
    //            {
    //                nextValue = f(toService);
    //                serviceName = toService.ServiceName;
    //            }

    //            //if (previousValue != nextValue)
    //            //{
    //            //    upgradeReport.ReportStatusVariables.Single(x => x.Name == "HasChanges").Value = "true";
    //            //}

    //            if (justChanges && previousValue == nextValue)
    //                return;

    //            upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //            {
    //                ServiceName = serviceName,
    //                PropertyName = propertyName,
    //                OldValue = previousValue,
    //                NewValue = nextValue
    //            });
    //        };

    //        addServicePropertyChange(s => s.ApplicationName, "Application");
    //        addServicePropertyChange(s => s.NodeName, "Node");
    //        addServicePropertyChange(s => s.ProjectName, "Project");
    //        addServicePropertyChange(s => s.ProjectVersionTag, "Version");

    //        IEnumerable<string> allParameterNames = fromService.ServiceConfigurationSnapshotParameters.Select(x => x.ParameterName).Union(toService.ServiceConfigurationSnapshotParameters.Select(x => x.ParameterName)).ToHashSet().OrderBy(x => x);

    //        foreach (var parameterName in allParameterNames)
    //        {
    //            var fromParameter = fromService.ServiceConfigurationSnapshotParameters.SingleOrDefault(x => x.ParameterName == parameterName);
    //            var toParameter = toService.ServiceConfigurationSnapshotParameters.SingleOrDefault(x => x.ParameterName == parameterName);

    //            switch (fromParameter, toParameter)
    //            {
    //                case (null, null):
    //                    throw new Exception("Parameters exception!");
    //                case (null, MdsCommon.ServiceConfigurationSnapshotParameter _):
    //                    {
    //                        upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //                        {
    //                            ServiceName = currentServiceName,
    //                            PropertyName = toParameter.ParameterName,
    //                            OldValue = "(none, just added)",
    //                            NewValue = toParameter.DeployedValue
    //                        });
    //                        //upgradeReport.ReportStatusVariables.Single(x => x.Name == "HasChanges").Value = "true";
    //                    }
    //                    break;
    //                case (MdsCommon.ServiceConfigurationSnapshotParameter _, null):
    //                    {
    //                        upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //                        {
    //                            ServiceName = currentServiceName,
    //                            PropertyName = fromParameter.ParameterName,
    //                            OldValue = fromParameter.DeployedValue,
    //                            NewValue = "(removed)",
    //                        });
    //                        //upgradeReport.ReportStatusVariables.Single(x => x.Name == "HasChanges").Value = "true";
    //                    }
    //                    break;
    //                case (MdsCommon.ServiceConfigurationSnapshotParameter _, MdsCommon.ServiceConfigurationSnapshotParameter _):
    //                    {
    //                        //if (fromParameter.DeployedValue != toParameter.DeployedValue)
    //                        //{
    //                        //    upgradeReport.ReportStatusVariables.Single(x => x.Name == "HasChanges").Value = "true";
    //                        //}

    //                        if (justChanges && fromParameter.DeployedValue == toParameter.DeployedValue)
    //                            continue;

    //                        upgradeReport.UpgradeReportPropertyChanges.Add(new Record.ServicePropertyChange()
    //                        {
    //                            ServiceName = currentServiceName,
    //                            PropertyName = fromParameter.ParameterName,
    //                            OldValue = fromParameter.DeployedValue,
    //                            NewValue = toParameter.DeployedValue,
    //                        });
    //                    }
    //                    break;
    //            }
    //        }
    //    }

    //    return upgradeReport;
    //}


        //        public static UI.Svelte.View RenderViewDeploymentReportPage(ViewDeploymentReportPage viewDeploymentReportPage, UI.Svelte.ViewBuilder viewBuilder)
        //        {
        //            bool isActiveDeployment = viewDeploymentReportPage.GetBool("IsActive");

        //            var headerBuilder = viewBuilder.CreatePageHeader(
        //                $"Deployment report: {viewDeploymentReportPage.Deployment.GetLastDeployment().Timestamp.ItalianFormat()} {viewDeploymentReportPage.Deployment.GetLastDeployment().ConfigurationName}");

        //            headerBuilder.AddHeaderCommand<ViewDeploymentReportPage, ViewChangesReportPage>(
        //                "Deploy again",
        //                async (CommandContext commandContext, ViewDeploymentReportPage dataModel, Guid referencedId) =>
        //                {
        //                    var activeDeployment = await commandContext.Do(MdsInfrastructureApplication.LoadCurrentDeployment);

        //                    Deployment deploymentCandidate = new Deployment();
        //                    Record.Deployment deployment = new Record.Deployment()
        //                    {
        //                        ConfigurationName = dataModel.Deployment.GetLastDeployment().ConfigurationName,
        //                        Timestamp = DateTime.UtcNow,
        //                        ConfigurationHeaderId = dataModel.Deployment.GetLastDeployment().ConfigurationHeaderId
        //                    };
        //                    deploymentCandidate.Deployments.Add(deployment);

        //                    foreach (var serviceConfiguration in viewDeploymentReportPage.Deployment.GetDeployedSnapshots())
        //                    {
        //                        var candidateTransition = new Record.DeploymentServiceTransition()
        //                        {
        //                            DeploymentId = deployment.Id,
        //                            ToServiceConfigurationSnapshotId = serviceConfiguration.Service().Id,
        //                        };
        //                        deploymentCandidate.Transitions.Add(candidateTransition);

        //                        deploymentCandidate.Merge(serviceConfiguration);

        //                        var activeService = activeDeployment.GetDeployedSnapshot(serviceConfiguration.Service().ServiceName);
        //                        if (!activeService.IsEmpty())
        //                        {
        //                            candidateTransition.FromServiceConfigurationSnapshotId = activeService.Service().Id;
        //                            if (!deploymentCandidate.ServiceConfigurationSnapshots.Any(x => x.Id == activeService.Service().Id))
        //                            {
        //                                deploymentCandidate.Merge(activeService);
        //                            }
        //                        }
        //                    }

        //                    // If they existed before, but not anymore, they are considered removed
        //                    foreach (MdsCommon.ServiceConfigurationSnapshot activeService in activeDeployment.GetDeployedServices())
        //                    {
        //                        if (!deploymentCandidate.ServiceConfigurationSnapshots.Any(x => x.ServiceName == activeService.ServiceName))
        //                        {
        //                            deploymentCandidate.Transitions.Add(new Record.DeploymentServiceTransition()
        //                            {
        //                                DeploymentId = deployment.Id,
        //                                FromServiceConfigurationSnapshotId = activeService.Id,
        //                                ToServiceConfigurationSnapshotId = Guid.Empty
        //                            });
        //                            deploymentCandidate.ServiceConfigurationSnapshots.Add(activeService);
        //                        }
        //                    }

        //                    ViewChangesReportPage viewChangesReportPage = new ViewChangesReportPage()
        //                    {
        //                        Page = dataModel.Page,
        //                        Deployment = deploymentCandidate,
        //                        UpgradeReport = GetUpgradeReport(deploymentCandidate, dataModel.GetString(ReportType) == ShowJustDiff)
        //                    };

        //                    viewChangesReportPage.SetValue(nameof(DeploymentType), DeploymentType.Rollback);

        //                    return viewChangesReportPage;
        //                }, Guid.Empty, !isActiveDeployment);


        //            string showReportType = viewDeploymentReportPage.GetString(ReportType);
        //            if (showReportType == ShowJustDiff)
        //            {
        //                headerBuilder.AddHeaderCommand<ViewDeploymentReportPage>("Show all", async (context, page, id) =>
        //                {
        //                    page.SetValue(ReportType, ShowAll);
        //                    page.UpgradeReport.ReportStatusVariables.RemoveAll();
        //                    page.UpgradeReport.UpgradeReportPropertyChanges.RemoveAll();
        //                    page.UpgradeReport = GetUpgradeReport(page.Deployment, false);
        //                    return page;
        //                }, true);
        //            }
        //            else
        //            {
        //                headerBuilder.AddHeaderCommand<ViewDeploymentReportPage>("Just differences", async (context, page, id) =>
        //                {
        //                    page.SetValue(ReportType, ShowJustDiff);
        //                    page.UpgradeReport.ReportStatusVariables.RemoveAll();
        //                    page.UpgradeReport.UpgradeReportPropertyChanges.RemoveAll();
        //                    page.UpgradeReport = GetUpgradeReport(page.Deployment, true);
        //                    return page;
        //                }, true);
        //            }

        //            var backButton = headerBuilder.AddHeaderCommand<ViewDeploymentReportPage, ListDeploymentsPage>(MdsCommon.MdsCommonFunctions.BackLabel, async (commandContext, model, id) =>
        //            {
        //                ListDeploymentsPage listDeploymentsPage = await Deployments(commandContext);
        //                listDeploymentsPage.Page.Variables.AddRange(model.Page.Variables);
        //                return listDeploymentsPage;
        //            },
        //            Guid.Empty,
        //            true);
        //            backButton.Styling = "Secondary";

        //            RenderUpgradeReport(viewDeploymentReportPage, viewBuilder);

        //            return viewBuilder.OutputView;
        //        }

        //        public static void RenderUpgradeReport(IUpgradeReport upgradeReportPage, UI.Svelte.ViewBuilder viewBuilder)
        //        {
        //            viewBuilder.InPlaceSelectionGrid<IUpgradeReport, Record.UpgradeReportPropertyChange, IUpgradeReport>(
        //                "grdUpgrade",
        //                Guid.Empty,
        //                upgradeReportPage,
        //                ViewBuilder.RootGroupId,
        //                (model, _notUsed) => model.UpgradeReport.UpgradeReportPropertyChanges,
        //                () => new List<CaptionMapping>()
        //                {
        //                    new CaptionMapping()
        //                    {
        //                        CaptionText = "Service name",
        //                        FieldName = nameof(Record. UpgradeReportPropertyChange.ServiceName)
        //                    },
        //                    new CaptionMapping()
        //                    {
        //                        CaptionText = "Service property",
        //                        FieldName = nameof(Record. UpgradeReportPropertyChange.PropertyName)
        //                    },
        //                    new CaptionMapping()
        //                    {
        //                        CaptionText = "Current value",
        //                        FieldName = nameof(Record.UpgradeReportPropertyChange.OldValue)
        //                    },
        //                    new CaptionMapping()
        //                    {
        //                        CaptionText = "Next value",
        //                        FieldName = nameof(Record.UpgradeReportPropertyChange.NewValue)
        //                    }
        //                },
        //                (model, propertyChange, field) =>
        //                {
        //                    return propertyChange.GetType().GetProperty(field).GetValue(propertyChange).ToString();
        //                },
        //                string.Empty, null, null);
        //        }

        //        public static UI.Svelte.View RenderViewChangesReportPage(ViewChangesReportPage viewChangesReportPage, UI.Svelte.ViewBuilder viewBuilder)
        //        {
        //            string toConfigurationName = viewChangesReportPage.GetLastDeployment().ConfigurationName;

        //            var headerBuilder = viewBuilder.CreatePageHeader($"Deployment report: {toConfigurationName}");
        //            var deployNow = headerBuilder.AddHeaderCommand<ViewChangesReportPage, StatusPage>(
        //                "OK, DEPLOY NOW!", 
        //                DeployNow,
        //                Guid.Empty, 
        //                viewChangesReportPage.UpgradeReport.ReportStatusVariables.Single().Value == "true");

        //            deployNow.Styling = "Danger";

        //            string showReportType = viewChangesReportPage.GetString(ReportType);
        //            if (showReportType == ShowJustDiff)
        //            {
        //                headerBuilder.AddHeaderCommand<ViewChangesReportPage>("Show all", async (context, page, id) =>
        //                {
        //                    page.SetValue(ReportType, ShowAll);
        //                    page.UpgradeReport = GetUpgradeReport(page.Deployment, false);
        //                    return page;
        //                }, true);
        //            }
        //            else
        //            {
        //                headerBuilder.AddHeaderCommand<ViewChangesReportPage>("Just differences", async (context, page, id) =>
        //                {
        //                    page.SetValue(ReportType, ShowJustDiff);
        //                    page.UpgradeReport =  GetUpgradeReport(page.Deployment, true);
        //                    return page;
        //                }, true);
        //            }

        //            DeploymentType deploymentType = (DeploymentType)(Enum.Parse(typeof(DeploymentType), viewChangesReportPage.GetString(nameof(DeploymentType))));

        //            if (deploymentType == DeploymentType.FromConfiguration)
        //            {
        //                var backButton = headerBuilder.AddHeaderCommand<ViewChangesReportPage, EditConfigurationPage>(
        //                    MdsCommon.MdsCommonFunctions.BackLabel,
        //                    async (cc, model, someId) =>
        //                    {
        //                        return await InitializeEditConfiguration(cc, model.InfrastructureConfiguration);
        //                    },
        //                    Guid.Empty,
        //                    true);
        //                backButton.Styling = "Secondary";
        //            }
        //            else
        //            {
        //                var historyButton = headerBuilder.AddHeaderCommand<ViewChangesReportPage, ListDeploymentsPage>("History", BackToDeploymentHistory, Guid.Empty, true);
        //                historyButton.Styling = "Secondary";
        //            }

        //            RenderUpgradeReport(viewChangesReportPage, viewBuilder);

        //            return viewBuilder.OutputView;
        //        }

        //        public static async Task<ListDeploymentsPage> BackToDeploymentHistory(CommandContext commandContext, ViewChangesReportPage dataModel, Guid referencedId)
        //        {
        //            return await Deployments(commandContext);
        //        }

        //        public static async Task<StatusPage> DeployNow(CommandContext commandContext, ViewChangesReportPage dataModel, Guid referencedId)
        //        {
        //            // If null, we're not deploying a configuration, but redeploy from history
        //            if (dataModel.InfrastructureConfiguration !=null)
        //            {
        //                await commandContext.Do(MdsInfrastructureApplication.SaveConfiguration, dataModel.InfrastructureConfiguration);
        //            }

        //            await commandContext.Do(MdsInfrastructureApplication.ConfirmDeployment, dataModel.Deployment);

        //            MdsInfrastructureApplication.Event.BroadcastDeployment broadcastDeployment = new MdsInfrastructureApplication.Event.BroadcastDeployment();
        //            commandContext.PostEvent(broadcastDeployment);
        //            dataModel.SetValue("DeploymentSuccessful", "DeploymentSuccessful");

        //            return await Status(commandContext);// dataModel;
        //        }
    }
}