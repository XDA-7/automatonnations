using System.Collections.Generic;

namespace AutomatonNations
{
    public class MilitaryProductionResult
    {
        public double EmpireProduction { get; }

        public IEnumerable<Leader> UpdatedLeaders { get; }

        public MilitaryProductionResult(double empireProduction, IEnumerable<Leader> updatedLeaders)
        {
            EmpireProduction = empireProduction;
            UpdatedLeaders = updatedLeaders;
        }
    }
}