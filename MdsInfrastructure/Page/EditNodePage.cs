//using Metapsi;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using UI.Svelte;

//namespace MdsInfrastructure
//{
//    public class EditNodePage : EditConfigurationPage
//    {
//        public Record.InfrastructureNode PendingNode { get; set; }
//    }

//    public static partial class MdsInfrastructureFunctions
//    {
//        public static List<CaptionMapping> GetListNodesColumns()
//        {
//            List<CaptionMapping> captionMappings = new List<CaptionMapping>();

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "Name",
//                FieldName = nameof(Record.InfrastructureNode.NodeName)
//            });

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "IP",
//                FieldName = nameof(Record.InfrastructureNode.MachineIp)
//            });

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "Controller UI",
//                FieldName = "UI"
//            });

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "Type",
//                FieldName = nameof(Record.InfrastructureNode.EnvironmentTypeId)
//            });

//            return captionMappings;
//        }

//        public static string GetNodeCellValue(EditConfigurationPage dataModel, Record.InfrastructureNode node, string fieldName)
//        {
//            if (fieldName == nameof(node.NodeName))
//                return node.NodeName;

//            if (fieldName == nameof(node.MachineIp))
//                return node.MachineIp;

//            if (fieldName == "UI")
//                return $"http://{node.MachineIp}:{node.UiPort}";

//            if (fieldName == nameof(node.EnvironmentTypeId))
//            {
//                var env = dataModel.EnvironmentTypes.SingleOrDefault(x => x.Id == node.EnvironmentTypeId);
//                if (env == null)
//                    return string.Empty;
//                return $"{env.Name}";
//            }

//            return string.Empty;
//        }

//        public static async Task<EditNodePage> AddNode(CommandContext commandContext, EditConfigurationPage dataModel, Guid referencedId)
//        {
//            EditNodePage editNodePage = Copy.To<EditNodePage>(dataModel);
//            editNodePage.PendingNode = new Record.InfrastructureNode()
//            {
//                ConfigurationHeaderId = dataModel.Configuration.ConfigurationHeader.Single().Id
//            };
//            return editNodePage;
//        }

//        public static async Task<EditNodePage> EditNode(CommandContext commandContext, EditConfigurationPage dataModel, System.Guid referencedId)
//        {
//            RegisterSelectionId<Record.InfrastructureNode>(dataModel);
            
//            EditNodePage editNodePage = Copy.To<EditNodePage>(dataModel);
//            editNodePage.PendingNode = dataModel.GetSelected(dataModel.Configuration.InfrastructureNodes).Clone();
            
//            return editNodePage;
//        }

//        public static async Task<EditConfigurationPage> DeleteNode(CommandContext commandContext, EditConfigurationPage listNodesPage, Guid referencedId)
//        {
//            RegisterSelectionId<Record.InfrastructureNode>(listNodesPage);

//            Record.InfrastructureNode selectedNode = listNodesPage.GetSelected(listNodesPage.Configuration.InfrastructureNodes);

//            if (Metapsi.Record.IsEmpty(selectedNode))
//                return listNodesPage;

//            if (listNodesPage.Configuration.IsNodeInUse(selectedNode))
//            {
//                listNodesPage.Page.ValidationMessages.Add(new ValidationMessage()
//                {
//                    MessageType = "Danger",
//                    ValidationMessageText = "Node is in use"
//                });
//                return listNodesPage;
//            }

//            listNodesPage.Configuration.InfrastructureNodes.Remove(selectedNode.Id);

//            return listNodesPage;
//        }

//        public static UI.Svelte.View RenderEditNodePage(EditNodePage dataModel, UI.Svelte.ViewBuilder viewBuilder)
//        {
//            viewBuilder.AddOkBackHeader<EditNodePage, EditConfigurationPage, Record.InfrastructureNode>(
//                dataModel,
//                "Edit node",
//                page => Metapsi.Record.Different(page.PendingNode, dataModel.Configuration.InfrastructureNodes.ByIdOrDefault(page.PendingNode.Id)),
//                //async (cc, dataModel, _) =>
//                //{
//                //    if (dataModel.PendingNode.EnvironmentTypeId == Guid.Empty)
//                //        return "Environment type not selected!";

//                //    if (string.IsNullOrWhiteSpace(dataModel.PendingNode.NodeName))
//                //        return "Node name not valid!";

//                //    return string.Empty;
//                //},
//                async (cc, page, id) =>
//                {
//                    page.Configuration.InfrastructureNodes.Set(page.PendingNode);
//                    return page;
//                },
//                async (cc, page, id) => Copy.To<EditConfigurationPage>(page));

//            var editedNode = dataModel.PendingNode;

//            var line1 = viewBuilder.Group("grpFirstLine", editedNode.Id, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");
//            var line2 = viewBuilder.Group("grpSecondLine", editedNode.Id, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");

//            viewBuilder.TextBox<EditNodePage>(
//                "txtNodeName",
//                editedNode.Id,
//                editedNode.NodeName,
//                "Node name",
//                EditNodeName,
//                -1,
//                line1.Id);

//            viewBuilder.TextBox<EditNodePage>(
//                "txtNodeIp",
//                editedNode.Id,
//                editedNode.MachineIp,
//                "Machine address",
//                EditNodeIp,
//                -1,
//                line1.Id);

//            viewBuilder.TextBox<EditNodePage>(
//                "txtRedisAddress",
//                editedNode.Id,
//                editedNode.UiPort.ToString(),
//                "Node UI port",
//                EditNodeUiPort,
//                -1,
//                line2.Id);

//            var envDropDown = viewBuilder.DropDown<EditNodePage>("ddEnvType", editedNode.Id, line2.Id,
//                EditNodeEnvironmentType,
//                editedNode.EnvironmentTypeId,
//                "OS type",
//                -1,
//                "Select OS type");

//            foreach (var envType in dataModel.EnvironmentTypes)
//            {
//                viewBuilder.DropDownItem(envDropDown.Id, envType.Id, $"{envType.OsType}");
//            }

//            return viewBuilder.OutputView;
//        }

//        public static EditNodePage EditNodeName(EditNodePage dataModel, System.Guid referencedId, System.String commandValue)
//        {
//            dataModel.PendingNode.NodeName = commandValue;
//            return dataModel;
//        }

//        public static EditNodePage EditNodeIp(EditNodePage dataModel, System.Guid referencedId, System.String commandValue)
//        {
//            dataModel.PendingNode.MachineIp = commandValue;
//            return dataModel;
//        }

//        public static EditNodePage EditNodeUiPort(EditNodePage dataModel, System.Guid referencedId, System.String commandValue)
//        {
//            dataModel.PendingNode.UiPort = Int32.Parse(commandValue);
//            return dataModel;
//        }

//        public static EditNodePage EditNodeEnvironmentType(EditNodePage dataModel, System.Guid referencedId, System.Guid selectedId)
//        {
//            dataModel.PendingNode.EnvironmentTypeId = selectedId;
//            return dataModel;
//        }
//    }
//}
