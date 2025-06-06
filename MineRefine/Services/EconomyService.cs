using MineRefine.Models;
using System;
using System.Collections.Generic;

namespace MineRefine.Services
{
    public class EconomyService
    {
        private readonly Random _random = new();
        private readonly MarketService _marketService;
        private double _inflationRate = 0.02; // 2% annual inflation
        private double _economicCycle = 0.0; // -1 to 1, recession to boom
        private DateTime _lastEconomicUpdate = DateTime.UtcNow;

        public EconomyService(MarketService marketService)
        {
            _marketService = marketService;
        }

        public void UpdateEconomy()
        {
            if ((DateTime.UtcNow - _lastEconomicUpdate).TotalDays < 1) return;

            // Update economic cycle (boom/bust cycles)
            var cycleChange = (_random.NextDouble() - 0.5) * 0.1; // Small random changes
            _economicCycle = Math.Max(-1.0, Math.Min(1.0, _economicCycle + cycleChange));

            // Update inflation
            _inflationRate += (_random.NextDouble() - 0.5) * 0.005; // ±0.25% change
            _inflationRate = Math.Max(0.0, Math.Min(0.1, _inflationRate)); // 0-10% inflation

            _lastEconomicUpdate = DateTime.UtcNow;
        }

        public double GetEconomicMultiplier()
        {
            UpdateEconomy();
            
            // Economic cycle affects all prices
            var cycleMultiplier = 1.0 + (_economicCycle * 0.3); // ±30% based on cycle
            
            // Inflation affects purchasing power
            var inflationMultiplier = 1.0 - (_inflationRate * 0.5); // Reduces real value
            
            return cycleMultiplier * inflationMultiplier;
        }

        public string GetEconomicStatus()
        {
            return _economicCycle switch
            {
                > 0.5 => "🚀 Economic Boom",
                > 0.2 => "📈 Growth Period",
                > -0.2 => "📊 Stable Market",
                > -0.5 => "📉 Economic Decline",
                _ => "💥 Recession"
            };
        }

        public List<InvestmentOpportunity> GetInvestmentOpportunities(Player player)
        {
            var opportunities = new List<InvestmentOpportunity>();

            if (player.TotalMoney >= 100000)
            {
                opportunities.Add(new InvestmentOpportunity
                {
                    Name = "Mining Stock Portfolio",
                    Description = "Diversified mining company stocks",
                    Cost = 100000,
                    ExpectedReturn = 0.15,
                    RiskLevel = 2,
                    Duration = 30 // days
                });
            }

            if (player.TotalMoney >= 500000)
            {
                opportunities.Add(new InvestmentOpportunity
                {
                    Name = "Mineral Futures",
                    Description = "Bet on future mineral prices",
                    Cost = 500000,
                    ExpectedReturn = 0.25,
                    RiskLevel = 4,
                    Duration = 14
                });
            }

            if (player.TotalMoney >= 1000000)
            {
                opportunities.Add(new InvestmentOpportunity
                {
                    Name = "Mining Infrastructure",
                    Description = "Build automated mining facilities",
                    Cost = 1000000,
                    ExpectedReturn = 0.08,
                    RiskLevel = 1,
                    Duration = 90 // Passive income generator
                });
            }

            return opportunities;
        }

        public BusinessExpansion[] GetExpansionOptions(Player player)
        {
            return new[]
            {
                new BusinessExpansion
                {
                    Name = "Hire Mining Crew",
                    Description = "Hire workers to mine while you're away",
                    Cost = 250000,
                    Benefit = "Passive income: £1000/hour",
                    RequiredRank = 2
                },
                new BusinessExpansion
                {
                    Name = "Build Refinery",
                    Description = "Automated refining of collected minerals",
                    Cost = 750000,
                    Benefit = "Auto-refine collected minerals",
                    RequiredRank = 3
                },
                new BusinessExpansion
                {
                    Name = "Research Laboratory",
                    Description = "Develop new mining technologies",
                    Cost = 2000000,
                    Benefit = "Unlock unique equipment and bonuses",
                    RequiredRank = 4
                }
            };
        }
    }

    public class InvestmentOpportunity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long Cost { get; set; }
        public double ExpectedReturn { get; set; }
        public int RiskLevel { get; set; } // 1-5
        public int Duration { get; set; } // days
    }

    public class BusinessExpansion
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long Cost { get; set; }
        public string Benefit { get; set; } = string.Empty;
        public int RequiredRank { get; set; }
    }
}