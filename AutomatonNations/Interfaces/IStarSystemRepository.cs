using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IStarSystemRepository
    {
        void ApplyDevelopment(IEnumerable<Delta<decimal>> deltas);

        ConnectedSystemsView GetConnectedSystems(ObjectId systemId);
    }
}