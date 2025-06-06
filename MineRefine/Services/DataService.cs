using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using MineRefine.Models;
using Windows.Storage;

namespace MineRefine.Services
{
    public class DataService
    {
        private readonly string _savesFileName = "saves.txt";
        private readonly string _settingsFileName = "settings.json";

        // Constants - Updated to exact current timestamp
        private const string CURRENT_DATETIME = "2025-06-06 20:57:47";
        private const string CURRENT_USER = "Valinor-70";

        public async Task<List<Player>> LoadPlayersAsync()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var file = await localFolder.GetFileAsync(_savesFileName);
                var content = await FileIO.ReadTextAsync(file);

                var players = new List<Player>();
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var player = JsonSerializer.Deserialize<Player>(line);
                    if (player != null)
                        players.Add(player);
                }

                return players;
            }
            catch (FileNotFoundException)
            {
                return new List<Player>();
            }
        }

        public async Task SavePlayersAsync(List<Player> players)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var file = await localFolder.CreateFileAsync(_savesFileName, CreationCollisionOption.ReplaceExisting);

                var lines = players.Select(player => JsonSerializer.Serialize(player));
                var content = string.Join("\n", lines);

                await FileIO.WriteTextAsync(file, content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving players: {ex.Message}");
            }
        }

        #region Enhanced Minerals - Compatible with All Difficulties

        public List<Mineral> GetMinerals()
        {
            return new List<Mineral>
            {
                #region Legendary Minerals (Ultimate Tier)
                new() {
                    Id = "refined_element",
                    Name = "Refined Element",
                    Description = "A perfectly refined elemental material of immense value",
                    Value = 10000000,
                    MineralType = MineralType.CRYSTAL,
                    RefinedValue = 20000000,
                    Weight = 0.5,
                    Rarity = "Legendary",
                    Icon = "🔮",
                    Color = "Prismatic",
                    UpgradeRequired = "Advanced Refinery"
                },
                new() {
                    Id = "platinum",
                    Name = "Platinum",
                    Description = "Rare precious metal with exceptional properties",
                    Value = 10000000,
                    MineralType = MineralType.PRECIOUS,
                    RefinedValue = 20000000,
                    Weight = 0.25,
                    Rarity = "Legendary",
                    Icon = "🥈",
                    Color = "Silver-White",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "volcanic_depths", "quantum_realm" }
                },
                new() {
                    Id = "void_crystal",
                    Name = "Void Crystal",
                    Description = "A mysterious crystal from the quantum realm that defies physics",
                    Value = 50000000,
                    MineralType = MineralType.CRYSTAL,
                    RefinedValue = 100000000,
                    Weight = 0.1,
                    Rarity = "Legendary",
                    Icon = "🌌",
                    Color = "Deep Purple",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "quantum_realm" }
                },
                new() {
                    Id = "antimatter_fragment",
                    Name = "Antimatter Fragment",
                    Description = "Dangerous but incredibly valuable antimatter sample from quantum realm",
                    Value = 25000000,
                    MineralType = MineralType.RADIOACTIVE,
                    RefinedValue = 50000000,
                    Weight = 0.2,
                    Rarity = "Legendary",
                    Icon = "☢️",
                    Color = "Energy Blue",
                    UpgradeRequired = "",
                    IsRadioactive = true,
                    FoundInLocations = new() { "quantum_realm" }
                },
                new() {
                    Id = "temporal_gem",
                    Name = "Temporal Gem",
                    Description = "A gem that exists across multiple timelines simultaneously",
                    Value = 75000000,
                    MineralType = MineralType.CRYSTAL,
                    RefinedValue = 150000000,
                    Weight = 0.15,
                    Rarity = "Legendary",
                    Icon = "⏰",
                    Color = "Temporal Shimmer",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "quantum_realm" }
                },
                #endregion

                #region Epic Minerals (High Tier)
                new() {
                    Id = "diamond",
                    Name = "Diamond",
                    Description = "The hardest known natural substance, extremely valuable",
                    Value = 1000000,
                    MineralType = MineralType.GEMSTONE,
                    RefinedValue = 2000000,
                    Weight = 1,
                    Rarity = "Epic",
                    Icon = "💎",
                    Color = "Clear",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "crystal_chambers", "deep_caverns" }
                },
                new() {
                    Id = "unrefined_element",
                    Name = "Unrefined Element",
                    Description = "Raw elemental material requiring advanced processing",
                    Value = 5000000,
                    MineralType = MineralType.RARE_EARTH,
                    RefinedValue = 10000000,
                    Weight = 0.75,
                    Rarity = "Epic",
                    Icon = "⚛️",
                    Color = "Metallic",
                    UpgradeRequired = "Basic Refinery"
                },
                new() {
                    Id = "palladium",
                    Name = "Palladium",
                    Description = "Rare silvery-white metal with exceptional catalytic properties",
                    Value = 5000000,
                    MineralType = MineralType.PRECIOUS,
                    RefinedValue = 10000000,
                    Weight = 0.5,
                    Rarity = "Epic",
                    Icon = "🥇",
                    Color = "Silver",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "volcanic_depths" }
                },
                new() {
                    Id = "rhodium",
                    Name = "Rhodium",
                    Description = "Ultra-rare and highly reflective precious metal",
                    Value = 2500000,
                    MineralType = MineralType.PRECIOUS,
                    RefinedValue = 5000000,
                    Weight = 0.75,
                    Rarity = "Epic",
                    Icon = "✨",
                    Color = "Bright Silver",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "iridium",
                    Name = "Iridium",
                    Description = "Extremely dense and corrosion-resistant metal",
                    Value = 1000000,
                    MineralType = MineralType.PRECIOUS,
                    RefinedValue = 2000000,
                    Weight = 1,
                    Rarity = "Epic",
                    Icon = "⭐",
                    Color = "White",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "quantum_ore",
                    Name = "Quantum Ore",
                    Description = "Ore with quantum properties affecting local space-time",
                    Value = 750000,
                    MineralType = MineralType.MAGNETIC,
                    RefinedValue = 1500000,
                    Weight = 1.5,
                    Rarity = "Epic",
                    Icon = "🧲",
                    Color = "Quantum Blue",
                    UpgradeRequired = "",
                    IsMagnetic = true,
                    FoundInLocations = new() { "magnetic_fields", "quantum_realm" }
                },
                #endregion

                #region Rare Minerals (Mid Tier)
                new() {
                    Id = "ruby",
                    Name = "Ruby",
                    Description = "Beautiful red gemstone prized for its vibrant color",
                    Value = 500000,
                    MineralType = MineralType.GEMSTONE,
                    RefinedValue = 1000000,
                    Weight = 2,
                    Rarity = "Rare",
                    Icon = "💎",
                    Color = "Deep Red",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "crystal_chambers" }
                },
                new() {
                    Id = "emerald",
                    Name = "Emerald",
                    Description = "Vibrant green gemstone of exceptional beauty",
                    Value = 250000,
                    MineralType = MineralType.GEMSTONE,
                    RefinedValue = 500000,
                    Weight = 3,
                    Rarity = "Rare",
                    Icon = "💚",
                    Color = "Emerald Green",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "crystal_chambers" }
                },
                new() {
                    Id = "sapphire",
                    Name = "Sapphire",
                    Description = "Brilliant blue gemstone of remarkable hardness",
                    Value = 350000,
                    MineralType = MineralType.GEMSTONE,
                    RefinedValue = 700000,
                    Weight = 2.5,
                    Rarity = "Rare",
                    Icon = "💙",
                    Color = "Royal Blue",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "crystal_chambers" }
                },
                new() {
                    Id = "osmium",
                    Name = "Osmium",
                    Description = "The densest naturally occurring element",
                    Value = 500000,
                    MineralType = MineralType.METALLIC,
                    RefinedValue = 1000000,
                    Weight = 1.5,
                    Rarity = "Rare",
                    Icon = "🔩",
                    Color = "Blue-Gray",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "ruthenium",
                    Name = "Ruthenium",
                    Description = "Rare transition metal with unique properties",
                    Value = 250000,
                    MineralType = MineralType.METALLIC,
                    RefinedValue = 500000,
                    Weight = 2,
                    Rarity = "Rare",
                    Icon = "⚙️",
                    Color = "Silver-Gray",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "amethyst",
                    Name = "Amethyst",
                    Description = "Beautiful purple crystal with mystical properties",
                    Value = 150000,
                    MineralType = MineralType.CRYSTAL,
                    RefinedValue = 300000,
                    Weight = 3,
                    Rarity = "Rare",
                    Icon = "🟣",
                    Color = "Purple",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "crystal_chambers" }
                },
                #endregion

                #region Uncommon Minerals (Common+ Tier)
                new() {
                    Id = "gold",
                    Name = "Gold",
                    Description = "Classic precious metal, universal symbol of wealth",
                    Value = 100000,
                    MineralType = MineralType.PRECIOUS,
                    RefinedValue = 200000,
                    Weight = 5,
                    Rarity = "Uncommon",
                    Icon = "🥇",
                    Color = "Golden Yellow",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "deep_caverns", "crystal_chambers" }
                },
                new() {
                    Id = "silver",
                    Name = "Silver",
                    Description = "Valuable white metal with excellent conductivity",
                    Value = 50000,
                    MineralType = MineralType.PRECIOUS,
                    RefinedValue = 100000,
                    Weight = 10,
                    Rarity = "Uncommon",
                    Icon = "🥈",
                    Color = "Silver",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "surface_mine", "deep_caverns" }
                },
                new() {
                    Id = "copper",
                    Name = "Copper",
                    Description = "Reddish metal essential for electrical applications",
                    Value = 25000,
                    MineralType = MineralType.METALLIC,
                    RefinedValue = 50000,
                    Weight = 15,
                    Rarity = "Uncommon",
                    Icon = "🔶",
                    Color = "Copper",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "surface_mine", "deep_caverns" }
                },
                new() {
                    Id = "magnetite",
                    Name = "Magnetite",
                    Description = "Natural magnetic iron ore with industrial applications",
                    Value = 10000,
                    MineralType = MineralType.MAGNETIC,
                    RefinedValue = 20000,
                    Weight = 20,
                    Rarity = "Uncommon",
                    Icon = "🧲",
                    Color = "Black",
                    UpgradeRequired = "",
                    IsMagnetic = true,
                    FoundInLocations = new() { "magnetic_fields", "surface_mine" }
                },
                new() {
                    Id = "crystal_shard",
                    Name = "Crystal Shard",
                    Description = "Fragment of a larger crystal formation",
                    Value = 75000,
                    MineralType = MineralType.CRYSTAL,
                    RefinedValue = 150000,
                    Weight = 8,
                    Rarity = "Uncommon",
                    Icon = "🔮",
                    Color = "Translucent",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "crystal_chambers" }
                },
                new() {
                    Id = "lead",
                    Name = "Lead",
                    Description = "Dense metal useful for radiation shielding",
                    Value = 15000,
                    MineralType = MineralType.METALLIC,
                    RefinedValue = 30000,
                    Weight = 25,
                    Rarity = "Uncommon",
                    Icon = "🔘",
                    Color = "Gray",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "deep_caverns" }
                },
                new() {
                    Id = "zinc",
                    Name = "Zinc",
                    Description = "Anti-corrosive metal essential for galvanization",
                    Value = 20000,
                    MineralType = MineralType.METALLIC,
                    RefinedValue = 40000,
                    Weight = 18,
                    Rarity = "Uncommon",
                    Icon = "🔵",
                    Color = "Bluish-Gray",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "deep_caverns" }
                },
                #endregion

                #region Common Minerals (Base Tier)
                new() {
                    Id = "iron_ore",
                    Name = "Iron Ore",
                    Description = "Essential iron ore for construction and basic tools",
                    Value = 5000,
                    MineralType = MineralType.MAGNETIC,
                    RefinedValue = 10000,
                    Weight = 25,
                    Rarity = "Common",
                    Icon = "⚙️",
                    Color = "Gray",
                    UpgradeRequired = "",
                    IsMagnetic = true,
                    FoundInLocations = new() { "surface_mine", "deep_caverns" }
                },
                new() {
                    Id = "coal",
                    Name = "Coal",
                    Description = "Fossil fuel and carbon source for energy production",
                    Value = 2500,
                    MineralType = MineralType.FOSSIL,
                    RefinedValue = 5000,
                    Weight = 30,
                    Rarity = "Common",
                    Icon = "⚫",
                    Color = "Black",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "surface_mine" }
                },
                new() {
                    Id = "quartz",
                    Name = "Quartz",
                    Description = "Common crystalline mineral with technological uses",
                    Value = 1000,
                    MineralType = MineralType.CRYSTAL,
                    RefinedValue = 2000,
                    Weight = 35,
                    Rarity = "Common",
                    Icon = "💎",
                    Color = "Clear",
                    UpgradeRequired = "",
                    FoundInLocations = new() { "surface_mine", "crystal_chambers" }
                },
                #endregion

                #region Gangue Minerals (Waste Rock)
                new() {
                    Id = "calcite",
                    Name = "Calcite",
                    Description = "Common carbonate mineral, useful for construction",
                    Value = 2500,
                    MineralType = MineralType.GANGUE,
                    RefinedValue = 5000,
                    Weight = 5,
                    Rarity = "Common",
                    Icon = "🪨",
                    Color = "White",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "dolomite",
                    Name = "Dolomite",
                    Description = "Carbonate rock mineral with industrial applications",
                    Value = 1000,
                    MineralType = MineralType.GANGUE,
                    RefinedValue = 2000,
                    Weight = 10,
                    Rarity = "Common",
                    Icon = "🪨",
                    Color = "Gray",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "feldspar",
                    Name = "Feldspar",
                    Description = "Rock-forming mineral used in ceramics",
                    Value = 500,
                    MineralType = MineralType.GANGUE,
                    RefinedValue = 1000,
                    Weight = 15,
                    Rarity = "Common",
                    Icon = "🪨",
                    Color = "Pink",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "gypsum",
                    Name = "Gypsum",
                    Description = "Soft sulfate mineral used in construction",
                    Value = 250,
                    MineralType = MineralType.GANGUE,
                    RefinedValue = 500,
                    Weight = 20,
                    Rarity = "Common",
                    Icon = "🪨",
                    Color = "White",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "barite",
                    Name = "Barite",
                    Description = "Dense barium sulfate mineral for drilling fluids",
                    Value = 100,
                    MineralType = MineralType.GANGUE,
                    RefinedValue = 200,
                    Weight = 1,
                    Rarity = "Common",
                    Icon = "🪨",
                    Color = "Clear",
                    UpgradeRequired = ""
                },
                new() {
                    Id = "fluorite",
                    Name = "Fluorite",
                    Description = "Colorful fluorine mineral with optical properties",
                    Value = 5,
                    MineralType = MineralType.GANGUE,
                    RefinedValue = 10,
                    Weight = 5,
                    Rarity = "Common",
                    Icon = "💜",
                    Color = "Purple",
                    UpgradeRequired = ""
                }
                #endregion
            };
        }

        public List<Mineral> GetEnhancedMinerals()
        {
            return GetMinerals(); // Use the main minerals list
        }

        #endregion

        #region Enhanced Mining Locations - Difficulty Scaled

        public List<MiningLocation> GetMiningLocations()
        {
            return new List<MiningLocation>
            {
                new()
                {
                    Id = "surface_mine",
                    Name = "Mountain Surface Mine",
                    Description = "A safe, open-pit mining operation perfect for beginners. Abundant common minerals with low danger levels.",
                    Icon = "🏔️",
                    RequiredRank = "BEGINNER",
                    RequiredLevel = 1,
                    UnlockCost = 0,
                    IsUnlocked = true,
                    DangerLevel = 1,
                    Depth = 1,
                    StaminaCost = 0.8,
                    BackgroundColor = "#8B7355",
                    Climate = "Temperate",
                    MineralBonuses = new() {
                        { "Coal", 1.2 },
                        { "Iron Ore", 1.1 },
                        { "Copper", 1.15 }
                    },
                    UniqueMinerals = new() { "coal", "iron_ore", "copper", "quartz" },
                    EnvironmentalHazards = new() { "Dust storms", "Equipment wear" },
                    Narrative = "Your journey begins here, where countless miners have started their careers. The surface mine offers a perfect introduction to the art of mineral extraction."
                },
                new()
                {
                    Id = "deep_caverns",
                    Name = "Underground Cavern System",
                    Description = "Ancient underground caverns with rich mineral deposits. Moderate danger from cave-ins and gas pockets.",
                    Icon = "🕳️",
                    RequiredRank = "INTERMEDIATE",
                    RequiredLevel = 5,
                    UnlockCost = 50000,
                    DangerLevel = 2,
                    Depth = 2,
                    StaminaCost = 1.2,
                    BackgroundColor = "#4A4A4A",
                    Climate = "Underground",
                    MineralBonuses = new() {
                        { "Gold", 1.3 },
                        { "Silver", 1.2 },
                        { "Copper", 1.5 },
                        { "Lead", 1.25 },
                        { "Zinc", 1.2 }
                    },
                    UniqueMinerals = new() { "gold", "silver", "lead", "zinc", "iron_ore" },
                    EnvironmentalHazards = new() { "Cave-ins", "Gas pockets", "Underground flooding" },
                    Narrative = "Deep beneath the earth's surface, these ancient caverns hold secrets from geological eras past. The risks are higher, but so are the rewards."
                },
                new()
                {
                    Id = "crystal_chambers",
                    Name = "Crystalline Chambers",
                    Description = "Mystical caves filled with precious crystals and gems. Home to the most beautiful minerals on Earth.",
                    Icon = "💎",
                    RequiredRank = "EXPERT",
                    RequiredLevel = 12,
                    UnlockCost = 250000,
                    DangerLevel = 3,
                    Depth = 3,
                    StaminaCost = 1.5,
                    BackgroundColor = "#663399",
                    Climate = "Mystical",
                    MineralBonuses = new() {
                        { "Diamond", 1.4 },
                        { "Ruby", 1.3 },
                        { "Emerald", 1.3 },
                        { "Sapphire", 1.2 },
                        { "Amethyst", 1.25 },
                        { "Crystal Shard", 1.5 }
                    },
                    UniqueMinerals = new() { "diamond", "ruby", "emerald", "sapphire", "amethyst", "crystal_shard", "quartz" },
                    EnvironmentalHazards = new() { "Crystal storms", "Energy fluctuations", "Unstable formations" },
                    Narrative = "Where nature's artistry meets geological wonder. These chambers have taken millions of years to form, creating the perfect environment for precious crystal growth."
                },
                new()
                {
                    Id = "volcanic_depths",
                    Name = "Volcanic Mineral Depths",
                    Description = "Dangerous volcanic mines with rare metals. Extreme high-temperature mining near active volcanic vents.",
                    Icon = "🌋",
                    RequiredRank = "EXPERT",
                    RequiredLevel = 20,
                    UnlockCost = 1000000,
                    DangerLevel = 4,
                    Depth = 4,
                    StaminaCost = 2.0,
                    BackgroundColor = "#CC3300",
                    Climate = "Volcanic",
                    MineralBonuses = new() {
                        { "Platinum", 1.5 },
                        { "Palladium", 1.4 },
                        { "Rhodium", 1.3 },
                        { "Iridium", 1.25 },
                        { "Gold", 1.2 }
                    },
                    UniqueMinerals = new() { "platinum", "palladium", "rhodium", "iridium" },
                    EnvironmentalHazards = new() { "Lava flows", "Toxic gases", "Extreme heat", "Volcanic eruptions" },
                    Narrative = "At the very edge of what's possible for human mining. The volcanic depths offer incredible mineral wealth for those brave enough to face the inferno."
                },
                new()
                {
                    Id = "abyssal_depths",
                    Name = "Abyssal Ocean Depths",
                    Description = "Deep-sea mining operation on the ocean floor. Advanced technology required for extreme pressure environment.",
                    Icon = "🌊",
                    RequiredRank = "MASTER",
                    RequiredLevel = 30,
                    UnlockCost = 2000000,
                    DangerLevel = 4,
                    Depth = 5,
                    StaminaCost = 2.5,
                    BackgroundColor = "#001133",
                    Climate = "Abyssal",
                    MineralBonuses = new() {
                        { "Osmium", 1.6 },
                        { "Ruthenium", 1.4 },
                        { "Platinum", 1.3 },
                        { "Rare Earth", 1.5 }
                    },
                    UniqueMinerals = new() { "osmium", "ruthenium", "unrefined_element" },
                    EnvironmentalHazards = new() { "Crushing pressure", "Equipment failure", "Deep-sea creatures", "Underwater currents" },
                    Narrative = "The final frontier of terrestrial mining. Here, in the deepest trenches of the ocean, lie mineral treasures beyond imagination."
                },
                new()
                {
                    Id = "quantum_realm",
                    Name = "Quantum Mining Dimension",
                    Description = "A dimension beyond reality where physics bend and impossible minerals exist. Ultimate danger, ultimate rewards.",
                    Icon = "🌌",
                    RequiredRank = "ASCENDED_MINER",
                    RequiredLevel = 45,
                    UnlockCost = 10000000,
                    DangerLevel = 5,
                    Depth = 6,
                    StaminaCost = 3.0,
                    BackgroundColor = "#000033",
                    Climate = "Quantum Flux",
                    MineralBonuses = new() {
                        { "Void Crystal", 2.0 },
                        { "Antimatter Fragment", 1.8 },
                        { "Quantum Ore", 1.6 },
                        { "Temporal Gem", 2.2 },
                        { "Refined Element", 1.5 }
                    },
                    UniqueMinerals = new() { "void_crystal", "antimatter_fragment", "quantum_ore", "temporal_gem", "refined_element" },
                    EnvironmentalHazards = new() { "Reality distortion", "Quantum flux", "Temporal anomalies", "Dimensional rifts", "Consciousness fragmentation" },
                    Narrative = "Beyond the boundaries of known physics lies the Quantum Realm. Here, the very nature of reality becomes malleable, and miners can discover materials that shouldn't exist."
                },
                new()
                {
                    Id = "magnetic_fields",
                    Name = "Electromagnetic Anomaly Zone",
                    Description = "Electromagnetic anomaly zones rich in magnetic minerals. Equipment interference is common but rewards are substantial.",
                    Icon = "🧲",
                    RequiredRank = "EXPERT",
                    RequiredLevel = 15,
                    UnlockCost = 500000,
                    DangerLevel = 3,
                    Depth = 3,
                    StaminaCost = 1.8,
                    BackgroundColor = "#006666",
                    Climate = "Electromagnetic",
                    MineralBonuses = new() {
                        { "Magnetite", 2.0 },
                        { "Quantum Ore", 1.5 },
                        { "Iron Ore", 1.3 }
                    },
                    UniqueMinerals = new() { "magnetite", "quantum_ore", "iron_ore" },
                    EnvironmentalHazards = new() { "Magnetic storms", "Equipment interference", "Compass disruption" },
                    Narrative = "Where the Earth's magnetic field creates impossible concentrations of magnetic minerals. Your equipment may malfunction, but the rewards make it worthwhile."
                }
            };
        }

        public List<MiningLocation> GetEnhancedMiningLocations()
        {
            return GetMiningLocations(); // Use the main locations list
        }

        #endregion

        #region Market Data - Real-time Current Prices

        public List<MarketData> GetMarketData()
        {
            var currentTime = DateTime.Parse(CURRENT_DATETIME);
            var random = new Random(currentTime.DayOfYear + currentTime.Hour);
            var minerals = GetMinerals();
            var marketData = new List<MarketData>();

            // Top 20 minerals for market display
            var topMinerals = minerals.Where(m => m.Value >= 1000).OrderByDescending(m => m.Value).Take(20);

            foreach (var mineral in topMinerals)
            {
                var trend = random.NextDouble() * 2.0 - 1.0; // -1.0 to 1.0
                var trendIcon = trend > 0.1 ? "📈" : trend < -0.1 ? "📉" : "➡️";
                var sentiment = trend > 0.3 ? "Bullish" : trend < -0.3 ? "Bearish" : "Neutral";

                // Create more realistic price fluctuations
                var baseMultiplier = 1.0;
                var dailyVolatility = mineral.Rarity switch
                {
                    "Legendary" => 0.15, // 15% daily volatility
                    "Epic" => 0.12,      // 12% daily volatility
                    "Rare" => 0.08,      // 8% daily volatility
                    "Uncommon" => 0.05,  // 5% daily volatility
                    "Common" => 0.03,    // 3% daily volatility
                    _ => 0.05
                };

                var priceMultiplier = Math.Max(0.5, Math.Min(1.8, baseMultiplier + (trend * dailyVolatility)));

                marketData.Add(new MarketData
                {
                    MineralName = mineral.Name,
                    PriceMultiplier = priceMultiplier,
                    TrendIcon = trendIcon,
                    Trend = trend > 0.05 ? "Rising" : trend < -0.05 ? "Falling" : "Stable",
                    LastUpdated = currentTime.AddMinutes(-random.Next(1, 60)),
                    LastUpdate = currentTime.AddMinutes(-random.Next(1, 60)),
                    PriceHistory = Enumerable.Range(0, 7).Select(i =>
                        1.0 + ((random.NextDouble() - 0.5) * dailyVolatility * 2)).ToList(),
                    MarketSentiment = sentiment,
                    Volume = random.Next(100, 10000),
                    DailyHigh = priceMultiplier * (1.0 + random.NextDouble() * 0.1),
                    DailyLow = priceMultiplier * (1.0 - random.NextDouble() * 0.1)
                });
            }

            return marketData.OrderByDescending(m => m.PriceMultiplier).ToList();
        }

        #endregion

        #region Economic Events - Current Global Situation

        public List<EconomicEvent> GetEconomicEvents()
        {
            var currentTime = DateTime.Parse(CURRENT_DATETIME);

            return new List<EconomicEvent>
            {
                new()
                {
                    Id = "quantum_tech_boom_2025",
                    Name = "Quantum Technology Revolution",
                    Description = "Breakthrough in quantum computing drives massive demand for quantum materials (+60% for quantum minerals)",
                    Icon = "🚀",
                    StartDate = currentTime.AddHours(-8),
                    DurationDays = 5,
                    Duration = 5,
                    Severity = "Critical",
                    MineralEffects = new()
                    {
                        { "Quantum Ore", 1.60 },
                        { "Void Crystal", 1.40 },
                        { "Antimatter Fragment", 1.35 },
                        { "Temporal Gem", 1.45 }
                    }
                },
                new()
                {
                    Id = "precious_metals_surge_2025",
                    Name = "Global Precious Metals Surge",
                    Description = "Economic uncertainty drives investors to precious metals, increasing prices by 30%",
                    Icon = "📈",
                    StartDate = currentTime.AddHours(-12),
                    DurationDays = 4,
                    Duration = 4,
                    Severity = "High",
                    MineralEffects = new()
                    {
                        { "Gold", 1.30 },
                        { "Silver", 1.25 },
                        { "Platinum", 1.35 },
                        { "Palladium", 1.28 }
                    }
                },
                new()
                {
                    Id = "energy_transition_2025",
                    Name = "Clean Energy Transition",
                    Description = "Massive shift to renewable energy increases demand for rare earth elements (+25%)",
                    Icon = "⚡",
                    StartDate = currentTime.AddDays(-2),
                    DurationDays = 7,
                    Duration = 7,
                    Severity = "Normal",
                    MineralEffects = new()
                    {
                        { "Unrefined Element", 1.25 },
                        { "Ruthenium", 1.20 },
                        { "Osmium", 1.15 },
                        { "Copper", 1.10 }
                    }
                },
                new()
                {
                    Id = "gem_market_volatility_2025",
                    Name = "Luxury Market Volatility",
                    Description = "Fluctuating luxury demand affects gemstone prices (-15% for decorative gems)",
                    Icon = "📊",
                    StartDate = currentTime.AddHours(-18),
                    DurationDays = 3,
                    Duration = 3,
                    Severity = "Low",
                    MineralEffects = new()
                    {
                        { "Diamond", 0.85 },
                        { "Ruby", 0.82 },
                        { "Emerald", 0.88 },
                        { "Sapphire", 0.90 }
                    }
                },
                new()
                {
                    Id = "magnetic_materials_crisis_2025",
                    Name = "Magnetic Materials Crisis",
                    Description = "Supply shortage for magnetic materials drives prices up by 50%",
                    Icon = "🧲",
                    StartDate = currentTime.AddHours(-6),
                    DurationDays = 6,
                    Duration = 6,
                    Severity = "High",
                    MineralEffects = new()
                    {
                        { "Magnetite", 1.50 },
                        { "Iron Ore", 1.20 },
                        { "Quantum Ore", 1.25 }
                    }
                }
            };
        }

        #endregion

        #region Comprehensive Achievement System

        public List<Achievement> GetAchievements()
        {
            return new List<Achievement>
            {
                #region Wealth Achievements
                new() {
                    Id = "first_thousand",
                    Name = "First Steps to Wealth",
                    Description = "Earn your first £1,000 - the beginning of your mining empire",
                    Type = AchievementType.TotalMoney,
                    Target = 1000,
                    Icon = "💰",
                    RewardMoney = 500,
                    RewardSkillPoints = 1,
                    Category = "Wealth",
                    Difficulty = "Easy"
                },
                new() {
                    Id = "ten_thousand",
                    Name = "Getting Prosperous",
                    Description = "Accumulate £10,000 in your mining accounts",
                    Type = AchievementType.TotalMoney,
                    Target = 10000,
                    Icon = "💵",
                    RewardMoney = 2000,
                    RewardSkillPoints = 2,
                    Category = "Wealth",
                    Difficulty = "Normal"
                },
                new() {
                    Id = "hundred_thousand",
                    Name = "Wealthy Miner",
                    Description = "Reach the milestone of £100,000 in wealth",
                    Type = AchievementType.TotalMoney,
                    Target = 100000,
                    Icon = "💎",
                    RewardMoney = 10000,
                    RewardSkillPoints = 3,
                    Category = "Wealth",
                    Difficulty = "Normal"
                },
                new() {
                    Id = "millionaire",
                    Name = "Mining Millionaire",
                    Description = "Join the exclusive millionaire miners club",
                    Type = AchievementType.TotalMoney,
                    Target = 1000000,
                    Icon = "🏆",
                    RewardMoney = 100000,
                    RewardSkillPoints = 5,
                    Category = "Wealth",
                    Difficulty = "Hard"
                },
                new() {
                    Id = "ten_million",
                    Name = "Mining Tycoon",
                    Description = "Amass an incredible £10,000,000 fortune",
                    Type = AchievementType.TotalMoney,
                    Target = 10000000,
                    Icon = "👑",
                    RewardMoney = 1000000,
                    RewardSkillPoints = 10,
                    Category = "Wealth",
                    Difficulty = "Expert"
                },
                new() {
                    Id = "hundred_million",
                    Name = "Mining Magnate",
                    Description = "Achieve the legendary £100,000,000 milestone",
                    Type = AchievementType.TotalMoney,
                    Target = 100000000,
                    Icon = "💸",
                    RewardMoney = 10000000,
                    RewardSkillPoints = 20,
                    Category = "Wealth",
                    Difficulty = "Legendary"
                },
                #endregion

                #region Mining Count Achievements
                new() {
                    Id = "first_mine",
                    Name = "First Strike",
                    Description = "Complete your very first mining operation",
                    Type = AchievementType.TotalMines,
                    Target = 1,
                    Icon = "⛏️",
                    RewardMoney = 100,
                    RewardSkillPoints = 1,
                    Category = "Mining",
                    Difficulty = "Easy"
                },
                new() {
                    Id = "hundred_mines",
                    Name = "Century Miner",
                    Description = "Complete 100 mining operations",
                    Type = AchievementType.TotalMines,
                    Target = 100,
                    Icon = "🔨",
                    RewardMoney = 5000,
                    RewardSkillPoints = 3,
                    Category = "Mining",
                    Difficulty = "Normal"
                },
                new() {
                    Id = "thousand_mines",
                    Name = "Veteran Miner",
                    Description = "Complete 1,000 mining operations",
                    Type = AchievementType.TotalMines,
                    Target = 1000,
                    Icon = "⭐",
                    RewardMoney = 25000,
                    RewardSkillPoints = 5,
                    Category = "Mining",
                    Difficulty = "Hard"
                },
                new() {
                    Id = "ten_thousand_mines",
                    Name = "Mining Legend",
                    Description = "Complete an incredible 10,000 mining operations",
                    Type = AchievementType.TotalMines,
                    Target = 10000,
                    Icon = "🌟",
                    RewardMoney = 100000,
                    RewardSkillPoints = 10,
                    Category = "Mining",
                    Difficulty = "Expert"
                },
                #endregion

                #region Special Mineral Discovery Achievements
                new() {
                    Id = "first_diamond",
                    Name = "Diamond Hunter",
                    Description = "Discover your first precious diamond",
                    Type = AchievementType.SpecificMineral,
                    Target = 1,
                    Icon = "💎",
                    RewardMoney = 50000,
                    RewardSkillPoints = 3,
                    Category = "Discovery",
                    Difficulty = "Hard"
                },
                new() {
                    Id = "platinum_finder",
                    Name = "Platinum Pioneer",
                    Description = "Unearth the ultra-rare platinum",
                    Type = AchievementType.SpecificMineral,
                    Target = 1,
                    Icon = "🥇",
                    RewardMoney = 100000,
                    RewardSkillPoints = 5,
                    Category = "Discovery",
                    Difficulty = "Expert"
                },
                new() {
                    Id = "void_crystal_master",
                    Name = "Void Walker",
                    Description = "Obtain the legendary Void Crystal from the quantum realm",
                    Type = AchievementType.SpecificMineral,
                    Target = 1,
                    Icon = "🌌",
                    RewardMoney = 500000,
                    RewardSkillPoints = 15,
                    Category = "Discovery",
                    Difficulty = "Legendary"
                },
                new() {
                    Id = "antimatter_collector",
                    Name = "Antimatter Collector",
                    Description = "Successfully contain an antimatter fragment",
                    Type = AchievementType.SpecificMineral,
                    Target = 1,
                    Icon = "☢️",
                    RewardMoney = 750000,
                    RewardSkillPoints = 12,
                    Category = "Discovery",
                    Difficulty = "Legendary"
                },
                new() {
                    Id = "temporal_gem_finder",
                    Name = "Time Master",
                    Description = "Discover a temporal gem that exists across timelines",
                    Type = AchievementType.SpecificMineral,
                    Target = 1,
                    Icon = "⏰",
                    RewardMoney = 1000000,
                    RewardSkillPoints = 20,
                    Category = "Discovery",
                    Difficulty = "Legendary"
                },
                #endregion

                #region Location Exploration Achievements
                new() {
                    Id = "surface_graduate",
                    Name = "Surface Graduate",
                    Description = "Master the surface mine and move to greater challenges",
                    Type = AchievementType.LocationMastery,
                    Target = 1,
                    Icon = "🏔️",
                    RewardMoney = 2500,
                    RewardSkillPoints = 2,
                    Category = "Exploration",
                    Difficulty = "Easy"
                },
                new() {
                    Id = "deep_explorer",
                    Name = "Deep Cave Explorer",
                    Description = "Unlock and explore the underground cavern system",
                    Type = AchievementType.LocationUnlock,
                    Target = 1,
                    Icon = "🕳️",
                    RewardMoney = 10000,
                    RewardSkillPoints = 3,
                    Category = "Exploration",
                    Difficulty = "Normal"
                },
                new() {
                    Id = "crystal_spelunker",
                    Name = "Crystal Chamber Spelunker",
                    Description = "Gain access to the mystical crystalline chambers",
                    Type = AchievementType.LocationUnlock,
                    Target = 1,
                    Icon = "💎",
                    RewardMoney = 50000,
                    RewardSkillPoints = 5,
                    Category = "Exploration",
                    Difficulty = "Hard"
                },
                new() {
                    Id = "volcano_diver",
                    Name = "Volcanic Depth Diver",
                    Description = "Brave the extreme heat of volcanic mining depths",
                    Type = AchievementType.LocationUnlock,
                    Target = 1,
                    Icon = "🌋",
                    RewardMoney = 200000,
                    RewardSkillPoints = 8,
                    Category = "Exploration",
                    Difficulty = "Expert"
                },
                new() {
                    Id = "abyssal_pioneer",
                    Name = "Abyssal Pioneer",
                    Description = "Descend to the crushing depths of the ocean floor",
                    Type = AchievementType.LocationUnlock,
                    Target = 1,
                    Icon = "🌊",
                    RewardMoney = 500000,
                    RewardSkillPoints = 10,
                    Category = "Exploration",
                    Difficulty = "Expert"
                },
                new() {
                    Id = "quantum_pioneer",
                    Name = "Quantum Realm Pioneer",
                    Description = "Access the reality-bending quantum mining dimension",
                    Type = AchievementType.LocationUnlock,
                    Target = 1,
                    Icon = "🌌",
                    RewardMoney = 1000000,
                    RewardSkillPoints = 20,
                    Category = "Exploration",
                    Difficulty = "Legendary"
                },
                new() {
                    Id = "magnetic_master",
                    Name = "Magnetic Field Master",
                    Description = "Navigate the electromagnetic anomaly zones",
                    Type = AchievementType.LocationUnlock,
                    Target = 1,
                    Icon = "🧲",
                    RewardMoney = 150000,
                    RewardSkillPoints = 6,
                    Category = "Exploration",
                    Difficulty = "Hard"
                },
                #endregion

                #region Skill and Performance Achievements
                new() {
                    Id = "lucky_streak",
                    Name = "Lucky Mining Streak",
                    Description = "Complete 10 successful mining operations in a row",
                    Type = AchievementType.ConsecutiveSuccess,
                    Target = 10,
                    Icon = "🍀",
                    RewardMoney = 15000,
                    RewardSkillPoints = 3,
                    Category = "Skills",
                    Difficulty = "Normal"
                },
                new() {
                    Id = "master_streak",
                    Name = "Master Mining Streak",
                    Description = "Achieve 25 consecutive successful mining operations",
                    Type = AchievementType.ConsecutiveSuccess,
                    Target = 25,
                    Icon = "🔥",
                    RewardMoney = 50000,
                    RewardSkillPoints = 6,
                    Category = "Skills",
                    Difficulty = "Hard"
                },
                new() {
                    Id = "legendary_streak",
                    Name = "Legendary Mining Streak",
                    Description = "Accomplish 50 consecutive successful mining operations",
                    Type = AchievementType.ConsecutiveSuccess,
                    Target = 50,
                    Icon = "⚡",
                    RewardMoney = 150000,
                    RewardSkillPoints = 10,
                    Category = "Skills",
                    Difficulty = "Expert"
                },
                new() {
                    Id = "skill_collector",
                    Name = "Skill Point Collector",
                    Description = "Accumulate 50 skill points for character development",
                    Type = AchievementType.SkillPoints,
                    Target = 50,
                    Icon = "📚",
                    RewardMoney = 25000,
                    RewardSkillPoints = 5,
                    Category = "Skills",
                    Difficulty = "Hard"
                },
                new() {
                    Id = "skill_master",
                    Name = "Skill Tree Master",
                    Description = "Earn 100 skill points and master your abilities",
                    Type = AchievementType.SkillPoints,
                    Target = 100,
                    Icon = "🎓",
                    RewardMoney = 100000,
                    RewardSkillPoints = 10,
                    Category = "Skills",
                    Difficulty = "Expert"
                },
                #endregion

                #region Rank Progression Achievements
                new() {
                    Id = "experienced_promotion",
                    Name = "Experienced Miner Promotion",
                    Description = "Advance to Experienced Miner rank",
                    Type = AchievementType.RankAdvancement,
                    Target = 1,
                    Icon = "📋",
                    RewardMoney = 5000,
                    RewardSkillPoints = 3,
                    Category = "Progression",
                    Difficulty = "Normal"
                },
                new() {
                    Id = "expert_promotion",
                    Name = "Expert Miner Promotion",
                    Description = "Achieve Expert Miner status",
                    Type = AchievementType.RankAdvancement,
                    Target = 1,
                    Icon = "🎖️",
                    RewardMoney = 25000,
                    RewardSkillPoints = 5,
                    Category = "Progression",
                    Difficulty = "Hard"
                },
                new() {
                    Id = "master_promotion",
                    Name = "Master Miner Promotion",
                    Description = "Become a Master Miner",
                    Type = AchievementType.RankAdvancement,
                    Target = 1,
                    Icon = "🏅",
                    RewardMoney = 100000,
                    RewardSkillPoints = 8,
                    Category = "Progression",
                    Difficulty = "Expert"
                },
                new() {
                    Id = "legendary_promotion",
                    Name = "Legendary Miner Promotion",
                    Description = "Attain the prestigious Legendary Miner rank",
                    Type = AchievementType.RankAdvancement,
                    Target = 1,
                    Icon = "🥇",
                    RewardMoney = 500000,
                    RewardSkillPoints = 12,
                    Category = "Progression",
                    Difficulty = "Expert"
                },
                new() {
                    Id = "ascended_promotion",
                    Name = "Ascended Miner Transcendence",
                    Description = "Transcend to the ultimate Ascended Miner rank",
                    Type = AchievementType.RankAdvancement,
                    Target = 1,
                    Icon = "👑",
                    RewardMoney = 2500000,
                    RewardSkillPoints = 25,
                    Category = "Progression",
                    Difficulty = "Legendary"
                }
                #endregion
            };
        }

        public List<Achievement> GetComprehensiveAchievements()
        {
            return GetAchievements(); // Use the main achievements list
        }

        #endregion

        #region Daily Challenges - Difficulty Adaptive

        public List<DailyChallenge> GetDailyChallenges()
        {
            var currentTime = DateTime.Parse(CURRENT_DATETIME);
            var today = currentTime.Date;
            var random = new Random(today.DayOfYear + currentTime.Hour);

            var challenges = new List<DailyChallenge>();

            // Define challenge templates based on difficulty and current date
            var challengeTemplates = new[]
            {
                // Beginner Friendly Challenges
                ("Daily Earnings", "Earn £50,000 today", 50000L, "", "💰", "Easy"),
                ("Mining Practice", "Complete 20 mining operations", 20L, "", "⛏️", "Easy"),
                ("Surface Mining", "Mine 15 times in Surface Mine", 15L, "surface_mine", "🏔️", "Easy"),
                ("Coal Collection", "Collect 10 Coal minerals", 10L, "Coal", "⚫", "Easy"),
                ("Iron Gathering", "Find 8 Iron Ore samples", 8L, "Iron Ore", "⚙️", "Easy"),
                
                // Intermediate Challenges
                ("Gold Rush", "Mine 5 Gold minerals", 5L, "Gold", "🥇", "Normal"),
                ("Big Earner", "Earn £200,000 today", 200000L, "", "💵", "Normal"),
                ("Deep Explorer", "Mine 10 times in Deep Caverns", 10L, "deep_caverns", "🕳️", "Normal"),
                ("Silver Collection", "Gather 6 Silver minerals", 6L, "Silver", "🥈", "Normal"),
                ("Mining Marathon", "Complete 50 mining operations", 50L, "", "🏃", "Normal"),
                
                // Advanced Challenges
                ("Crystal Hunter", "Find 3 Crystal Shards", 3L, "Crystal Shard", "🔮", "Hard"),
                ("Precious Metals", "Collect 2 Platinum samples", 2L, "Platinum", "🥈", "Hard"),
                ("Million Maker", "Earn £1,000,000 today", 1000000L, "", "💎", "Hard"),
                ("Volcanic Explorer", "Mine 5 times in Volcanic Depths", 5L, "volcanic_depths", "🌋", "Hard"),
                ("Diamond Quest", "Discover 1 Diamond", 1L, "Diamond", "💎", "Hard"),
                
                // Expert/Legendary Challenges
                ("Quantum Mining", "Mine 3 times in Quantum Realm", 3L, "quantum_realm", "🌌", "Expert"),
                ("Void Crystal Hunt", "Find 1 Void Crystal", 1L, "Void Crystal", "🌌", "Legendary"),
                ("Antimatter Search", "Collect 1 Antimatter Fragment", 1L, "Antimatter Fragment", "☢️", "Legendary"),
                ("Ultra Wealth", "Earn £10,000,000 today", 10000000L, "", "👑", "Expert"),
                ("Temporal Quest", "Discover 1 Temporal Gem", 1L, "Temporal Gem", "⏰", "Legendary")
            };

            // Select 3-5 challenges based on current date and time
            var selectedChallenges = challengeTemplates
                .OrderBy(_ => random.Next())
                .Take(random.Next(3, 6))
                .ToArray();

            for (int i = 0; i < selectedChallenges.Length; i++)
            {
                var challenge = selectedChallenges[i];
                var baseReward = challenge.Item6 switch
                {
                    "Easy" => random.Next(5000, 15000),
                    "Normal" => random.Next(15000, 35000),
                    "Hard" => random.Next(35000, 75000),
                    "Expert" => random.Next(75000, 150000),
                    "Legendary" => random.Next(150000, 300000),
                    _ => 10000
                };

                var skillReward = challenge.Item6 switch
                {
                    "Easy" => random.Next(1, 3),
                    "Normal" => random.Next(2, 4),
                    "Hard" => random.Next(3, 6),
                    "Expert" => random.Next(5, 8),
                    "Legendary" => random.Next(8, 15),
                    _ => 2
                };

                challenges.Add(new DailyChallenge
                {
                    Id = $"daily_{today:yyyyMMdd}_{i}_{CURRENT_USER}",
                    Name = challenge.Item1,
                    Title = challenge.Item1,
                    Description = challenge.Item2,
                    Type = ChallengeType.Daily,
                    Target = challenge.Item3,
                    StartDate = today,
                    EndDate = today.AddDays(1),
                    ExpiryDate = today.AddDays(1),
                    Icon = challenge.Item5,
                    Difficulty = challenge.Item6,
                    Reward = new Reward
                    {
                        Money = baseReward,
                        SkillPoints = skillReward,
                        Description = $"Daily challenge reward for {challenge.Item1}"
                    },
                    RewardMoney = baseReward,
                    RewardSkillPoints = skillReward,
                    Progress = 0,
                    IsCompleted = false,
                    RequiredMineral = string.IsNullOrEmpty(challenge.Item4) ? null : challenge.Item4,
                    RequiredLocation = string.IsNullOrEmpty(challenge.Item4) && challenge.Item4.Contains("_") ? challenge.Item4 : null
                });
            }

            return challenges.OrderBy(c => c.Difficulty switch { "Easy" => 1, "Normal" => 2, "Hard" => 3, "Expert" => 4, "Legendary" => 5, _ => 3 }).ToList();
        }

        #endregion

        #region Equipment System - Compatible with All Classes

        public List<Equipment> GetStartingEquipment()
        {
            return new List<Equipment>
            {
                new()
                {
                    Id = "basic_pickaxe",
                    Name = "Standard Mining Pickaxe",
                    Description = "A reliable mining pickaxe perfect for beginners. Built to last.",
                    Type = EquipmentType.Pickaxe,
                    Durability = 100,
                    MaxDurability = 100,
                    Level = 1,
                    Icon = "⛏️",
                    Rarity = "Common",
                    Bonuses = new() { { "mining_efficiency", 1.0 }, { "stamina_efficiency", 0.05 } },
                    UpgradeCost = 5000,
                    PurchaseCost = 2500,
                    IsEquipped = true,
                    SpecialAbilities = new() { "Basic Mining", "Durability Tracking" }
                },
                new()
                {
                    Id = "mining_helmet",
                    Name = "Professional Mining Helmet",
                    Description = "Advanced protective headgear with integrated lighting for underground operations",
                    Type = EquipmentType.Helmet,
                    Durability = 100,
                    MaxDurability = 100,
                    Level = 1,
                    Icon = "⛑️",
                    Rarity = "Common",
                    Bonuses = new() { { "safety", 0.1 }, { "visibility", 0.15 } },
                    UpgradeCost = 3000,
                    PurchaseCost = 1500,
                    IsEquipped = true,
                    SpecialAbilities = new() { "Head Protection", "Emergency Lighting" }
                },
                                new()
                {
                    Id = "basic_lantern",
                    Name = "High-Intensity Mining Lantern",
                    Description = "Essential LED lighting system for dark mining environments",
                    Type = EquipmentType.Lantern,
                    Durability = 100,
                    MaxDurability = 100,
                    Level = 1,
                    Icon = "🔦",
                    Rarity = "Common",
                    Bonuses = new() { { "visibility", 0.2 }, { "danger_reduction", 0.05 } },
                    UpgradeCost = 2000,
                    PurchaseCost = 1000,
                    IsEquipped = true,
                    SpecialAbilities = new() { "Cave Illumination", "Emergency Beacon" }
                },
                new()
                {
                    Id = "safety_gear",
                    Name = "Complete Safety Gear Set",
                    Description = "Professional mining safety equipment including boots, gloves, and protective clothing",
                    Type = EquipmentType.Armor,
                    Durability = 100,
                    MaxDurability = 100,
                    Level = 1,
                    Icon = "🦺",
                    Rarity = "Common",
                    Bonuses = new() { { "safety", 0.15 }, { "stamina_efficiency", 0.1 } },
                    UpgradeCost = 4000,
                    PurchaseCost = 2000,
                    IsEquipped = true,
                    SpecialAbilities = new() { "Full Body Protection", "Weather Resistance" }
                }
            };
        }

        #endregion

        #region Skill Tree System - All Difficulties Compatible

        public List<SkillNode> GetSkillTree()
        {
            return new List<SkillNode>
            {
                #region Mining Efficiency Branch
                new()
                {
                    Id = "efficient_mining",
                    Name = "Efficient Mining",
                    Description = "Reduce stamina consumption by 20% for all mining operations",
                    Category = "Mining",
                    RequiredPoints = 5,
                    Icon = "⚡",
                    X = 100, Y = 100,
                    Effect = "Reduces stamina cost by 20%",
                    Bonuses = new() { { "stamina_efficiency", 0.2 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "lucky_strike",
                    Name = "Lucky Strike",
                    Description = "10% chance to find double minerals in any mining operation",
                    Category = "Mining",
                    RequiredPoints = 8,
                    Icon = "🍀",
                    X = 200, Y = 100,
                    Prerequisites = new() { "efficient_mining" },
                    Effect = "10% chance for double minerals",
                    Bonuses = new() { { "double_chance", 0.1 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "deep_mining",
                    Name = "Deep Mining Mastery",
                    Description = "Unlock advanced deep mining techniques for better rare mineral discovery",
                    Category = "Mining",
                    RequiredPoints = 12,
                    Icon = "🕳️",
                    X = 300, Y = 100,
                    Prerequisites = new() { "lucky_strike" },
                    Effect = "Increased rare mineral discovery in deep locations",
                    UnlockedAbilities = new() { "deep_mining_bonus" },
                    Bonuses = new() { { "rare_mineral_chance", 0.15 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "master_miner",
                    Name = "Master Miner",
                    Description = "Ultimate mining expertise - 25% bonus to all mining operations",
                    Category = "Mining",
                    RequiredPoints = 20,
                    Icon = "👨‍⚒️",
                    X = 400, Y = 100,
                    Prerequisites = new() { "deep_mining" },
                    Effect = "25% bonus to all mining efficiency",
                    Bonuses = new() { { "mining_master_bonus", 0.25 } },
                    IsUnlocked = false
                },
                #endregion

                #region Refining and Processing Branch
                new()
                {
                    Id = "basic_refining",
                    Name = "Basic Refining",
                    Description = "Improve mineral refining efficiency by 15%",
                    Category = "Efficiency",
                    RequiredPoints = 6,
                    Icon = "🔬",
                    X = 100, Y = 200,
                    Effect = "Improve refining efficiency by 15%",
                    Bonuses = new() { { "refining_efficiency", 0.15 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "advanced_refining",
                    Name = "Advanced Refining",
                    Description = "25% chance to produce bonus refined materials",
                    Category = "Efficiency",
                    RequiredPoints = 10,
                    Icon = "⚗️",
                    X = 200, Y = 200,
                    Prerequisites = new() { "basic_refining" },
                    Effect = "25% chance for bonus refined materials",
                    Bonuses = new() { { "refining_bonus_chance", 0.25 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "quantum_processing",
                    Name = "Quantum Processing",
                    Description = "Unlock quantum-level mineral processing for ultimate efficiency",
                    Category = "Efficiency",
                    RequiredPoints = 18,
                    Icon = "⚛️",
                    X = 300, Y = 200,
                    Prerequisites = new() { "advanced_refining" },
                    Effect = "Quantum-level processing capabilities",
                    UnlockedAbilities = new() { "quantum_refining" },
                    Bonuses = new() { { "quantum_efficiency", 0.4 } },
                    IsUnlocked = false
                },
                #endregion

                #region Economic and Trading Branch
                new()
                {
                    Id = "market_insight",
                    Name = "Market Insight",
                    Description = "Gain access to detailed market trends and price predictions",
                    Category = "Economics",
                    RequiredPoints = 7,
                    Icon = "📈",
                    X = 100, Y = 300,
                    Effect = "See market trends and price predictions",
                    UnlockedAbilities = new() { "market_predictions", "price_alerts" },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "negotiation",
                    Name = "Expert Negotiation",
                    Description = "Negotiate 10% better prices for all mineral sales",
                    Category = "Economics",
                    RequiredPoints = 9,
                    Icon = "🤝",
                    X = 200, Y = 300,
                    Prerequisites = new() { "market_insight" },
                    Effect = "Get 10% better prices for all minerals",
                    Bonuses = new() { { "price_bonus", 0.1 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "trade_mastery",
                    Name = "Trade Mastery",
                    Description = "Master trader status - 20% better prices and exclusive deals",
                    Category = "Economics",
                    RequiredPoints = 15,
                    Icon = "💼",
                    X = 300, Y = 300,
                    Prerequisites = new() { "negotiation" },
                    Effect = "Master trader benefits and exclusive market access",
                    Bonuses = new() { { "trade_master_bonus", 0.2 } },
                    UnlockedAbilities = new() { "exclusive_deals", "bulk_trading" },
                    IsUnlocked = false
                },
                #endregion

                #region Exploration and Discovery Branch
                new()
                {
                    Id = "pathfinding",
                    Name = "Expert Pathfinding",
                    Description = "Reduce location unlock costs by 25% through efficient exploration",
                    Category = "Discovery",
                    RequiredPoints = 8,
                    Icon = "🗺️",
                    X = 100, Y = 400,
                    Effect = "Reduce location unlock costs by 25%",
                    Bonuses = new() { { "unlock_cost_reduction", 0.25 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "danger_sense",
                    Name = "Danger Sense",
                    Description = "Reduce environmental danger effects by 50%",
                    Category = "Discovery",
                    RequiredPoints = 11,
                    Icon = "👁️",
                    X = 200, Y = 400,
                    Prerequisites = new() { "pathfinding" },
                    Effect = "Reduce danger level effects by 50%",
                    Bonuses = new() { { "danger_resistance", 0.5 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "quantum_navigation",
                    Name = "Quantum Navigation",
                    Description = "Navigate quantum dimensions and unlock reality-bending locations",
                    Category = "Discovery",
                    RequiredPoints = 22,
                    Icon = "🌌",
                    X = 300, Y = 400,
                    Prerequisites = new() { "danger_sense" },
                    Effect = "Access to quantum realm and dimensional mining",
                    UnlockedAbilities = new() { "quantum_travel", "dimensional_mining" },
                    IsUnlocked = false
                },
                #endregion

                #region Survival and Equipment Branch
                new()
                {
                    Id = "equipment_mastery",
                    Name = "Equipment Mastery",
                    Description = "Equipment durability decreases 30% slower",
                    Category = "Survival",
                    RequiredPoints = 10,
                    Icon = "🔧",
                    X = 100, Y = 500,
                    Effect = "Equipment lasts 30% longer",
                    Bonuses = new() { { "durability_bonus", 0.3 } },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "automation",
                    Name = "Mining Automation",
                    Description = "Automated mining systems increase efficiency by 40%",
                    Category = "Survival",
                    RequiredPoints = 14,
                    Icon = "🤖",
                    X = 200, Y = 500,
                    Prerequisites = new() { "equipment_mastery" },
                    Effect = "Automated mining assistance",
                    Bonuses = new() { { "automation_bonus", 0.4 } },
                    UnlockedAbilities = new() { "auto_mining", "efficiency_boost" },
                    IsUnlocked = false
                },
                new()
                {
                    Id = "quantum_mastery",
                    Name = "Quantum Mastery",
                    Description = "Transcend physical limitations - ultimate quantum mining abilities",
                    Category = "Survival",
                    RequiredPoints = 25,
                    Icon = "⚛️",
                    X = 300, Y = 500,
                    Prerequisites = new() { "automation", "quantum_navigation" },
                    Effect = "Master of quantum mining dimensions",
                    UnlockedAbilities = new() { "quantum_mastery", "reality_manipulation" },
                    Bonuses = new() { { "quantum_master", 1.0 } },
                    IsUnlocked = false
                }
                #endregion
            };
        }

        #endregion

        #region Story Events - Narrative Framework

        public List<StoryEvent> GetStoryEvents()
        {
            return new List<StoryEvent>
            {
                new()
                {
                    Id = "welcome_to_mining",
                    Title = "Welcome to the Mining World",
                    Description = "You arrive at the remote mining settlement with nothing but determination and a dream of striking it rich. An experienced miner approaches you with valuable advice.",
                    Icon = "👴",
                    Choices = new() { "Ask for comprehensive mining tips", "Start mining immediately with confidence" },
                    ChoiceEffects = new()
                    {
                        { "Ask for comprehensive mining tips", player => {
                            player.SkillPoints += 2;
                            player.TotalMoney += 1000;
                        }},
                        { "Start mining immediately with confidence", player => {
                            player.TotalMoney += 5000;
                            player.Stamina -= 10;
                        }}
                    },
                    NarrativeText = "The choice you make here will shape your mining career. Will you prioritize learning or immediate action?"
                },
                new()
                {
                    Id = "first_fortune_discovery",
                    Title = "Your First Major Discovery",
                    Description = "After weeks of hard work, you've made your first significant mineral discovery! A wealthy merchant from the city has heard about your success and offers to buy your entire collection for a substantial premium.",
                    Icon = "💰",
                    Choices = new() { "Accept the merchant's generous offer", "Keep mining and build your own empire", "Negotiate for an even better deal" },
                    ChoiceEffects = new()
                    {
                        { "Accept the merchant's generous offer", player => {
                            player.TotalMoney += 75000;
                            player.AddExperience(500);
                        }},
                        { "Keep mining and build your own empire", player => {
                            player.SkillPoints += 3;
                            player.Multiplier += 0.1;
                        }},
                        { "Negotiate for an even better deal", player => {
                            player.TotalMoney += 100000;
                            player.AddExperience(250);
                        }}
                    },
                    NarrativeText = "This decision will affect your reputation in the mining community and your future opportunities."
                },
                new()
                {
                    Id = "ancient_equipment_discovery",
                    Title = "Ancient Mining Technology",
                    Description = "Deep in the caverns, you stumble upon what appears to be ancient mining equipment of unknown origin. The technology seems far more advanced than anything you've seen, but there's an aura of mystery and potential danger surrounding it.",
                    Icon = "🔍",
                    Choices = new() { "Carefully study and use the ancient equipment", "Leave it undisturbed and continue safely", "Take detailed notes and report to authorities" },
                    ChoiceEffects = new()
                    {
                        { "Carefully study and use the ancient equipment", player => {
                            player.UnlockedSkills.Add("ancient_technology");
                            player.SkillPoints += 5;
                            player.Stamina -= 20; // Risk involved
                        }},
                        { "Leave it undisturbed and continue safely", player => {
                            player.TotalMoney += 50000;
                            player.AddExperience(300);
                        }},
                        { "Take detailed notes and report to authorities", player => {
                            player.TotalMoney += 25000;
                            player.SkillPoints += 2;
                            player.AddExperience(400);
                        }}
                    },
                    NarrativeText = "Ancient technologies can be powerful, but they often come with unexpected consequences."
                },
                new()
                {
                    Id = "quantum_entity_encounter",
                    Title = "Encounter with Quantum Beings",
                    Description = "In the reality-bending quantum realm, you encounter beings of pure energy who exist outside normal space-time. They communicate through quantum entanglement and offer to share cosmic knowledge in exchange for rare minerals from your collection.",
                    Icon = "👽",
                    Choices = new() { "Trade rare minerals for cosmic knowledge", "Politely decline and mine cautiously", "Attempt to communicate and learn more" },
                    ChoiceEffects = new()
                    {
                        { "Trade rare minerals for cosmic knowledge", player => {
                            player.TotalMoney -= 2000000;
                            player.SkillPoints += 15;
                            player.UnlockedSkills.Add("quantum_communication");
                        }},
                        { "Politely decline and mine cautiously", player => {
                            player.AddExperience(1000);
                            player.Stamina += 10;
                        }},
                        { "Attempt to communicate and learn more", player => {
                            player.SkillPoints += 8;
                            player.UnlockedSkills.Add("quantum_understanding");
                            player.AddExperience(1500);
                        }}
                    },
                    NarrativeText = "The quantum beings exist beyond our understanding of reality. Your choice here may unlock secrets of the universe itself."
                },
                new()
                {
                    Id = "rival_miner_challenge",
                    Title = "Rival Miner Competition",
                    Description = "A rival mining corporation has challenged you to a competitive mining contest. The winner gets exclusive access to a newly discovered mineral-rich zone, while the loser faces significant setbacks.",
                    Icon = "⚔️",
                    Choices = new() { "Accept the challenge confidently", "Propose collaborative mining instead", "Decline and focus on current operations" },
                    ChoiceEffects = new()
                    {
                        { "Accept the challenge confidently", player => { 
                            // 70% chance of winning based on player level
                            var success = player.Level > 20;
                            if (success) {
                                player.TotalMoney += 500000;
                                player.UnlockedLocations.Add("exclusive_zone");
                                player.AddExperience(2000);
                            } else {
                                player.TotalMoney -= 100000;
                                player.Stamina -= 30;
                            }
                        }},
                        { "Propose collaborative mining instead", player => {
                            player.TotalMoney += 250000;
                            player.SkillPoints += 5;
                            player.AddExperience(1000);
                        }},
                        { "Decline and focus on current operations", player => {
                            player.Multiplier += 0.15;
                            player.AddExperience(500);
                        }}
                    },
                    NarrativeText = "Competition can drive innovation and growth, but collaboration often yields more sustainable results."
                },
                new()
                {
                    Id = "environmental_crisis",
                    Title = "Environmental Mining Crisis",
                    Description = "Your mining operations have uncovered an environmental issue that could affect the entire region. Local communities are looking to you for leadership in handling this crisis responsibly.",
                    Icon = "🌍",
                    Choices = new() { "Invest heavily in environmental restoration", "Implement minimal required measures", "Develop innovative green mining technology" },
                    ChoiceEffects = new()
                    {
                        { "Invest heavily in environmental restoration", player => {
                            player.TotalMoney -= 1000000;
                            player.SkillPoints += 10;
                            player.UnlockedSkills.Add("environmental_stewardship");
                            player.AddExperience(3000);
                        }},
                        { "Implement minimal required measures", player => {
                            player.TotalMoney -= 100000;
                            player.AddExperience(500);
                        }},
                        { "Develop innovative green mining technology", player => {
                            player.TotalMoney -= 500000;
                            player.SkillPoints += 8;
                            player.UnlockedSkills.Add("green_technology");
                            player.Multiplier += 0.2;
                            player.AddExperience(2500);
                        }}
                    },
                    NarrativeText = "How you handle environmental challenges will define your legacy as a miner and impact future generations."
                }
            };
        }

        #endregion

        #region Settings Management

        public async Task<Dictionary<string, object>> LoadSettingsAsync()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var file = await localFolder.GetFileAsync(_settingsFileName);
                var content = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<Dictionary<string, object>>(content) ?? GetDefaultSettings();
            }
            catch
            {
                return GetDefaultSettings();
            }
        }

        public async Task SaveSettingsAsync(Dictionary<string, object> settings)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var file = await localFolder.CreateFileAsync(_settingsFileName, CreationCollisionOption.ReplaceExisting);
                var content = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await FileIO.WriteTextAsync(file, content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private Dictionary<string, object> GetDefaultSettings()
        {
            return new Dictionary<string, object>
            {
                { "Theme", "Dark" },
                { "AnimationSpeed", 1.0 },
                { "AutoSave", true },
                { "ShowTutorials", true },
                { "Language", "English" },
                { "MasterVolume", 0.8 },
                { "SoundEffects", true },
                { "BackgroundMusic", true },
                { "ReducedAnimations", false },
                { "HighContrast", false },
                { "LastUpdated", CURRENT_DATETIME },
                { "CurrentUser", CURRENT_USER },
                { "Version", "1.0.0-alpha" },
                { "GameMode", "Ultimate Edition" },
                { "SessionStartTime", CURRENT_DATETIME },
                { "TotalPlaySessions", 1 },
                { "PreferredDifficulty", "EXPERT" }
            };
        }

        #endregion
    }
}