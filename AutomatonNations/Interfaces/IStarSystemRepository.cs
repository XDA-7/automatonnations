using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IStarSystemRepository
    {
        IEnumerable<StarSystem> GetForSimulation(ObjectId simulationId);
        
        void ApplyDevelopment(IEnumerable<Delta<double>> deltas);

        void ApplyDamage(IEnumerable<Delta<double>> deltas);

        ConnectedSystemsView GetConnectedSystems(ObjectId systemId);
    }
}