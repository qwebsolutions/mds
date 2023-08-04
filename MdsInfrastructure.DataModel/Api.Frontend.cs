using Metapsi;
using Metapsi.Hyperapp;

namespace MdsInfrastructure
{
    public static class Frontend
    {
        public class SaveResponse : ApiResponse { }
        public static Request<SaveResponse, InfrastructureConfiguration> SaveConfiguration { get; set; } = new(nameof(SaveConfiguration));
    }
}
