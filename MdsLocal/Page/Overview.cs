using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    //public static class Overview
    //{
    //    public static async Task<OverviewPage> Load(CommandContext commandContext)
    //    {
    //        return new OverviewPage()
    //        {
    //            LocalSettings = await commandContext.Do(MdsLocalApplication.GetLocalSettings),
    //            Warnings = await commandContext.Do(MdsLocalApplication.GetWarnings),
    //            FullLocalStatus = await commandContext.Do(MdsLocalApplication.GetFullLocalStatus),
    //            ServiceProcesses = await commandContext.Do(MdsLocalApplication.GetRunningProcesses)
    //        };
    //    }

    //    public static async Task<IResult> ListProcesses(CommandContext commandContext, HttpContext requestData)
    //    {
    //        var dataModel = await Load(commandContext);
    //        return Page.Result(dataModel);
    //    }
    //}

    public class ListProcessesHandler : RouteHandler<Overview.ListProcesses>
    {
        public override async Task<IResult> Get(CommandContext commandContext, HttpContext httpContext)
        {
            var page = new OverviewPage()
            {
                //LocalSettings = await commandContext.Do(MdsLocalApplication.GetLocalSettings),
                //Warnings = await commandContext.Do(MdsLocalApplication.GetWarnings),
                //FullLocalStatus = await commandContext.Do(MdsLocalApplication.GetFullLocalStatus),
                //ServiceProcesses = await commandContext.Do(MdsLocalApplication.GetRunningProcesses)
            };

            return Page.Result(page);
        }
    }
}

