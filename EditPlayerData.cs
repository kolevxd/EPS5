using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using EditPlayerData.UI;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Data.Boss;
using Il2CppAssets.Scripts.Data.TrophyStore;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Store.Loot;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.Player;
using Il2CppAssets.Scripts.Unity.UI_New.Achievements;
using Il2CppAssets.Scripts.Unity.UI_New.ChallengeEditor;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppAssets.Scripts.Utils;
using Il2CppNinjaKiwi.Common;
using Il2CppSystem.Linq;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;
using Enum = System.Enum;
using Object = Il2CppSystem.Object;

namespace EditPlayerData.UI;

public class EditPlayerDataMenu : ModGameMenu<ContentBrowser>
{
    private static readonly Dictionary<string, List<PlayerDataSetting>> Settings = new()
    {
        {
            "General", new List<PlayerDataSetting>
            {
                new BoolPlayerDataSetting("Export Profile", VanillaSprites.GreenBtn, false,
                    () => false, _ => ExportProfile()),
                new BoolPlayerDataSetting("Import Profile", VanillaSprites.BlueBtn, false,
                    () => false, _ => ImportProfile()),
                new BoolPlayerDataSetting("Unlock Everything", VanillaSprites.GreenBtn, false,
                    () => false, _ => UnlockEverything()),
                new PurchasePlayerDataSetting("Unlocked Double Cash", VanillaSprites.DoubleCashModeShop, "btd6_doublecashmode"),
                new PurchasePlayerDataSetting("Unlocked Fast Track", VanillaSprites.FastTrackModeIcon,
                    "btd6_fasttrackpack",
                    () => GetPlayer().Data.unlockedFastTrack,
                    t => GetPlayer().Data.unlockedFastTrack = t),
                new PurchasePlayerDataSetting("Unlocked Rogue Legends", VanillaSprites.LegendsBtn, "btd6_legendsrogue"),
                new PurchasePlayerDataSetting("Unlocked Map Editor", VanillaSprites.MapEditorBtn, "btd6_mapeditorsupporter_new"),
                new NumberPlayerDataSetting("Monkey Money", VanillaSprites.MonkeyMoneyShop, 0,
                    () => GetPlayer().Data.monkeyMoney.ValueInt, t => GetPlayer().Data.monkeyMoney.Value = t),
                new NumberPlayerDataSetting("Monkey Knowledge", VanillaSprites.KnowledgeIcon, 0,
                    () => GetPlayer().Data.knowledgePoints.ValueInt, t => GetPlayer().Data.knowledgePoints.Value = t),
                new RankPlayerDataSetting(GetPlayer),
                
                new NumberPlayerDataSetting("Trophies", VanillaSprites.TrophyIcon, 0,
                    () => GetPlayer().Data.trophies.ValueInt,
                    t => GetPlayer().GainTrophies(t - GetPlayer().Data.trophies.ValueInt, "")),
                new NumberPlayerDataSetting("Games Won", VanillaSprites.ConfettiIcon, 0,
                    () => GetPlayer().Data.completedGame, t => GetPlayer().Data.completedGame = t),
                new NumberPlayerDataSetting("Highest Seen Round", VanillaSprites.BadBloonIcon, 0,
                    () => GetPlayer().Data.highestSeenRound, t => GetPlayer().Data.highestSeenRound = t),
                new NumberPlayerDataSetting("Continues", VanillaSprites.ContinueIcon, 0,
                    () => GetPlayer().Data.continuesUsed.ValueInt, t => GetPlayer().Data.continuesUsed.Value = t),
                new BoolPlayerDataSetting("Unlocked Big Bloons", VanillaSprites.BigBloonModeIcon, false,
                    () => GetPlayer().Data.unlockedBigBloons, t => GetPlayer().Data.unlockedBigBloons = t),
                new BoolPlayerDataSetting("Unlocked Small Bloons", VanillaSprites.SmallBloonModeIcon, false,
                    () => GetPlayer().Data.unlockedSmallBloons, t => GetPlayer().Data.unlockedSmallBloons = t),
                new BoolPlayerDataSetting("Unlocked Big Monkeys", VanillaSprites.BigMonkeysModeIcon, false,
                    () => GetPlayer().Data.unlockedBigTowers, t => GetPlayer().Data.unlockedBigTowers = t),
                new BoolPlayerDataSetting("Unlocked Small Monkeys", VanillaSprites.SmallMonkeysModeIcon, false,
                    () => GetPlayer().Data.unlockedSmallTowers, t => GetPlayer().Data.unlockedSmallTowers = t),
                
                new NumberPlayerDataSetting("Challenges Shared", VanillaSprites.CreateChallengesIcon, 0,
                    () => GetPlayer().Data.challengesShared.ValueInt, t => GetPlayer().Data.challengesShared.Value = t),
                new NumberPlayerDataSetting("Challenges Played", VanillaSprites.ChallengesIcon, 0,
                    () => GetPlayer().Data.challengesPlayed.ValueInt, t => GetPlayer().Data.challengesPlayed.Value = t),
                new NumberPlayerDataSetting("Odysseys Completed", VanillaSprites.OdysseyIcon, 0,
                    () => GetPlayer().Data.totalCompletedOdysseys.ValueInt,
                    t => GetPlayer().Data.totalCompletedOdysseys.Value = t),
                new NumberPlayerDataSetting("Tower Gift Unlock Pops", VanillaSprites.GiftBoxIcon, 0,
                    () => GetPlayer().Data.currentTowerGiftProgress.ValueInt,
                    t => GetPlayer().Data.currentTowerGiftProgress.Value = t),
                new NumberPlayerDataSetting("Daily Reward Index", VanillaSprites.DailyChestIcon, 0,
                    () => GetPlayer().Data.dailyRewardIndex, t => GetPlayer().Data.dailyRewardIndex = t),
                
                new NumberPlayerDataSetting("Total Daily Challenges Completed", VanillaSprites.ChallengeTrophyIcon, 0,
                    () => GetPlayer().Data.totalDailyChallengesCompleted,
                    t => GetPlayer().Data.totalDailyChallengesCompleted = t),
                new NumberPlayerDataSetting("Consecutive Daily Challenges Completed",
                    VanillaSprites.ChallengeThumbsUpIcon, 0,
                    () => GetPlayer().Data.consecutiveDailyChallengesCompleted,
                    t => GetPlayer().Data.consecutiveDailyChallengesCompleted = t),
                new NumberPlayerDataSetting("Challenges Played", VanillaSprites.ChallengesIcon, 0,
                    () => GetPlayer().Data.challengesPlayed.ValueInt, t => GetPlayer().Data.challengesPlayed.Value = t),
                new NumberPlayerDataSetting("Hosted Coop Games", VanillaSprites.CoOpIcon, 0,
                    () => GetPlayer().Data.hostedCoopGames, t => GetPlayer().Data.hostedCoopGames = t),
                new NumberPlayerDataSetting("Collection Event Crates Opened",
                    VanillaSprites.CollectionEventLootIconEaster, 0,
                    () => GetPlayer().Data.collectionEventCratesOpened,
                    t => GetPlayer().Data.collectionEventCratesOpened = t),
                
                new NumberPlayerDataSetting("Golden Bloons Popped", VanillaSprites.GoldenBloonIcon, 0,
                    () => GetPlayer().Data.goldenBloonsPopped, t => GetPlayer().Data.goldenBloonsPopped = t),
            }
        },
        {
            "Trophy Store", new List<PlayerDataSetting>() // uses a loop to reduce hard-coded values             
        },
        {
            "Maps", new List<PlayerDataSetting>() // uses a loop to reduce hard-coded values
        },
        {
            "Maps - Coop", new List<PlayerDataSetting>() // uses a loop to reduce hard-coded values
        },
        {
            "Tower XP", new List<PlayerDataSetting>() // uses a loop to reduce hard-coded values
        },
        {
            "Powers", new List<PlayerDataSetting>() // uses a loop to reduce hard-coded values
        },
        {
            "Instas", new List<PlayerDataSetting>() // uses a loop to reduce hard-coded values
        },
        {
            "Online Modes", new List<PlayerDataSetting>() // uses a loop to reduce hard-coded values
        }
    };

