using MongoDB.Bson;

namespace AutomatonNations
{
    public class War
    {
        public ObjectId Id { get; set; }
        
        public ObjectId AttackerId { get; set; }

        public ObjectId DefenderId { get; set; }

        public int Ticks { get; set; }

        public double AttackerDamage { get; set; }

        public double DefenderDamage { get; set; }

        public bool Active { get; set; }
    }
}