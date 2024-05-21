using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Routing.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MdsCommon.Controls
{
    public static class MdsDefaultBuilder
    {
        public static DataTableBuilder<TRow> DataTable<TRow>()
        {
            return new DataTableBuilder<TRow>()
            {
                SetTableProps = b =>
                {
                    b.SetClass("bg-white border-collapse w-full overflow-hidden");
                },
                SetTheadProps = b =>
                {
                    b.SetClass("text-left text-sm text-gray-500 bg-white drop-shadow-sm");
                },
                SetThProps = (b, column) => b.SetClass("py-4 border-b border-gray-300 bg-white"),
                SetTdProps = (b, row, column) => b.SetClass("py-4 border-b border-gray-300")
            };
        }

        public static DataGridBuilder<TRow> DataGrid<TRow>()
        {
            return new DataGridBuilder<TRow>()
            {
                DataTableBuilder = MdsDefaultBuilder.DataTable<TRow>(),
                SetContainerProps = b =>
                {
                    b.SetClass("flex flex-col w-full bg-white gap-8");
                }
            };
        }
    }
}