    private static bool _isOpen;

    private const int EntriesPerPage = 5;

    public static void InitSettings(ProfileModel data)
    {
        Settings["Trophy Store"].Clear();
        Settings["Maps"].Clear();
        Settings["Maps - Coop"].Clear();
        Settings["Tower XP"].Clear();
        Settings["Powers"].Clear();
        Settings["Instas"].Clear();
        Settings["Online Modes"].Clear();
        
        foreach (var item in GameData.Instance.trophyStoreItems.GetAllItems())
        {
            Settings["Trophy Store"].Add(new BoolPlayerDataSetting(item.GetLocalizedShortName()+" Enabled", item.icon.AssetGUID,
                false,
                () => Game.Player.EnabledTrophyStoreItems().Contains(item.Id),
                val => Game.Player.Data.trophyStorePurchasedItems[item.Id].enabled = val
            ).Unlockable(
                () => !Game.Player.Data.trophyStorePurchasedItems.ContainsKey(item.Id),
                () => Game.Player.AddTrophyStoreItem(item.id)));
        }

        
        foreach (var details in GameData.Instance.mapSet.StandardMaps.ToIl2CppList())
        {
            Settings["Maps"].Add(new MapPlayerDataSetting(details, data.mapInfo.GetMap(details.id), false)
                .Unlockable(
                    () => !data.mapInfo.IsMapUnlocked(details.id),
                    () => data.mapInfo.UnlockMap(details.id)));
            Settings["Maps - Coop"].Add(new MapPlayerDataSetting(details, data.mapInfo.GetMap(details.id), true)
                .Unlockable(
                    () => !data.mapInfo.IsMapUnlocked(details.id),
                    () => data.mapInfo.UnlockMap(details.id)));
        }

        foreach (var power in Game.instance.model.powers)
        {
            if (power.name is "CaveMonkey" or "DungeonStatue" or "SpookyCreature") continue;

            Settings["Powers"].Add(new NumberPlayerDataSetting(
                LocalizationManager.Instance.Format(power.name),
                power.icon.GetGUID(), 0,
                () => GetPlayer().GetPowerData(power.name)?.Quantity ?? 0,
                t =>
                {
                    if (GetPlayer().IsPowerAvailable(power.name))
                    {
                        GetPlayer().GetPowerData(power.name).Quantity = t;
                    }
                    else
                    {
                        GetPlayer().AddPower(power.name, t);
                    }
                }));
        }

        foreach (var tower in Game.instance.GetTowerDetailModels())
        {
            Settings["Tower XP"].Add(new TowerPlayerDataSetting(tower, GetPlayer).Unlockable(
                () => !data.unlockedTowers.Contains(tower.towerId),
                () =>
                {
                    Game.instance.towerGoalUnlockManager.CompleteGoalForTower(tower.towerId);
                    data.UnlockTower(tower.towerId);

                    foreach (var quest in Game.instance.questTrackerManager.QuestData.TowerUnlockQuestsContainer.items
                                 .ToList()
                                 .Where(quest => quest.towerId == tower.towerId))
                    {
                        var questData = Game.Player.GetQuestSaveData(quest.unlockQuestId);

                        questData.hasSeenQuest = true;
                        questData.hasSeenQuestCompleteDialogue = true;
                        questData.hasCollectedRewards = true;

                        foreach (var part in questData.questPartSaveData)
                        {
                            part.hasSeenQuestPart = true;
                            part.hasSeenQuestCompleteDialogue = true;
                            part.hasCollectedRewards = true;
                            part.completed = true;

                            foreach (var task in part.tasksSaveData)
                            {
                                task.hasCollectedRewards = true;
                                task.completed = true;
                            }
                        }

                        foreach (var task in questData.tasksSaveData)
                        {
                            task.hasCollectedRewards = true;
                            task.completed = true;
                        }

                        Game.Player.SetQuestSaveData(questData);
                    }
                }));

            Settings["Instas"].Add(new InstaMonkeyPlayerDataSetting(tower, GetPlayer));
        }

        foreach (var boss in Enum.GetValues<BossType>())
        {
            Settings["Online Modes"].Add(new NumberPlayerDataSetting($"{boss} Normal",
                VanillaSprites.ByName[$"{boss}Badge"], 0,
                () => GetPlayer().Data.bossMedals.ContainsKey((int)boss)
                    ? GetPlayer().Data.bossMedals[(int)boss].normalBadges.ValueInt
                    : 0,
                t =>
                {
                    if (!GetPlayer().Data.bossMedals.ContainsKey((int)boss))
                    {
                        GetPlayer().Data.bossMedals[(int)boss] = new BossMedalSaveData();
                    }

                    GetPlayer().Data.bossMedals[(int)boss].normalBadges.Value = t;
                }));
            Settings["Online Modes"].Add(new NumberPlayerDataSetting($"{boss} Elite",
                    VanillaSprites.ByName[$"{boss}EliteBadge"], 0,
                () => GetPlayer().Data.bossMedals.ContainsKey((int)boss)
                    ? GetPlayer().Data.bossMedals[(int)boss].eliteBadges.ValueInt
                    : 0,
                t =>
                {
                    if (!GetPlayer().Data.bossMedals.ContainsKey((int)boss))
                    {
                        GetPlayer().Data.bossMedals[(int)boss] = new BossMedalSaveData();
                    }

                    GetPlayer().Data.bossMedals[(int)boss].eliteBadges.Value = t;
                }));
        }

        var badgeToName = new Dictionary<LeaderboardBadgeType, string> {
            {LeaderboardBadgeType.BlackDiamond, "1st"},
            {LeaderboardBadgeType.RedDiamond, "2nd"},
            {LeaderboardBadgeType.BlueDiamond, "3rd"},
            {LeaderboardBadgeType.GoldDiamond, "Top 50"},
            {LeaderboardBadgeType.DoubleGold, "Top 1%"},
            {LeaderboardBadgeType.GoldSilver, "Top 10%"},
            {LeaderboardBadgeType.DoubleSilver, "Top 25%"},
            {LeaderboardBadgeType.Silver, "Top 50%"},
            {LeaderboardBadgeType.Bronze, "Top 75%"},
        };
        foreach (var leaderboard in badgeToName.Keys)
        {
            var name = leaderboard == LeaderboardBadgeType.BlueDiamond ? "Diamond" : leaderboard.ToString();
            Settings["Online Modes"].Add(new NumberPlayerDataSetting($"Boss {badgeToName[leaderboard]}",
                    VanillaSprites.ByName[$"BossMedalEvent{name}Medal"], 0,
                () => GetPlayer().Data.bossLeaderboardMedals.ContainsKey((int)leaderboard) ? GetPlayer().Data.bossLeaderboardMedals[(int)leaderboard].ValueInt : 0,
                t =>
                {
                    if (!GetPlayer().Data.bossLeaderboardMedals.ContainsKey((int)leaderboard))
                    {
                        GetPlayer().Data.bossLeaderboardMedals[(int)leaderboard] = new KonFuze_NoShuffle();
                    }
                    
                    GetPlayer().Data.bossLeaderboardMedals[(int)leaderboard].Value = t;
                }));
        }
        foreach (var leaderboard in badgeToName.Keys)
        {
            var name = leaderboard == LeaderboardBadgeType.BlueDiamond ? "Diamond" : leaderboard.ToString();
            Settings["Online Modes"].Add(new NumberPlayerDataSetting($"Elite Boss {badgeToName[leaderboard]}",
                VanillaSprites.ByName[$"EliteBossMedalEvent{name}Medal"], 0,
                () => GetPlayer().Data.bossLeaderboardEliteMedals.ContainsKey((int)leaderboard) ? GetPlayer().Data.bossLeaderboardEliteMedals[(int)leaderboard].ValueInt : 0,
                t =>
                {
                    if (!GetPlayer().Data.bossLeaderboardEliteMedals.ContainsKey((int)leaderboard))
                    {
                        GetPlayer().Data.bossLeaderboardEliteMedals[(int)leaderboard] = new KonFuze_NoShuffle();
                    }
                    
                    GetPlayer().Data.bossLeaderboardEliteMedals[(int)leaderboard].Value = t;
                }));
        }
        foreach (var leaderboard in badgeToName.Keys)
        {
            var name = leaderboard == LeaderboardBadgeType.BlueDiamond ? "Diamond" : leaderboard.ToString();
            Settings["Online Modes"].Add(new NumberPlayerDataSetting($"Race {badgeToName[leaderboard]}",
                    VanillaSprites.ByName[$"MedalEvent{name}Medal"], 0,
                () => GetPlayer().Data.raceMedalData.ContainsKey((int)leaderboard) ? GetPlayer().Data.raceMedalData[(int)leaderboard].ValueInt : 0,
                t =>
                {
                    if (!GetPlayer().Data.raceMedalData.ContainsKey((int)leaderboard))
                    {
                        GetPlayer().Data.raceMedalData[(int)leaderboard] = new KonFuze_NoShuffle();
                    }
                    
                    GetPlayer().Data.raceMedalData[(int)leaderboard].Value = t;
                }));
        }
        
        badgeToName = new Dictionary<LeaderboardBadgeType, string> {
            {LeaderboardBadgeType.BlackDiamond, "1st"},
            {LeaderboardBadgeType.RedDiamond, "2nd"},
            {LeaderboardBadgeType.BlueDiamond, "3rd"},
            {LeaderboardBadgeType.GoldDiamond, "4th-10th"},
            {LeaderboardBadgeType.DoubleGold, "11th-20th"},
            {LeaderboardBadgeType.Silver, "21st-40th"},
            {LeaderboardBadgeType.Bronze, "41st-60th"},
        };
        foreach (var leaderboard in badgeToName.Keys)
        {
            var name = leaderboard == LeaderboardBadgeType.BlueDiamond ? "Diamond" : leaderboard.ToString();
            Settings["Online Modes"].Add(new NumberPlayerDataSetting($"CT Local {badgeToName[leaderboard]}",
                    VanillaSprites.ByName[$"CtLocalPlayer{name}Medal"], 0,
                () => GetPlayer().GetCtLeaderboardBadges(false).ContainsKey((int)leaderboard) ? GetPlayer().GetCtLeaderboardBadges(false)[(int)leaderboard].ValueInt : 0,
                t =>
                {
                    if (!GetPlayer().GetCtLeaderboardBadges(false).ContainsKey((int)leaderboard))
                    {
                        GetPlayer().GetCtLeaderboardBadges(false)[(int)leaderboard] = new KonFuze_NoShuffle();
                    }

                    GetPlayer().GetCtLeaderboardBadges(false)[(int)leaderboard].Value = t;
                }));
        }
        
        badgeToName = new Dictionary<LeaderboardBadgeType, string> {
            {LeaderboardBadgeType.BlueDiamond, "Top 25"},
            {LeaderboardBadgeType.GoldDiamond, "Top 100"},
            {LeaderboardBadgeType.DoubleGold, "Top 1%"},
            {LeaderboardBadgeType.GoldSilver, "Top 10%"},
            {LeaderboardBadgeType.DoubleSilver, "Top 25%"},
            {LeaderboardBadgeType.Silver, "Top 50%"},
            {LeaderboardBadgeType.Bronze, "Top 75%"},
        };
        foreach (var leaderboard in badgeToName.Keys)
        {
            var name = leaderboard == LeaderboardBadgeType.BlueDiamond ? "Diamond" : leaderboard.ToString();
            Settings["Online Modes"].Add(new NumberPlayerDataSetting($"CT Global {badgeToName[leaderboard]}",
                VanillaSprites.ByName[$"CtGlobalPlayer{name}Medal"], 0,
                () => GetPlayer().GetCtLeaderboardBadges(true).ContainsKey((int)leaderboard) ? GetPlayer().GetCtLeaderboardBadges(true)[(int)leaderboard].ValueInt : 0,
                t =>
                {
                    if (!GetPlayer().GetCtLeaderboardBadges(true).ContainsKey((int)leaderboard))
                    {
                        GetPlayer().GetCtLeaderboardBadges(true)[(int)leaderboard] = new KonFuze_NoShuffle();
                    }

                    GetPlayer().GetCtLeaderboardBadges(true)[(int)leaderboard].Value = t;
                }));
        }
        
        // Add mass edit buttons to each section
        AddMassEditButtons();
    }

