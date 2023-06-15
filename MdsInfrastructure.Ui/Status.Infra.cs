using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using Metapsi;
using System;
using System.Threading.Tasks;
using MdsCommon;
using Metapsi.Hyperapp;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class Status
    {
        public static async Task<InfrastructureStatus> LoadStatus(CommandContext commandContext)
        {
            string validation = await commandContext.Do(Api.ValidateSchema);

            if(!string.IsNullOrEmpty(validation))
            {
                return new InfrastructureStatus()
                {
                    SchemaValidationMessage = validation
                };
            }

            return await commandContext.Do(Api.LoadInfraStatus);
        }
    }
}

