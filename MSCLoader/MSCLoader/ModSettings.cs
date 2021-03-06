﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
//using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace MSCLoader
{
#pragma warning disable CS1591
    public class ModSettingsUI : MonoBehaviour
    {
        public ModSettings ms = null;
        public void LoadUI()
        {
            StartCoroutine(LoadUIc());
        }
        IEnumerator LoadUIc()
        {
            AssetBundle ab = new AssetBundle();
            yield return StartCoroutine(ModLoader.loadAssets.LoadBundleAsync(new ModSettings(), "settingsui.unity3d", value => ab = value));
           
            ms.UI = ab.LoadAsset("MSCLoader Settings.prefab") as GameObject;

            ms.ModButton = ab.LoadAsset("ModButton.prefab") as GameObject;
            ms.ModButton_Pre = ab.LoadAsset("ModButton_Pre.prefab") as GameObject;
            ms.ModButton_Invalid = ab.LoadAsset("ModButton_Invalid.prefab") as GameObject;
            ms.ModViewLabel = ab.LoadAsset("ModViewLabel.prefab") as GameObject;

            ms.KeyBind = ab.LoadAsset("KeyBind.prefab") as GameObject;

            ms.HasAssets = ab.LoadAsset("HasAssets.prefab") as GameObject;
            ms.PluginOk = ab.LoadAsset("PluginOK.prefab") as GameObject;
            ms.PluginDisabled = ab.LoadAsset("PluginDisabled.prefab") as GameObject;
            ms.InMenu = ab.LoadAsset("InMenu.prefab") as GameObject;
            ms.update = ab.LoadAsset("Update.prefab") as GameObject;

            //exit coroutine
            ms.CreateSettingsUI();

            //freeup memory
            ab.Unload(false);
            Destroy(gameObject);
        }
    }
    public class ModSettings : Mod
    {

        public override bool LoadInMenu => true;
        public override bool UseAssetsFolder => true;
        public override string ID => "MSCLoader_Settings";
        public override string Name => "Settings";
        public override string Version => ModLoader.Version;
        public override string Author => "piotrulos";

        private static Mod selectedMod = null;

        public GameObject UI;
        public GameObject ModButton;
        public GameObject ModButton_Pre;
        public GameObject ModButton_Invalid;
        public GameObject ModViewLabel;
        public GameObject KeyBind;

        //icons for SettinsView
        public GameObject HasAssets;
        public GameObject PluginOk;
        public GameObject PluginDisabled;
        public GameObject InMenu;
        public GameObject update;

        private Keybind menuKey = new Keybind("Open", "Open menu", KeyCode.M, KeyCode.LeftControl);
        public SettingsView settings;

        public void CreateSettingsUI()
        {

            //ModConsole.Print(UI.name);
            UI = GameObject.Instantiate(UI);
            UI.AddComponent<ModUIDrag>();
            settings = UI.AddComponent<SettingsView>();
            UI.GetComponent<SettingsView>().settingView = UI;
            UI.GetComponent<SettingsView>().settingViewContainer = UI.transform.GetChild(0).gameObject;
            UI.GetComponent<SettingsView>().modList = UI.GetComponent<SettingsView>().settingViewContainer.transform.GetChild(2).gameObject;
            UI.GetComponent<SettingsView>().modView = UI.GetComponent<SettingsView>().modList.transform.GetChild(0).gameObject;
            UI.GetComponent<SettingsView>().modSettings = UI.GetComponent<SettingsView>().settingViewContainer.transform.GetChild(1).gameObject;
            UI.GetComponent<SettingsView>().ModSettingsView = UI.GetComponent<SettingsView>().modSettings.transform.GetChild(0).gameObject;
            UI.GetComponent<SettingsView>().goBackBtn = UI.GetComponent<SettingsView>().settingViewContainer.transform.GetChild(0).GetChild(0).gameObject;
            UI.GetComponent<SettingsView>().goBackBtn.GetComponent<Button>().onClick.AddListener(() => UI.GetComponent<SettingsView>().goBack());
            UI.GetComponent<SettingsView>().keybindsList = UI.GetComponent<SettingsView>().ModSettingsView.transform.GetChild(8).gameObject;
            UI.GetComponent<SettingsView>().DisableMod = UI.GetComponent<SettingsView>().ModSettingsView.transform.GetChild(4).GetComponent<Toggle>();
            UI.GetComponent<SettingsView>().DisableMod.onValueChanged.AddListener(UI.GetComponent<SettingsView>().disableMod);

            UI.GetComponent<SettingsView>().ModButton = ModButton;
            UI.GetComponent<SettingsView>().ModButton_Pre = ModButton_Pre;
            UI.GetComponent<SettingsView>().ModButton_Invalid = ModButton_Invalid;
            UI.GetComponent<SettingsView>().ModViewLabel = ModViewLabel;
            UI.GetComponent<SettingsView>().KeyBind = KeyBind;

            UI.GetComponent<SettingsView>().HasAssets = HasAssets;
            UI.GetComponent<SettingsView>().PluginOk = PluginOk;
            UI.GetComponent<SettingsView>().PluginDisabled = PluginDisabled;
            UI.GetComponent<SettingsView>().InMenu = InMenu;
            UI.GetComponent<SettingsView>().update = update;

            UI.GetComponent<SettingsView>().IDtxt = UI.GetComponent<SettingsView>().ModSettingsView.transform.GetChild(0).GetComponent<Text>();
            UI.GetComponent<SettingsView>().Nametxt = UI.GetComponent<SettingsView>().ModSettingsView.transform.GetChild(1).GetComponent<Text>();
            UI.GetComponent<SettingsView>().Versiontxt = UI.GetComponent<SettingsView>().ModSettingsView.transform.GetChild(2).GetComponent<Text>();
            UI.GetComponent<SettingsView>().Authortxt = UI.GetComponent<SettingsView>().ModSettingsView.transform.GetChild(3).GetComponent<Text>();

            UI.transform.SetParent(GameObject.Find("MSCLoader Canvas").transform, false);
            settings.setVisibility(false);
        }

        // Reset keybinds
        private void resetBinds()
        {
            if (selectedMod != null)
            {
                // Delete file
                string path = Path.Combine(ModLoader.GetModConfigFolder(selectedMod), "keybinds.xml");
                File.WriteAllText(path, "");

                // Revert binds
                foreach (Keybind bind in Keybind.Get(selectedMod))
                {
                    Keybind original = Keybind.DefaultKeybinds.Find(x => x.Mod == selectedMod && x.ID == bind.ID);

                    if (original != null)
                    {
                        bind.Key = original.Key;
                        bind.Modifier = original.Modifier;

                        ModConsole.Print(original.Key.ToString() + " -> " + bind.Key.ToString());
                    }
                }

                // Save binds
                SaveModBinds(selectedMod);
            }
        }


        // Save all keybinds to config files.
        public static void SaveAllBinds()
        {
            foreach (Mod mod in ModLoader.LoadedMods)
            {
                SaveModBinds(mod);
            }
        }


        // Save keybind for a single mod to config file.
        public static void SaveModBinds(Mod mod)
        {

            KeybindList list = new KeybindList();
            string path = Path.Combine(ModLoader.GetModConfigFolder(mod), "keybinds.json");
            
            foreach (Keybind bind in Keybind.Get(mod))
            {
                Keybinds keybinds = new Keybinds
                {
                    ID = bind.ID,
                    Key = bind.Key,
                    Modifier = bind.Modifier
                };

                list.keybinds.Add(keybinds);
            }

            string serializedData = JsonConvert.SerializeObject(list);
            File.WriteAllText(path, serializedData);

        }


        // Load all keybinds.
        public static void LoadBinds()
        {
            foreach (Mod mod in ModLoader.LoadedMods)
            {
                //delete old xml file (if exists)
                string path = Path.Combine(ModLoader.GetModConfigFolder(mod), "keybinds.xml");
                if (File.Exists(path))
                    File.Delete(path);

                // Check if there is custom keybinds file (if not, create)
                path = Path.Combine(ModLoader.GetModConfigFolder(mod), "keybinds.json");
                if (!File.Exists(path))
                {
                    SaveModBinds(mod);
                    continue;
                }

                //Load and deserialize 
                KeybindList keybinds = new KeybindList();
                string serializedData = File.ReadAllText(path);
                keybinds = JsonConvert.DeserializeObject<KeybindList>(serializedData);
                if (keybinds.keybinds.Count == 0)
                    continue;
                foreach(var kb in keybinds.keybinds)
                {
                    Keybind bind = Keybind.Keybinds.Find(x => x.Mod == mod && x.ID == kb.ID);
                    if (bind == null)
                        continue;
                    bind.Key = kb.Key;
                    bind.Modifier = kb.Modifier;
                }
            }
        }

        public override void OnMenuLoad()
        {
            try
            {
                GameObject go = new GameObject();
                ModSettingsUI ui = go.AddComponent<ModSettingsUI>();
                ui.ms = this;
                ui.LoadUI();
            }
            catch (Exception e)
            {
                ModConsole.Print(e.Message); //debug only
            }
            Keybind.Add(this, menuKey);
            // Load the keybinds.
            LoadBinds();
        }


        // Open menu if the key is pressed.
        public override void Update()
        {
            // Open menu
            if (menuKey.IsDown())
            {
                settings.toggleVisibility();
            }
        }
    }
#pragma warning restore CS1591
}
