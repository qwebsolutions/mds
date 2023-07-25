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
//        public static async Task<IResponse> Node(CommandContext commandContext, HttpContext requestData)
//        {
//            var pageData = await LoadStatus(commandContext);

//            //NodeStatusPage nodesStatusPage = new NodeStatusPage()
//            //{
//            //    HealthStatus = pageData.HealthStatus,
//            //    InfrastructureConfiguration = pageData.InfrastructureConfiguration,
//            //    InfrastructureEvents = pageData.InfrastructureEvents,
//            //    Deployment = pageData.Deployment,
//            //};

//            var selectedNodeId = Guid.Parse(requestData.EntityId());

//            return Page.Response(pageData,
//                (b, clientModel) => 
//}

