using System;
using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
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
            "Trophy Store", new List<PlayerDataSetting>()
        },
        {
            "Maps", new List<PlayerDataSetting>()
        },
        {
            "Maps - Coop", new List<PlayerDataSetting>()
        },
        {
            "Tower XP", new List<PlayerDataSetting>()
        },
        {
            "Powers", new List<PlayerDataSetting>()
        },
        {
            "Instas", new List<PlayerDataSetting>()
        },
        {
            "Online Modes", new List<PlayerDataSetting>()
        },
        {
            "Prefix", new List<PlayerDataSetting>()
        }
    };

    private static bool _isOpen;

    private const int EntriesPerPage = 5;

    // Prefix system variables
    private static class PrefixSettings
    {
        public static bool MonkeyMoney = true;
        public static int MonkeyMoneyMin = 500000;
        public static int MonkeyMoneyMax = 550000;
        
        public static bool Powers = true;
        public static int PowersAmount = 400;
        
        public static bool InstaMonkeys = true;
        public static int InstaMonkeysAmount = 100;
        
        public static bool UnlockAllTowers = true;
        
        public static bool TowerXP = true;
        public static int TowerXPMin = 450000;
        public static int TowerXPMax = 550000;
        
        public static bool DoubleCash = true;
        public static bool FastTrack = true;
        public static bool RogueLegends = true;
        public static bool MapEditor = true;
        
        public static bool PlayerLevel = true;
        public static int PlayerLevelValue = 155;
        
        public static bool ApplyMedals = true;
        public static int MedalsPercentage = 75;
        
        public static Random random = new Random();
    }

    public static void InitSettings(ProfileModel data)
    {
        Settings["Trophy Store"].Clear();
        Settings["Maps"].Clear();
        Settings["Maps - Coop"].Clear();
        Settings["Tower XP"].Clear();
        Settings["Powers"].Clear();
        Settings["Instas"].Clear();
        Settings["Online Modes"].Clear();
        Settings["Prefix"].Clear();
        
        // Initialize Prefix settings
        Settings["Prefix"].Add(new PrefixSettingDisplay());
        
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

    private int LastPage => Math.Max(0, (Settings[_category].Count(s => s.Name.ContainsIgnoreCase(_searchValue))-1) / EntriesPerPage);

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
        // Special handling for Prefix category
        if (_category == "Prefix")
        {
            _topArea!.GetDescendent<ModHelperButton>("UnlockAll").SetActive(false);
            _topArea.GetDescendent<ModHelperPanel>("UnlockAll Filler").SetActive(true);
            
            // Clear all entries first
            for (var i = 0; i < EntriesPerPage; i++)
            {
                _entries[i].SetActive(false);
            }
            
            // Show the prefix UI in the first entry
            if (Settings["Prefix"].Count > 0 && Settings["Prefix"][0] is PrefixSettingDisplay prefixSetting)
            {
                _entries[0].SetActive(true);
                _entries[0].transform.DestroyAllChildren();
                _entries[0].RectTransform.sizeDelta = new Vector2(0, 2000); // Make it bigger for the content
                prefixSetting.CreatePrefixUI(_entries[0]);
            }
            
            // Hide pagination for prefix
            GameMenu.firstPageBtn.interactable = false;
            GameMenu.previousPageBtn.interactable = false;
            GameMenu.lastPageBtn.interactable = false;
            GameMenu.nextPageBtn.interactable = false;
            GameMenu.SetCurrentPage(1);
            GameMenu.totalPages = 1;
            
            return;
        }
        
        var anyUnlockable = Settings[_category].Any(s => !s.IsUnlocked());
        _topArea!.GetDescendent<ModHelperButton>("UnlockAll").SetActive(anyUnlockable);
        _topArea.GetDescendent<ModHelperPanel>("UnlockAll Filler").SetActive(!anyUnlockable);

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
    
    // Custom Prefix Setting Display
    public class PrefixSettingDisplay : PlayerDataSetting
    {
        public PrefixSettingDisplay() : base("Quick Preset System", VanillaSprites.SettingsIcon)
        {
        }

        protected override ModHelperComponent GetValue()
        {
            return ModHelperText.Create(new Info("Text", InfoPreset.FillParent), "Configure and apply presets", 60);
        }

        protected override void ShowEditValuePopup(PopupScreen screen)
        {
            // Not used for prefix
        }

        public override void ResetToDefault()
        {
            // Reset all prefix settings to default
            PrefixSettings.MonkeyMoney = true;
            PrefixSettings.MonkeyMoneyMin = 500000;
            PrefixSettings.MonkeyMoneyMax = 550000;
            PrefixSettings.Powers = true;
            PrefixSettings.PowersAmount = 400;
            PrefixSettings.InstaMonkeys = true;
            PrefixSettings.InstaMonkeysAmount = 100;
            PrefixSettings.UnlockAllTowers = true;
            PrefixSettings.TowerXP = true;
            PrefixSettings.TowerXPMin = 450000;
            PrefixSettings.TowerXPMax = 550000;
            PrefixSettings.DoubleCash = true;
            PrefixSettings.FastTrack = true;
            PrefixSettings.RogueLegends = true;
            PrefixSettings.MapEditor = true;
            PrefixSettings.PlayerLevel = true;
            PrefixSettings.PlayerLevelValue = 155;
            PrefixSettings.ApplyMedals = true;
            PrefixSettings.MedalsPercentage = 75;
        }

        public void CreatePrefixUI(ModHelperPanel parent)
        {
            parent.transform.DestroyAllChildren();
            
            var scrollPanel = parent.AddScrollPanel(new Info("PrefixScroll", 0, 0, 1950, 1800), 
                RectTransform.Axis.Vertical, VanillaSprites.MainBGPanelBlue, 50, 50);
            
            var content = scrollPanel.ScrollContent;
            
            // Title
            content.AddText(new Info("Title", 1800, 120), "Quick Preset Configuration", 100)
                .Text.alignment = TextAlignmentOptions.Center;
            
            // Monkey Money
            var moneyPanel = content.AddPanel(new Info("MoneyPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            var moneyCheckbox = moneyPanel.AddCheckbox(new Info("MoneyCheck", 100), 
                PrefixSettings.MonkeyMoney, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.MonkeyMoney = b));
            moneyPanel.AddText(new Info("MoneyLabel", 400), "Monkey Money:", 60);
            var moneyMin = moneyPanel.AddInputField(new Info("MoneyMin", 300, 100), 
                PrefixSettings.MonkeyMoneyMin.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.MonkeyMoneyMin = v; }),
                50, TMP_InputField.CharacterValidation.Integer);
            moneyPanel.AddText(new Info("ToDash", 100), " - ", 50);
            var moneyMax = moneyPanel.AddInputField(new Info("MoneyMax", 300, 100), 
                PrefixSettings.MonkeyMoneyMax.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.MonkeyMoneyMax = v; }),
                50, TMP_InputField.CharacterValidation.Integer);
            
            // Powers
            var powersPanel = content.AddPanel(new Info("PowersPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            powersPanel.AddCheckbox(new Info("PowersCheck", 100), 
                PrefixSettings.Powers, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.Powers = b));
            powersPanel.AddText(new Info("PowersLabel", 400), "Powers Amount:", 60);
            powersPanel.AddInputField(new Info("PowersAmount", 300, 100), 
                PrefixSettings.PowersAmount.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.PowersAmount = v; }),
                50, TMP_InputField.CharacterValidation.Integer);
            
            // Insta Monkeys
            var instaPanel = content.AddPanel(new Info("InstaPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            instaPanel.AddCheckbox(new Info("InstaCheck", 100), 
                PrefixSettings.InstaMonkeys, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.InstaMonkeys = b));
            instaPanel.AddText(new Info("InstaLabel", 400), "Insta Monkeys:", 60);
            instaPanel.AddInputField(new Info("InstaAmount", 300, 100), 
                PrefixSettings.InstaMonkeysAmount.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.InstaMonkeysAmount = v; }),
                50, TMP_InputField.CharacterValidation.Integer);
            
            // Tower XP
            var xpPanel = content.AddPanel(new Info("XPPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            xpPanel.AddCheckbox(new Info("XPCheck", 100), 
                PrefixSettings.TowerXP, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.TowerXP = b));
            xpPanel.AddText(new Info("XPLabel", 400), "Tower XP:", 60);
            xpPanel.AddInputField(new Info("XPMin", 300, 100), 
                PrefixSettings.TowerXPMin.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.TowerXPMin = v; }),
                50, TMP_InputField.CharacterValidation.Integer);
            xpPanel.AddText(new Info("ToDash2", 100), " - ", 50);
            xpPanel.AddInputField(new Info("XPMax", 300, 100), 
                PrefixSettings.TowerXPMax.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.TowerXPMax = v; }),
                50, TMP_InputField.CharacterValidation.Integer);
            
            // Unlock All Towers
            var unlockPanel = content.AddPanel(new Info("UnlockPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            unlockPanel.AddCheckbox(new Info("UnlockCheck", 100), 
                PrefixSettings.UnlockAllTowers, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.UnlockAllTowers = b));
            unlockPanel.AddText(new Info("UnlockLabel", 600), "Unlock All Towers", 60);
            
            // Player Level
            var levelPanel = content.AddPanel(new Info("LevelPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            levelPanel.AddCheckbox(new Info("LevelCheck", 100), 
                PrefixSettings.PlayerLevel, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.PlayerLevel = b));
            levelPanel.AddText(new Info("LevelLabel", 400), "Player Level:", 60);
            levelPanel.AddInputField(new Info("LevelValue", 300, 100), 
                PrefixSettings.PlayerLevelValue.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.PlayerLevelValue = v; }),
                50, TMP_InputField.CharacterValidation.Integer);
            
            // Premium Features Header
            content.AddText(new Info("PremiumHeader", 1800, 100), "Premium Features", 80)
                .Text.alignment = TextAlignmentOptions.Center;
            
            // Double Cash
            var doubleCashPanel = content.AddPanel(new Info("DoubleCashPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            doubleCashPanel.AddCheckbox(new Info("DoubleCashCheck", 100), 
                PrefixSettings.DoubleCash, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.DoubleCash = b));
            doubleCashPanel.AddText(new Info("DoubleCashLabel", 600), "Double Cash Mode", 60);
            
            // Fast Track
            var fastTrackPanel = content.AddPanel(new Info("FastTrackPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            fastTrackPanel.AddCheckbox(new Info("FastTrackCheck", 100), 
                PrefixSettings.FastTrack, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.FastTrack = b));
            fastTrackPanel.AddText(new Info("FastTrackLabel", 600), "Fast Track Mode", 60);
            
            // Rogue Legends
            var roguePanel = content.AddPanel(new Info("RoguePanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            roguePanel.AddCheckbox(new Info("RogueCheck", 100), 
                PrefixSettings.RogueLegends, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.RogueLegends = b));
            roguePanel.AddText(new Info("RogueLabel", 600), "Rogue Legends", 60);
            
            // Map Editor
            var mapEditorPanel = content.AddPanel(new Info("MapEditorPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            mapEditorPanel.AddCheckbox(new Info("MapEditorCheck", 100), 
                PrefixSettings.MapEditor, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.MapEditor = b));
            mapEditorPanel.AddText(new Info("MapEditorLabel", 600), "Map Editor", 60);
            
            // Medals
            var medalsPanel = content.AddPanel(new Info("MedalsPanel", 1800, 150), 
                VanillaSprites.MainBgPanelWhiteSmall, RectTransform.Axis.Horizontal, 25);
            medalsPanel.AddCheckbox(new Info("MedalsCheck", 100), 
                PrefixSettings.ApplyMedals, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(b => PrefixSettings.ApplyMedals = b));
            medalsPanel.AddText(new Info("MedalsLabel", 600), "Apply Medals (% based on level):", 60);
            medalsPanel.AddInputField(new Info("MedalsPercent", 200, 100), 
                PrefixSettings.MedalsPercentage.ToString(), VanillaSprites.BlueInsertPanelRound,
                new Action<string>(s => { if (int.TryParse(s, out var v)) PrefixSettings.MedalsPercentage = Math.Clamp(v, 0, 100); }),
                50, TMP_InputField.CharacterValidation.Integer);
            medalsPanel.AddText(new Info("PercentSign", 50), "%", 50);
            
            // Apply Button
            content.AddButton(new Info("ApplyButton", 800, 200), VanillaSprites.GreenBtnLong,
                new Action(() => ApplyPreset())).AddText(new Info("ApplyText"), "Apply Preset", 80);
        }
        
        private void ApplyPreset()
        {
            PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, "Apply Preset?",
                "This will apply all selected settings. Are you sure?",
                new Action(() =>
                {
                    var player = GetPlayer();
                    
                    // Monkey Money
                    if (PrefixSettings.MonkeyMoney)
                    {
                        var amount = PrefixSettings.random.Next(PrefixSettings.MonkeyMoneyMin, PrefixSettings.MonkeyMoneyMax + 1);
                        player.Data.monkeyMoney.Value = amount;
                    }
                    
                    // Powers
                    if (PrefixSettings.Powers)
                    {
                        foreach (var power in Game.instance.model.powers)
                        {
                            if (power.name is "CaveMonkey" or "DungeonStatue" or "SpookyCreature") continue;
                            
                            if (player.IsPowerAvailable(power.name))
                            {
                                player.GetPowerData(power.name).Quantity = PrefixSettings.PowersAmount;
                            }
                            else
                            {
                                player.AddPower(power.name, PrefixSettings.PowersAmount);
                            }
                        }
                    }
                    
                    // Unlock All Towers
                    if (PrefixSettings.UnlockAllTowers)
                    {
                        foreach (var tower in Game.instance.GetTowerDetailModels())
                        {
                            if (!player.Data.unlockedTowers.Contains(tower.towerId))
                            {
                                Game.instance.towerGoalUnlockManager.CompleteGoalForTower(tower.towerId);
                                player.Data.UnlockTower(tower.towerId);
                            }
                        }
                    }
                    
                    // Tower XP
                    if (PrefixSettings.TowerXP)
                    {
                        var towers = Game.instance.GetTowerDetailModels().ToList();
                        foreach (var tower in towers)
                        {
                            var xp = PrefixSettings.random.Next(PrefixSettings.TowerXPMin, PrefixSettings.TowerXPMax + 1);
                            // Add some variation between towers
                            xp += PrefixSettings.random.Next(-5000, 5001);
                            xp = Math.Max(0, xp);
                            
                            if (!player.Data.towerXp.ContainsKey(tower.towerId))
                            {
                                player.Data.towerXp[tower.towerId] = new KonFuze_NoShuffle(xp);
                            }
                            else
                            {
                                player.Data.towerXp[tower.towerId].Value = xp;
                            }
                            
                            // Unlock all upgrades for this tower
                            foreach (var upgrade in Game.instance.model.GetTower(tower.towerId, pathOneTier: 5).appliedUpgrades
                                .Concat(Game.instance.model.GetTower(tower.towerId, pathTwoTier: 5).appliedUpgrades)
                                .Concat(Game.instance.model.GetTower(tower.towerId, pathThreeTier: 5).appliedUpgrades))
                            {
                                if (!player.HasUpgrade(upgrade))
                                {
                                    player.Data.acquiredUpgrades.Add(upgrade);
                                }
                            }
                            
                            var paragon = Game.instance.model.GetParagonUpgradeForTowerId(tower.towerId);
                            if (paragon != null && !player.HasUpgrade(paragon.name))
                            {
                                player.Data.acquiredUpgrades.Add(paragon.name);
                            }
                        }
                    }
                    
                    // Insta Monkeys
                    if (PrefixSettings.InstaMonkeys)
                    {
                        foreach (var tower in Game.instance.GetTowerDetailModels())
                        {
                            // Add 000 instas
                            player.GetInstaTower(tower.towerId, new[] {0, 0, 0}).Quantity += PrefixSettings.InstaMonkeysAmount;
                        }
                    }
                    
                    // Player Level
                    if (PrefixSettings.PlayerLevel)
                    {
                        player.Data.seenVeteranRankInfo = true;
                        var rankInfo = GameData.Instance.rankInfo;
                        var rank = Math.Min(PrefixSettings.PlayerLevelValue, rankInfo.GetMaxRank());
                        var veteranRank = Math.Max(PrefixSettings.PlayerLevelValue - rankInfo.GetMaxRank(), 0);
                        
                        player.Data.rank.Value = rank;
                        player.Data.veteranRank.Value = rank == rankInfo.GetMaxRank() ? veteranRank + 1 : 0;
                        player.Data.xp.Value = rankInfo.GetRankInfo(rank-1).totalXpNeeded;
                        player.Data.veteranXp.Value = (long) veteranRank * rankInfo.xpNeededPerVeteranRank;
                    }
                    
                    // Premium Features
                    if (PrefixSettings.DoubleCash)
                    {
                        player.Data.purchase.AddOneTimePurchaseItem("btd6_doublecashmode");
                    }
                    
                    if (PrefixSettings.FastTrack)
                    {
                        player.Data.unlockedFastTrack = true;
                    }
                    
                    if (PrefixSettings.RogueLegends)
                    {
                        player.Data.purchase.AddOneTimePurchaseItem("btd6_legendsrogue");
                    }
                    
                    if (PrefixSettings.MapEditor)
                    {
                        player.Data.purchase.AddOneTimePurchaseItem("btd6_mapeditorsupporter_new");
                    }
                    
                    // Medals based on level
                    if (PrefixSettings.ApplyMedals && PrefixSettings.PlayerLevel)
                    {
                        var medalCount = (int)(PrefixSettings.PlayerLevelValue * PrefixSettings.MedalsPercentage / 100.0);
                        
                        // Distribute medals across different types
                        var bossMedals = medalCount / 6;
                        var raceMedals = medalCount / 8;
                        var ctMedals = medalCount / 10;
                        
                        // Boss medals
                        foreach (var boss in Enum.GetValues<BossType>())
                        {
                            if (!player.Data.bossMedals.ContainsKey((int)boss))
                            {
                                player.Data.bossMedals[(int)boss] = new BossMedalSaveData();
                            }
                            player.Data.bossMedals[(int)boss].normalBadges.Value = Math.Max(1, bossMedals);
                            player.Data.bossMedals[(int)boss].eliteBadges.Value = Math.Max(1, bossMedals / 2);
                        }
                        
                        // Race medals
                        if (!player.Data.raceMedalData.ContainsKey(4))
                            player.Data.raceMedalData[4] = new KonFuze_NoShuffle(raceMedals); // DoubleGold
                        else
                            player.Data.raceMedalData[4].Value = raceMedals;
                            
                        if (!player.Data.raceMedalData.ContainsKey(5))
                            player.Data.raceMedalData[5] = new KonFuze_NoShuffle(raceMedals / 2); // GoldSilver
                        else
                            player.Data.raceMedalData[5].Value = raceMedals / 2;
                            
                        if (!player.Data.raceMedalData.ContainsKey(8))
                            player.Data.raceMedalData[8] = new KonFuze_NoShuffle(raceMedals / 4); // Bronze
                        else
                            player.Data.raceMedalData[8].Value = raceMedals / 4;
                    }
                    
                    Game.Player.SaveNow();
                    
                    PopupScreen.instance.ShowOkPopup("Success!");
                }),
                "Yes", new Action(() => {}), "Cancel", Popup.TransitionAnim.Scale);
        }
    }
}
