using System;
using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Data.MapSets;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Player;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppAssets.Scripts.Utils;
using Il2CppNinjaKiwi.Common;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = System.Random;

namespace EditPlayerData.UI
{
    public static class MassEditFeatures
    {
        private static readonly Random Rng = new Random();

        // Add "Set All Tower XP" button to Tower XP section
        public static void AddMassSetTowerXpButton(ModHelperPanel panel)
        {
            panel.AddButton(new Info("SetAllTowerXP", 800, 150), VanillaSprites.GreenBtnLong, () =>
            {
                ShowSetAllTowerXpPopup();
            }).AddText(new Info("Text", 800, 150), "Set XP for All Towers", 60);
        }

        // Add "Set All Powers" button to Powers section
        public static void AddMassSetPowersButton(ModHelperPanel panel)
        {
            panel.AddButton(new Info("SetAllPowers", 800, 150), VanillaSprites.GreenBtnLong, () =>
            {
                ShowSetAllPowersPopup();
            }).AddText(new Info("Text", 800, 150), "Set Quantity for All Powers", 60);
        }

        // Add "Apply Medal Template" button to Maps section
        public static void AddApplyMedalTemplateButton(ModHelperPanel panel, bool coop)
        {
            panel.AddButton(new Info("ApplyMedalTemplate", 800, 150), VanillaSprites.GreenBtnLong, () =>
            {
                ShowApplyMedalTemplatePopup(coop);
            }).AddText(new Info("Text", 800, 150), "Apply Medal Template to Maps", 60);
        }

        private static void ShowSetAllTowerXpPopup()
        {
            var useRange = false;
            var minValue = 500000;
            var maxValue = 1000000;
            var fixedValue = 500000;

            var popup = PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
                "Set XP for All Towers", 
                "Set XP for all towers at once.\nYou can set a fixed value or a random range.",
                new Action(() =>
                {
                    ApplyTowerXpToAll(useRange, fixedValue, minValue, maxValue);
                    PopupScreen.instance.ShowOkPopup("XP has been applied to all towers!");
                }), 
                "Apply", 
                new Action(() => {}), 
                "Cancel",
                Popup.TransitionAnim.Scale, 
                PopupScreen.BackGround.Grey);

            var layout = popup.FindObject("Layout");
            
            // Option toggles
            var togglePanel = layout.AddModHelperPanel(new Info("TogglePanel", 800, 100), 
                null, RectTransform.Axis.Horizontal, 20);
            togglePanel.AddText(new Info("Label", 400), "Use Random Range:", 60);
            
            var toggle = togglePanel.AddModHelperComponent(
                ModHelperCheckbox.Create(new Info("UseRangeToggle", 80, 80),
                useRange, VanillaSprites.SmallSquareDarkInner,
                new Action<bool>(value => 
                { 
                    useRange = value;
                    rangePanel.SetActive(useRange);
                    fixedPanel.SetActive(!useRange);
                })));

            // Fixed value input
            var fixedPanel = layout.AddModHelperPanel(new Info("FixedPanel", 800, 150));
            fixedPanel.AddText(new Info("FixedLabel", 800, 50), "Fixed XP Value:", 60);
            var fixedInput = fixedPanel.AddInputField(new Info("FixedInput", 500, 100), 
                fixedValue.ToString(), VanillaSprites.BlueInsertPanelRound, 
                new Action<string>(value => 
                {
                    if (int.TryParse(value, out int result))
                    {
                        fixedValue = result;
                    }
                }), 60, TMP_InputField.CharacterValidation.Digit);

            // Range inputs
            var rangePanel = layout.AddModHelperPanel(new Info("RangePanel", 800, 250));
            rangePanel.AddText(new Info("MinLabel", 800, 50), "Minimum XP Value:", 60);
            var minInput = rangePanel.AddInputField(new Info("MinInput", 500, 100), 
                minValue.ToString(), VanillaSprites.BlueInsertPanelRound, 
                new Action<string>(value => 
                {
                    if (int.TryParse(value, out int result))
                    {
                        minValue = result;
                    }
                }), 60, TMP_InputField.CharacterValidation.Digit);
                
