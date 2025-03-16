using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using EditPlayerData.Utils;
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
using MelonLoader;
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
                new NumberPlayerDataSetting("Highest Round", VanillaSprites.BadBloonIcon, 0,
                    () => GetPlayer().Data.highestSeenRound, t => GetPlayer().Data.highestSeenRound = t),
                
                // Original items continue below
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
    }

    private int LastPage => (Settings[_category].Count(s => s.Name.ContainsIgnoreCase(_searchValue))-1) / EntriesPerPage;

    private readonly PlayerDataSettingDisplay[] _entries = new PlayerDataSettingDisplay[EntriesPerPage];

    private static TMP_InputField? _searchInput;
    private string _searchValue = "";
    private string _category = "General";
    private int _pageIdx;

    private ModHelperPanel? _topArea;
    private ModHelperPanel? _bottomArea;

    private static Btd6Player GetPlayer()
    {
        return Game.Player;
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
        
        // Add "Unlock Everything" button
        _topArea.AddButton(new Info("UnlockEverything", 650, 200), VanillaSprites.GreenBtnLong, new Action(() =>
        {
            UnlockEverything();
            UpdateVisibleEntries();
        })).AddText(new Info("UnlockEverythingText", 650, 200), "Unlock Everything", 60);
        
        // Add bottom area for profile import/export buttons
        _bottomArea = GameMenu.GetComponentFromChildrenByName<RectTransform>("Container").gameObject
            .AddModHelperPanel(new Info("BottomArea")
            {
                Y = 250, Height = 170, Pivot = new Vector2(0.5f, 0),
                AnchorMin = new Vector2(0, 0), AnchorMax = new Vector2(1, 0)
            }, VanillaSprites.MainBGPanelBlue, RectTransform.Axis.Horizontal, padding: 50);

        // Add Export Profile button
        _bottomArea.AddButton(new Info("ExportProfile", 650, 140), VanillaSprites.GreenBtnLong, new Action(() =>
        {
            ExportPlayerProfile();
        })).AddText(new Info("ExportText", 650, 140), "Export Profile", 60);
        
        _bottomArea.AddPanel(new Info("Spacing", InfoPreset.Flex));
        
        // Add Import Profile button
        _bottomArea.AddButton(new Info("ImportProfile", 650, 140), VanillaSprites.YellowBtnLong, new Action(() =>
        {
            PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
                "Confirm Profile Import", 
                "This will overwrite your current profile data. Are you sure you want to continue?",
                new Action(() => { ImportPlayerProfile(); }), "Yes, Import",
                new Action(() => { }), "Cancel",
                Popup.TransitionAnim.Scale, PopupScreen.BackGround.Grey);
        })).AddText(new Info("ImportText", 650, 140), "Import Profile", 60);
        
        GenerateEntries();
        SetPage(0);

        // for no discernible reason, this defaults to 300
        GameMenu.scrollRect.scrollSensitivity = 50;
        
        return false;
    }

    // Method to export the player profile
    private void ExportPlayerProfile()
    {
        try
        {
            var playerData = GetPlayer().Data;
            var profileData = new Dictionary<string, object>();
            
            // Core account data
            profileData["Rank"] = playerData.rank.ValueInt;
            profileData["VeteranRank"] = playerData.veteranRank.ValueInt;
            profileData["XP"] = playerData.xp.ValueLong;
            profileData["VeteranXP"] = playerData.veteranXp.ValueLong;
            profileData["MonkeyMoney"] = playerData.monkeyMoney.ValueInt;
            profileData["Trophies"] = playerData.trophies.ValueInt;
            profileData["KnowledgePoints"] = playerData.knowledgePoints.ValueInt;
            
            // Game statistics
            profileData["CompletedGame"] = playerData.completedGame;
            profileData["HighestSeenRound"] = playerData.highestSeenRound;
            profileData["GoldenBloonsPopped"] = playerData.goldenBloonsPopped;
            profileData["ConsecutiveDailyChallengesCompleted"] = playerData.consecutiveDailyChallengesCompleted;
            profileData["TotalDailyChallengesCompleted"] = playerData.totalDailyChallengesCompleted;
            profileData["HostedCoopGames"] = playerData.hostedCoopGames;
            profileData["CollectionEventCratesOpened"] = playerData.collectionEventCratesOpened;
            profileData["DailyRewardIndex"] = playerData.dailyRewardIndex;
            
            // Feature unlocks
            var unlocks = new Dictionary<string, bool>
            {
                ["DoubleCash"] = playerData.purchase.HasMadeOneTimePurchase("btd6_doublecashmode"),
                ["FastTrack"] = playerData.unlockedFastTrack,
                ["MapEditor"] = playerData.purchase.HasMadeOneTimePurchase("btd6_mapeditorsupporter_new"),
                ["RogueLegends"] = playerData.purchase.HasMadeOneTimePurchase("btd6_legendsrogue"),
                ["BigBloons"] = playerData.unlockedBigBloons,
                ["SmallBloons"] = playerData.unlockedSmallBloons,
                ["BigMonkeys"] = playerData.unlockedBigTowers,
                ["SmallMonkeys"] = playerData.unlockedSmallTowers
            };
            profileData["Unlocks"] = unlocks;
            
            // Monkey Knowledge Points and unlocks
            profileData["KnowledgePoints"] = playerData.knowledgePoints.ValueInt;
            var knowledgeData = new Dictionary<string, Dictionary<string, bool>>();
            foreach (var path in playerData.knowledge.paths)
            {
                var pathData = new Dictionary<string, bool>();
                foreach (var upgrade in path.Value.upgrades)
                {
                    pathData[upgrade.Key] = upgrade.Value.unlocked;
                }
                knowledgeData[path.Key] = pathData;
            }
            profileData["Knowledge"] = knowledgeData;
            
            // Tower XP
            var towerXpData = new Dictionary<string, int>();
            foreach (var tower in playerData.towerXp)
            {
                towerXpData[tower.Key] = tower.Value.ValueInt;
            }
            profileData["TowerXP"] = towerXpData;
            
            // Unlocked towers
            profileData["UnlockedTowers"] = playerData.unlockedTowers.ToArray();
            
            // Acquired upgrades
            profileData["AcquiredUpgrades"] = playerData.acquiredUpgrades.ToArray();
            
            // Powers
            var powersData = new Dictionary<string, int>();
            foreach (var power in playerData.powers)
            {
                powersData[power.type] = power.Quantity;
            }
            profileData["Powers"] = powersData;
            
            // Insta monkeys
            var instaData = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var towerType in playerData.instaTowers)
            {
                var towerInstasData = new List<Dictionary<string, object>>();
                foreach (var insta in towerType.Value)
                {
                    towerInstasData.Add(new Dictionary<string, object>
                    {
                        ["Tiers"] = new [] { insta.tier1, insta.tier2, insta.tier3 },
                        ["Quantity"] = insta.Quantity,
                        ["Favorite"] = insta.favorite,
                        ["Sacrificed"] = insta.sacrificed,
                        ["CosmeticId"] = insta.cosmeticId
                    });
                }
                instaData[towerType.Key] = towerInstasData;
            }
            profileData["InstaTowers"] = instaData;
            
            // Map data
            var mapsData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var map in playerData.mapInfo.maps)
            {
                var mapData = new Dictionary<string, object>();
                mapData["Unlocked"] = playerData.mapInfo.IsMapUnlocked(map.Key);
                
                // Single player difficulty modes
                var spDifficulties = new Dictionary<string, Dictionary<string, object>>();
                foreach (var difficulty in map.Value.difficult)
                {
                    var difficultyData = new Dictionary<string, object>();
                    var modes = new Dictionary<string, Dictionary<string, object>>();
                    
                    foreach (var mode in difficulty.Value.modes)
                    {
                        modes[mode.Key] = new Dictionary<string, object>
                        {
                            ["TimesCompleted"] = mode.Value.timesCompleted,
                            ["CompletedWithoutLoadingSave"] = mode.Value.completedWithoutLoadingSave
                        };
                    }
                    
                    difficultyData["Modes"] = modes;
                    spDifficulties[difficulty.Key] = difficultyData;
                }
                mapData["SinglePlayer"] = spDifficulties;
                
                // Co-op difficulty modes
                var coopDifficulties = new Dictionary<string, Dictionary<string, object>>();
                foreach (var difficulty in map.Value.difficult)
                {
                    var difficultyData = new Dictionary<string, object>();
                    var modes = new Dictionary<string, Dictionary<string, object>>();
                    
                    foreach (var mode in difficulty.Value.coopModes)
                    {
                        modes[mode.Key] = new Dictionary<string, object>
                        {
                            ["TimesCompleted"] = mode.Value.timesCompleted,
                            ["CompletedWithoutLoadingSave"] = mode.Value.completedWithoutLoadingSave
                        };
                    }
                    
                    difficultyData["Modes"] = modes;
                    coopDifficulties[difficulty.Key] = difficultyData;
                }
                mapData["Coop"] = coopDifficulties;
                
                mapsData[map.Key] = mapData;
            }
            profileData["Maps"] = mapsData;
            
            // Boss medals
            var bossData = new Dictionary<string, Dictionary<string, int>>();
            foreach (var boss in playerData.bossMedals)
            {
                bossData[boss.Key.ToString()] = new Dictionary<string, int>
                {
                    ["Normal"] = boss.Value.normalBadges.ValueInt,
                    ["Elite"] = boss.Value.eliteBadges.ValueInt
                };
            }
            profileData["BossMedals"] = bossData;
            
            // Leaderboard medals
            var leaderboardData = new Dictionary<string, Dictionary<string, int>>();
            
            // Boss leaderboard medals
            var bossLeaderboardData = new Dictionary<string, int>();
            foreach (var medal in playerData.bossLeaderboardMedals)
            {
                bossLeaderboardData[medal.Key.ToString()] = medal.Value.ValueInt;
            }
            leaderboardData["Boss"] = bossLeaderboardData;
            
            // Elite boss leaderboard medals
            var eliteBossLeaderboardData = new Dictionary<string, int>();
            foreach (var medal in playerData.bossLeaderboardEliteMedals)
            {
                eliteBossLeaderboardData[medal.Key.ToString()] = medal.Value.ValueInt;
            }
            leaderboardData["EliteBoss"] = eliteBossLeaderboardData;
            
            // Race leaderboard medals
            var raceLeaderboardData = new Dictionary<string, int>();
            foreach (var medal in playerData.raceMedalData)
            {
                raceLeaderboardData[medal.Key.ToString()] = medal.Value.ValueInt;
            }
            leaderboardData["Race"] = raceLeaderboardData;
            
            profileData["LeaderboardMedals"] = leaderboardData;
            
            // Trophy store items
            var trophyStoreData = new Dictionary<string, bool>();
            foreach (var item in playerData.trophyStorePurchasedItems)
            {
                trophyStoreData[item.Key] = item.Value.enabled;
            }
            profileData["TrophyStore"] = trophyStoreData;
            
            // Current settings
            var settingsData = new Dictionary<string, object>();
            settingsData["MusicOn"] = playerData.settings.musicOn;
            settingsData["SoundEffectsOn"] = playerData.settings.soundEffectsOn;
            settingsData["AllSoundOn"] = playerData.settings.allSoundOn;
            settingsData["FastForwardSpeed"] = playerData.settings.fastForwardSpeed;
            settingsData["AlwaysShowEnemyPath"] = playerData.settings.AlwaysShowEnemyPath;
            settingsData["AlwaysShowRanges"] = playerData.settings.AlwaysShowRanges;
            profileData["Settings"] = settingsData;
            
            // Save purchases
            var purchasesData = new List<string>();
            foreach (var purchase in playerData.purchase.madeOneTimePurchaseItems)
            {
                purchasesData.Add(purchase);
            }
            profileData["Purchases"] = purchasesData;
            
            // Try to save the file
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(profileData, Newtonsoft.Json.Formatting.Indented);
            string filePath = Path.Combine(MelonEnvironment.ModsDirectory, "PlayerProfileBackup.json");
            File.WriteAllText(filePath, json);
            
            ModHelper.Msg<EditPlayerData>("Profile successfully exported to: " + filePath);
        }
        catch (Exception e)
        {
            ModHelper.Error<EditPlayerData>("Error exporting profile: " + e.Message);
        }
    }

    // Method to import the player profile
    private void ImportPlayerProfile()
    {
        try
        {
            string filePath = Path.Combine(MelonEnvironment.ModsDirectory, "PlayerProfileBackup.json");
            if (!File.Exists(filePath))
            {
                ModHelper.Error<EditPlayerData>("Profile backup file not found at: " + filePath);
                return;
            }
            
            string json = File.ReadAllText(filePath);
            var profileData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            
            if (profileData == null)
            {
                ModHelper.Error<EditPlayerData>("Failed to parse profile data");
                return;
            }
            
            var playerData = GetPlayer().Data;
            
            // Helper function to get nested values
            T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue = default)
            {
                if (dict.TryGetValue(key, out var value))
                {
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
            
            // Import core account data
            playerData.rank.Value = Convert.ToInt32(GetValue<object>(profileData, "Rank", 1));
            playerData.veteranRank.Value = Convert.ToInt32(GetValue<object>(profileData, "VeteranRank", 0));
            playerData.xp.Value = Convert.ToInt64(GetValue<object>(profileData, "XP", 0));
            playerData.veteranXp.Value = Convert.ToInt64(GetValue<object>(profileData, "VeteranXP", 0));
            playerData.monkeyMoney.Value = Convert.ToInt32(GetValue<object>(profileData, "MonkeyMoney", 0));
            playerData.trophies.Value = Convert.ToInt32(GetValue<object>(profileData, "Trophies", 0));
            playerData.knowledgePoints.Value = Convert.ToInt32(GetValue<object>(profileData, "KnowledgePoints", 0));
            
            // Import game statistics
            playerData.completedGame = Convert.ToInt32(GetValue<object>(profileData, "CompletedGame", 0));
            playerData.highestSeenRound = Convert.ToInt32(GetValue<object>(profileData, "HighestSeenRound", 0));
            playerData.goldenBloonsPopped = Convert.ToInt32(GetValue<object>(profileData, "GoldenBloonsPopped", 0));
            playerData.consecutiveDailyChallengesCompleted = Convert.ToInt32(GetValue<object>(profileData, "ConsecutiveDailyChallengesCompleted", 0));
            playerData.totalDailyChallengesCompleted = Convert.ToInt32(GetValue<object>(profileData, "TotalDailyChallengesCompleted", 0));
            playerData.hostedCoopGames = Convert.ToInt32(GetValue<object>(profileData, "HostedCoopGames", 0));
            playerData.collectionEventCratesOpened = Convert.ToInt32(GetValue<object>(profileData, "CollectionEventCratesOpened", 0));
            playerData.dailyRewardIndex = Convert.ToInt32(GetValue<object>(profileData, "DailyRewardIndex", 0));
            
            // Import feature unlocks
            if (profileData.TryGetValue("Unlocks", out var unlocksObj) && unlocksObj is Dictionary<string, object> unlocks)
            {
                bool doubleCash = GetValue<bool>(unlocks, "DoubleCash", false);
                if (doubleCash)
                {
                    playerData.purchase.AddOneTimePurchaseItem("btd6_doublecashmode");
                }
                else
                {
                    playerData.purchase.RemoveOneTimePurchaseItem("btd6_doublecashmode");
                }
                
                playerData.unlockedFastTrack = GetValue<bool>(unlocks, "FastTrack", false);
                
                bool mapEditor = GetValue<bool>(unlocks, "MapEditor", false);
                if (mapEditor)
                {
                    playerData.purchase.AddOneTimePurchaseItem("btd6_mapeditorsupporter_new");
                }
                else
                {
                    playerData.purchase.RemoveOneTimePurchaseItem("btd6_mapeditorsupporter_new");
                }
                
                bool rogueLegends = GetValue<bool>(unlocks, "RogueLegends", false);
                if (rogueLegends)
                {
                    playerData.purchase.AddOneTimePurchaseItem("btd6_legendsrogue");
                }
                else
                {
                    playerData.purchase.RemoveOneTimePurchaseItem("btd6_legendsrogue");
                }
                
                playerData.unlockedBigBloons = GetValue<bool>(unlocks, "BigBloons", false);
                playerData.unlockedSmallBloons = GetValue<bool>(unlocks, "SmallBloons", false);
                playerData.unlockedBigTowers = GetValue<bool>(unlocks, "BigMonkeys", false);
                playerData.unlockedSmallTowers = GetValue<bool>(unlocks, "SmallMonkeys", false);
            }
            
            // Import tower XP
            if (profileData.TryGetValue("TowerXP", out var towerXpObj) && towerXpObj is Dictionary<string, object> towerXpData)
            {
                foreach (var tower in towerXpData)
                {
                    if (!playerData.towerXp.ContainsKey(tower.Key))
                    {
                        playerData.towerXp[tower.Key] = new KonFuze_NoShuffle();
                    }
                    playerData.towerXp[tower.Key].Value = Convert.ToInt32(tower.Value);
                }
            }
            
            // Import unlocked towers
            if (profileData.TryGetValue("UnlockedTowers", out var unlockedTowersObj))
            {
                if (unlockedTowersObj is Newtonsoft.Json.Linq.JArray unlockedTowersArray)
                {
                    playerData.unlockedTowers.Clear();
                    foreach (var tower in unlockedTowersArray)
                    {
                        playerData.unlockedTowers.Add(tower.ToString());
                    }
                }
            }
            
            // Import acquired upgrades
            if (profileData.TryGetValue("AcquiredUpgrades", out var acquiredUpgradesObj))
            {
                if (acquiredUpgradesObj is Newtonsoft.Json.Linq.JArray acquiredUpgradesArray)
                {
                    playerData.acquiredUpgrades.Clear();
                    foreach (var upgrade in acquiredUpgradesArray)
                    {
                        playerData.acquiredUpgrades.Add(upgrade.ToString());
                    }
                }
            }
            
            // Import powers
            if (profileData.TryGetValue("Powers", out var powersObj) && powersObj is Dictionary<string, object> powersData)
            {
                foreach (var power in powersData)
                {
                    if (GetPlayer().IsPowerAvailable(power.Key))
                    {
                        GetPlayer().GetPowerData(power.Key).Quantity = Convert.ToInt32(power.Value);
                    }
                    else
                    {
                        GetPlayer().AddPower(power.Key, Convert.ToInt32(power.Value));
                    }
                }
            }
            
            // Import insta monkeys
            if (profileData.TryGetValue("InstaTowers", out var instaObj) && instaObj is Dictionary<string, object> instaData)
            {
                // Clear existing instas
                foreach (var tower in playerData.instaTowers)
                {
                    tower.Value.Clear();
                }
                
                // Add imported instas
                foreach (var towerType in instaData)
                {
                    if (towerType.Value is Newtonsoft.Json.Linq.JArray towerInstas)
                    {
                        foreach (var instaJObj in towerInstas)
                        {
                            try
                            {
                                var instaTower = new InstaTowerModel();
                                
                                if (instaJObj["Tiers"] is Newtonsoft.Json.Linq.JArray tiersArray)
                                {
                                    instaTower.tier1 = Convert.ToInt32(tiersArray[0]);
                                    instaTower.tier2 = Convert.ToInt32(tiersArray[1]);
                                    instaTower.tier3 = Convert.ToInt32(tiersArray[2]);
                                }
                                
                                instaTower.Quantity = Convert.ToInt32(instaJObj["Quantity"] ?? 1);
                                instaTower.favorite = Convert.ToBoolean(instaJObj["Favorite"] ?? false);
                                instaTower.sacrificed = Convert.ToBoolean(instaJObj["Sacrificed"] ?? false);
                                instaTower.cosmeticId = Convert.ToString(instaJObj["CosmeticId"] ?? "");
                                
                                GetPlayer().GetInstaTower(towerType.Key, new[] { instaTower.tier1, instaTower.tier2, instaTower.tier3 }).Quantity = instaTower.Quantity;
                            }
                            catch (Exception e)
                            {
                                ModHelper.Warning<EditPlayerData>($"Error importing insta monkey: {e.Message}");
                            }
                        }
                    }
                }
            }
            
            // Import map data
            if (profileData.TryGetValue("Maps", out var mapsObj) && mapsObj is Dictionary<string, object> mapsData)
            {
                foreach (var mapEntry in mapsData)
                {
                    var mapInfo = playerData.mapInfo.GetMap(mapEntry.Key);
                    
                    if (mapEntry.Value is Dictionary<string, object> mapData)
                    {
                        // Unlock map if needed
                        bool isUnlocked = GetValue<bool>(mapData, "Unlocked", false);
                        if (isUnlocked && !playerData.mapInfo.IsMapUnlocked(mapEntry.Key))
                        {
                            playerData.mapInfo.UnlockMap(mapEntry.Key);
                        }
                        
                        // Import single player mode data
                        if (mapData.TryGetValue("SinglePlayer", out var spObj) && spObj is Dictionary<string, object> spDifficulties)
                        {
                            foreach (var difficulty in spDifficulties)
                            {
                                if (difficulty.Value is Dictionary<string, object> difficultyData && 
                                    difficultyData.TryGetValue("Modes", out var modesObj) && 
                                    modesObj is Dictionary<string, object> modes)
                                {
                                    foreach (var mode in modes)
                                    {
                                        if (mode.Value is Dictionary<string, object> modeData)
                                        {
                                            var modeInfo = mapInfo.GetOrCreateDifficulty(difficulty.Key).GetOrCreateMode(mode.Key, false);
                                            modeInfo.timesCompleted = Convert.ToInt32(GetValue<object>(modeData, "TimesCompleted", 0));
                                            modeInfo.completedWithoutLoadingSave = GetValue<bool>(modeData, "CompletedWithoutLoadingSave", false);
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Import co-op mode data
                        if (mapData.TryGetValue("Coop", out var coopObj) && coopObj is Dictionary<string, object> coopDifficulties)
                        {
                            foreach (var difficulty in coopDifficulties)
                            {
                                if (difficulty.Value is Dictionary<string, object> difficultyData && 
                                    difficultyData.TryGetValue("Modes", out var modesObj) && 
                                    modesObj is Dictionary<string, object> modes)
                                {
                                    foreach (var mode in modes)
                                    {
                                        if (mode.Value is Dictionary<string, object> modeData)
                                        {
                                            var modeInfo = mapInfo.GetOrCreateDifficulty(difficulty.Key).GetOrCreateMode(mode.Key, true);
                                            modeInfo.timesCompleted = Convert.ToInt32(GetValue<object>(modeData, "TimesCompleted", 0));
                                            modeInfo.completedWithoutLoadingSave = GetValue<bool>(modeData, "CompletedWithoutLoadingSave", false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Import boss medals
            if (profileData.TryGetValue("BossMedals", out var bossObj) && bossObj is Dictionary<string, object> bossData)
            {
                foreach (var boss in bossData)
                {
                    if (int.TryParse(boss.Key, out int bossId) && boss.Value is Dictionary<string, object> medalData)
                    {
                        if (!playerData.bossMedals.ContainsKey(bossId))
                        {
                            playerData.bossMedals[bossId] = new BossMedalSaveData();
                        }
                        
                        playerData.bossMedals[bossId].normalBadges.Value = Convert.ToInt32(GetValue<object>(medalData, "Normal", 0));
                        playerData.bossMedals[bossId].eliteBadges.Value = Convert.ToInt32(GetValue<object>(medalData, "Elite", 0));
                    }
                }
            }
            
            // Import leaderboard medals
            if (profileData.TryGetValue("LeaderboardMedals", out var leaderboardObj) && leaderboardObj is Dictionary<string, object> leaderboardData)
            {
                // Boss leaderboard medals
                if (leaderboardData.TryGetValue("Boss", out var bossLeaderboardObj) && bossLeaderboardObj is Dictionary<string, object> bossLeaderboardData)
                {
                    foreach (var medal in bossLeaderboardData)
                    {
                        if (int.TryParse(medal.Key, out int medalId))
                        {
                            if (!playerData.bossLeaderboardMedals.ContainsKey(medalId))
                            {
                                playerData.bossLeaderboardMedals[medalId] = new KonFuze_NoShuffle();
                            }
                            
                            playerData.bossLeaderboardMedals[medalId].Value = Convert.ToInt32(medal.Value);
                        }
                    }
                }
                
                // Elite boss leaderboard medals
                if (leaderboardData.TryGetValue("EliteBoss", out var eliteBossLeaderboardObj) && eliteBossLeaderboardObj is Dictionary<string, object> eliteBossLeaderboardData)
                {
                    foreach (var medal in eliteBossLeaderboardData)
                    {
                        if (int.TryParse(medal.Key, out int medalId))
                        {
                            if (!playerData.bossLeaderboardEliteMedals.ContainsKey(medalId))
                            {
                                playerData.bossLeaderboardEliteMedals[medalId] = new KonFuze_NoShuffle();
                            }
                            
                            playerData.bossLeaderboardEliteMedals[medalId].Value = Convert.ToInt32(medal.Value);
                        }
                    }
                }
                
                // Race leaderboard medals
                if (leaderboardData.TryGetValue("Race", out var raceLeaderboardObj) && raceLeaderboardObj is Dictionary<string, object> raceLeaderboardData)
                {
                    foreach (var medal in raceLeaderboardData)
                    {
                        if (int.TryParse(medal.Key, out int medalId))
                        {
                            if (!playerData.raceMedalData.ContainsKey(medalId))
                            {
                                playerData.raceMedalData[medalId] = new KonFuze_NoShuffle();
                            }
                            
                            playerData.raceMedalData[medalId].Value = Convert.ToInt32(medal.Value);
                        }
                    }
                }
            }
            
            // Import trophy store items
            if (profileData.TryGetValue("TrophyStore", out var trophyStoreObj) && trophyStoreObj is Dictionary<string, object> trophyStoreData)
            {
                foreach (var item in trophyStoreData)
                {
                    bool enabled = Convert.ToBoolean(item.Value);
                    
                    if (!playerData.trophyStorePurchasedItems.ContainsKey(item.Key) && enabled)
                    {
                        GetPlayer().AddTrophyStoreItem(item.Key);
                    }
                    
                    if (playerData.trophyStorePurchasedItems.ContainsKey(item.Key))
                    {
                        playerData.trophyStorePurchasedItems[item.Key].enabled = enabled;
                    }
                }
            }
            
            // Import settings
            if (profileData.TryGetValue("Settings", out var settingsObj) && settingsObj is Dictionary<string, object> settingsData)
            {
                playerData.settings.musicOn = GetValue<bool>(settingsData, "MusicOn", true);
                playerData.settings.soundEffectsOn = GetValue<bool>(settingsData, "SoundEffectsOn", true);
                playerData.settings.allSoundOn = GetValue<bool>(settingsData, "AllSoundOn", true);
                playerData.settings.fastForwardSpeed = GetValue<float>(settingsData, "FastForwardSpeed", 3f);
                playerData.settings.AlwaysShowEnemyPath = GetValue<bool>(settingsData, "AlwaysShowEnemyPath", false);
                playerData.settings.AlwaysShowRanges = GetValue<bool>(settingsData, "AlwaysShowRanges", false);
            }
            
            // Import purchases
            if (profileData.TryGetValue("Purchases", out var purchasesObj))
            {
                if (purchasesObj is Newtonsoft.Json.Linq.JArray purchasesArray)
                {
                    foreach (var purchaseId in purchasesArray)
                    {
                        playerData.purchase.AddOneTimePurchaseItem(purchaseId.ToString());
                    }
                }
            }
            
            // Save changes
            GetPlayer().SaveNow();
            
            // Update UI
            UpdateVisibleEntries();
            
            ModHelper.Msg<EditPlayerData>("Profile successfully imported!");
        }
        catch (Exception e)
        {
            ModHelper.Error<EditPlayerData>("Error importing profile: " + e.Message);
        }
    }

    // Add method to implement the "Unlock Everything" functionality
    private void UnlockEverything()
    {
        try
        {
            // Unlock game modes
            var doubleCashSetting = Settings["General"].Find(s => s.Name == "Unlocked Double Cash") as PurchasePlayerDataSetting;
            doubleCashSetting?.Unlock();
            
            var fastTrackSetting = Settings["General"].Find(s => s.Name == "Unlocked Fast Track") as PurchasePlayerDataSetting;
            fastTrackSetting?.Unlock();
            
            var rogueLegendsSettings = Settings["General"].Find(s => s.Name == "Unlocked Rogue Legends") as PurchasePlayerDataSetting;
            rogueLegendsSettings?.Unlock();
            
            var mapEditorSetting = Settings["General"].Find(s => s.Name == "Unlocked Map Editor") as PurchasePlayerDataSetting;
            mapEditorSetting?.Unlock();
            
            // Set Monkey Money to 795,000
            GetPlayer().Data.monkeyMoney.Value = 795000;
            
            // Trophy Store Items: Unlock all
            foreach (var setting in Settings["Trophy Store"])
            {
                setting.Unlock();
            }
            
            // Maps: Unlock all
            foreach (var setting in Settings["Maps"])
            {
                setting.Unlock();
            }
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error during initial unlocks: " + e.Message);
        }
        
        // Unlock all map badges (both single player and co-op)
        try
        {
            var difficultyModes = new Dictionary<string, string[]>
            {
                { "Easy", new[] { "Standard", "PrimaryOnly", "Deflation" } },
                { "Medium", new[] { "Standard", "MilitaryOnly", "Reverse", "Apopalypse" } },
                { "Hard", new[] { "Standard", "MagicOnly", "AlternateBloonsRounds", "DoubleMoabHealth", "HalfCash", "Impoppable", "Clicks" } }
            };
            
            foreach (var map in GameData.Instance.mapSet.StandardMaps.ToIl2CppList())
            {
                var mapInfo = Game.Player.Data.mapInfo.GetMap(map.id);
                foreach (var difficultyEntry in difficultyModes)
                {
                    var difficulty = mapInfo.GetOrCreateDifficulty(difficultyEntry.Key);
                    
                    // Single-player medals
                    foreach (var mode in difficultyEntry.Value)
                    {
                        var modeInfo = difficulty.GetOrCreateMode(mode, false);
                        modeInfo.timesCompleted = 1;
                        
                        // For CHIMPS mode, set completed without loading save
                        if (mode == "Clicks")
                        {
                            modeInfo.completedWithoutLoadingSave = true;
                        }
                    }
                    
                    // Co-op medals
                    foreach (var mode in difficultyEntry.Value)
                    {
                        var modeInfo = difficulty.GetOrCreateMode(mode, true);
                        modeInfo.timesCompleted = 1;
                        
                        // For CHIMPS mode, set completed without loading save
                        if (mode == "Clicks")
                        {
                            modeInfo.completedWithoutLoadingSave = true;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error unlocking map medals: " + e.Message);
        }
        
        // Unlock all towers and upgrades
        try
        {
            foreach (var tower in Game.instance.GetTowerDetailModels())
            {
                // Unlock tower
                if (!GetPlayer().Data.unlockedTowers.Contains(tower.towerId))
                {
                    GetPlayer().Data.UnlockTower(tower.towerId);
                }
                
                // Add high XP to ensure upgrades unlock
                if (!GetPlayer().Data.towerXp.ContainsKey(tower.towerId))
                {
                    GetPlayer().Data.towerXp[tower.towerId] = new KonFuze_NoShuffle(5000000);
                }
                else
                {
                    GetPlayer().Data.towerXp[tower.towerId].Value = 5000000;
                }
                
                // Explicitly unlock all upgrades
                var model = Game.instance.model;
                var upgrades = new List<string>();
                
                try 
                {
                    if (model.GetTower(tower.towerId, pathOneTier: 5) != null)
                    {
                        foreach (var upgrade in model.GetTower(tower.towerId, pathOneTier: 5).appliedUpgrades)
                        {
                            upgrades.Add(upgrade);
                        }
                    }
                } 
                catch { }
                
                try 
                {
                    if (model.GetTower(tower.towerId, pathTwoTier: 5) != null)
                    {
                        foreach (var upgrade in model.GetTower(tower.towerId, pathTwoTier: 5).appliedUpgrades)
                        {
                            upgrades.Add(upgrade);
                        }
                    }
                } 
                catch { }
                
                try 
                {
                    if (model.GetTower(tower.towerId, pathThreeTier: 5) != null)
                    {
                        foreach (var upgrade in model.GetTower(tower.towerId, pathThreeTier: 5).appliedUpgrades)
                        {
                            upgrades.Add(upgrade);
                        }
                    }
                } 
                catch { }
                
                try
                {
                    var paragon = model.GetParagonUpgradeForTowerId(tower.towerId);
                    if (paragon != null)
                    {
                        upgrades.Add(paragon.name);
                    }
                }
                catch { }
                
                foreach (var upgrade in upgrades)
                {
                    if (!GetPlayer().Data.acquiredUpgrades.Contains(upgrade))
                    {
                        GetPlayer().Data.acquiredUpgrades.Add(upgrade);
                    }
                }
            }
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error unlocking towers: " + e.Message);
        }
        
        // Set all powers to 450
        try
        {
            foreach (var power in Game.instance.model.powers)
            {
                if (power.name is "CaveMonkey" or "DungeonStatue" or "SpookyCreature") continue;
                
                if (GetPlayer().IsPowerAvailable(power.name))
                {
                    GetPlayer().GetPowerData(power.name).Quantity = 450;
                }
                else
                {
                    GetPlayer().AddPower(power.name, 450);
                }
            }
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error setting powers: " + e.Message);
        }
        
        // Set all instas to 100 of each
        try
        {
            foreach (var tower in Game.instance.GetTowerDetailModels())
            {
                var tierSet = new HashSet<int[]>(new TowerTiersEqualityComparer());
                
                for (var mainPath = 0; mainPath < 3; mainPath++)
                {
                    for (var mainPathTier = 0; mainPathTier <= 5; mainPathTier++)
                    {
                
                        for (var crossPath = 0; crossPath < 3; crossPath++)
                        {
                            for (var crossPathTier = 0; crossPathTier <= 2; crossPathTier++)
                            {
                                var tiers = new[] { 0, 0, 0 };
                                tiers[crossPath] = crossPathTier;
                                tiers[mainPath] = mainPathTier;

                                tierSet.Add(tiers);
                            }
                        }
                    }
                }
                
                foreach (var tiers in tierSet)
                {
                    try 
                    {
                        GetPlayer().GetInstaTower(tower.towerId, tiers).Quantity = 100;
                    }
                    catch { /* Ignore invalid tower configurations */ }
                }
            }
        }
        catch (Exception e)
        {
            ModHelper.Msg<EditPlayerData>("Error setting instas: " + e.Message);
        }
        
        // Save changes
        Game.Player.SaveNow();
        
        ModHelper.Msg<EditPlayerData>("Successfully unlocked everything!");
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
        _topArea?.GetDescendent<ModHelperButton>("UnlockAll")?.SetActive(anyUnlockable);
        var filler = _topArea?.GetDescendent<ModHelperPanel>("UnlockAll Filler");
        if (filler != null) filler.SetActive(!anyUnlockable);

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
