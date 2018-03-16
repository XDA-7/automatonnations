using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ISimulationRepository
    {
        Simulation GetSimulation(ObjectId simulationId);

        SimulationView GetSimulationView(ObjectId simulationId);

        ObjectId Create(ObjectId sectorId, IEnumerable<ObjectId> empireIds);

        void IncrementTicks(ObjectId simulationId, int ticks);
    }
}