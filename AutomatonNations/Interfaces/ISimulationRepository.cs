using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ISimulationRepository
    {
        Simulation GetSimulation(ObjectId simulationId);

        SimulationView GetSimulationView(ObjectId simulationId);

        ObjectId Create(ObjectId sectorId);
    }
}