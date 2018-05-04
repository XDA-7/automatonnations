using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class Leader
    {
        public IEnumerable<ObjectId> StarSystemIds { get; set; }

        public double Military { get; set; }
        
        public int MilitaryLevy { get; set; }

        public int SystemLimit { get; set; }

        public double IncomeRateBonus { get; set; }

        public double MilitaryWitholdingRate { get; set; }
    }
}