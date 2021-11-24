
using Orleans;
using Orleans.WebApi.Abstractions;

namespace Sample.Gateway
{
    public class ClusterFactory : IClusterFactory
    {
        private readonly IClusterClient clusterClient;

        public ClusterFactory(IClusterClient clusterClient)
        {
            this.clusterClient = clusterClient;
        }
        public IGrainFactory GetCluster<IT, PK>(PK pk)
        {
            return clusterClient;
        }
    }
}
