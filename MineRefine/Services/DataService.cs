using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using MineRefine.Models;
using SystemIO = System.IO; // FIXED: Alias to avoid namespace conflict

namespace MineRefine.Services
{
    public class DataService
    {
        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-07-31 13:29:22";
        private const string CURRENT_USER = "Valinor-70";

        private const string SAVE_FILE_NAME = "MineRefineSave.json";
        private const string SETTINGS_FILE_NAME = "MineRefineSettings.json";

        // FIXED: Proper AES-256 key (32 bytes) and IV (16 bytes)
        private static readonly byte[] ENCRYPTION_KEY = new byte[32]
        {
            0x4D, 0x69, 0x6E, 0x65, 0x52, 0x65, 0x66, 0x69, 0x6E, 0x65, 0x55, 0x6C, 0x74, 0x69, 0x6D, 0x61,
            0x74, 0x65, 0x32, 0x30, 0x32, 0x35, 0x4B, 0x65, 0x79, 0x56, 0x61, 0x6C, 0x75, 0x65, 0x33, 0x32
        };

        private static readonly byte[] ENCRYPTION_IV = new byte[16]
        {
            0x4D, 0x69, 0x6E, 0x65, 0x52, 0x65, 0x66, 0x69, 0x6E, 0x65, 0x49, 0x56, 0x31, 0x36, 0x42, 0x21
        };

        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Dictionary<string, object> _cache;
        private readonly object _lockObject = new object();

        // Storage strategy flags
        private StorageFolder? _gameFolder;
        private string? _fallbackPath;
        private bool _useWinRTStorage = false;
        private bool _storageInitialized = false;

