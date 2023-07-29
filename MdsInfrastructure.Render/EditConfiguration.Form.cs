using Metapsi.Syntax;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> Form(this BlockBuilder b, Var<HyperNode> toolbar)
        {
            //var container = b.Div("flex flex-col p-4 gap-4");
            //var top = b.Add(container, b.Div("flex flex-row justify-end"));
            //b.Add(top, toolbar);
            //var grid = b.Add(container, b.Div("grid grid-cols-2 gap-4 items-center"));
            //return grid;

            var grid = b.Div("grid grid-cols-2 gap-4 items-center p-4");

            var toolbarRow = b.Add(grid, b.Div("col-span-2 flex flex-row justify-end items-center"));
            b.Add(toolbarRow, toolbar);

            return grid;
        }

        public static void FormField(this BlockBuilder b, Var<HyperNode> form, string label, Var<HyperNode> fieldControl)
        {
            b.Add(form, b.Text(label));
            b.Add(form, fieldControl);
        }

        public static Var<string> WithDefault(this BlockBuilder b, Var<string> value)
        {
            return b.If(b.HasValue(value), b => value, b => b.Const("(not set)"));
        }
    }
}
