using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IStarSystemRepository
    {
        void ApplyDevelopment(IEnumerable<Delta<double>> deltas);

        ConnectedSystemsView GetConnectedSystems(ObjectId systemId);
    }
}