using System;
using System.Collections.Generic;
using MineRefine.Models;

namespace MineRefine.Services
{
    public class WeatherService
    {
        private readonly WeatherSystem _weatherSystem;
        private readonly Random _random;

        public event EventHandler<WeatherCondition>? WeatherChanged;

        public WeatherService()
        {
            _weatherSystem = new WeatherSystem();
            _weatherSystem.InitializeWeatherEffects();
            _random = new Random();
        }

        public WeatherSystem GetWeatherSystem() => _weatherSystem;

        public WeatherCondition GetCurrentWeather() => _weatherSystem.CurrentWeather;

        public WeatherEffect GetCurrentWeatherEffect() => _weatherSystem.GetCurrentWeatherEffect();

        public void UpdateWeather()
        {
            var previousWeather = _weatherSystem.CurrentWeather;
            _weatherSystem.UpdateWeather();
            
            if (previousWeather != _weatherSystem.CurrentWeather)
            {
                OnWeatherChanged(_weatherSystem.CurrentWeather);
            }
        }

        public void ForceWeatherChange()
        {
            var previousWeather = _weatherSystem.CurrentWeather;
            _weatherSystem.ChangeWeather();
            
            if (previousWeather != _weatherSystem.CurrentWeather)
            {
                OnWeatherChanged(_weatherSystem.CurrentWeather);
            }
        }

        public void ApplyWeatherEffects(ref double miningEfficiency, ref double safetyModifier, ref double staminaCost, ref double quantumBonus, ref double rareBonus)
        {
            var effect = GetCurrentWeatherEffect();
            miningEfficiency *= effect.MiningEfficiency;
            safetyModifier *= effect.SafetyModifier;
            staminaCost *= effect.StaminaCost;
            quantumBonus *= effect.QuantumBonus;
            rareBonus *= effect.RareBonus;
        }

        public string GetWeatherDescription()
        {
            var effect = GetCurrentWeatherEffect();
            return $"{effect.Icon} {effect.Name}: {effect.Description}";
        }

        public string GetWeatherImpactDescription()
        {
            var effect = GetCurrentWeatherEffect();
            var impacts = new List<string>();

            if (effect.MiningEfficiency != 1.0)
            {
                var change = (effect.MiningEfficiency - 1.0) * 100;
                impacts.Add($"Mining Efficiency: {change:+0;-0}%");
            }

            if (effect.SafetyModifier != 1.0)
            {
                var change = (effect.SafetyModifier - 1.0) * 100;
                impacts.Add($"Safety: {change:+0;-0}%");
            }

            if (effect.StaminaCost != 1.0)
            {
                var change = (effect.StaminaCost - 1.0) * 100;
                impacts.Add($"Stamina Cost: {change:+0;-0}%");
            }

            if (effect.QuantumBonus > 1.0)
            {
                var bonus = (effect.QuantumBonus - 1.0) * 100;
                impacts.Add($"Quantum Bonus: +{bonus:0}%");
            }

            if (effect.RareBonus > 1.0)
            {
                var bonus = (effect.RareBonus - 1.0) * 100;
                impacts.Add($"Rare Find Bonus: +{bonus:0}%");
            }

            return impacts.Count > 0 ? string.Join(", ", impacts) : "No significant effects";
        }

        public TimeSpan GetTimeUntilWeatherChange()
        {
            var elapsed = DateTime.Parse("2025-07-31 13:29:22").Subtract(_weatherSystem.LastWeatherChange);
            var remaining = TimeSpan.FromMinutes(_weatherSystem.WeatherDuration) - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public bool IsExtremeWeather()
        {
            return _weatherSystem.CurrentWeather switch
            {
                WeatherCondition.Stormy => true,
                WeatherCondition.QuantumFlux => true,
                WeatherCondition.TemporalStorm => true,
                WeatherCondition.RealityDistortion => true,
                WeatherCondition.CosmicRadiation => true,
                WeatherCondition.DimensionalRift => true,
                _ => false
            };
        }

        public bool IsQuantumWeather()
        {
            return (int)_weatherSystem.CurrentWeather >= 8; // Quantum weather starts at index 8
        }

        public double GetWeatherRiskMultiplier()
        {
            var effect = GetCurrentWeatherEffect();
            return 2.0 - effect.SafetyModifier; // Convert safety modifier to risk multiplier
        }

        public void InitializeWeatherForPlayer(Player player)
        {
            player.CurrentWeather = _weatherSystem.CurrentWeather;
            player.LastWeatherChange = _weatherSystem.LastWeatherChange;
        }

        public void SyncPlayerWeather(Player player)
        {
            if (player.CurrentWeather != _weatherSystem.CurrentWeather)
            {
                player.CurrentWeather = _weatherSystem.CurrentWeather;
                player.LastWeatherChange = _weatherSystem.LastWeatherChange;
            }
        }

        private void OnWeatherChanged(WeatherCondition newWeather)
        {
            WeatherChanged?.Invoke(this, newWeather);
        }

        public string GetWeatherForecast()
        {
            // Simple forecast - predicts next likely weather
            var timeUntilChange = GetTimeUntilWeatherChange();
            var isQuantum = IsQuantumWeather();
            
            if (timeUntilChange.TotalMinutes > 5)
            {
                return $"Current weather will persist for {timeUntilChange.TotalMinutes:0} more minutes";
            }
            else
            {
                var forecast = isQuantum ? "Reality stabilization expected" : "Weather change imminent";
                return $"{forecast} in {timeUntilChange.TotalMinutes:0} minutes";
            }
        }

        public WeatherCondition GetRandomWeatherForLocation(string locationId)
        {
            // Different locations have different weather patterns
            return locationId switch
            {
                "surface_mine" => GetSurfaceWeather(),
                "underground_cavern" => GetUndergroundWeather(),
                "volcanic_depths" => GetVolcanicWeather(),
                "quantum_realm" => GetQuantumRealmWeather(),
                _ => WeatherCondition.Clear
            };
        }

        private WeatherCondition GetSurfaceWeather()
        {
            var surfaceWeathers = new[] 
            { 
                WeatherCondition.Clear, WeatherCondition.Cloudy, WeatherCondition.Rainy, 
                WeatherCondition.Stormy, WeatherCondition.Foggy, WeatherCondition.Snowy, WeatherCondition.Windy 
            };
            return surfaceWeathers[_random.Next(surfaceWeathers.Length)];
        }

        private WeatherCondition GetUndergroundWeather()
        {
            var undergroundWeathers = new[] 
            { 
                WeatherCondition.Clear, WeatherCondition.Foggy, WeatherCondition.Windy 
            };
            return undergroundWeathers[_random.Next(undergroundWeathers.Length)];
        }

        private WeatherCondition GetVolcanicWeather()
        {
            var volcanicWeathers = new[] 
            { 
                WeatherCondition.Clear, WeatherCondition.Stormy, WeatherCondition.CosmicRadiation 
            };
            return volcanicWeathers[_random.Next(volcanicWeathers.Length)];
        }

        private WeatherCondition GetQuantumRealmWeather()
        {
            var quantumWeathers = new[] 
            { 
                WeatherCondition.QuantumFlux, WeatherCondition.TemporalStorm, 
                WeatherCondition.RealityDistortion, WeatherCondition.DimensionalRift 
            };
            return quantumWeathers[_random.Next(quantumWeathers.Length)];
        }
    }
}