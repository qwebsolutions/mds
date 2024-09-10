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
                FullLocalStatus = await commandContext.Do(MdsLocalApplication.GetFullLocalStatus),
                ServiceProcesses = await commandContext.Do(MdsLocalApplication.GetRunningProcesses)
            };

            List<ProcessRow> rows = new List<ProcessRow>();
            foreach (var serviceSnapshot in page.FullLocalStatus.LocalServiceSnapshots)
            {
                var serviceProcess = page.ServiceProcesses.SingleOrDefault(x => x.ServiceName == serviceSnapshot.ServiceName);

                var processRow = new ProcessRow()
                {
                    ServiceName = serviceSnapshot.ServiceName,
                    ProjectName = serviceSnapshot.ProjectName,
                    ProjectVersion = serviceSnapshot.ProjectVersionTag,
                    Disabled = !serviceSnapshot.Enabled
                };

                if (serviceProcess != null)
                {
                    processRow.Running = true;
                    TimeSpan running = DateTime.UtcNow - serviceProcess.StartTimestampUtc;
                    TimeSpan rounded = TimeSpan.FromSeconds((int)running.TotalSeconds);
                    processRow.RunningSince = $"{serviceProcess.StartTimestampUtc} ({rounded.ToString("c")})";
                    processRow.Pid = serviceProcess.Pid.ToString();
                    processRow.UsedRam = serviceProcess.UsedRamMB.ToString();
                }
                else
                {
                    processRow.Running = false;
                    processRow.Pid = "";
                    processRow.UsedRam = "";
                    processRow.RunningSince = "";
                }

                rows.Add(processRow);
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