            rangePanel.AddText(new Info("MaxLabel", 800, 50), "Maximum XP Value:", 60);
            var maxInput = rangePanel.AddInputField(new Info("MaxInput", 500, 100), 
                maxValue.ToString(), VanillaSprites.BlueInsertPanelRound, 
                new Action<string>(value => 
                {
                    if (int.TryParse(value, out int result))
                    {
                        maxValue = result;
                    }
                }), 60, TMP_InputField.CharacterValidation.Digit);

            // Initially hide range panel
            rangePanel.SetActive(false);
            
            // Add spacing
            layout.AddModHelperPanel(new Info("Spacing", 800, 50));
        }

        private static void ApplyTowerXpToAll(bool useRange, int fixedValue, int minValue, int maxValue)
        {
            var player = Game.Player;
            var towers = Game.instance.GetTowerDetailModels();
            
            foreach (var tower in towers)
            {
                int xpValue;
                
                if (useRange)
                {
                    // Ensure min is actually less than max
                    int min = Math.Min(minValue, maxValue);
                    int max = Math.Max(minValue, maxValue);
                    
                    // Generate random value in range
                    xpValue = Rng.Next(min, max + 1);
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
        }

        private static void ShowSetAllPowersPopup()
        {
            var quantity = 100;
            
            var popup = PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
                "Set All Powers", 
                "Set the same quantity for all powers at once.",
                new Action(() =>
                {
                    ApplyQuantityToAllPowers(quantity);
                    PopupScreen.instance.ShowOkPopup("Quantity has been applied to all powers!");
                }), 
                "Apply", 
                new Action(() => {}), 
                "Cancel",
                Popup.TransitionAnim.Scale, 
                PopupScreen.BackGround.Grey);

            var layout = popup.FindObject("Layout");
            
            // Quantity input
            layout.AddText(new Info("QuantityLabel", 800, 50), "Quantity for all powers:", 60);
            var quantityInput = layout.AddInputField(new Info("QuantityInput", 500, 100), 
                quantity.ToString(), VanillaSprites.BlueInsertPanelRound, 
                new Action<string>(value => 
                {
                    if (int.TryParse(value, out int result))
                    {
                        quantity = result;
                    }
                }), 60, TMP_InputField.CharacterValidation.Digit);
                
            // Add spacing
            layout.AddModHelperPanel(new Info("Spacing", 800, 50));
        }

        private static void ApplyQuantityToAllPowers(int quantity)
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
        }

        private static void ShowApplyMedalTemplatePopup(bool coop)
        {
            var modeSettings = new Dictionary<string, bool>();
            var modeWins = new Dictionary<string, int>();
            var mapCount = 5; // Default number of maps to apply to
            
            // Initialize dictionaries with all modes
            foreach (var difficultyEntry in MapPlayerDataSetting.Difficulties)
            {
                string difficulty = difficultyEntry.Key;
                foreach (var mode in difficultyEntry.Value)
                {
                    string key = $"{difficulty}/{mode}";
                    modeSettings[key] = false;
                    modeWins[key] = 1;
                }
            }
            
            var popup = PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, 
                "Apply Medal Template", 
                "Select medals to apply to random maps.",
                new Action(() =>
                {
                    ApplyMedalTemplateToMaps(modeSettings, modeWins, mapCount, coop);
                    PopupScreen.instance.ShowOkPopup($"Medal template has been applied to {mapCount} maps!");
                }), 
                "Apply", 
                new Action(() => {}), 
                "Cancel",
                Popup.TransitionAnim.Scale, 
                PopupScreen.BackGround.Grey);

            var layout = popup.FindObject("Layout");
            
            // Create scrollable area for all the mode checkboxes
            var scrollPanel = layout.AddModHelperScrollPanel(new Info("ScrollPanel", 800, 400), 
                RectTransform.Axis.Vertical, VanillaSprites.BlueInsertPanelRound, 20);
            