    private static void AddMassEditButtons()
    {
        // Create a panel for the Tower XP mass edit button
        var towerXpPanel = ModHelperPanel.Create(new Info("MassSetTowerXP", 800, 150));
        MassEditFeatures.AddMassSetTowerXpButton(towerXpPanel);
        Settings["Tower XP"].Insert(0, new CustomPanelSetting("Set XP for All Towers", towerXpPanel));

        // Create a panel for the Powers mass edit button
        var powersPanel = ModHelperPanel.Create(new Info("MassSetPowers", 800, 150));
        MassEditFeatures.AddMassSetPowersButton(powersPanel);
        Settings["Powers"].Insert(0, new CustomPanelSetting("Set Quantity for All Powers", powersPanel));

        // Create panels for the medal template buttons
        var mapsPanel = ModHelperPanel.Create(new Info("ApplyMedalTemplate", 800, 150));
        MassEditFeatures.AddApplyMedalTemplateButton(mapsPanel, false);
        Settings["Maps"].Insert(0, new CustomPanelSetting("Apply Medal Template", mapsPanel));

        var coopMapsPanel = ModHelperPanel.Create(new Info("ApplyMedalTemplateCoop", 800, 150));
        MassEditFeatures.AddApplyMedalTemplateButton(coopMapsPanel, true);
        Settings["Maps - Coop"].Insert(0, new CustomPanelSetting("Apply Medal Template (Co-op)", coopMapsPanel));
    }

