using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IWarRepository
    {
        IEnumerable<War> GetWars(ObjectId simulationId);

        ObjectId BeginWar(ObjectId simulationId, ObjectId attackerId, ObjectId defenderId);

        void ContinueWar(ObjectId warId, double attackerDamage, double defenderDamage);

        void EndWar(ObjectId warId);
    }
}