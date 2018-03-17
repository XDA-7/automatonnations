using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IWarRepository
    {
        IEnumerable<War> GetWars(ObjectId simulationId);

        ObjectId BeginWar(ObjectId attackerId, ObjectId defenderId);

        void ContinueWar(ObjectId warId, int attackerDamage, int defenderDamage);

        void EndWar(ObjectId warId);
    }
}