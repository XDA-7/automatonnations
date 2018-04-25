using AutomatonNations.Presentation;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class PresentationRepository : IPresentationRepository
    {
        private IMongoCollection<Presentation.Sector> _sectorCollection;

        public PresentationRepository(IDatabaseProvider databaseProvider)
        {
            _sectorCollection = databaseProvider.Database.GetCollection<Presentation.Sector>(Collections.PresentationSector);
        }

        public void Create(Presentation.Sector sector) => _sectorCollection.InsertOne(sector);
    }
}