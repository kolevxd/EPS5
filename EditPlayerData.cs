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
using Random = System.Random;

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

        // Add map medal template button at the beginning of Maps section
        Settings["Maps"].Add(new BoolPlayerDataSetting("Apply Medal Template", VanillaSprites.GreenBtn, false,
            () => false, _ => ShowApplyMedalTemplatePopup(false)));
        
        // Add map medal template button at the beginning of Maps - Coop section
        Settings["Maps - Coop"].Add(new BoolPlayerDataSetting("Apply Medal Template", VanillaSprites.GreenBtn, false,
            () => false, _ => ShowApplyMedalTemplatePopup(true)));
        
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

        // Add bulk set powers button at the beginning of Powers section
        Settings["Powers"].Add(new BoolPlayerDataSetting("Set Quantity for All Powers", VanillaSprites.GreenBtn, false,
            () => false, _ => ShowSetAllPowersPopup()));
        
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

        // Add bulk set XP button at the beginning of Tower XP section
        Settings["Tower XP"].Add(new BoolPlayerDataSetting("Set XP for All Towers", VanillaSprites.GreenBtn, false,
            () => false, _ => ShowSetAllTowerXpPopup()));
        
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
    }

    // Enhanced Tower XP setting function with range options
    private static void ShowSetAllTowerXpPopup()
    {
        var fixedValue = 500000;
        var minValue = 100000;
        var maxValue = 1000000;
        var useRange = false;
        
        var popup = PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
            "Set XP for All Towers", 
            "Set XP for all towers at once.",
            new Action(() =>
            {
                var player = Game.Player;
                var towers = Game.instance.GetTowerDetailModels();
                var random = new Random();
                
                foreach (var tower in towers)
                {
                    // Determine the XP value based on selected method
                    int xpValue;
                    if (useRange)
                    {
                        xpValue = random.Next(minValue, maxValue + 1);
                    }
                    else
                    {
                        xpValue = fixedValue;
                    }
                    
                    if (!player.Data.towerXp.ContainsKey(tower.towerId))
                    {
                        player.Data.towerXp[tower.towerId] = new KonFuze_NoShuffle(xpValue);
                    }
                    else
                    {
                        player.Data.towerXp[tower.towerId].Value = xpValue;
                    }
                }
                
                // Save changes
                player.SaveNow();
                
                PopupScreen.instance.ShowOkPopup("XP has been applied to all towers!");
            }), 
            "Apply", 
            new Action(() => {}), 
            "Cancel",
            Popup.TransitionAnim.Scale, 
            PopupScreen.BackGround.Grey);

        var layout = popup.FindObject("Layout");
        
        // XP Mode selection
        var modePanel = layout.AddModHelperPanel(new Info("ModePanel", 800, 100), 
            null, RectTransform.Axis.Horizontal, 20);
        
        var modeLabel = modePanel.AddText(new Info("ModeLabel", 350, 50), "XP Mode:", 60);
        modeLabel.Text.alignment = TextAlignmentOptions.MidlineRight;
        
        var modeToggle = modePanel.AddPanel(new Info("ModeToggle", 400, 50), 
            null, RectTransform.Axis.Horizontal, 10);
        
        var fixedButton = modeToggle.AddButton(new Info("FixedButton", 190, 50), 
            VanillaSprites.GreenBtnLong, new Action(() => {
                useRange = false;
                fixedPanel.SetActive(true);
                rangePanel.SetActive(false);
                fixedButton.Image.color = Color.green;
                rangeButton.Image.color = Color.white;
            }));
        fixedButton.AddText(new Info("FixedText", 190, 50), "Fixed", 40);
        fixedButton.Image.color = Color.green;
        
        var rangeButton = modeToggle.AddButton(new Info("RangeButton", 190, 50), 
            VanillaSprites.BlueBtnLong, new Action(() => {
                useRange = true;
                fixedPanel.SetActive(false);
                rangePanel.SetActive(true);
                fixedButton.Image.color = Color.white;
                rangeButton.Image.color = Color.green;
            }));
        rangeButton.AddText(new Info("RangeText", 190, 50), "Range", 40);
        
        // Add spacing
        layout.AddModHelperPanel(new Info("Spacing1", 800, 20));
        
        // Fixed value panel
        var fixedPanel = layout.AddModHelperPanel(new Info("FixedPanel", 800, 100), 
            null, RectTransform.Axis.Horizontal, 20);
        
        var fixedLabel = fixedPanel.AddText(new Info("FixedLabel", 350, 50), "XP Value:", 60);
        fixedLabel.Text.alignment = TextAlignmentOptions.MidlineRight;
        
        var fixedInput = fixedPanel.AddInputField(new Info("FixedInput", 400, 80), 
            fixedValue.ToString(), VanillaSprites.BlueInsertPanelRound, 
            new Action<string>(value => 
            {
                if (int.TryParse(value, out int result))
                {
                    fixedValue = result;
                }
            }), 50, TMP_InputField.CharacterValidation.Digit);
        
        // Range values panel (initially hidden)
        var rangePanel = layout.AddModHelperPanel(new Info("RangePanel", 800, 150), 
            null, RectTransform.Axis.Vertical, 20);
        rangePanel.SetActive(false);
        
        var minPanel = rangePanel.AddPanel(new Info("MinPanel", 800, 60), 
            null, RectTransform.Axis.Horizontal, 20);
        
        var minLabel = minPanel.AddText(new Info("MinLabel", 350, 50), "Min XP:", 60);
        minLabel.Text.alignment = TextAlignmentOptions.MidlineRight;
        
        var minInput = minPanel.AddInputField(new Info("MinInput", 400, 60), 
            minValue.ToString(), VanillaSprites.BlueInsertPanelRound, 
            new Action<string>(value => 
            {
                if (int.TryParse(value, out int result))
                {
                    minValue = result;
                }
            }), 50, TMP_InputField.CharacterValidation.Digit);
        
        var maxPanel = rangePanel.AddPanel(new Info("MaxPanel", 800, 60), 
            null, RectTransform.Axis.Horizontal, 20);
        
        var maxLabel = maxPanel.AddText(new Info("MaxLabel", 350, 50), "Max XP:", 60);
        maxLabel.Text.alignment = TextAlignmentOptions.MidlineRight;
        
        var maxInput = maxPanel.AddInputField(new Info("MaxInput", 400, 60), 
            maxValue.ToString(), VanillaSprites.BlueInsertPanelRound, 
            new Action<string>(value => 
            {
                if (int.TryParse(value, out int result))
                {
                    maxValue = result;
                }
            }), 50, TMP_InputField.CharacterValidation.Digit);
        
        // Add final spacing
        layout.AddModHelperPanel(new Info("Spacing2", 800, 50));
    }

    // Set all powers at once
    private static void ShowSetAllPowersPopup()
    {
        var quantity = 100;
        
        var popup = PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
            "Set All Powers", 
            "Set the same quantity for all powers at once.",
            new Action(() =>
            {
                var player = Game.Player;
                var powers = Game.instance.model.powers;
                
                foreach (var power in powers)
                {
                    if (power.name is "CaveMonkey" or "DungeonStatue" or "SpookyCreature") continue;
                    
                    if (player.IsPowerAvailable(power.name))
                    {
                        player.GetPowerData(power.name).Quantity = quantity;
                    }
                    else
                    {
                        player.AddPower(power.name, quantity);
                    }
                }
                
                // Save changes
                player.SaveNow();
                
                PopupScreen.instance.ShowOkPopup("Quantity has been applied to all powers!");
            }), 
            "Apply", 
            new Action(() => {}), 
            "Cancel",
            Popup.TransitionAnim.Scale, 
            PopupScreen.BackGround.Grey);

        var layout = popup.FindObject("Layout");
        
        // Quantity input
        var quantityPanel = layout.AddModHelperPanel(new Info("QuantityPanel", 800, 100),
            null, RectTransform.Axis.Horizontal, 20);
        
        var quantityLabel = quantityPanel.AddText(new Info("QuantityLabel", 350, 50), "Quantity:", 60);
        quantityLabel.Text.alignment = TextAlignmentOptions.MidlineRight;
        
        var quantityInput = quantityPanel.AddInputField(new Info("QuantityInput", 400, 80), 
            quantity.ToString(), VanillaSprites.BlueInsertPanelRound, 
            new Action<string>(value => 
            {
                if (int.TryParse(value, out int result))
                {
                    quantity = result;
                }
            }), 50, TMP_InputField.CharacterValidation.Digit);
        
        // Add spacing
        layout.AddModHelperPanel(new Info("Spacing", 800, 50));
    }

    // Apply medal template to maps
    private static void ShowApplyMedalTemplatePopup(bool isCoop)
    {
        var medalSettings = new Dictionary<string, Dictionary<string, bool>>
        {
            {
                "Easy", new Dictionary<string, bool>
                {
                    { "Standard", true },
                    { "PrimaryOnly", false },
                    { "Deflation", false }
                }
            },
            {
                "Medium", new Dictionary<string, bool>
                {
                    { "Standard", true },
                    { "MilitaryOnly", false },
                    { "Reverse", false },
                    { "Apopalypse", false }
                }
            },
            {
                "Hard", new Dictionary<string, bool>
                {
                    { "Standard", true },
                    { "MagicOnly", false },
                    { "AlternateBloonsRounds", false },
                    { "DoubleMoabHealth", false },
                    { "HalfCash", false },
                    { "Impoppable", false },
                    { "Clicks", false } // CHIMPS
                }
            }
        };
        
        var medalCompletionsCount = 1; // Default number of medal completions
        var applyEasy = true;
        var applyMedium = true;
        var applyHard = true;
        var selectedMapDifficulty = "All"; // All, Beginner, Intermediate, Advanced, Expert
        var percentageOfMaps = 100; // Percentage of maps to apply template to
        var exitless = false; // For black border/CHIMPS completion
        
        var popup = PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
            $"Apply Medal Template ({(isCoop ? "Co-op" : "Solo")})", 
            "Select which medals to apply to maps.",
            new Action(() =>
            {
                var player = Game.Player;
                var mapSet = GameData.Instance.mapSet;
                var maps = new List<string>();
                
                // Get maps based on selected difficulty
                var allMaps = mapSet.StandardMaps.ToIl2CppList();
                if (selectedMapDifficulty == "All")
                {
                    maps = allMaps.Select(m => m.id).ToList();
                }
                else
                {
                    maps = allMaps.Where(m => m.difficulty == selectedMapDifficulty)
                        .Select(m => m.id).ToList();
                }
                
                // If percentage is less than 100%, randomly select subset of maps
                if (percentageOfMaps < 100)
                {
                    var random = new Random();
                    var count = (int)Math.Ceiling(maps.Count * (percentageOfMaps / 100.0));
                    var tmpMaps = maps.ToList();
                    maps.Clear();
                    
                    for (int i = 0; i < count && tmpMaps.Count > 0; i++)
                    {
                        int idx = random.Next(tmpMaps.Count);
                        maps.Add(tmpMaps[idx]);
                        tmpMaps.RemoveAt(idx);
                    }
                }
                
                // Apply medals to selected maps
                foreach (var mapId in maps)
                {
                    var mapInfo = player.Data.mapInfo.GetMap(mapId);
                    
                    // Apply Easy medals
                    if (applyEasy)
                    {
                        foreach (var mode in medalSettings["Easy"])
                        {
                            if (mode.Value)
                            {
                                var modeInfo = mapInfo.GetOrCreateDifficulty("Easy")
                                    .GetOrCreateMode(mode.Key, isCoop);
                                modeInfo.timesCompleted = medalCompletionsCount;
                            }
                        }
                    }
                    
                    // Apply Medium medals
                    if (applyMedium)
                    {
                        foreach (var mode in medalSettings["Medium"])
                        {
                            if (mode.Value)
                            {
                                var modeInfo = mapInfo.GetOrCreateDifficulty("Medium")
                                    .GetOrCreateMode(mode.Key, isCoop);
                                modeInfo.timesCompleted = medalCompletionsCount;
                            }
                        }
                    }
                    
                    // Apply Hard medals
                    if (applyHard)
                    {
                        foreach (var mode in medalSettings["Hard"])
                        {
                            if (mode.Value)
                            {
                                var modeInfo = mapInfo.GetOrCreateDifficulty("Hard")
                                    .GetOrCreateMode(mode.Key, isCoop);
                                modeInfo.timesCompleted = medalCompletionsCount;
                                
                                // For CHIMPS, set exitless if needed
                                if (mode.Key == "Clicks" && exitless)
                                {
                                    modeInfo.completedWithoutLoadingSave = true;
                                }
                            }
                        }
                    }
                }
                
                // Save changes
                player.SaveNow();
                
                PopupScreen.instance.ShowOkPopup("Medal template has been applied to selected maps!");
            }), 
            "Apply", 
            new Action(() => {}), 
            "Cancel",
            Popup.TransitionAnim.Scale, 
            PopupScreen.BackGround.Grey);

        var layout = popup.FindObject("Layout");
        layout.gameObject.AddComponent<VerticalLayoutGroup>();
        var vlg = layout.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        
        // Medal Completions Count
        var completionsPanel = layout.AddModHelperPanel(new Info("CompletionsPanel", 800, 70),
            null, RectTransform.Axis.Horizontal, 20);
        
        completionsPanel.AddText(new Info("CompletionsLabel", 300, 50), "Completions:", 60)
            .Text.alignment = TextAlignmentOptions.MidlineRight;
        
        completionsPanel.AddInputField(new Info("CompletionsInput", 150, 70), 
            medalCompletionsCount.ToString(), VanillaSprites.BlueInsertPanelRound, 
            new Action<string>(value => 
            {
                if (int.TryParse(value, out int result) && result > 0)
                {
                    medalCompletionsCount = result;
                }
            }), 50, TMP_InputField.CharacterValidation.Digit);
        
        // Black Border option (CHIMPS without exiting)
        var blackBorderPanel = layout.AddModHelperPanel(new Info("BlackBorderPanel", 800, 60),
            null, RectTransform.Axis.Horizontal, 20);
        
        blackBorderPanel.AddText(new Info("BlackBorderLabel", 500, 50), "Black Border (CHIMPS without exiting):", 50)
            .Text.alignment = TextAlignmentOptions.MidlineRight;
        
        blackBorderPanel.AddModHelperComponent(
            ModHelperCheckbox.Create(new Info("BlackBorderCheckbox", 60, 60), 
                exitless, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => exitless = b)));
        
        // Map selection options
        var mapSelectionPanel = layout.AddModHelperPanel(new Info("MapSelectionPanel", 800, 80),
            null, RectTransform.Axis.Horizontal, 20);
        
        mapSelectionPanel.AddText(new Info("MapSelectionLabel", 300, 50), "Map Difficulty:", 50)
            .Text.alignment = TextAlignmentOptions.MidlineRight;
        
        mapSelectionPanel.AddDropdown(new Info("MapSelectionDropdown", 300, 80),
            new[] { "All", "Beginner", "Intermediate", "Advanced", "Expert" }.ToIl2CppList(), 300,
            new Action<int>(i => 
            {
                selectedMapDifficulty = new[] { "All", "Beginner", "Intermediate", "Advanced", "Expert" }[i];
            }), VanillaSprites.BlueInsertPanelRound, 50);
        
        // Percentage of maps
        var percentagePanel = layout.AddModHelperPanel(new Info("PercentagePanel", 800, 70),
            null, RectTransform.Axis.Horizontal, 20);
        
        percentagePanel.AddText(new Info("PercentageLabel", 500, 50), "Percentage of Maps (Random Selection):", 50)
            .Text.alignment = TextAlignmentOptions.MidlineRight;
        
        percentagePanel.AddInputField(new Info("PercentageInput", 150, 70), 
            percentageOfMaps.ToString(), VanillaSprites.BlueInsertPanelRound, 
            new Action<string>(value => 
            {
                if (int.TryParse(value, out int result) && result > 0 && result <= 100)
                {
                    percentageOfMaps = result;
                }
            }), 50, TMP_InputField.CharacterValidation.Digit);
        
        // Difficulty toggles
        var difficultyPanel = layout.AddModHelperPanel(new Info("DifficultyPanel", 800, 60),
            null, RectTransform.Axis.Horizontal, 20);
        
        var easyToggle = difficultyPanel.AddButton(new Info("EasyToggle", 200, 60), 
            VanillaSprites.GreenBtnLong, new Action(() => {
                applyEasy = !applyEasy;
                easyToggle.Image.color = applyEasy ? Color.green : Color.white;
            }));
        easyToggle.AddText(new Info("EasyText", 200, 60), "Easy", 50);
        easyToggle.Image.color = Color.green;
        
        var mediumToggle = difficultyPanel.AddButton(new Info("MediumToggle", 200, 60), 
            VanillaSprites.GreenBtnLong, new Action(() => {
                applyMedium = !applyMedium;
                mediumToggle.Image.color = applyMedium ? Color.green : Color.white;
            }));
        mediumToggle.AddText(new Info("MediumText", 200, 60), "Medium", 50);
        mediumToggle.Image.color = Color.green;
        
        var hardToggle = difficultyPanel.AddButton(new Info("HardToggle", 200, 60), 
            VanillaSprites.GreenBtnLong, new Action(() => {
                applyHard = !applyHard;
                hardToggle.Image.color = applyHard ? Color.green : Color.white;
            }));
        hardToggle.AddText(new Info("HardText", 200, 60), "Hard", 50);
        hardToggle.Image.color = Color.green;
        
        // Medal Selection
        var medalSelectionTitle = layout.AddText(new Info("MedalSelectionTitle", 800, 50), 
            "Select Medals to Apply:", 60);
        medalSelectionTitle.Text.alignment = TextAlignmentOptions.Midline;
        
        // Add individual medal toggles for each difficulty
        foreach (var difficulty in medalSettings.Keys)
        {
            var diffTitle = layout.AddText(new Info($"{difficulty}Title", 800, 40), 
                $"{difficulty} Medals:", 50);
            diffTitle.Text.alignment = TextAlignmentOptions.MidlineLeft;
            
            var modePanel = layout.AddModHelperPanel(new Info($"{difficulty}Panel", 800, 60),
                null, RectTransform.Axis.Horizontal, 15);
            
            foreach (var mode in medalSettings[difficulty].Keys.ToList())
            {
                var modeToggle = modePanel.AddButton(new Info($"{mode}Toggle", 180, 60), 
                    VanillaSprites.BlueBtnLong, new Action(() => {
                        medalSettings[difficulty][mode] = !medalSettings[difficulty][mode];
                        modeToggle.Image.color = medalSettings[difficulty][mode] ? Color.green : Color.white;
                    }));
                
                var modeName = mode == "Clicks" ? "CHIMPS" : mode;
                modeToggle.AddText(new Info($"{mode}Text", 180, 60), 
                    string.Join(" ", System.Text.RegularExpressions.Regex.Split(modeName, @"(?<!^)(?=[A-Z])"))
                    .Replace("Only", " Only").Replace("Bloons", " Bloons"), 30);
                
                modeToggle.Image.color = medalSettings[difficulty][mode] ? Color.green : Color.white;
            }
        }
        
        // Add spacing
        layout.AddModHelperPanel(new Info("Spacing", 800, 20));
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
