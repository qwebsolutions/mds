using Metapsi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static partial class MdsInfrastructureFunctions
{
    public static async Task<ConfigurationHeadersList> Configurations(CommandContext commandContext)
    {
        return await commandContext.Do(Backend.LoadAllConfigurationHeaders);
    }

    public static string GetConfigurationCellValue(ConfigurationHeadersList dataModel, InfrastructureConfiguration configurationHeader, string fieldName)
    {
        if (fieldName == "ServicesCount")
        {
            return dataModel.Services.Where(x => x.ConfigurationHeaderId == configurationHeader.Id).Count().ToString();
        }

        if (fieldName == nameof(InfrastructureConfiguration.Name))
            return configurationHeader.Name;

        return string.Empty;
    }

    public static async Task<EditConfigurationPage> InitializeEditConfiguration(
        CommandContext commandContext,
        InfrastructureConfiguration configuration)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        EditConfigurationPage editConfigurationPage = new EditConfigurationPage();
        editConfigurationPage.Configuration = configuration;

        var activeInfraDeployment = await commandContext.Do(Backend.LoadCurrentDeployment);

        editConfigurationPage.LastConfigurationDeployment = await commandContext.Do(Backend.LoadLastConfigurationDeployment, editConfigurationPage.Configuration.Id);
        editConfigurationPage.LastInfrastructureDeployment = activeInfraDeployment;
        editConfigurationPage.AllProjects = await commandContext.Do(Backend.LoadAllProjects);
        //editConfigurationPage.InitialConfiguration = editConfigurationPage.Configuration.Clone();
        editConfigurationPage.EnvironmentTypes = await commandContext.Do(Backend.LoadEnvironmentTypes);
        editConfigurationPage.InfrastructureNodes = await commandContext.Do(Backend.LoadAllNodes);

        editConfigurationPage.ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes);
        editConfigurationPage.NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes);
        editConfigurationPage.LastDeployed = editConfigurationPage.LastDeploymentLabel();

        Console.WriteLine($"=== InitializeEditConfiguration : {sw.ElapsedMilliseconds} ms ===");
        return editConfigurationPage;
    }

    public static string LastDeploymentLabel(this EditConfigurationPage dataModel)
    {
        string lastDeployment = "never deployed";

        if (Metapsi.Record.IsValid(dataModel.LastConfigurationDeployment))
        {
            lastDeployment = dataModel.LastConfigurationDeployment.Timestamp.ItalianFormat();
        }

        return $"Last deployed: {lastDeployment}";
    }



}