using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IEmpireGenerator
    {
        IEnumerable<ObjectId> CreatePerSystem(int starSystemCount, IEnumerable<ObjectId> starSystemIds);

        void CreateForSecedingLeader(DeltaMetadata deltaMetadata, Empire empire, Leader leader);
    }
}