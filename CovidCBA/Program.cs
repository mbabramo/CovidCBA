using System;

namespace CovidCBA
{
    class Covid
    {
        public double BenefitOfLifePerPeriod;
        public double CostOfDistancingPerPeriod;
        public double RiskOfInfectionIfNotDistancingPerPeriod;
        public double RiskOfDeathGivenInfection;
        public double DiscountRate;
        public int PeriodsOfLife;
        public int PeriodsOfPandemic;

        public double Utility(int numPeriodsOfDistancing)
        {
            double totalBenefitsOfLife = 0;
            double totalCostsOfDistancing = 0;
            double discountFactor = 1.0;
            // The following three categories are assumed to add up to 1.0.
            double probabilityDead = 0;
            double probabilityImmune = 0;
            double probabilityNeverInfected = 1.0;
            for (int period = 0; period < PeriodsOfLife; period++)
            {
                double probabilityAlive = 1.0 - probabilityDead;
                double benefitThisPeriod = probabilityAlive * BenefitOfLifePerPeriod;
                double pastBenefits = totalBenefitsOfLife * discountFactor;
                totalBenefitsOfLife = benefitThisPeriod + pastBenefits;

                if (period < PeriodsOfPandemic)
                {
                    bool distancingIfNeverInfected = (PeriodsOfPandemic - (period + 1)) < numPeriodsOfDistancing;
                    double probabilityDistancing = distancingIfNeverInfected ? probabilityNeverInfected : 0;
                    double costOfDistancingThisPeriod = probabilityDistancing * CostOfDistancingPerPeriod;
                    double pastCosts = totalCostsOfDistancing * discountFactor;
                    totalCostsOfDistancing = costOfDistancingThisPeriod + pastCosts;
                    if (!distancingIfNeverInfected)
                    {
                        double probabilityInfectedThisPeriod = probabilityNeverInfected * RiskOfInfectionIfNotDistancingPerPeriod; // can only become infected if never infected before
                        double probabilityDeathThisPeriod = probabilityInfectedThisPeriod * RiskOfDeathGivenInfection; // can only die if infected this period
                        probabilityDead += probabilityAlive * probabilityDeathThisPeriod;
                        probabilityNeverInfected = probabilityNeverInfected - probabilityInfectedThisPeriod; // all of those infected were previously never infected
                        probabilityImmune = 1.0 - probabilityDead - probabilityNeverInfected;
                    }
                }
                discountFactor /= (1.0 + DiscountRate);
            }
            return totalBenefitsOfLife - totalCostsOfDistancing;
        }

        public int OptimizePeriodsOfDistancing()
        {
            int optimalValue = 0;
            double bestUtility = double.MinValue;
            for (int numPeriodsOfDistancing = 0; numPeriodsOfDistancing <= PeriodsOfPandemic; numPeriodsOfDistancing++)
            {
                double utility = Utility(numPeriodsOfDistancing);
                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    optimalValue = numPeriodsOfDistancing;
                }
            }
            return optimalValue;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Covid c = new Covid()
            {
                BenefitOfLifePerPeriod = 1.0,
                CostOfDistancingPerPeriod = 0.25,
                RiskOfInfectionIfNotDistancingPerPeriod = 0.05,
                RiskOfDeathGivenInfection = 0.01,
                DiscountRate = 0.0,
                PeriodsOfLife = 50 * 12
            };
            for (int pandemicLength = 1; pandemicLength <= 3 * 12; pandemicLength += 1)
            {
                c.PeriodsOfPandemic = pandemicLength;
                int optimizePeriods = c.OptimizePeriodsOfDistancing();
                Console.WriteLine($"Pandemic periods {pandemicLength} Optimal periods of distancing at end {optimizePeriods}");
            }
        }
    }
}
