using Metapsi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdsCommon
{

    public static partial class MdsCommonFunctions
    {

        public static void AddInfrastructureEventsToSqlite(this ImplementationGroup implementationGroup, string fullDbPath)
        {
            //implementationGroup.MapCommand(SaveInfrastructureEvent, async (rc, infrastructureEvent) =>
            //{
            //    await Db.SaveInfrastructureEvent(fullDbPath, infrastructureEvent);
            //});

            implementationGroup.MapRequest(MdsCommon.Api.GetAllInfrastructureEvents, async (rc) =>
            {
                return await Db.LoadAllInfrastructureEvents(fullDbPath);
            });

            implementationGroup.MapRequest(MdsCommon.Api.GetMostRecentEventOfService, async (rc, serviceName) =>
            {
                return await Db.LoadMostRecentInfrastructureEvent(fullDbPath, serviceName);
            });
        }

        public static Guid EventsGridId = Guid.Parse("5fafc6be-aec9-49f7-9c9c-a95831dda204");

        public const string BackLabel = "Back";
        //public static View RenderListInfrastructureEventsPage(ListInfrastructureEventsPage dataModel, UI.Svelte.ViewBuilder viewBuilder)
        //{
        //    var selectedEvent = dataModel.GetSelected(dataModel.InfrastructureEvents.InfrastructureEvents);
        //    if(!Record.IsEmpty(selectedEvent))
        //    { 
        //        var sidePanel = new Group()
        //        {
        //            Orientation = "Vertical",
        //            Name = "grpSidePanel"
        //        };
        //        viewBuilder.OutputView.Groups.Add(sidePanel);

        //        viewBuilder.Label("lblTimestamp", Guid.Empty, $"Event timestamp: {selectedEvent.Timestamp.ToString("G", new System.Globalization.CultureInfo("it-IT"))}", -1, sidePanel.Id);
        //        viewBuilder.Label("lblType", Guid.Empty, $"Event description: {selectedEvent.ShortDescription}", -1, sidePanel.Id);
        //        //viewBuilder.Label("lblType", Guid.Empty, $"Event type: {selectedEvent.Type}", -1, UI.ViewBuilder.RootGroupId);
        //        //viewBuilder.Label("lblMdsLocal", Guid.Empty, $"Local controller: {selectedEvent.LocalControllerName}", -1, UI.ViewBuilder.RootGroupId);
        //        if (!string.IsNullOrEmpty(selectedEvent.Source))
        //        {
        //            viewBuilder.Label("lblService", Guid.Empty, $"Service name: {selectedEvent.Source}", -1, sidePanel.Id);
        //        }

        //        if (!string.IsNullOrWhiteSpace(selectedEvent.FullDescription))
        //        {
        //            viewBuilder.Label("lblContent", Guid.Empty, $"Data : <pre> {selectedEvent.FullDescription}</pre>", -1, sidePanel.Id);
        //        }

        //        viewBuilder.OutputView.SidePanelGroupId = sidePanel.Id;

        //        System.Func<ListInfrastructureEventsPage, string, Guid, ListInfrastructureEventsPage> closeSidePanel =
        //            (page, s, i) =>
        //            {
        //                page.ClearSelected<InfrastructureEvent>();
        //                return page;
        //            };

        //        var callbackIdentifier = viewBuilder.Callback(closeSidePanel.Method, closeSidePanel.Target, "notUsed", Guid.Empty);
        //        viewBuilder.OutputView.CloseSidePanelCallbackId = callbackIdentifier.Id;
        //    }

        //    var dataGrid = viewBuilder.InPlaceSelectionGrid<MdsCommon.ListInfrastructureEventsPage, MdsCommon.InfrastructureEvent, MdsCommon.ListInfrastructureEventsPage>(
        //        "grdListEvents",
        //        Guid.Empty,
        //        dataModel,
        //        ViewBuilder.RootGroupId,
        //        (pageData, _) => pageData.InfrastructureEvents.InfrastructureEvents,
        //        GetInfrastructureEventColumns,
        //        GetInfrastructureEventCellValue,
        //        string.Empty,
        //        null,
        //        async (commandContext, localDataModel, selectedId) =>
        //        {
        //            var commandValue = localDataModel.GetString("CommandValue");
        //            localDataModel.SetSelectedId<InfrastructureEvent>(Guid.Parse(commandValue));
        //            return localDataModel;
        //        });

        //    viewBuilder.OutputView.HeaderText = "Infrastructure events";

        //    var alertTypes = AlertTags();

        //    foreach (var infraEvent in dataModel.InfrastructureEvents.InfrastructureEvents)
        //    {
        //        if (alertTypes.Contains(infraEvent.Criticality.ToLower()))
        //        {
        //            dataGrid.Badges.Add(new DataGridBadge()
        //            {
        //                ColumnName = nameof(MdsCommon.InfrastructureEvent.Timestamp),
        //                RowReferencedId = infraEvent.Id.ToString(),
        //                Badge = new Badge()
        //                {
        //                    Name = "alertBadge",
        //                    ReferencedId = infraEvent.Id,
        //                    Styling = "danger",
        //                    Text = "Alert"
        //                }
        //            });
        //        }
        //    }

        //    return viewBuilder.OutputView;
        //}

        //public static List<CaptionMapping> GetInfrastructureEventColumns()
        //{
        //    List<CaptionMapping> captionMappings = new List<CaptionMapping>();

        //    captionMappings.Add(new CaptionMapping()
        //    {
        //        CaptionText = "Timestamp",
        //        FieldName = nameof(MdsCommon.InfrastructureEvent.Timestamp)
        //    });

        //    captionMappings.Add(new CaptionMapping()
        //    {
        //        CaptionText = "Criticality",
        //        FieldName = nameof(MdsCommon.InfrastructureEvent.Criticality)
        //    });

        //    captionMappings.Add(new CaptionMapping()
        //    {
        //        CaptionText = "Source",
        //        FieldName = nameof(MdsCommon.InfrastructureEvent.Source)
        //    });

        //    captionMappings.Add(new CaptionMapping()
        //    {
        //        CaptionText = "Description",
        //        FieldName = nameof(MdsCommon.InfrastructureEvent.ShortDescription)
        //    });

        //    return captionMappings;
        //}

        //public static string GetInfrastructureEventCellValue(MdsCommon.ListInfrastructureEventsPage dataModel, MdsCommon.InfrastructureEvent infrastructureEvent, string fieldName)
        //{
        //    switch (fieldName)
        //    {
        //        case nameof(MdsCommon.InfrastructureEvent.Source):
        //            return infrastructureEvent.Source;

        //        case nameof(MdsCommon.InfrastructureEvent.Type):
        //            return infrastructureEvent.Type;

        //        case nameof(MdsCommon.InfrastructureEvent.Criticality):
        //            return infrastructureEvent.Criticality;

        //        case nameof(MdsCommon.InfrastructureEvent.Timestamp):
        //            return infrastructureEvent.Timestamp.ToString("G", new System.Globalization.CultureInfo("it-IT"));

        //        case nameof(MdsCommon.InfrastructureEvent.ShortDescription):
        //            return infrastructureEvent.ShortDescription;
        //    }

        //    return string.Empty;
        //}


        public static async Task<MdsCommon.ListInfrastructureEventsPage> ListInfrastructureEvents(CommandContext commandContext)
        {
            var allEvents = await commandContext.Do(MdsCommon.Api.GetAllInfrastructureEvents);

            MdsCommon.ListInfrastructureEventsPage listInfrastructureEventsPage = new MdsCommon.ListInfrastructureEventsPage();
            listInfrastructureEventsPage.InfrastructureEvents.AddRange(allEvents);

            return listInfrastructureEventsPage;
        }

        //public static UI.View RenderViewInfrastructureEventPage(MdsCommon.ViewInfrastructureEventPage dataModel, UI.ViewBuilder viewBuilder)
        //{
        //    var selectedEvent = dataModel.InfrastructureEvents.ById(dataModel.GetId(Keys.SelectedInfrastructureEventId));

        //    var pageHeader = viewBuilder.CreatePageHeader("Infrastructure event");
        //    var backButton = pageHeader.AddHeaderCommand<MdsCommon.ViewInfrastructureEventPage, MdsCommon.ListInfrastructureEventsPage>(BackLabel, (context, model, id) =>
        //    {
        //        //CommandContext commandContext = new CommandContext(context.ExecutionFlowId, context.CurrentProcessorId)
        //        //{
        //        //    Events = context.Events,
        //        //    MetaEvents = context.MetaEvents,
        //        //    Morphology = context.Morphology,
        //        //    References = context.References,
        //        //    State = context.State,
        //        //    Trace = context.Trace,
        //        //    RequestRouter = context.RequestRouter,
        //        //    Request = MdsCommon.MdsCommonRequests.BuildRouter(new MdsCommonRequests.State()
        //        //    {
        //        //        FullDbPath = context.State as .. the fuck?
        //        //    })
        //        //};

        //        return MdsCommon.MdsCommonFunctions.ListInfrastructureEvents(context);
        //    }, Guid.Empty, true);
        //    backButton.Styling = "Secondary";

        //    viewBuilder.Label("lblTimestamp", Guid.Empty, $"Event timestamp: {selectedEvent.Timestamp.ToString("G", new System.Globalization.CultureInfo("it-IT"))}", -1, UI.ViewBuilder.RootGroupId);
        //    viewBuilder.Label("lblType", Guid.Empty, $"Event description: {selectedEvent.ShortDescription}", -1, UI.ViewBuilder.RootGroupId);
        //    //viewBuilder.Label("lblType", Guid.Empty, $"Event type: {selectedEvent.Type}", -1, UI.ViewBuilder.RootGroupId);
        //    //viewBuilder.Label("lblMdsLocal", Guid.Empty, $"Local controller: {selectedEvent.LocalControllerName}", -1, UI.ViewBuilder.RootGroupId);
        //    if (!string.IsNullOrEmpty(selectedEvent.Source))
        //    {
        //        viewBuilder.Label("lblService", Guid.Empty, $"Service name: {selectedEvent.Source}", -1, UI.ViewBuilder.RootGroupId);
        //    }

        //    if (!string.IsNullOrWhiteSpace(selectedEvent.FullDescription))
        //    {
        //        viewBuilder.Label("lblContent", Guid.Empty, $"Data : <pre> {selectedEvent.FullDescription}</pre>", -1, UI.ViewBuilder.RootGroupId);
        //    }

        //    return viewBuilder.OutputView;
        //}

        //public static HeaderBuilder CreatePageHeader(
        //    this UI.Svelte.ViewBuilder viewBuilder,
        //    string titleText,
        //    string headerName = null)
        //{
        //    if (string.IsNullOrEmpty(headerName))
        //        headerName = Guid.NewGuid().ToString();

        //    viewBuilder.OutputView.HeaderText = titleText;

        //    var grpPageHeader = viewBuilder.Group("grpPageHeader", System.Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");
        //    viewBuilder.Label("lblHeaderText", System.Guid.Empty, "", -1, grpPageHeader.Id);
        //    return new HeaderBuilder() { HeaderGroup = grpPageHeader, ViewBuilder = viewBuilder, HeaderName = headerName };
        //}

        //public static void PersistEntity(UI.Svelte.IPageBehavior page, string key, object singleObject)
        //{
        //    string serializedSingle = Metapsi.Serialize.ToTypedJson(singleObject);
        //    page.SetValue(Keys.Persisted(key), serializedSingle);
        //}

        public static HashSet<string> AlertTags()
        {
            HashSet<string> alertTags = new HashSet<string>();
            alertTags.Add("critical");
            alertTags.Add("fatal");
            alertTags.Add("error");

            return alertTags;
        }

        public static bool IsAlertTag(string tag)
        {
            return AlertTags().Contains(tag.ToLower());
        }

    }
}