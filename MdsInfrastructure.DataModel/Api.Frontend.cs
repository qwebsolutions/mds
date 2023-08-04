using Metapsi;
using Metapsi.Hyperapp;

namespace MdsInfrastructure
{
    public static class Frontend
    {
        public static Request<SaveResponse, InfrastructureConfiguration> SaveConfiguration { get; set; } = new(nameof(SaveConfiguration));

        public class SaveResponse : ApiResponse { }
    }
}
