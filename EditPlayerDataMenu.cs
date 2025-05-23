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
using Il2CppAssets.Scripts.Data.MapSets;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Store.Loot;
using Il2CppAssets.Scripts.Models.TowerSets;
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
using EditPlayerData.Utils;
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
    }

    private int LastPage => (Settings[_category].Count(s => s.Name.ContainsIgnoreCase(_searchValue))-1) / EntriesPerPage;

    private readonly PlayerDataSettingDisplay[] _entries = new PlayerDataSettingDisplay[EntriesPerPage];

    private static TMP_InputField? _searchInput;
    private string _searchValue = "";
    private string _category = "General";
    private int _pageIdx;

    private ModHelperPanel _topArea;
    private ModHelperPanel _bulkActionsPanel;

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
                UpdateBulkActions();
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

        // Add bulk actions panel
        _bulkActionsPanel = GameMenu.GetComponentFromChildrenByName<RectTransform>("Container").gameObject
            .AddModHelperPanel(new Info("BulkActionsPanel")
            {
                Y = -550, Height = 200, Pivot = new Vector2(0.5f, 1),
                AnchorMin = new Vector2(0, 1), AnchorMax = new Vector2(1, 1)
            }, layoutAxis: RectTransform.Axis.Horizontal, padding: 50, spacing: 50);
        
        UpdateBulkActions();
        
        GenerateEntries();
        SetPage(0);

        // for no discernible reason, this defaults to 300
        GameMenu.scrollRect.scrollSensitivity = 50;
        
        return false;
    }

    private void UpdateBulkActions()
    {
        _bulkActionsPanel.transform.DestroyAllChildren();
        
        if (_category == "Powers")
        {
            _bulkActionsPanel.AddButton(new Info("SetAllPowers", 450, 150), VanillaSprites.GreenBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowSetValuePopup("Set All Powers", "Set all powers to this value:",
                    new Action<int>(value =>
                    {
                        foreach (var setting in Settings["Powers"])
                        {
                            if (setting is NumberPlayerDataSetting numSetting)
                            {
                                numSetting.ResetToDefault();
                                var setter = numSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Action<int>;
                                setter?.Invoke(value);
                            }
                        }
                        UpdateVisibleEntries();
                    }), 400);
            })).AddText(new Info("SetAllPowersText"), "Set All Powers", 50);
            
            _bulkActionsPanel.AddButton(new Info("AddToPowers", 450, 150), VanillaSprites.BlueBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowSetValuePopup("Add to Powers", "Add this amount to all powers:",
                    new Action<int>(value =>
                    {
                        foreach (var setting in Settings["Powers"])
                        {
                            if (setting is NumberPlayerDataSetting numSetting)
                            {
                                var getter = numSetting.GetType().GetField("Getter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Func<int>;
                                var setter = numSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Action<int>;
                                if (getter != null && setter != null)
                                {
                                    setter.Invoke(getter.Invoke() + value);
                                }
                            }
                        }
                        UpdateVisibleEntries();
                    }), 10);
            })).AddText(new Info("AddToPowersText"), "Add to All", 50);
        }
        else if (_category == "Maps" || _category == "Maps - Coop")
        {
            _bulkActionsPanel.AddButton(new Info("CompleteAllMaps", 450, 150), VanillaSprites.GreenBtnLong, new Action(() =>
            {
                PopupScreen screen = null!;
                var popup = PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, "Complete All Maps", 
                    "Configure map completion settings",
                    new Action(() =>
                    {
                        var difficulty = screen.GetComponentInChildren<ModHelperDropdown>().Dropdown.value;
                        var winCount = int.Parse(screen.GetComponentsInChildren<ModHelperInputField>()[0].CurrentValue);
                        var noExit = screen.GetComponentInChildren<ModHelperCheckbox>().CurrentValue;
                        
                        foreach (var setting in Settings[_category])
                        {
                            if (setting is MapPlayerDataSetting mapSetting)
                            {
                                mapSetting.Unlock();
                                
                                var map = Game.Player.Data.mapInfo.GetMap((mapSetting.GetType()
                                    .GetField("_details", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                    ?.GetValue(mapSetting) as MapDetails)?.id ?? "");
                                if (map == null) continue;
                                
                                var difficulties = difficulty == 0 ? new[] { "Easy", "Medium", "Hard" } : 
                                    difficulty == 1 ? new[] { "Easy" } :
                                    difficulty == 2 ? new[] { "Medium" } : new[] { "Hard" };
                                
                                foreach (var diff in difficulties)
                                {
                                    var modes = MapPlayerDataSetting.Difficulties[diff];
                                    foreach (var mode in modes)
                                    {
                                        var mapMode = map.GetOrCreateDifficulty(diff).GetOrCreateMode(mode, _category == "Maps - Coop");
                                        mapMode.timesCompleted = winCount;
                                        mapMode.completedWithoutLoadingSave = noExit;
                                    }
                                }
                            }
                        }
                        UpdateVisibleEntries();
                    }), "Ok", null, "Cancel",
                    Popup.TransitionAnim.Scale, PopupScreen.BackGround.Grey);
                
                screen = popup.WaitForCompletion();
                var popupBody = screen.FindObject("Body");
                
                var panel = popupBody.AddModHelperPanel(new Info("Panel", 1000, 400), layoutAxis: RectTransform.Axis.Vertical, spacing: 25);
                panel.AddDropdown(new Info("Difficulty", 800, 125),
                    new[] { "All", "Easy", "Medium", "Hard" }.ToIl2CppList(), 400,
                    null, VanillaSprites.BlueInsertPanelRound, 50);
                panel.AddInputField(new Info("WinCount", 800, 125), "1",
                    VanillaSprites.BlueInsertPanelRound, null, 50, TMP_InputField.CharacterValidation.Digit,
                    TextAlignmentOptions.Center, "Win Count");
                panel.AddCheckbox(new Info("NoExit", 800, 125), true,
                    VanillaSprites.BlueInsertPanelRound, null).AddText(new Info("NoExitText"), "Complete Without Exit", 50);
            })).AddText(new Info("CompleteAllMapsText"), "Complete All Maps", 45);
            
            _bulkActionsPanel.AddButton(new Info("ResetAllMaps", 450, 150), VanillaSprites.RedBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, "Reset All Maps?", 
                    "This will reset all map progress. Are you sure?",
                    new Action(() =>
                    {
                        foreach (var setting in Settings[_category])
                        {
                            setting.ResetToDefault();
                        }
                        UpdateVisibleEntries();
                    }), "Yes", null, "No",
                    Popup.TransitionAnim.Scale, PopupScreen.BackGround.Grey);
            })).AddText(new Info("ResetAllMapsText"), "Reset All Maps", 45);
        }
        else if (_category == "Tower XP")
        {
            _bulkActionsPanel.AddButton(new Info("MaxAllTowers", 450, 150), VanillaSprites.GreenBtnLong, new Action(() =>
            {
                foreach (var setting in Settings["Tower XP"])
                {
                    setting.Unlock();
                    if (setting is TowerPlayerDataSetting towerSetting)
                    {
                        var setter = towerSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(towerSetting) as Action<int>;
                        setter?.Invoke(999999999);
                    }
                }
                UpdateVisibleEntries();
            })).AddText(new Info("MaxAllTowersText"), "Max All Towers", 50);
            
            _bulkActionsPanel.AddButton(new Info("UnlockAllUpgrades", 450, 150), VanillaSprites.BlueBtnLong, new Action(() =>
            {
                foreach (var tower in Game.instance.GetTowerDetailModels())
                {
                    if (!GetPlayer().Data.unlockedTowers.Contains(tower.towerId))
                    {
                        GetPlayer().Data.UnlockTower(tower.towerId);
                    }
                    
                    var model = Game.instance.model;
                    foreach (var upgrade in model.GetTower(tower.towerId, pathOneTier: 5).appliedUpgrades
                        .Concat(model.GetTower(tower.towerId, pathTwoTier: 5).appliedUpgrades)
                        .Concat(model.GetTower(tower.towerId, pathThreeTier: 5).appliedUpgrades))
                    {
                        if (!GetPlayer().HasUpgrade(upgrade))
                        {
                            GetPlayer().Data.acquiredUpgrades.Add(upgrade);
                        }
                    }
                    
                    var paragon = Game.instance.model.GetParagonUpgradeForTowerId(tower.towerId);
                    if (paragon != null && !GetPlayer().HasUpgrade(paragon.name))
                    {
                        GetPlayer().Data.acquiredUpgrades.Add(paragon.name);
                    }
                }
                UpdateVisibleEntries();
            })).AddText(new Info("UnlockAllUpgradesText"), "Unlock All Upgrades", 45);
        }
        else if (_category == "Instas")
        {
            _bulkActionsPanel.AddButton(new Info("AddAllInstas", 450, 150), VanillaSprites.GreenBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowSetValuePopup("Add Instas", "Add this many of each tier to all towers:",
                    new Action<int>(value =>
                    {
                        foreach (var setting in Settings["Instas"])
                        {
                            if (setting is InstaMonkeyPlayerDataSetting instaSetting)
                            {
                                var tower = instaSetting.GetType().GetField("_tower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(instaSetting) as TowerDetailsModel;
                                if (tower == null) continue;
                                
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
                                    GetPlayer().GetInstaTower(tower.towerId, tiers).Quantity += value;
                                }
                            }
                        }
                        UpdateVisibleEntries();
                    }), 100);
            })).AddText(new Info("AddAllInstasText"), "Add All Instas", 50);
            
            _bulkActionsPanel.AddButton(new Info("FillCollections", 450, 150), VanillaSprites.BlueBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowOkPopup("Fill Collections", "This will add any missing insta monkeys to complete all collections. Continue?",
                    new Action(() =>
                    {
                        foreach (var setting in Settings["Instas"])
                        {
                            if (setting is InstaMonkeyPlayerDataSetting instaSetting)
                            {
                                var tower = instaSetting.GetType().GetField("_tower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(instaSetting) as TowerDetailsModel;
                                if (tower == null) continue;
                                
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
                                    var insta = GetPlayer().GetInstaTower(tower.towerId, tiers);
                                    if (insta.Quantity == 0)
                                    {
                                        insta.Quantity = 1;
                                    }
                                }
                            }
                        }
                        UpdateVisibleEntries();
                    }));
            })).AddText(new Info("FillCollectionsText"), "Fill Collections", 50);
        }
        else if (_category == "Online Modes")
        {
            _bulkActionsPanel.AddButton(new Info("SetAllBosses", 450, 150), VanillaSprites.GreenBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowSetValuePopup("Set All Boss Medals", "Set all boss medals to:",
                    new Action<int>(value =>
                    {
                        foreach (var setting in Settings["Online Modes"])
                        {
                            if (setting.Name.Contains("Boss") && !setting.Name.Contains("Elite") && !setting.Name.Contains("st"))
                            {
                                if (setting is NumberPlayerDataSetting numSetting)
                                {
                                    var setter = numSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Action<int>;
                                    setter?.Invoke(value);
                                }
                            }
                        }
                        UpdateVisibleEntries();
                    }), 15);
            })).AddText(new Info("SetAllBossesText"), "Set Boss Medals", 50);
            
            _bulkActionsPanel.AddButton(new Info("SetAllElites", 450, 150), VanillaSprites.BlueBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowSetValuePopup("Set All Elite Boss Medals", "Set all elite boss medals to:",
                    new Action<int>(value =>
                    {
                        foreach (var setting in Settings["Online Modes"])
                        {
                            if (setting.Name.Contains("Elite") && setting.Name.Contains("Boss") && !setting.Name.Contains("st"))
                            {
                                if (setting is NumberPlayerDataSetting numSetting)
                                {
                                    var setter = numSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Action<int>;
                                    setter?.Invoke(value);
                                }
                            }
                        }
                        UpdateVisibleEntries();
                    }), 10);
            })).AddText(new Info("SetAllElitesText"), "Set Elite Medals", 50);
            
            _bulkActionsPanel.AddButton(new Info("SetAllRaces", 450, 150), VanillaSprites.YellowBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowSetValuePopup("Set All Race Medals", "Set all race medals to:",
                    new Action<int>(value =>
                    {
                        foreach (var setting in Settings["Online Modes"])
                        {
                            if (setting.Name.Contains("Race"))
                            {
                                if (setting is NumberPlayerDataSetting numSetting)
                                {
                                    var setter = numSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Action<int>;
                                    setter?.Invoke(value);
                                }
                            }
                        }
                        UpdateVisibleEntries();
                    }), 3);
            })).AddText(new Info("SetAllRacesText"), "Set Race Medals", 50);
        }
        else if (_category == "General")
        {
            _bulkActionsPanel.AddButton(new Info("UnlockEverything", 450, 150), VanillaSprites.GreenBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowOkPopup("Unlock Everything", "This will unlock all content. Continue?",
                    new Action(() =>
                    {
                        foreach (var categorySettings in Settings.Values)
                        {
                            foreach (var setting in categorySettings)
                            {
                                setting.Unlock();
                            }
                        }
                        UpdateVisibleEntries();
                    }));
            })).AddText(new Info("UnlockEverythingText"), "Unlock Everything", 45);
            
            _bulkActionsPanel.AddButton(new Info("MaxAccount", 450, 150), VanillaSprites.BlueBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowOkPopup("Max Account", "This will max out your account progress. Continue?",
                    new Action(() =>
                    {
                        GetPlayer().Data.monkeyMoney.Value = 550000;
                        GetPlayer().Data.knowledgePoints.Value = 999;
                        GetPlayer().Data.trophies.Value = 999999;
                        GetPlayer().Data.rank.Value = 155;
                        GetPlayer().Data.veteranRank.Value = 100;
                        GetPlayer().Data.xp.Value = GameData.Instance.rankInfo.GetRankInfo(154).totalXpNeeded;
                        GetPlayer().Data.veteranXp.Value = 99L * GameData.Instance.rankInfo.xpNeededPerVeteranRank;
                        
                        foreach (var categorySettings in Settings.Values)
                        {
                            foreach (var setting in categorySettings)
                            {
                                setting.Unlock();
                            }
                        }
                        UpdateVisibleEntries();
                    }));
            })).AddText(new Info("MaxAccountText"), "Max Account", 50);
            
            _bulkActionsPanel.AddButton(new Info("Completionist", 450, 150), VanillaSprites.YellowBtnLong, new Action(() =>
            {
                PopupScreen.instance.ShowOkPopup("Completionist Mode", "This will 100% complete the game. Continue?",
                    new Action(() =>
                    {
                        // Max general stats
                        GetPlayer().Data.monkeyMoney.Value = 550000;
                        GetPlayer().Data.knowledgePoints.Value = 999;
                        GetPlayer().Data.trophies.Value = 999999;
                        GetPlayer().Data.rank.Value = 155;
                        GetPlayer().Data.veteranRank.Value = 100;
                        
                        // Complete all maps
                        foreach (var setting in Settings["Maps"].Concat(Settings["Maps - Coop"]))
                        {
                            if (setting is MapPlayerDataSetting mapSetting)
                            {
                                mapSetting.Unlock();
                                var map = Game.Player.Data.mapInfo.GetMap((mapSetting.GetType()
                                    .GetField("_details", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                    ?.GetValue(mapSetting) as MapDetails)?.id ?? "");
                                if (map == null) continue;
                                
                                foreach (var difficulty in new[] { "Easy", "Medium", "Hard" })
                                {
                                    foreach (var mode in MapPlayerDataSetting.Difficulties[difficulty])
                                    {
                                        var mapMode = map.GetOrCreateDifficulty(difficulty).GetOrCreateMode(mode, setting.Name.Contains("Coop"));
                                        mapMode.timesCompleted = 79;
                                        mapMode.completedWithoutLoadingSave = true;
                                    }
                                }
                            }
                        }
                        
                        // Max all towers
                        foreach (var setting in Settings["Tower XP"])
                        {
                            setting.Unlock();
                            if (setting is TowerPlayerDataSetting towerSetting)
                            {
                                var setter = towerSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(towerSetting) as Action<int>;
                                setter?.Invoke(999999999);
                            }
                        }
                        
                        // Add powers
                        foreach (var setting in Settings["Powers"])
                        {
                            if (setting is NumberPlayerDataSetting numSetting)
                            {
                                var setter = numSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Action<int>;
                                setter?.Invoke(400);
                            }
                        }
                        
                        // Add instas
                        foreach (var setting in Settings["Instas"])
                        {
                            if (setting is InstaMonkeyPlayerDataSetting instaSetting)
                            {
                                var tower = instaSetting.GetType().GetField("_tower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(instaSetting) as TowerDetailsModel;
                                if (tower == null) continue;
                                
                                GetPlayer().GetInstaTower(tower.towerId, new[] {0, 0, 0}).Quantity = 100;
                                GetPlayer().GetInstaTower(tower.towerId, new[] {2, 0, 0}).Quantity = 100;
                                GetPlayer().GetInstaTower(tower.towerId, new[] {0, 2, 0}).Quantity = 100;
                                GetPlayer().GetInstaTower(tower.towerId, new[] {0, 0, 2}).Quantity = 100;
                                GetPlayer().GetInstaTower(tower.towerId, new[] {2, 2, 0}).Quantity = 50;
                                GetPlayer().GetInstaTower(tower.towerId, new[] {2, 0, 2}).Quantity = 50;
                                GetPlayer().GetInstaTower(tower.towerId, new[] {0, 2, 2}).Quantity = 50;
                            }
                        }
                        
                        // Add online medals
                        foreach (var setting in Settings["Online Modes"])
                        {
                            if (setting is NumberPlayerDataSetting numSetting)
                            {
                                var setter = numSetting.GetType().GetField("Setter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(numSetting) as Action<int>;
                                if (setting.Name.Contains("Boss") && !setting.Name.Contains("Elite") && !setting.Name.Contains("st"))
                                {
                                    setter?.Invoke(20);
                                }
                                else if (setting.Name.Contains("Elite") && setting.Name.Contains("Boss") && !setting.Name.Contains("st"))
                                {
                                    setter?.Invoke(10);
                                }
                                else if (setting.Name.Contains("Race"))
                                {
                                    setter?.Invoke(5);
                                }
                                else if (setting.Name.Contains("CT"))
                                {
                                    setter?.Invoke(3);
                                }
                            }
                        }
                        
                        // Unlock everything else
                        foreach (var categorySettings in Settings.Values)
                        {
                            foreach (var setting in categorySettings)
                            {
                                setting.Unlock();
                            }
                        }
                        
                        UpdateVisibleEntries();
                    }));
            })).AddText(new Info("CompletionistText"), "Completionist", 50);
        }
        
        // Adjust scroll rect position if bulk actions panel is shown
        if (_bulkActionsPanel.transform.childCount > 0)
        {
            GameMenu.scrollRect.rectTransform.localPosition = new Vector3(0, 200, 0);
        }
        else
        {
            GameMenu.scrollRect.rectTransform.localPosition = new Vector3(0, 100, 0);
        }
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
        _topArea.GetDescendent<ModHelperButton>("UnlockAll").SetActive(anyUnlockable);
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
}
