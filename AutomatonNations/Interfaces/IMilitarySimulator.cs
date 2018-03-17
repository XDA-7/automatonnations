using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IMilitarySimulator
    {
        void Run(Simulation simulation);
    }
}