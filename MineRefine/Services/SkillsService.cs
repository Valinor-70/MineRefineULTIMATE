using System;
using System.Collections.Generic;
using System.Linq;
using MineRefine.Models;

namespace MineRefine.Services
{
    public class SkillsService
    {
        private readonly SkillTree _skillTree;
        private readonly DataService _dataService;

        public SkillsService()
        {
            _skillTree = new SkillTree { Name = "Mining Mastery" };
            _skillTree.InitializeSkills();
            _dataService = new DataService();
        }

        public SkillTree GetSkillTree() => _skillTree;

        public List<Skill> GetSkillsByCategory(SkillCategory category)
        {
            return _skillTree.SkillsByCategory.GetValueOrDefault(category, new List<Skill>());
        }

        public Skill? GetSkill(string skillId)
        {
            return _skillTree.Skills.FirstOrDefault(s => s.Id == skillId);
        }

        public bool CanUnlockSkill(Player player, string skillId)
        {
            var skill = GetSkill(skillId);
            if (skill == null) return false;

            // Check if already unlocked
            if (skill.IsUnlocked || player.SkillLevels.ContainsKey(skillId)) return false;

            // Check skill points
            if (player.SkillPoints < skill.SkillPointCost) return false;

            // Check prerequisites
            foreach (var prerequisiteId in skill.Prerequisites)
            {
                var prerequisite = GetSkill(prerequisiteId);
                if (prerequisite == null || !player.SkillLevels.ContainsKey(prerequisiteId))
                    return false;
            }

            return true;
        }

        public bool UnlockSkill(Player player, string skillId)
        {
            if (!CanUnlockSkill(player, skillId)) return false;

            var skill = GetSkill(skillId);
            if (skill == null) return false;

            // Spend skill points
            player.SkillPoints -= skill.SkillPointCost;
            
            // Add to player's skills
            player.SkillLevels[skillId] = 1;
            if (!player.UnlockedSkills.Contains(skillId))
                player.UnlockedSkills.Add(skillId);

            skill.IsUnlocked = true;
            skill.Level = 1;

            return true;
        }

        public bool CanUpgradeSkill(Player player, string skillId)
        {
            var skill = GetSkill(skillId);
            if (skill == null) return false;

            // Check if skill is unlocked
            if (!player.SkillLevels.ContainsKey(skillId)) return false;

            // Check if not already maxed
            var currentLevel = player.SkillLevels[skillId];
            if (currentLevel >= skill.MaxLevel) return false;

            // Check skill points (cost increases with level)
            var upgradeCost = skill.SkillPointCost * (currentLevel + 1);
            if (player.SkillPoints < upgradeCost) return false;

            return true;
        }

        public bool UpgradeSkill(Player player, string skillId)
        {
            if (!CanUpgradeSkill(player, skillId)) return false;

            var skill = GetSkill(skillId);
            if (skill == null) return false;

            var currentLevel = player.SkillLevels[skillId];
            var upgradeCost = skill.SkillPointCost * (currentLevel + 1);

            // Spend skill points
            player.SkillPoints -= upgradeCost;

            // Upgrade skill
            player.SkillLevels[skillId] = currentLevel + 1;
            skill.Level = currentLevel + 1;

            return true;
        }

        public double GetSkillBonus(Player player, string bonusType)
        {
            double totalBonus = 0.0;

            foreach (var skillEntry in player.SkillLevels)
            {
                var skill = GetSkill(skillEntry.Key);
                if (skill != null && skill.Bonuses.ContainsKey(bonusType))
                {
                    totalBonus += skill.Bonuses[bonusType] * skillEntry.Value;
                }
            }

            return totalBonus;
        }

        public Dictionary<string, double> GetAllSkillBonuses(Player player)
        {
            var bonuses = new Dictionary<string, double>();

            foreach (var skillEntry in player.SkillLevels)
            {
                var skill = GetSkill(skillEntry.Key);
                if (skill != null)
                {
                    foreach (var bonus in skill.Bonuses)
                    {
                        if (!bonuses.ContainsKey(bonus.Key))
                            bonuses[bonus.Key] = 0.0;
                        
                        bonuses[bonus.Key] += bonus.Value * skillEntry.Value;
                    }
                }
            }

            return bonuses;
        }

        public int GetSkillLevel(Player player, string skillId)
        {
            return player.SkillLevels.GetValueOrDefault(skillId, 0);
        }

        public int GetTotalSkillsUnlocked(Player player)
        {
            return player.SkillLevels.Count;
        }

        public int GetTotalSkillLevels(Player player)
        {
            return player.SkillLevels.Values.Sum();
        }

        public List<Skill> GetUnlockedSkills(Player player)
        {
            return _skillTree.Skills.Where(s => player.SkillLevels.ContainsKey(s.Id)).ToList();
        }

        public List<Skill> GetAvailableSkills(Player player)
        {
            return _skillTree.Skills.Where(s => CanUnlockSkill(player, s.Id)).ToList();
        }

        public void ApplySkillBonusesToMining(Player player, ref double efficiency, ref double safety, ref double staminaCost)
        {
            efficiency *= (1.0 + GetSkillBonus(player, "mining_efficiency"));
            safety *= (1.0 + GetSkillBonus(player, "safety_bonus"));
            staminaCost *= (1.0 - GetSkillBonus(player, "stamina_efficiency"));

            // Apply equipment bonuses from skills
            var equipmentBonus = GetSkillBonus(player, "equipment_bonus");
            efficiency *= (1.0 + equipmentBonus);
        }

        public void ApplySkillBonusesToWeather(Player player, ref double weatherPenalty)
        {
            var weatherResistance = GetSkillBonus(player, "weather_resistance");
            weatherPenalty *= (1.0 - weatherResistance);
        }

        public double GetQuantumBonus(Player player)
        {
            return GetSkillBonus(player, "quantum_discovery") + GetSkillBonus(player, "quantum_safety");
        }

        public bool HasSkill(Player player, SkillType skillType)
        {
            var skill = _skillTree.Skills.FirstOrDefault(s => s.Type == skillType);
            return skill != null && player.SkillLevels.ContainsKey(skill.Id);
        }

        public void InitializePlayerSkills(Player player)
        {
            // Sync player skills with skill tree
            foreach (var skillEntry in player.SkillLevels.ToList())
            {
                var skill = GetSkill(skillEntry.Key);
                if (skill != null)
                {
                    skill.IsUnlocked = true;
                    skill.Level = skillEntry.Value;
                }
            }
        }
    }
}