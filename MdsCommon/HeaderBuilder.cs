//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using UI.Svelte;

//namespace Metapsi
//{
//    public class HeaderBuilder
//    {
//        public UI.Svelte.ViewBuilder ViewBuilder { get; set; }
//        public UI.Svelte.Group HeaderGroup { get; set; }
//        public string HeaderName { get; set; } = string.Empty;
//    }


//    public static partial class HeaderBuilderFunctions
//    {
//        public static void AddOkBackHeader<TEditModel, TListModel, TPendingData>(
//            this UI.Svelte.ViewBuilder viewBuilder,
//            TEditModel pageModel,
//            string titleText,
//            Func<TEditModel, bool> hasPendingData,
//            //Func<CommandContext, TEditModel, Guid, Task<string>> validate,
//            Func<CommandContext, TEditModel, Guid, Task<TEditModel>> commitPendingChanges,
//            Func<CommandContext, TEditModel, Guid, Task<TListModel>> back)
//            where TEditModel : IPage
//        {
//            //var onOk = async (CommandContext commandContext, TEditModel model, Guid referencedId) =>
//            //{
//            //    string validationMessage = await validate(commandContext, model, referencedId);
//            //    if (!string.IsNullOrEmpty(validationMessage))
//            //    {
//            //        model.Page.ValidationMessages.Add(new ValidationMessage()
//            //        {
//            //            MessageType = "Danger",
//            //            ValidationMessageText = validationMessage
//            //        });
//            //    }
//            //    else
//            //    {
//            //        await commitPendingChanges(commandContext, model, referencedId);
//            //    }

//            //    return model;
//            //};

//            var headerBuilder = viewBuilder.CreatePageHeader(titleText);
//            bool hasChanges = hasPendingData(pageModel);

//            string secondButtonText = "Back";

//            if (hasChanges)
//            {
//                secondButtonText = "Discard";
//            }

//            var okButton = headerBuilder.AddHeaderCommand<TEditModel>("OK", commitPendingChanges, hasChanges);

//            var backButton = headerBuilder.AddHeaderCommand<TEditModel, TListModel>(secondButtonText, back, Guid.Empty, true);
//            backButton.Styling = "Secondary";
//        }

//        public static HeaderBuilder AddSaveBackHeader<TEditModel, TListModel>(
//            this HeaderBuilder headerBuilder,
//            TEditModel pageModel,
//            Func<TEditModel, bool> hasPendingData,
//            Func<CommandContext, TEditModel, Guid, Task<TEditModel>> save,
//            Func<CommandContext, TEditModel, Guid, Task<TListModel>> back,
//            Func<CommandContext, TEditModel, Guid, Task<bool>> validate)
//        {
//            //var headerBuilder = viewBuilder.AddPageHeader(titleText);
//            bool hasChanges = hasPendingData(pageModel);

//            string secondButtonText = "Back";

//            if (hasChanges)
//            {
//                secondButtonText = "Discard";
//            }

//            var saveButton = headerBuilder.AddHeaderCommand<TEditModel>("Save",
//                async (CommandContext commandContext, TEditModel model, Guid referencedId) =>
//                {
//                    bool isValid = await validate(commandContext, model, referencedId);
//                    if (!isValid)
//                    {
//                        return model;
//                    }
//                    else
//                    {
//                        return await save(commandContext, model, referencedId);
//                    }
//                }, hasChanges);
//            saveButton.Styling = "Primary";

//            var backButton = headerBuilder.AddHeaderCommand<TEditModel, TListModel>(secondButtonText, back, Guid.Empty, true);
//            backButton.Styling = "Secondary";

//            return headerBuilder;
//        }

//        public static UI.Svelte.Button AddHeaderCommand<TDataModel>(
//            this HeaderBuilder headerBuilder,
//            string text,
//            Func<CommandContext, TDataModel, Guid, Task<TDataModel>> command,
//            bool enabled)
//        {
//            var controlsCount = headerBuilder.ViewBuilder.OutputView.ControlPositions.Where(x => x.ParentGroupId == headerBuilder.HeaderGroup.Id).Count();
//            return headerBuilder.ViewBuilder.Button<TDataModel>(
//                $"{headerBuilder.HeaderName}_btnHeaderCommand{controlsCount}",
//                Guid.Empty,
//                headerBuilder.HeaderGroup.Id, text, command, 200, enabled);
//        }

//        public static UI.Svelte.Button AddHeaderCommand<TDataModel, TOutModel>(
//            this HeaderBuilder headerBuilder,
//            string text,
//            Func<CommandContext, TDataModel, Guid, Task<TOutModel>> command,
//            Guid referencedId,
//            bool enabled)
//        {
//            var controlsCount = headerBuilder.ViewBuilder.OutputView.ControlPositions.Where(x => x.ParentGroupId == headerBuilder.HeaderGroup.Id).Count();
//            var button = headerBuilder.ViewBuilder.Button<TDataModel, TOutModel>(
//                $"{headerBuilder.HeaderName}_btnHeaderCommand{controlsCount}",
//                referencedId,
//                headerBuilder.HeaderGroup.Id, text, command, 200, enabled);

//            return button;
//        }

//        public static HeaderBuilder CreatePageHeader(
//            this UI.Svelte.ViewBuilder viewBuilder,
//            string titleText,
//            string headerName = null)
//        {
//            if (string.IsNullOrEmpty(headerName))
//                headerName = Guid.NewGuid().ToString();

//            viewBuilder.OutputView.HeaderText = titleText;

//            var grpPageHeader = viewBuilder.Group("grpPageHeader", System.Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");
//            viewBuilder.Label("lblHeaderText", System.Guid.Empty, "", -1, grpPageHeader.Id);
//            return new HeaderBuilder() { HeaderGroup = grpPageHeader, ViewBuilder = viewBuilder, HeaderName = headerName };
//        }

//        public static Func<CommandContext, TDataModel, Guid, Task<string>> NoValidation<TDataModel>(
//            this TDataModel dataModel) where TDataModel : IPage
//        {
//            return async (cc, m, _) => string.Empty;
//        }
//    }
//}