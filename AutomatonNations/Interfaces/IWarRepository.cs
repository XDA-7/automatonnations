using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IWarRepository
    {
        IEnumerable<War> GetWars(ObjectId simulationId);

        IEnumerable<War> GetWarsForEmpire(ObjectId empireId);

        ObjectId BeginWar(DeltaMetadata deltaMetadata, ObjectId attackerId, ObjectId defenderId);

        void ContinueWar(DeltaMetadata deltaMetadata, ObjectId warId, double attackerDamage, double defenderDamage);

        void EndWar(DeltaMetadata deltaMetadata, ObjectId warId);

        void EndWarsWithParticipant(DeltaMetadata deltaMetadata, ObjectId empireId);
    }
}