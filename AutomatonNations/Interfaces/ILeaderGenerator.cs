using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ILeaderGenerator
    {
        void GenerateLeadersForEmpire(DeltaMetadata deltaMetadata, ObjectId empireId);
    }
}