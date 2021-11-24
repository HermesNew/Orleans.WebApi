namespace Orleans.WebApi.Abstractions
{
    public interface IClusterFactory
    {
        public IGrainFactory GetCluster<IT,PK>(PK pk);
    }
}
