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
    public class ListProcessesHandler : Http.Get<Overview.ListProcesses>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var page = await Load(commandContext, httpContext);
            return Page.Result(page);
        }

        public static async Task<OverviewPage> Load(CommandContext commandContext, HttpContext httpContext)
        {
            var page = new OverviewPage()
            {
                LocalSettings = await commandContext.Do(MdsLocalApplication.GetLocalSettings),
                Warnings = await commandContext.Do(MdsLocalApplication.GetWarnings),
                FullLocalStatus = await commandContext.Do(MdsLocalApplication.GetFullLocalStatus),
                ServiceProcesses = await commandContext.Do(MdsLocalApplication.GetRunningProcesses)
            };

            List<ProcessRow> rows = new List<ProcessRow>();
            foreach (var serviceSnapshot in page.FullLocalStatus.LocalServiceSnapshots)
            {
                var serviceProcess = page.ServiceProcesses.SingleOrDefault(x => x.ServiceName == serviceSnapshot.ServiceName);

                var pid = "Not running";
                var runningStatus = "Not running";
                string usedRamMb = "0";

                if (serviceProcess != null)
                {
                    TimeSpan running = DateTime.UtcNow - serviceProcess.StartTimestampUtc;
                    TimeSpan rounded = TimeSpan.FromSeconds((int)running.TotalSeconds);
                    runningStatus = $"{serviceProcess.StartTimestampUtc} ({rounded.ToString("c")})";

                    pid = serviceProcess.Pid.ToString();
                    usedRamMb = serviceProcess.UsedRamMB.ToString();
                }

                rows.Add(new ProcessRow()
                {
                    ServiceName = serviceSnapshot.ServiceName,
                    ProjectName = serviceSnapshot.ProjectName,
                    ProjectVersion = serviceSnapshot.ProjectVersionTag,
                    RunningSince = runningStatus,
                    Pid = pid,
                    UsedRam = usedRamMb,
                    HasError = serviceProcess == null
                });
            }

            page.Processes = rows;

            string overviewText = $"Total services: 0";

            var lastChange = page.FullLocalStatus.SyncResults.Where(x => x.ResultCode == SyncStatusCodes.Changed).OrderByDescending(x => x.Timestamp).FirstOrDefault();

            if (lastChange != null)
            {
                overviewText = $"Local services: {page.FullLocalStatus.LocalServiceSnapshots.Count()}, last change: {lastChange.Timestamp.ToString("G")}";
            }

            page.OverviewText = overviewText;

            return page;
        }
    }
}