    public class CustomPanelSetting : PlayerDataSetting
    {
        private readonly ModHelperPanel _panel;

        public CustomPanelSetting(string name, ModHelperPanel panel) : base(name, "")
        {
            _panel = panel;
        }

        protected override ModHelperComponent GetValue()
        {
            return _panel;
        }

        protected override void ShowEditValuePopup(PopupScreen screen)
        {
            // This is intentionally empty as the panel itself has the functionality
        }

        public override void ResetToDefault()
        {
            // No reset needed
        }

        public override ModHelperImage GetIcon()
        {
            // Empty icon
            return ModHelperImage.Create(new Info("Icon") { X = -50, Size = 350 }, VanillaSprites.BlueBtn);
        }
    }

    private int LastPage => (Settings[_category].Count(s => s.Name.ContainsIgnoreCase(_searchValue))-1) / EntriesPerPage;

    private readonly PlayerDataSettingDisplay[] _entries = new PlayerDataSettingDisplay[EntriesPerPage];

    private static TMP_InputField? _searchInput;
    private string _searchValue = "";
    private string _category = "General";
    private int _pageIdx;

    private ModHelperPanel? _topArea;

    private static Btd6Player GetPlayer()
    {
        return Game.Player;
    }

    // Export player data to a JSON file
    private static void ExportProfile()
    {
        try
        {
            // Get a simplified view of player data to export
            var playerData = Game.Player.Data;
            
            // Create a string to store the JSON data
            var jsonString = "{\n";
            
            // Add basic currencies and statistics
            jsonString += $"  \"monkeyMoney\": {playerData.monkeyMoney.ValueInt},\n";
            jsonString += $"  \"knowledgePoints\": {playerData.knowledgePoints.ValueInt},\n";
            jsonString += $"  \"trophies\": {playerData.trophies.ValueInt},\n";
            jsonString += $"  \"rank\": {playerData.rank.ValueInt},\n";
            jsonString += $"  \"veteranRank\": {playerData.veteranRank.ValueInt},\n";
            jsonString += $"  \"xp\": {playerData.xp.ValueInt},\n";
            jsonString += $"  \"completedGame\": {playerData.completedGame},\n";
            jsonString += $"  \"highestSeenRound\": {playerData.highestSeenRound},\n";
            
            // Add unlocks
            jsonString += $"  \"unlockedFastTrack\": {playerData.unlockedFastTrack.ToString().ToLower()},\n";
            jsonString += $"  \"unlockedBigBloons\": {playerData.unlockedBigBloons.ToString().ToLower()},\n";
            jsonString += $"  \"unlockedSmallBloons\": {playerData.unlockedSmallBloons.ToString().ToLower()},\n";
            jsonString += $"  \"unlockedBigTowers\": {playerData.unlockedBigTowers.ToString().ToLower()},\n";
            jsonString += $"  \"unlockedSmallTowers\": {playerData.unlockedSmallTowers.ToString().ToLower()},\n";
            
            // Skip one-time purchases since we don't have an easy way to enumerate them
            
            // Add tower XP
            jsonString += "  \"towerXp\": {\n";
            var towerXpItems = new List<string>();
            foreach (var kvp in playerData.towerXp)
            {
                towerXpItems.Add($"    \"{kvp.Key}\": {kvp.Value.ValueInt}");
            }
            jsonString += string.Join(",\n", towerXpItems);
            jsonString += "\n  }\n";
            
            // Close the JSON object
            jsonString += "}";
            
            // Get a file path in the game's directory
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "PlayerDataExport.json");
            
            // Save the JSON data to file
            File.WriteAllText(filePath, jsonString);
            
            // Show success message
            PopupScreen.instance.ShowOkPopup($"Profile exported successfully to:\n{filePath}");
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error exporting profile: " + e.Message);
            PopupScreen.instance.ShowOkPopup("Error exporting profile:\n" + e.Message);
        }
    }

    // Import player data from a JSON file
    private static void ImportProfile()
    {
        try
        {
            // Get file path in the game's directory
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "PlayerDataExport.json");
            
            if (!File.Exists(filePath))
            {
                PopupScreen.instance.ShowOkPopup($"Profile data file not found at:\n{filePath}");
                return;
            }
            
            // Read JSON file content
            var jsonContent = File.ReadAllText(filePath);
            
            // Confirm with the user
            PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
                "Import Profile Data", 
                "Are you sure you want to import player data?\nThis will overwrite your current profile data.",
                new Action(() => 
                {
                    try
                    {
                        // Since we can't easily use JSON parsing, we'll manually parse simple values
                        var playerData = Game.Player.Data;
                        
                        // Helper function to extract value from JSON content
                        int ExtractInt(string key)
                        {
                            var pattern = $"\"{key}\": (\\d+)";
                            var match = System.Text.RegularExpressions.Regex.Match(jsonContent, pattern);
                            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                        }
                        
                        bool ExtractBool(string key)
                        {
                            var pattern = $"\"{key}\": (true|false)";
                            var match = System.Text.RegularExpressions.Regex.Match(jsonContent, pattern);
                            return match.Success && match.Groups[1].Value == "true";
                        }
                        
                        // Apply basic currencies and statistics
                        playerData.monkeyMoney.Value = ExtractInt("monkeyMoney");
                        playerData.knowledgePoints.Value = ExtractInt("knowledgePoints");
                        playerData.trophies.Value = ExtractInt("trophies");
                        playerData.rank.Value = ExtractInt("rank");
                        playerData.veteranRank.Value = ExtractInt("veteranRank");
                        playerData.xp.Value = ExtractInt("xp");
                        playerData.completedGame = ExtractInt("completedGame");
                        playerData.highestSeenRound = ExtractInt("highestSeenRound");
                        
                        // Apply unlocks
                        playerData.unlockedFastTrack = ExtractBool("unlockedFastTrack");
                        playerData.unlockedBigBloons = ExtractBool("unlockedBigBloons");
                        playerData.unlockedSmallBloons = ExtractBool("unlockedSmallBloons");
                        playerData.unlockedBigTowers = ExtractBool("unlockedBigTowers");
                        playerData.unlockedSmallTowers = ExtractBool("unlockedSmallTowers");
                        
                        // Apply tower XP (simple approach)
                        var towerXpPattern = "\"towerXp\":\\s*\\{([^}]+)\\}";
                        var towerXpMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, towerXpPattern, System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (towerXpMatch.Success)
                        {
                            var towerXpText = towerXpMatch.Groups[1].Value;
                            var itemPattern = "\"([^\"]+)\":\\s*(\\d+)";
                            var itemMatches = System.Text.RegularExpressions.Regex.Matches(towerXpText, itemPattern);
                            
                            foreach (System.Text.RegularExpressions.Match itemMatch in itemMatches)
                            {
                                var towerId = itemMatch.Groups[1].Value;
                                var xpValue = int.Parse(itemMatch.Groups[2].Value);
                                
                                if (!playerData.towerXp.ContainsKey(towerId))
                                {
                                    playerData.towerXp[towerId] = new KonFuze_NoShuffle(xpValue);
                                }
                                else
                                {
                                    playerData.towerXp[towerId].Value = xpValue;
                                }
                            }
                        }
                        
                        // Save changes
                        Game.Player.SaveNow();
                        
                        // Show success message
                        PopupScreen.instance.ShowOkPopup("Profile imported successfully!");
                    }
                    catch (Exception e)
                    {
                        ModHelper.Msg<EditPlayerData>("Error during import: " + e.Message);
                        PopupScreen.instance.ShowOkPopup("Error during import:\n" + e.Message);
                    }
                }), 
                "Import", 
                new Action(() => {}), 
                "Cancel",
                Popup.TransitionAnim.Scale,
                PopupScreen.BackGround.Grey);
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error importing profile: " + e.Message);
            PopupScreen.instance.ShowOkPopup("Error importing profile:\n" + e.Message);
        }
    }

    // Unlock everything - towers, maps, modes, currencies
    private static void UnlockEverything()
    {
        try
        {
            PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
                "Unlock Everything", 
                "This will unlock all towers, maps, modes, and set high values for currencies. Continue?",
                new Action(() => 
                {
                    var playerData = Game.Player.Data;
                    
                    // Set currencies to high values
                    playerData.monkeyMoney.Value = 999999;
                    playerData.knowledgePoints.Value = 9999;
                    playerData.trophies.Value = 9999;
                    
                    // Unlock all maps
                    foreach (var details in GameData.Instance.mapSet.StandardMaps.ToIl2CppList())
                    {
                        if (!playerData.mapInfo.IsMapUnlocked(details.id))
                        {
                            playerData.mapInfo.UnlockMap(details.id);
                        }
                    }
                    
                    // Unlock all towers
                    foreach (var tower in Game.instance.GetTowerDetailModels())
                    {
                        if (!playerData.unlockedTowers.Contains(tower.towerId))
                        {
                            Game.instance.towerGoalUnlockManager.CompleteGoalForTower(tower.towerId);
                            playerData.UnlockTower(tower.towerId);
                        }
                        
                        // Give XP to each tower
                        if (!playerData.towerXp.ContainsKey(tower.towerId))
                        {
                            playerData.towerXp[tower.towerId] = new KonFuze_NoShuffle(500000);
                        }
                        else
                        {
                            playerData.towerXp[tower.towerId].Value = 500000;
                        }
                        
                        // Unlock all upgrades
                        var model = Game.instance.model;
                        var upgrades = model.GetTower(tower.towerId, pathOneTier: 5).appliedUpgrades
                            .Concat(model.GetTower(tower.towerId, pathTwoTier: 5).appliedUpgrades)
                            .Concat(model.GetTower(tower.towerId, pathThreeTier: 5).appliedUpgrades);
                        
                        foreach (var upgrade in upgrades)
                        {
                            playerData.acquiredUpgrades.Add(upgrade);
                        }
                        
                        // Try to unlock paragon
                        var paragon = Game.instance.model.GetParagonUpgradeForTowerId(tower.towerId);
                        if (paragon != null)
                        {
                            playerData.acquiredUpgrades.Add(paragon.name);
                        }
                    }
                    
                    // Unlock premium features
                    playerData.unlockedFastTrack = true;
                    playerData.unlockedBigBloons = true;
                    playerData.unlockedSmallBloons = true;
                    playerData.unlockedBigTowers = true;
                    playerData.unlockedSmallTowers = true;
                    
                    // Add premium purchases
                    playerData.purchase.AddOneTimePurchaseItem("btd6_doublecashmode");
                    playerData.purchase.AddOneTimePurchaseItem("btd6_fasttrackpack");
                    playerData.purchase.AddOneTimePurchaseItem("btd6_legendsrogue");
                    playerData.purchase.AddOneTimePurchaseItem("btd6_mapeditorsupporter_new");
                    
                    // Save changes
                    Game.Player.SaveNow();
                    
                    // Show success message
                    PopupScreen.instance.ShowOkPopup("Everything has been unlocked successfully!");
                }), 
                "Unlock All", 
                new Action(() => {}), 
                "Cancel",
                Popup.TransitionAnim.Scale,
                PopupScreen.BackGround.Grey);
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error unlocking everything: " + e.Message);
            PopupScreen.instance.ShowOkPopup("Error unlocking everything:\n" + e.Message);
        }
    }

    public override bool OnMenuOpened(Object data)
    {
        _isOpen = true;
        
        GameMenu.GetComponentFromChildrenByName<NK_TextMeshProUGUI>("Title").SetText("Player Data");

        RemoveChild("TopBar");
        RemoveChild("Tabs");
        RemoveChild("RefreshBtn");
        GameMenu.requiresInternetObj.SetActive(false);

        GameMenu.firstPageBtn.SetOnClick(() => SetPage(0));
        GameMenu.previousPageBtn.SetOnClick(() => SetPage(_pageIdx - 1));
        GameMenu.nextPageBtn.SetOnClick(() => SetPage(_pageIdx + 1));
        GameMenu.lastPageBtn.SetOnClick(() => SetPage(LastPage));

        var verticalLayoutGroup = GameMenu.scrollRect.content.GetComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.SetPadding(50);
        verticalLayoutGroup.spacing = 50;
        verticalLayoutGroup.childControlWidth = true;
        verticalLayoutGroup.childControlHeight = true;
        GameMenu.scrollRect.rectTransform.sizeDelta += new Vector2(0, 200);
        GameMenu.scrollRect.rectTransform.localPosition += new Vector3(0, 100, 0);
        
        _topArea = GameMenu.GetComponentFromChildrenByName<RectTransform>("Container").gameObject
            .AddModHelperPanel(new Info("TopArea")
            {
                Y = -325, Height = 200, Pivot = new Vector2(0.5f, 1),
                AnchorMin = new Vector2(0, 1), AnchorMax = new Vector2(1, 1)
            }, layoutAxis: RectTransform.Axis.Horizontal, padding: 50);

        _topArea.AddDropdown(new Info("Category", 775, 150),
            Settings.Keys.ToIl2CppList(), 1400, new Action<int>(i =>
            {
                _category = Settings.Keys.ElementAt(i);
                SetPage(0);
            }), VanillaSprites.BlueInsertPanelRound, 80f);
        _topArea.AddPanel(new Info("Spacing", InfoPreset.Flex));
        _searchInput = _topArea.AddInputField(new Info("Search", 1500, 150), _searchValue,
            VanillaSprites.BlueInsertPanelRound,
            new Action<string>(s =>
            {
                _searchValue = s;
                UpdateVisibleEntries();
            }),
            80f, TMP_InputField.CharacterValidation.None,
            TextAlignmentOptions.CaplineLeft, "Search...",
            50).InputField;
        
        _topArea.AddPanel(new Info("Spacing", InfoPreset.Flex));
        
        _topArea.AddButton(new Info("UnlockAll", 650, 200), VanillaSprites.GreenBtnLong, new Action(() =>
        {
            Settings[_category].ForEach(s=>s.Unlock());
            UpdateVisibleEntries();
        })).AddText(new Info("UnlockAllText", 650, 200), "Unlock All", 60);
        _topArea.AddPanel(new Info("UnlockAll Filler", 650, 200));       

        
        GenerateEntries();
        SetPage(0);

        // for no discernible reason, this defaults to 300
        GameMenu.scrollRect.scrollSensitivity = 50;
        
        return false;
    }

    public override void OnMenuClosed()
    {
        _isOpen = false;
        
        Game.Player.SaveNow();
        _category = "General";
    }

    private void GenerateEntries()
    {
        GameMenu.scrollRect.content.GetComponentInChildren<HorizontalOrVerticalLayoutGroup>().spacing = 125;
        
        for (var i = 0; i < EntriesPerPage; i++)
        {
            _entries[i] = PlayerDataSettingDisplay.Generate($"Setting {i}");
            _entries[i].SetActive(false);
            _entries[i].AddTo(GameMenu.scrollRect.content);
        }
    }

    private void UpdateVisibleEntries()
    {
        var anyUnlockable = Settings[_category].Any(s => !s.IsUnlocked());
        var unlockAllBtn = _topArea?.GetDescendent<ModHelperButton>("UnlockAll");
        var unlockAllFiller = _topArea?.GetDescendent<ModHelperPanel>("UnlockAll Filler");
        
        if (unlockAllBtn != null)
            unlockAllBtn.SetActive(anyUnlockable);
        
        if (unlockAllFiller != null)
            unlockAllFiller.SetActive(!anyUnlockable);

        var settings = Settings[_category].FindAll(s => s.Name.ContainsIgnoreCase(_searchValue));
        SetPage(_pageIdx, false);
        
        for (var i = 0; i < EntriesPerPage; i++)
        {
            var idx = _pageIdx * EntriesPerPage + i;
            var entry = _entries[i];

            if (idx >= settings.Count)
            {
                entry.SetActive(false);
            }
            else
            {
                if (settings[idx].GetType() == typeof(MapPlayerDataSetting))
                {
                    ((MapPlayerDataSetting) settings[idx]).ReloadAllVisuals = UpdateVisibleEntries;
                }
                entry.SetSetting(settings[idx]);
                entry.SetActive(true);
            }
        }
    }

    private void SetPage(int page, bool updateEntries=true)
    {
        if (_pageIdx != page) GameMenu.scrollRect.verticalNormalizedPosition = 1f;
        _pageIdx = Mathf.Clamp(page, 0, LastPage);

        GameMenu.totalPages = LastPage + 1;
        GameMenu.SetCurrentPage(_pageIdx + 1);

        GameMenu.firstPageBtn.interactable = GameMenu.previousPageBtn.interactable = _pageIdx > 0;
        GameMenu.lastPageBtn.interactable = GameMenu.nextPageBtn.interactable = _pageIdx < LastPage;

        if (updateEntries)
        {
            MenuManager.instance.buttonClick2Sound.Play("ClickSounds");
            UpdateVisibleEntries();            
        }
    }

    private void RemoveChild(string name)
    {
        GameMenu.GetComponentFromChildrenByName<RectTransform>(name).gameObject.active = false;
    }
    
    [HarmonyPatch(typeof(TMP_InputField), nameof(TMP_InputField.KeyPressed))]
    // ReSharper disable once InconsistentNaming
    internal class TMP_InputField_KeyPressed
    {
        [HarmonyPrefix]
        internal static void Prefix(TMP_InputField __instance, ref Event evt)
        {
            if (_isOpen && __instance != _searchInput && (evt.character == '-' || !int.TryParse(__instance.text + evt.character, out _)))
            {
                evt.character = (char) 0;                
            }
        }
    }
}
