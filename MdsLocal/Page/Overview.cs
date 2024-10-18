using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Sqlite;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static class ListProcessesHandler
    {
        public static async Task<IResult> Get(SqliteQueue sqliteQueue, LocalControllerSettings localControllerSettings)
        {
            var page = await Load(sqliteQueue, localControllerSettings);
            return Page.Result(page);
        }

        public static async Task<OverviewPage> Load(SqliteQueue sqliteQueue, LocalControllerSettings localControllerSettings)
        {
            var page = new OverviewPage()
            {
                LocalSettings = new LocalSettings()
                {
                    FullDbPath = localControllerSettings.FullDbPath,
                    InfrastructureApiUrl = localControllerSettings.InfrastructureApiUrl,
                    NodeName = localControllerSettings.NodeName,
                },
                FullLocalStatus = await LocalDb.LoadFullLocalStatus(sqliteQueue, localControllerSettings.NodeName),
                ServiceProcesses = await ServiceProcessExtensions.GetRunningProcesses(localControllerSettings)
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

