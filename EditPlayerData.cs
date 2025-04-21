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
    // Add these methods to your existing EditPlayerDataMenu class:

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
    
}