        public DataService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            _cache = new Dictionary<string, object>();
            System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] DataService: Initialized successfully");
        }

        #region Storage Initialization

        private async Task InitializeStorageAsync()
        {
            if (_storageInitialized) return;

            try
            {
                // First, try WinRT storage
                await TryInitializeWinRTStorageAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] WinRT storage failed: {ex.Message}");
                // Fall back to file system storage
                InitializeFallbackStorage();
            }

            _storageInitialized = true;
        }

        private async Task TryInitializeWinRTStorageAsync()
        {
            // Check if ApplicationData.Current is available
            if (ApplicationData.Current == null)
            {
                throw new InvalidOperationException("ApplicationData.Current is not available. WinRT not initialized.");
            }

            var localFolder = ApplicationData.Current.LocalFolder;
            if (localFolder == null)
            {
                throw new InvalidOperationException("LocalFolder is not available.");
            }

            _gameFolder = localFolder;
            _useWinRTStorage = true;
            System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] DataService: Using WinRT storage");
        }

        private void InitializeFallbackStorage()
        {
            try
            {
                // Use Documents folder as fallback
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                _fallbackPath = SystemIO.Path.Combine(documentsPath, "MineRefineGame");

                if (!SystemIO.Directory.Exists(_fallbackPath))
                {
                    SystemIO.Directory.CreateDirectory(_fallbackPath);
                }

                _useWinRTStorage = false;
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] DataService: Using fallback storage at {_fallbackPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] Fallback storage failed: {ex.Message}");

                // Last resort: use temp directory
                _fallbackPath = SystemIO.Path.Combine(SystemIO.Path.GetTempPath(), "MineRefineGame");
                if (!SystemIO.Directory.Exists(_fallbackPath))
                {
                    SystemIO.Directory.CreateDirectory(_fallbackPath);
                }
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] DataService: Using temp storage at {_fallbackPath}");
            }
        }

        #endregion

        #region File Operations

        private async Task<string> GetSaveFilePathAsync()
        {
            await InitializeStorageAsync();

            if (_useWinRTStorage && _gameFolder != null)
            {
                var file = await _gameFolder.CreateFileAsync(SAVE_FILE_NAME, CreationCollisionOption.OpenIfExists);
                return file.Path;
            }
            else if (_fallbackPath != null)
            {
                return SystemIO.Path.Combine(_fallbackPath, SAVE_FILE_NAME);
            }
            else
            {
                throw new InvalidOperationException("No storage method available");
            }
        }

        private async Task<string> GetSettingsFilePathAsync()
        {
            await InitializeStorageAsync();

            if (_useWinRTStorage && _gameFolder != null)
            {
                var file = await _gameFolder.CreateFileAsync(SETTINGS_FILE_NAME, CreationCollisionOption.OpenIfExists);
                return file.Path;
            }
            else if (_fallbackPath != null)
            {
                return SystemIO.Path.Combine(_fallbackPath, SETTINGS_FILE_NAME);
            }
            else
            {
                throw new InvalidOperationException("No storage method available");
            }
        }

        private async Task<string> ReadFileAsync(string filePath)
        {
            if (_useWinRTStorage)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(filePath);
                    return await FileIO.ReadTextAsync(file);
                }
                catch (SystemIO.FileNotFoundException)
                {
                    return string.Empty;
                }
            }
            else
            {
                try
                {
                    // FIXED: Use SystemIO alias to avoid namespace conflict
                    if (SystemIO.File.Exists(filePath))
                    {
                        return await SystemIO.File.ReadAllTextAsync(filePath);
                    }
                    return string.Empty;
                }
                catch (SystemIO.FileNotFoundException)
                {
                    return string.Empty;
                }
            }
        }

        private async Task WriteFileAsync(string filePath, string content)
        {
            if (_useWinRTStorage)
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);
                await FileIO.WriteTextAsync(file, content);
            }
            else
            {
                // FIXED: Use SystemIO alias to avoid namespace conflict
                await SystemIO.File.WriteAllTextAsync(filePath, content);
            }
        }

        private async Task<bool> FileExistsAsync(string filePath)
        {
            if (_useWinRTStorage)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(filePath);
                    return file != null;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // FIXED: Use SystemIO alias to avoid namespace conflict
                return SystemIO.File.Exists(filePath);
            }
        }

        #endregion

        #region Encryption/Decryption - FIXED

        private static string EncryptData(string plainText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = ENCRYPTION_KEY;
                aes.IV = ENCRYPTION_IV;

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new SystemIO.MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (var swEncrypt = new SystemIO.StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EncryptData error: {ex.Message}");
                // Fallback: return base64 encoded data without encryption
                return "FALLBACK:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
            }
        }

        private static string DecryptData(string cipherText)
        {
            try
            {
                // Check for fallback data
                if (cipherText.StartsWith("FALLBACK:"))
                {
                    var fallbackData = cipherText.Substring(9);
                    return Encoding.UTF8.GetString(Convert.FromBase64String(fallbackData));
                }

                var cipherBytes = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = ENCRYPTION_KEY;
                aes.IV = ENCRYPTION_IV;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new SystemIO.MemoryStream(cipherBytes);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new SystemIO.StreamReader(csDecrypt);
                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DecryptData error: {ex.Message}");
                // Try to decode as unencrypted base64
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
                }
                catch
                {
                    // Return as-is if all else fails
                    return cipherText;
                }
            }
        }

        #endregion

        #region Player Data Management

        public async Task<List<Player>> LoadPlayersAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    if (_cache.ContainsKey("players"))
                    {
                        return ((List<Player>)_cache["players"]).ToList();
                    }
                }

                var saveFilePath = await GetSaveFilePathAsync();
                var encryptedData = await ReadFileAsync(saveFilePath);

                if (string.IsNullOrEmpty(encryptedData))
                {
                    return new List<Player>();
                }

                var jsonData = DecryptData(encryptedData);
                var saveData = JsonSerializer.Deserialize<SaveDataV1>(jsonData, _jsonOptions);

                var players = saveData?.Players ?? new List<Player>();

                // Validate and fix player data
                foreach (var player in players)
                {
                    ValidatePlayerData(player);
                }

                lock (_lockObject)
                {
                    _cache["players"] = players.ToList();
                }

                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] LoadPlayersAsync: Loaded {players.Count} players successfully");
                return players;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] LoadPlayersAsync: Error: {ex.Message}");
                return new List<Player>();
            }
        }

        public async Task SavePlayersAsync(List<Player> players)
        {
            try
            {
                // Validate players before saving
                foreach (var player in players)
                {
                    ValidatePlayerData(player);
                    player.LastPlayed = DateTime.Parse(CURRENT_DATETIME);
                }

                var saveData = new SaveDataV1
                {
                    Players = players,
                    SavedAt = DateTime.Parse(CURRENT_DATETIME),
                    SavedBy = CURRENT_USER,
                    Version = "1.0.0",
                    Checksum = CalculateChecksum(players)
                };

                var jsonData = JsonSerializer.Serialize(saveData, _jsonOptions);
                var encryptedData = EncryptData(jsonData);

                var saveFilePath = await GetSaveFilePathAsync();
                await WriteFileAsync(saveFilePath, encryptedData);

                // Update cache
                lock (_lockObject)
                {
                    _cache["players"] = players.ToList();
                }

                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] SavePlayersAsync: Successfully saved {players.Count} players");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] SavePlayersAsync: Error: {ex.Message}");
                throw new InvalidOperationException($"Failed to save players: {ex.Message}");
            }
        }

        public async Task<bool> HasExistingSaveAsync()
        {
            try
            {
                var saveFilePath = await GetSaveFilePathAsync();
                var exists = await FileExistsAsync(saveFilePath);

                if (exists)
                {
                    var data = await ReadFileAsync(saveFilePath);
                    return !string.IsNullOrEmpty(data);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Player> CreateNewPlayerAsync(string name, Difficulty difficulty, string settingsData = null)
        {
            try
            {
                await Task.Delay(1); // Minimal async operation

                var player = CreateNewPlayer(name, difficulty);
                var players = new List<Player> { player };
                await SavePlayersAsync(players);

                if (!string.IsNullOrEmpty(settingsData))
                {
                    var defaultSettings = new GameSettings
                    {
                        CurrentUser = CURRENT_USER,
                        LastUpdated = DateTime.Parse(CURRENT_DATETIME)
                    };
                    await SaveGameSettingsAsync(defaultSettings);
                }

                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] CreateNewPlayerAsync: Successfully created player {name} with difficulty {difficulty}");
                return player;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] CreateNewPlayerAsync: Error creating player {name}: {ex.Message}");
                throw new InvalidOperationException($"Failed to create player: {ex.Message}");
            }
        }

        private Player CreateNewPlayer(string name, Difficulty difficulty)
        {
            var startingMoney = difficulty switch
            {
                Difficulty.NOVICE => 1000L,
                Difficulty.EXPERIENCED => 500L,
                Difficulty.EXPERT => 200L,
                Difficulty.LEGENDARY => 100L,
                _ => 500L
            };

            var maxStamina = difficulty switch
            {
                Difficulty.NOVICE => 150,
                Difficulty.EXPERIENCED => 100,
                Difficulty.EXPERT => 80,
                Difficulty.LEGENDARY => 60,
                _ => 100
            };

            var player = new Player
            {
                Name = name,
                Difficulty = difficulty,
                Level = 1,
                ExperiencePoints = 0,
                Rank = Rank.NOVICE_MINER,
                TotalMoney = startingMoney,
                Stamina = maxStamina,
                MaxStamina = maxStamina,
                SkillPoints = 0,
                CurrentLocationId = "surface_mine",
                CreatedDate = DateTime.Parse(CURRENT_DATETIME),
                LastPlayed = DateTime.Parse(CURRENT_DATETIME),
                LastLogin = DateTime.Parse(CURRENT_DATETIME),
                TotalMinesCount = 0,
                TotalEarnings = 0,
                UnlockedLocations = new List<string> { "surface_mine" },
                MineralStats = new Dictionary<string, long>(),
                LocationFirstVisit = new Dictionary<string, DateTime>(),
                // Ensure Phase 2 collections are initialized
                UnlockedSkills = new List<string>(),
                CompletedAchievements = new List<string>(),
                CompletedTutorials = new List<string>(),
                UnlockedNarratives = new List<string>(),
                MiningHistory = new List<MiningSession>(),
                SessionHistory = new List<GameSession>(),
                WeatherEncounters = new Dictionary<string, int>(),
                LocationSuccessRates = new Dictionary<string, double>(),
                SkillLevels = new Dictionary<string, int>(),
                AchievementProgress = new List<string>(),
                DailyMiningStats = new Dictionary<string, int>(),
                Equipment = new List<Equipment>(),
                // Set default weather
                CurrentWeather = WeatherCondition.Clear,
                LastWeatherChange = DateTime.Parse(CURRENT_DATETIME)
            };

            // Initialize starting location visit
            player.LocationFirstVisit["surface_mine"] = DateTime.Parse(CURRENT_DATETIME);

            return player;
        }

        private void ValidatePlayerData(Player player)
        {
            // Ensure essential collections are not null
            player.UnlockedLocations ??= new List<string> { "surface_mine" };
            player.MineralStats ??= new Dictionary<string, long>();
            player.LocationFirstVisit ??= new Dictionary<string, DateTime>();
            
            // Ensure Phase 2 collections are not null
            player.UnlockedSkills ??= new List<string>();
            player.CompletedAchievements ??= new List<string>();
            player.CompletedTutorials ??= new List<string>();
            player.UnlockedNarratives ??= new List<string>();
            player.MiningHistory ??= new List<MiningSession>();
            player.SessionHistory ??= new List<GameSession>();
            player.WeatherEncounters ??= new Dictionary<string, int>();
            player.LocationSuccessRates ??= new Dictionary<string, double>();
            player.SkillLevels ??= new Dictionary<string, int>();
            player.AchievementProgress ??= new List<string>();
            player.DailyMiningStats ??= new Dictionary<string, int>();
            player.Equipment ??= new List<Equipment>();

            // Ensure minimum values
            if (player.Stamina < 0) player.Stamina = 0;
            if (player.MaxStamina < 50) player.MaxStamina = 100;
            if (player.TotalMoney < 0) player.TotalMoney = 0;
            if (player.Level < 1) player.Level = 1;

            // Ensure current location is unlocked
            if (!player.UnlockedLocations.Contains(player.CurrentLocationId))
            {
                player.CurrentLocationId = "surface_mine";
                if (!player.UnlockedLocations.Contains("surface_mine"))
                {
                    player.UnlockedLocations.Add("surface_mine");
                }
            }
        }

        #endregion

        #region Game Settings Management

        public async Task<GameSettings> LoadGameSettingsAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    if (_cache.ContainsKey("settings"))
                    {
                        return (GameSettings)_cache["settings"];
                    }
                }

                var settingsFilePath = await GetSettingsFilePathAsync();
                var encryptedData = await ReadFileAsync(settingsFilePath);

                if (string.IsNullOrEmpty(encryptedData))
                {
                    return null;
                }

                var jsonData = DecryptData(encryptedData);
                var settings = JsonSerializer.Deserialize<GameSettings>(jsonData, _jsonOptions);

                if (settings != null)
                {
                    ValidateGameSettings(settings);

                    lock (_lockObject)
                    {
                        _cache["settings"] = settings;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] LoadGameSettingsAsync: Settings loaded successfully");
                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] LoadGameSettingsAsync: Error: {ex.Message}");
                return null;
            }
        }

        public async Task SaveGameSettingsAsync(GameSettings settings)
        {
            try
            {
                ValidateGameSettings(settings);

                settings.LastUpdated = DateTime.Parse(CURRENT_DATETIME);
                settings.CurrentUser = CURRENT_USER;

                var jsonData = JsonSerializer.Serialize(settings, _jsonOptions);
                var encryptedData = EncryptData(jsonData);

                var settingsFilePath = await GetSettingsFilePathAsync();
                await WriteFileAsync(settingsFilePath, encryptedData);

                // Update cache
                lock (_lockObject)
                {
                    _cache["settings"] = settings;
                }

                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] SaveGameSettingsAsync: Settings saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{CURRENT_DATETIME}] [{CURRENT_USER}] SaveGameSettingsAsync: Error: {ex.Message}");
                throw new InvalidOperationException($"Failed to save settings: {ex.Message}");
            }
        }

        private void ValidateGameSettings(GameSettings settings)
        {
            // Ensure values are within valid ranges
            settings.MasterVolume = Math.Max(0.0, Math.Min(1.0, settings.MasterVolume));
            settings.AnimationSpeed = Math.Max(0.1, Math.Min(5.0, settings.AnimationSpeed));
            settings.AutoSaveInterval = Math.Max(10, Math.Min(3600, settings.AutoSaveInterval));

            // Ensure required fields have default values
            settings.CurrentUser ??= CURRENT_USER;
            if (settings.LastUpdated == default)
                settings.LastUpdated = DateTime.Parse(CURRENT_DATETIME);
        }

        #endregion

        #region Utility Methods

        private string CalculateChecksum(List<Player> players)
        {
            try
            {
                var data = JsonSerializer.Serialize(players, _jsonOptions);
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        public void ClearCache()
        {
            lock (_lockObject)
            {
                _cache.Clear();
            }
        }

        #endregion

        #region Mineral Data

        public List<Mineral> GetMinerals()
        {
            return new List<Mineral>
            {
                new Mineral
                {
                    Id = "copper",
                    Name = "Copper Ore",
                    Icon = "🟤",
                    Value = 10,
                    RefinedValue = 15,
                    Weight = 2.5,
                    Rarity = "Common",
                    MineralType = MineralType.BASE_METAL, // FIXED: Use correct enum value
                    Description = "A common reddish-brown metal ore found near the surface. Essential for basic equipment and electronics.",
                    FoundInLocations = new List<string> { "surface_mine", "deep_caves" },
                    DiscoveredDate = DateTime.Parse(CURRENT_DATETIME),
                    DiscoveredBy = CURRENT_USER,
                    IsRadioactive = false,
                    IsMagnetic = false,
                    IsLegendary = false,
                    SpecialProperties = new List<string> { "Conductive", "Malleable" }
                },
                new Mineral
                {
                    Id = "iron",
                    Name = "Iron Ore",
                    Icon = "⚫",
                    Value = 20,
                    RefinedValue = 30,
                    Weight = 3.0,
                    Rarity = "Common",
                    MineralType = MineralType.BASE_METAL, // FIXED: Use correct enum value
                    Description = "A strong, dark metallic ore. The backbone of industrial civilization and essential for construction.",
                    FoundInLocations = new List<string> { "surface_mine", "deep_caves" },
                    DiscoveredDate = DateTime.Parse(CURRENT_DATETIME),
                    DiscoveredBy = CURRENT_USER,
                    IsRadioactive = false,
                    IsMagnetic = true,
                    IsLegendary = false,
                    SpecialProperties = new List<string> { "Magnetic", "Durable" }
                },
                new Mineral
                {
                    Id = "silver",
                    Name = "Silver Ore",
                    Icon = "⚪",
                    Value = 50,
                    RefinedValue = 75,
                    Weight = 4.0,
                    Rarity = "Uncommon",
                    MineralType = MineralType.BASE_METAL, // FIXED: Use BASE_METAL instead of PRECIOUS_METAL
                    Description = "A lustrous precious metal with excellent conductivity. Prized for both industrial and decorative uses.",
                    FoundInLocations = new List<string> { "deep_caves", "volcanic_depths" },
                    DiscoveredDate = DateTime.Parse(CURRENT_DATETIME),
                    DiscoveredBy = CURRENT_USER,
                    IsRadioactive = false,
                    IsMagnetic = false,
                    IsLegendary = false,
                    SpecialProperties = new List<string> { "Conductive", "Antimicrobial" }
                },
                new Mineral
                {
                    Id = "gold",
                    Name = "Gold Ore",
                    Icon = "🟡",
                    Value = 100,
                    RefinedValue = 150,
                    Weight = 5.0,
                    Rarity = "Rare",
                    MineralType = MineralType.BASE_METAL, // FIXED: Use BASE_METAL instead of PRECIOUS_METAL
                    Description = "The most coveted of precious metals. Chemically inert and eternally valuable for technology and wealth.",
                    FoundInLocations = new List<string> { "deep_caves", "volcanic_depths" },
                    DiscoveredDate = DateTime.Parse(CURRENT_DATETIME),
                    DiscoveredBy = CURRENT_USER,
                    IsRadioactive = false,
                    IsMagnetic = false,
                    IsLegendary = false,
                    SpecialProperties = new List<string> { "Inert", "Conductive", "Non-tarnishing" }
                },
                new Mineral
                {
                    Id = "diamond",
                    Name = "Diamond",
                    Icon = "💎",
                    Value = 500,
                    RefinedValue = 750,
                    Weight = 1.5,
                    Rarity = "Epic",
                    MineralType = MineralType.GEMSTONE, // FIXED: Use correct enum value
                    Description = "The hardest known natural substance. Brilliant crystalline carbon prized for cutting tools and jewelry.",
                    FoundInLocations = new List<string> { "volcanic_depths" },
                    DiscoveredDate = DateTime.Parse(CURRENT_DATETIME),
                    DiscoveredBy = CURRENT_USER,
                    IsRadioactive = false,
                    IsMagnetic = false,
                    IsLegendary = false,
                    SpecialProperties = new List<string> { "Hardest", "Brilliant", "Thermal Conductor" }
                },
                new Mineral
                {
                    Id = "quantum_crystal",
                    Name = "Quantum Crystal",
                    Icon = "🌌",
                    Value = 10000,
                    RefinedValue = 15000,
                    Weight = 0.5,
                    Rarity = "Quantum",
                    MineralType = MineralType.EXOTIC_MATTER, // FIXED: Use correct enum value
                    Description = "A mysterious crystalline structure that exists in quantum superposition. Reality bends around these impossible gems.",
                    FoundInLocations = new List<string> { "quantum_realm" },
                    DiscoveredDate = DateTime.Parse(CURRENT_DATETIME),
                    DiscoveredBy = CURRENT_USER,
                    IsRadioactive = true,
                    IsMagnetic = false,
                    IsLegendary = true,
                    SpecialProperties = new List<string> { "Quantum Entangled", "Reality Distortion", "Temporal Flux" }
                }
            };
        }

        public List<MiningLocation> GetMiningLocations()
        {
            return new List<MiningLocation>
            {
                new MiningLocation
                {
                    Id = "surface_mine",
                    Name = "Surface Mine",
                    Icon = "⛰️",
                    Description = "The starting mining location. Open pit mining with basic equipment and safe conditions.",
                    RequiredLevel = 1,
                    StaminaCost = 10,
                    IsUnlocked = true
                },
                new MiningLocation
                {
                    Id = "deep_caves",
                    Name = "Deep Caves",
                    Icon = "🕳️",
                    Description = "Underground cave systems with higher mineral diversity but increased risks.",
                    RequiredLevel = 5,
                    StaminaCost = 15,
                    IsUnlocked = false
                },
                new MiningLocation
                {
                    Id = "volcanic_depths",
                    Name = "Volcanic Depths",
                    Icon = "🌋",
                    Description = "Extreme volcanic environment with rare minerals and extreme danger.",
                    RequiredLevel = 15,
                    StaminaCost = 25,
                    IsUnlocked = false
                },
                new MiningLocation
                {
                    Id = "quantum_realm",
                    Name = "Quantum Realm",
                    Icon = "🌌",
                    Description = "A dimension where physics breaks down and impossible materials exist.",
                    RequiredLevel = 50,
                    StaminaCost = 50,
                    IsUnlocked = false
                }
            };
        }

        public List<Equipment> GetAvailableEquipment()
        {
            return new List<Equipment>
            {
                new Equipment
                {
                    Id = "basic_pickaxe",
                    Name = "Basic Pickaxe",
                    Icon = "⛏️",
                    Description = "A simple mining tool for beginners.",
                    Level = 1,
                    Durability = 100,
                    MaxDurability = 100
                }
            };
        }

        public List<Achievement> GetAchievements()
        {
            return new List<Achievement>
            {
                new Achievement
                {
                    Id = "first_mine",
                    Name = "First Strike",
                    Description = "Complete your first mining operation.",
                    Icon = "🎯"
                }
            };
        }

        #endregion
    }
}