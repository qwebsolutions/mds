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
    public static class SyncHistoryHandler 
    {
        public static async Task<IResult> Get(SqliteQueue sqliteQueue)
        {
            var dataModel = new SyncHistory.DataModel()
            {
                SyncHistory = (await LocalDb.LoadSyncHistory(sqliteQueue)).OrderByDescending(x => x.Timestamp).ToList()
            };

            return Page.Result(dataModel);
        }
    }
}