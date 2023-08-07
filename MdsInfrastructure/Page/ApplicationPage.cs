//using Metapsi;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace MdsInfrastructure
//{
//    //public class EditApplicationPage : EditConfigurationPage
//    //{
//    //    public Application PendingApplication { get; set; }
//    //}

//    public static partial class MdsInfrastructureFunctions
//    {

//        //public static List<CaptionMapping> GetApplicationColumns()
//        //{
//        //    List<CaptionMapping> columns = new List<CaptionMapping>();

//        //    columns.Add(new CaptionMapping()
//        //    {
//        //        FieldName = nameof(Record.Application.Name),
//        //        CaptionText = "Application name"
//        //    });

//        //    return columns;
//        //}

//        //public static string GetApplicationValue(EditConfigurationPage dataModel, Application application, string fieldName)
//        //{
//        //    if (fieldName == nameof(Application.Name))
//        //        return application.Name;

//        //    throw new System.NotSupportedException();
//        //}

//        //public static async Task<EditApplicationPage> AddApplication(CommandContext commandContext, EditConfigurationPage dataModel, Guid referencedId)
//        //{
//        //    EditApplicationPage editApplicationPage = Copy.To<EditApplicationPage>(dataModel);
//        //    editApplicationPage.PendingApplication = new Application()
//        //    {
//        //        ConfigurationHeaderId = dataModel.Configuration.Id
//        //    };
            
//        //    return editApplicationPage;
//        //}

//        //public static async Task<EditApplicationPage> EditApplication(CommandContext commandContext, EditConfigurationPage dataModel, System.Guid referencedId)
//        //{
//        //    //RegisterSelectionId<Record.Application>(dataModel);
//        //    EditApplicationPage editApplicationPage = Copy.To<EditApplicationPage>(dataModel);
//        //    editApplicationPage.PendingApplication = dataModel.GetSelected(dataModel.Configuration.Applications).Clone();
//        //    return editApplicationPage;
//        //}


//        //public static async Task<EditConfigurationPage> DeleteApplication(CommandContext commandContext, EditConfigurationPage dataModel, Guid referencedId)
//        //{
//        //    //RegisterSelectionId<Record.Application>(dataModel);

//        //    var selectedApplication = dataModel.GetSelected(dataModel.Configuration.Applications);

//        //    if (IsApplicationInUse(dataModel.Configuration, selectedApplication))
//        //    {
//        //        dataModel.Page.ValidationMessages.Add(new ValidationMessage()
//        //        {
//        //            MessageType = "Danger",
//        //            ValidationMessageText = "Application is in use"
//        //        });
//        //        return dataModel;
//        //    }

//        //    dataModel.Configuration.Applications.Remove(selectedApplication.Id);

//        //    return dataModel;
//        //}

//        public static bool IsApplicationInUse(this InfrastructureConfiguration infrastructureConfiguration, Application application)
//        {
//            if (infrastructureConfiguration.InfrastructureServices.Any(x => x.ApplicationId == application.Id))
//            {
//                return true;
//            }

//            return false;
//        }

//        //public static View RenderEditApplicationPage(EditApplicationPage dataModel, UI.Svelte.ViewBuilder viewBuilder)
//        //{
//        //    viewBuilder.AddOkBackHeader<EditApplicationPage, EditConfigurationPage, Record.Application>(
//        //        dataModel,
//        //        "Edit application",
//        //        (page) => Metapsi.Record.Different(page.PendingApplication, dataModel.Configuration.Applications.ByIdOrDefault(page.PendingApplication.Id)),
//        //        //dataModel.NoValidation(),
//        //        async (cc, page, id) =>
//        //        {
//        //            page.Configuration.Applications.Set(page.PendingApplication);
//        //            return page;
//        //        },
//        //        async (cc, page, id) => Copy.To<EditConfigurationPage>(dataModel));

//        //    viewBuilder.TextBox<EditApplicationPage>(
//        //        "txtAppName",
//        //        dataModel.PendingApplication.Id,
//        //        dataModel.PendingApplication.Name,
//        //        "Application name",
//        //        (page, id, newValue) =>
//        //        {
//        //            page.PendingApplication.Name = newValue;
//        //            return page;
//        //        },
//        //        -1,
//        //        UI.Svelte.ViewBuilder.RootGroupId);

//        //    return viewBuilder.OutputView;
//        //}
//    }
//}