            // Add checkboxes for each difficulty and mode
            foreach (var difficultyEntry in MapPlayerDataSetting.Difficulties)
            {
                string difficulty = difficultyEntry.Key;
                
                var diffHeader = scrollPanel.AddPanel(new Info($"Header_{difficulty}", 800, 60));
                diffHeader.AddText(new Info("Label", 800, 60), $"{difficulty} Medals:", 65).Text.fontStyle = FontStyles.Bold;
                
                foreach (var mode in difficultyEntry.Value)
                {
                    string key = $"{difficulty}/{mode}";
                    
                    var modePanel = scrollPanel.AddPanel(new Info($"Mode_{key}", 800, 100), 
                        null, RectTransform.Axis.Horizontal);
                    
                    // Checkbox to enable/disable this mode
                    modePanel.AddModHelperComponent(
                        ModHelperCheckbox.Create(new Info("Checkbox", 80, 80),
                        modeSettings[key], VanillaSprites.SmallSquareDarkInner,
                        new Action<bool>(value => modeSettings[key] = value)));
                    
                    // Mode name
                    modePanel.AddText(new Info("Name", 400, 100), 
                        mode == "Standard" ? difficulty : $"{difficulty} - {mode}", 50);
                    
                    // Wins input field
                    var winsInput = modePanel.AddInputField(new Info("Wins", 200, 80), 
                        "1", VanillaSprites.BlueInsertPanelRound, 
                        new Action<string>(value => 
                        {
                            if (int.TryParse(value, out int result) && result > 0)
                            {
                                modeWins[key] = result;
                            }
                        }), 50, TMP_InputField.CharacterValidation.Digit);
                    
                    // Label for wins
                    modePanel.AddText(new Info("WinsLabel", 100, 100), "wins", 50);
                }
                
                // Add spacing between difficulties
                scrollPanel.AddPanel(new Info($"Spacing_{difficulty}", 800, 20));
            }
            
            // Map count selector
            var mapCountPanel = layout.AddModHelperPanel(new Info("MapCountPanel", 800, 100), 
                null, RectTransform.Axis.Horizontal, 20);
            mapCountPanel.AddText(new Info("Label", 500), "Number of maps to apply to:", 60);
            var mapCountInput = mapCountPanel.AddInputField(new Info("MapCountInput", 200, 80), 
                mapCount.ToString(), VanillaSprites.BlueInsertPanelRound, 
                new Action<string>(value => 
                {
                    if (int.TryParse(value, out int result) && result > 0)
                    {
                        mapCount = result;
                    }
                }), 50, TMP_InputField.CharacterValidation.Digit);
                
            // Add spacing
            layout.AddModHelperPanel(new Info("Spacing", 800, 50));
        }

        private static void ApplyMedalTemplateToMaps(Dictionary<string, bool> modeSettings, 
            Dictionary<string, int> modeWins, int mapCount, bool coop)
        {
            var player = Game.Player;
            var availableMaps = GameData.Instance.mapSet.StandardMaps.ToList();
            
            // Shuffle the maps for random selection
            for (int i = availableMaps.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(0, i + 1);
                var temp = availableMaps[i];
                availableMaps[i] = availableMaps[j];
                availableMaps[j] = temp;
            }
            
            // Take only the requested number of maps
            var selectedMaps = availableMaps.Take(Math.Min(mapCount, availableMaps.Count)).ToList();
            
            foreach (var map in selectedMaps)
            {
                var mapInfo = player.Data.mapInfo.GetMap(map.id);
                
                // Apply each selected mode
                foreach (var entry in modeSettings)
                {
                    if (!entry.Value) continue; // Skip if not selected
                    
                    string[] parts = entry.Key.Split('/');
                    string difficulty = parts[0];
                    string mode = MapPlayerDataSetting.Difficulties[difficulty][int.Parse(parts[1])];
                    
                    var modeInfo = mapInfo.GetOrCreateDifficulty(difficulty)
                        .GetOrCreateMode(mode, coop);
                    
                    modeInfo.timesCompleted = modeWins[entry.Key];
                    modeInfo.completedWithoutLoadingSave = true;
                }
            }
            
            // Save changes
            player.SaveNow();
        }
    }
}
