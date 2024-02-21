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
    public class SyncHistoryHandler : Http.Get<SyncHistory.List>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext requestData)
        {
            var dataModel = new SyncHistory.DataModel()
            {
                SyncHistory = (await commandContext.Do(MdsLocalApplication.GetSyncHistory)).OrderByDescending(x => x.Timestamp).ToList()
            };

            return Page.Result(dataModel);
        }
    }
}