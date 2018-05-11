using System.Collections.Generic;

namespace AutomatonNations
{
    public class EmpireMilitaryDamageResult
    {
        public double EmpireDamage { get; }

        public IEnumerable<Leader> UpdatedLeaders { get; set; }

        public EmpireMilitaryDamageResult(double empireDamage, IEnumerable<Leader> updatedLeaders)
        {
            EmpireDamage = empireDamage;
            UpdatedLeaders = updatedLeaders;
        }
    }
}