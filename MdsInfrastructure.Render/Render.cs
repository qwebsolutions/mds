using Metapsi.Syntax;

namespace MdsInfrastructure
{
    public static class EditConfiguration
    {
        public static Var<HyperNode> Render(this BlockBuilder b, Var<EditConfigurationPage> clientModel)
        {
            //var header = b.NewObj(new Header.Props()
            //{
            //    Main = new Header.Title() { Operation = "Edit configuration", Entity = serverModel.Configuration.Name },
            //    User = user
            //});
            //b.Set(b.Get(header, x => x.Main), x => x.Entity, b.Get(clientModel, x => x.Configuration.Name));

            //var layout = b.Layout(
            //    b.InfraMenu(nameof(Configuration), user.IsSignedIn()),
            //    b.Render(header),
            //    b.RenderCurrentView(
            //        clientModel,
            //        x => x.EditStack,
            //        // Default page view
            //        (BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
            //        {
            //            var hasChanges = b.Not(b.AreEqual(b.Serialize(clientModel), b.Serialize(b.Const(serverModel))));
            //            return MainPage(b, clientModel, hasChanges);
            //        }));
            //return layout;

            return b.Text("Not implemented");
        }
    }
}
