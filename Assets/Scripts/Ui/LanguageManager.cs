using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    private static LanguageManager _instance;
    public static LanguageManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LanguageManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("LanguageManager");
                    _instance = obj.AddComponent<LanguageManager>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(obj);
                    }
                }
            }
            return _instance;
        }
    }

    public event Action OnLanguageChanged;

    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";
    private string _currentLanguage = "en";
    private Dictionary<string, string> _textToKey;
    private int _refreshFramesRemaining;
    private Font _defaultLegacyFont;

    [Serializable]
    private class TranslationFile
    {
        public string language;
        public TranslationEntry[] entries;
    }

    [Serializable]
    private class TranslationEntry
    {
        public string key;
        public string value;
    }

    public string CurrentLanguage => _currentLanguage;

    private Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        {
            "en", new Dictionary<string, string>
            {
                // CORE UI
                { "shop", "Shop" },
                { "buy", "Buy" },
                { "sell", "Sell" },
                { "upgrade", "Upgrade" },
                { "close", "Close" },
                { "inventory", "Inventory" },
                { "all", "All" },
                { "equip", "Equip" },
                { "use", "Use" },
                { "stats", "Stats" },
                { "level_up", "Level Up" },
                { "skill", "Skill" },
                { "assign", "Assign" },
                { "learn", "Learn" },
                { "yes", "Yes" },
                { "no", "No" },
                { "roll", "ROLL" },
                { "roll_x5", "ROLL X5" },
                { "ok", "OK" },
                { "confirm", "Confirm" },
                { "reroll", "Reroll" },
                { "congratulations", "Congratulations!" },
                { "setting_title", "Setting" },
                { "bgm", "BGM" },
                { "sfx", "SFX" },
                { "language", "Language" },
                { "language_en", "English" },
                { "language_vi", "Tiếng Việt" },
                { "purchased", "Purchased" },
                { "unequip_all", "Unequip All" },
                { "sell_all", "Sell All" },
                { "auto_equip", "Auto Equip" },
                { "melee", "Melee" },
                { "ranged", "Ranged" },
                { "heavy_melee", "Heavy Melee" },

                // ITEMS
                { "armorsuper6", "armorsuper6" },
                { "armorsuper6_des", "armorsuper6_des" },
                { "axe2", "axe2" },
                { "axe2_des", "axe2_des" },
                { "axe1", "axe1" },
                { "axe1_des", "axe1_des" },
                { "axe3", "axe3" },
                { "axe3_des", "axe3_des" },
                { "armorsuper1", "armorsuper1" },
                { "armorsuper1_des", "armorsuper1_des" },
                { "bone armor", "bone armor" },
                { "bone armor_des", "bone armor_des" },
                { "Pboosts ", "Pboosts " },
                { "Pboosts _des", "Pboosts _des" },
                { "boostsWhite", "boostsWhite" },
                { "boostsWhite_des", "boostsWhite_des" },
                { "Bow", "Bow" },
                { "Bow_des", "Bow_des" },
                { "Bowpro", "Bowpro" },
                { "Bowpro_des", "Bowpro_des" },
                { "Bowtitan", "Bowtitan" },
                { "Bowtitan_des", "Bowtitan_des" },
                { "sword4", "sword4" },
                { "sword4_des", "sword4_des" },
                { "helmet5", "helmet5" },
                { "helmet5_des", "helmet5_des" },
                { "helmet7", "helmet7" },
                { "helmet7_des", "helmet7_des" },
                { "clockworkarmor", "clockworkarmor" },
                { "clockworkarmor_des", "clockworkarmor_des" },
                { "cloth armor", "cloth armor" },
                { "cloth armor_des", "cloth armor_des" },
                { "sword3", "sword3" },
                { "sword3_des", "sword3_des" },
                { "armor4", "armor4" },
                { "armor4_des", "armor4_des" },
                { "dagger", "dagger" },
                { "dagger_des", "dagger_des" },
                { "cape2", "cape2" },
                { "cape2_des", "cape2_des" },
                { "dark sword", "dark sword" },
                { "dark sword_des", "dark sword_des" },
                { "dark sword 2", "dark sword 2" },
                { "dark sword 2_des", "dark sword 2_des" },
                { "demonhide boosts", "demonhide boosts" },
                { "demonhide boosts_des", "demonhide boosts_des" },
                { "dragongay", "dragongay" },
                { "dragongay_des", "dragongay_des" },
                { "dragon boosts", "dragon boosts" },
                { "dragon boosts_des", "dragon boosts_des" },
                { "dressshoes", "dressshoes" },
                { "dressshoes_des", "dressshoes_des" },
                { "armor6", "armor6" },
                { "armor6_des", "armor6_des" },
                { "explorerboots", "explorerboots" },
                { "explorerboots_des", "explorerboots_des" },
                { "armor7", "armor7" },
                { "armor7_des", "armor7_des" },
                { "frostarmor", "frostarmor" },
                { "frostarmor_des", "frostarmor_des" },
                { "boosts2", "boosts2" },
                { "boosts2_des", "boosts2_des" },
                { "boosts1", "boosts1" },
                { "boosts1_des", "boosts1_des" },
                { "boosts6", "boosts6" },
                { "boosts6_des", "boosts6_des" },
                { "helmet4", "helmet4" },
                { "helmet4_des", "helmet4_des" },
                { "hair0", "hair0" },
                { "hair0_des", "hair0_des" },
                { "hair01", "hair01" },
                { "hair01_des", "hair01_des" },
                { "hair1", "hair1" },
                { "hair1_des", "hair1_des" },
                { "hair02", "hair02" },
                { "hair02_des", "hair02_des" },
                { "hair2", "hair2" },
                { "hair2_des", "hair2_des" },
                { "hair03", "hair03" },
                { "hair03_des", "hair03_des" },
                { "hair3", "hair3" },
                { "hair3_des", "hair3_des" },
                { "hair04", "hair04" },
                { "hair04_des", "hair04_des" },
                { "hair4", "hair4" },
                { "hair4_des", "hair4_des" },
                { "hair04_2", "hair04_2" },
                { "hair04_2_des", "hair04_2_des" },
                { "hair5", "hair5" },
                { "hair5_des", "hair5_des" },
                { "hair6", "hair6" },
                { "hair6_des", "hair6_des" },
                { "hair7", "hair7" },
                { "hair7_des", "hair7_des" },
                { "hair8", "hair8" },
                { "hair8_des", "hair8_des" },
                { "hair9", "hair9" },
                { "hair9_des", "hair9_des" },
                { "hammer", "hammer" },
                { "hammer_des", "hammer_des" },
                { "helmet8", "helmet8" },
                { "helmet8_des", "helmet8_des" },
                { "hourse4", "hourse4" },
                { "hourse4_des", "hourse4_des" },
                { "helmet", "helmet" },
                { "helmet_des", "helmet_des" },
                { "cape5", "cape5" },
                { "cape5_des", "cape5_des" },
                { "horned helmet", "horned helmet" },
                { "horned helmet_des", "horned helmet_des" },
                { "hourse", "hourse" },
                { "hourse_des", "hourse_des" },
                { "hpc", "hpc" },
                { "hpc_des", "hpc_des" },
                { "lllusionrobe", "lllusionrobe" },
                { "lllusionrobe_des", "lllusionrobe_des" },
                { "armor2", "armor2" },
                { "armor2_des", "armor2_des" },
                { "iron armor", "iron armor" },
                { "iron armor_des", "iron armor_des" },
                { "iron boosts", "iron boosts" },
                { "iron boosts_des", "iron boosts_des" },
                { "armor1", "armor1" },
                { "armor1_des", "armor1_des" },
                { "helmet10", "helmet10" },
                { "helmet10_des", "helmet10_des" },
                { "sword", "sword" },
                { "sword_des", "sword_des" },
                { "sword+1", "sword+1" },
                { "sword+1_des", "sword+1_des" },
                { "armorsupers", "armorsupers" },
                { "armorsupers_des", "armorsupers_des" },
                { "knight's helmet", "knight's helmet" },
                { "knight's helmet_des", "knight's helmet_des" },
                { "cape", "cape" },
                { "cape_des", "cape_des" },
                { "lance", "lance" },
                { "lance_des", "lance_des" },
                { "lance1", "lance1" },
                { "lance1_des", "lance1_des" },
                { "lanceoh", "lanceoh" },
                { "lanceoh_des", "lanceoh_des" },
                { "lance2", "lance2" },
                { "lance2_des", "lance2_des" },
                { "lance3", "lance3" },
                { "lance3_des", "lance3_des" },
                { "armor8", "armor8" },
                { "armor8_des", "armor8_des" },
                { "armorsuper5", "armorsuper5" },
                { "armorsuper5_des", "armorsuper5_des" },
                { "cape3", "cape3" },
                { "cape3_des", "cape3_des" },
                { "boosts8", "boosts8" },
                { "boosts8_des", "boosts8_des" },
                { "armor3", "armor3" },
                { "armor3_des", "armor3_des" },
                { "pig-sticking", "pig-sticking" },
                { "pig-sticking_des", "pig-sticking_des" },
                { "boosts3", "boosts3" },
                { "boosts3_des", "boosts3_des" },
                { "cape1", "cape1" },
                { "cape1_des", "cape1_des" },
                { "boosts7", "boosts7" },
                { "boosts7_des", "boosts7_des" },
                { "armorsuper7", "armorsuper7" },
                { "armorsuper7_des", "armorsuper7_des" },
                { "armorsuper3", "armorsuper3" },
                { "armorsuper3_des", "armorsuper3_des" },
                { "robber's cap", "robber's cap" },
                { "robber's cap_des", "robber's cap_des" },
                { "royal helmet", "royal helmet" },
                { "royal helmet_des", "royal helmet_des" },
                { "sword2", "sword2" },
                { "sword2_des", "sword2_des" },
                { "sword24", "sword24" },
                { "sword24_des", "sword24_des" },
                { "sword21", "sword21" },
                { "sword21_des", "sword21_des" },
                { "sword23", "sword23" },
                { "sword23_des", "sword23_des" },
                { "armor5", "armor5" },
                { "armor5_des", "armor5_des" },
                { "helmet1", "helmet1" },
                { "helmet1_des", "helmet1_des" },
                { "slippers", "slippers" },
                { "slippers_des", "slippers_des" },
                { "helmet2", "helmet2" },
                { "helmet2_des", "helmet2_des" },
                { "armorsuper", "armorsuper" },
                { "armorsuper_des", "armorsuper_des" },
                { "sword a", "sword a" },
                { "sword a_des", "sword a_des" },
                { "swordshort1", "swordshort1" },
                { "swordshort1_des", "swordshort1_des" },
                { "swordshort", "swordshort" },
                { "swordshort_des", "swordshort_des" },
                { "armorsuper4", "armorsuper4" },
                { "armorsuper4_des", "armorsuper4_des" },
                { "sword5", "sword5" },
                { "sword5_des", "sword5_des" },
                { "armorsuper8", "armorsuper8" },
                { "armorsuper8_des", "armorsuper8_des" },
                { "trihorn", "trihorn" },
                { "trihorn_des", "trihorn_des" },
                { "hourse1", "hourse1" },
                { "hourse1_des", "hourse1_des" },
                { "helmet3", "helmet3" },
                { "helmet3_des", "helmet3_des" },
                { "armorsuper2", "armorsuper2" },
                { "armorsuper2_des", "armorsuper2_des" },
                { "hourse2", "hourse2" },
                { "hourse2_des", "hourse2_des" },
                { "boosts5", "boosts5" },
                { "boosts5_des", "boosts5_des" },
                { "helmet6", "helmet6" },
                { "helmet6_des", "helmet6_des" },

                // SKILLS
                { "AttackSpeedBoost", "AttackSpeedBoost" },
                { "AttackSpeedBoost_des", "AttackSpeedBoost_des" },
                { "AttackSpeedBoost_des_template", "AttackSpeedBoost_des_template" },
                { "CritChanceBoost", "CritChanceBoost" },
                { "CritChanceBoost_des", "CritChanceBoost_des" },
                { "CritChanceBoost_des_template", "CritChanceBoost_des_template" },
                { "DamageBoost", "DamageBoost" },
                { "DamageBoost_des", "DamageBoost_des" },
                { "DamageBoost_des_template", "DamageBoost_des_template" },
                { "DefenseBoost", "DefenseBoost" },
                { "DefenseBoost_des", "DefenseBoost_des" },
                { "DefenseBoost_des_template", "DefenseBoost_des_template" },
                { "HealthBoost", "HealthBoost" },
                { "HealthBoost_des", "HealthBoost_des" },
                { "HealthBoost_des_template", "HealthBoost_des_template" },
                { "SpeedBoost", "SpeedBoost" },
                { "SpeedBoost_des", "SpeedBoost_des" },
                { "SpeedBoost_des_template", "SpeedBoost_des_template" },
                { "Dash", "Dash" },
                { "Dash_des", "Dash_des" },
                { "Dash_des_template", "Dash_des_template" },
                { "LifeDrain", "LifeDrain" },
                { "LifeDrain_des", "LifeDrain_des" },
                { "LifeDrain_des_template", "LifeDrain_des_template" },
                { "ManaBoost", "ManaBoost" },
                { "ManaBoost_des", "ManaBoost_des" },
                { "ManaBoost_des_template", "ManaBoost_des_template" },
                { "Invincible", "Invincible" },
                { "Invincible_des", "Invincible_des" },
                { "Invincible_des_template", "Invincible_des_template" },
                { "Slash", "Slash" },
                { "Slash_des", "Slash_des" },
                { "Slash_des_template", "Slash_des_template" },
                { "ShurikenThrow", "ShurikenThrow" },
                { "ShurikenThrow_des", "ShurikenThrow_des" },
                { "ShurikenThrow_des_template", "ShurikenThrow_des_template" },
            }
        },
        {
            "vi", new Dictionary<string, string>
            {
                // CORE UI
                { "shop", "Cửa Hàng" },
                { "buy", "Mua" },
                { "sell", "Bán" },
                { "upgrade", "Nâng Cấp" },
                { "close", "Đóng" },
                { "inventory", "Hành Trang" },
                { "all", "Tất Cả" },
                { "equip", "Trang Bị" },
                { "use", "Sử Dụng" },
                { "stats", "Chỉ Số" },
                { "level_up", "Nâng Cấp" },
                { "skill", "Kỹ Năng" },
                { "assign", "Gán" },
                { "learn", "Học" },
                { "yes", "Có" },
                { "no", "Không" },
                { "roll", "Quay x1" },
                { "roll_x5", "Quay x5" },
                { "ok", "Nhận" },
                { "confirm", "Xác Nhận" },
                { "reroll", "Quay Lại" },
                { "congratulations", "Chúc Mừng!" },
                { "setting_title", "Cài Đặt" },
                { "bgm", "Nhạc Nền" },
                { "sfx", "Âm Thanh" },
                { "language", "Ngôn Ngữ" },
                { "language_en", "English" },
                { "language_vi", "Tiếng Việt" },
                { "purchased", "Đã Mua" },
                { "unequip_all", "Tháo Hết" },
                { "sell_all", "Bán Hết" },
                { "auto_equip", "Tự Trang Bị" },
                { "melee", "Cận Chiến" },
                { "ranged", "Tầm Xa" },
                { "heavy_melee", "Cận Chiến Nặng" },

                // ITEMS
                { "armorsuper6", "armorsuper6" },
                { "armorsuper6_des", "armorsuper6_des" },
                { "axe2", "axe2" },
                { "axe2_des", "axe2_des" },
                { "axe1", "axe1" },
                { "axe1_des", "axe1_des" },
                { "axe3", "axe3" },
                { "axe3_des", "axe3_des" },
                { "armorsuper1", "armorsuper1" },
                { "armorsuper1_des", "armorsuper1_des" },
                { "bone armor", "bone armor" },
                { "bone armor_des", "bone armor_des" },
                { "Pboosts ", "Pboosts " },
                { "Pboosts _des", "Pboosts _des" },
                { "boostsWhite", "boostsWhite" },
                { "boostsWhite_des", "boostsWhite_des" },
                { "Bow", "Bow" },
                { "Bow_des", "Bow_des" },
                { "Bowpro", "Bowpro" },
                { "Bowpro_des", "Bowpro_des" },
                { "Bowtitan", "Bowtitan" },
                { "Bowtitan_des", "Bowtitan_des" },
                { "sword4", "sword4" },
                { "sword4_des", "sword4_des" },
                { "helmet5", "helmet5" },
                { "helmet5_des", "helmet5_des" },
                { "helmet7", "helmet7" },
                { "helmet7_des", "helmet7_des" },
                { "clockworkarmor", "clockworkarmor" },
                { "clockworkarmor_des", "clockworkarmor_des" },
                { "cloth armor", "cloth armor" },
                { "cloth armor_des", "cloth armor_des" },
                { "sword3", "sword3" },
                { "sword3_des", "sword3_des" },
                { "armor4", "armor4" },
                { "armor4_des", "armor4_des" },
                { "dagger", "dagger" },
                { "dagger_des", "dagger_des" },
                { "cape2", "cape2" },
                { "cape2_des", "cape2_des" },
                { "dark sword", "dark sword" },
                { "dark sword_des", "dark sword_des" },
                { "dark sword 2", "dark sword 2" },
                { "dark sword 2_des", "dark sword 2_des" },
                { "demonhide boosts", "demonhide boosts" },
                { "demonhide boosts_des", "demonhide boosts_des" },
                { "dragongay", "dragongay" },
                { "dragongay_des", "dragongay_des" },
                { "dragon boosts", "dragon boosts" },
                { "dragon boosts_des", "dragon boosts_des" },
                { "dressshoes", "dressshoes" },
                { "dressshoes_des", "dressshoes_des" },
                { "armor6", "armor6" },
                { "armor6_des", "armor6_des" },
                { "explorerboots", "explorerboots" },
                { "explorerboots_des", "explorerboots_des" },
                { "armor7", "armor7" },
                { "armor7_des", "armor7_des" },
                { "frostarmor", "frostarmor" },
                { "frostarmor_des", "frostarmor_des" },
                { "boosts2", "boosts2" },
                { "boosts2_des", "boosts2_des" },
                { "boosts1", "boosts1" },
                { "boosts1_des", "boosts1_des" },
                { "boosts6", "boosts6" },
                { "boosts6_des", "boosts6_des" },
                { "helmet4", "helmet4" },
                { "helmet4_des", "helmet4_des" },
                { "hair0", "hair0" },
                { "hair0_des", "hair0_des" },
                { "hair01", "hair01" },
                { "hair01_des", "hair01_des" },
                { "hair1", "hair1" },
                { "hair1_des", "hair1_des" },
                { "hair02", "hair02" },
                { "hair02_des", "hair02_des" },
                { "hair2", "hair2" },
                { "hair2_des", "hair2_des" },
                { "hair03", "hair03" },
                { "hair03_des", "hair03_des" },
                { "hair3", "hair3" },
                { "hair3_des", "hair3_des" },
                { "hair04", "hair04" },
                { "hair04_des", "hair04_des" },
                { "hair4", "hair4" },
                { "hair4_des", "hair4_des" },
                { "hair04_2", "hair04_2" },
                { "hair04_2_des", "hair04_2_des" },
                { "hair5", "hair5" },
                { "hair5_des", "hair5_des" },
                { "hair6", "hair6" },
                { "hair6_des", "hair6_des" },
                { "hair7", "hair7" },
                { "hair7_des", "hair7_des" },
                { "hair8", "hair8" },
                { "hair8_des", "hair8_des" },
                { "hair9", "hair9" },
                { "hair9_des", "hair9_des" },
                { "hammer", "hammer" },
                { "hammer_des", "hammer_des" },
                { "helmet8", "helmet8" },
                { "helmet8_des", "helmet8_des" },
                { "hourse4", "hourse4" },
                { "hourse4_des", "hourse4_des" },
                { "helmet", "helmet" },
                { "helmet_des", "helmet_des" },
                { "cape5", "cape5" },
                { "cape5_des", "cape5_des" },
                { "horned helmet", "horned helmet" },
                { "horned helmet_des", "horned helmet_des" },
                { "hourse", "hourse" },
                { "hourse_des", "hourse_des" },
                { "hpc", "hpc" },
                { "hpc_des", "hpc_des" },
                { "lllusionrobe", "lllusionrobe" },
                { "lllusionrobe_des", "lllusionrobe_des" },
                { "armor2", "armor2" },
                { "armor2_des", "armor2_des" },
                { "iron armor", "iron armor" },
                { "iron armor_des", "iron armor_des" },
                { "iron boosts", "iron boosts" },
                { "iron boosts_des", "iron boosts_des" },
                { "armor1", "armor1" },
                { "armor1_des", "armor1_des" },
                { "helmet10", "helmet10" },
                { "helmet10_des", "helmet10_des" },
                { "sword", "sword" },
                { "sword_des", "sword_des" },
                { "sword+1", "sword+1" },
                { "sword+1_des", "sword+1_des" },
                { "armorsupers", "armorsupers" },
                { "armorsupers_des", "armorsupers_des" },
                { "knight's helmet", "knight's helmet" },
                { "knight's helmet_des", "knight's helmet_des" },
                { "cape", "cape" },
                { "cape_des", "cape_des" },
                { "lance", "lance" },
                { "lance_des", "lance_des" },
                { "lance1", "lance1" },
                { "lance1_des", "lance1_des" },
                { "lanceoh", "lanceoh" },
                { "lanceoh_des", "lanceoh_des" },
                { "lance2", "lance2" },
                { "lance2_des", "lance2_des" },
                { "lance3", "lance3" },
                { "lance3_des", "lance3_des" },
                { "armor8", "armor8" },
                { "armor8_des", "armor8_des" },
                { "armorsuper5", "armorsuper5" },
                { "armorsuper5_des", "armorsuper5_des" },
                { "cape3", "cape3" },
                { "cape3_des", "cape3_des" },
                { "boosts8", "boosts8" },
                { "boosts8_des", "boosts8_des" },
                { "armor3", "armor3" },
                { "armor3_des", "armor3_des" },
                { "pig-sticking", "pig-sticking" },
                { "pig-sticking_des", "pig-sticking_des" },
                { "boosts3", "boosts3" },
                { "boosts3_des", "boosts3_des" },
                { "cape1", "cape1" },
                { "cape1_des", "cape1_des" },
                { "boosts7", "boosts7" },
                { "boosts7_des", "boosts7_des" },
                { "armorsuper7", "armorsuper7" },
                { "armorsuper7_des", "armorsuper7_des" },
                { "armorsuper3", "armorsuper3" },
                { "armorsuper3_des", "armorsuper3_des" },
                { "robber's cap", "robber's cap" },
                { "robber's cap_des", "robber's cap_des" },
                { "royal helmet", "royal helmet" },
                { "royal helmet_des", "royal helmet_des" },
                { "sword2", "sword2" },
                { "sword2_des", "sword2_des" },
                { "sword24", "sword24" },
                { "sword24_des", "sword24_des" },
                { "sword21", "sword21" },
                { "sword21_des", "sword21_des" },
                { "sword23", "sword23" },
                { "sword23_des", "sword23_des" },
                { "armor5", "armor5" },
                { "armor5_des", "armor5_des" },
                { "helmet1", "helmet1" },
                { "helmet1_des", "helmet1_des" },
                { "slippers", "slippers" },
                { "slippers_des", "slippers_des" },
                { "helmet2", "helmet2" },
                { "helmet2_des", "helmet2_des" },
                { "armorsuper", "armorsuper" },
                { "armorsuper_des", "armorsuper_des" },
                { "sword a", "sword a" },
                { "sword a_des", "sword a_des" },
                { "swordshort1", "swordshort1" },
                { "swordshort1_des", "swordshort1_des" },
                { "swordshort", "swordshort" },
                { "swordshort_des", "swordshort_des" },
                { "armorsuper4", "armorsuper4" },
                { "armorsuper4_des", "armorsuper4_des" },
                { "sword5", "sword5" },
                { "sword5_des", "sword5_des" },
                { "armorsuper8", "armorsuper8" },
                { "armorsuper8_des", "armorsuper8_des" },
                { "trihorn", "trihorn" },
                { "trihorn_des", "trihorn_des" },
                { "hourse1", "hourse1" },
                { "hourse1_des", "hourse1_des" },
                { "helmet3", "helmet3" },
                { "helmet3_des", "helmet3_des" },
                { "armorsuper2", "armorsuper2" },
                { "armorsuper2_des", "armorsuper2_des" },
                { "hourse2", "hourse2" },
                { "hourse2_des", "hourse2_des" },
                { "boosts5", "boosts5" },
                { "boosts5_des", "boosts5_des" },
                { "helmet6", "helmet6" },
                { "helmet6_des", "helmet6_des" },

                // SKILLS
                { "AttackSpeedBoost", "AttackSpeedBoost" },
                { "AttackSpeedBoost_des", "AttackSpeedBoost_des" },
                { "AttackSpeedBoost_des_template", "AttackSpeedBoost_des_template" },
                { "CritChanceBoost", "CritChanceBoost" },
                { "CritChanceBoost_des", "CritChanceBoost_des" },
                { "CritChanceBoost_des_template", "CritChanceBoost_des_template" },
                { "DamageBoost", "DamageBoost" },
                { "DamageBoost_des", "DamageBoost_des" },
                { "DamageBoost_des_template", "DamageBoost_des_template" },
                { "DefenseBoost", "DefenseBoost" },
                { "DefenseBoost_des", "DefenseBoost_des" },
                { "DefenseBoost_des_template", "DefenseBoost_des_template" },
                { "HealthBoost", "HealthBoost" },
                { "HealthBoost_des", "HealthBoost_des" },
                { "HealthBoost_des_template", "HealthBoost_des_template" },
                { "SpeedBoost", "SpeedBoost" },
                { "SpeedBoost_des", "SpeedBoost_des" },
                { "SpeedBoost_des_template", "SpeedBoost_des_template" },
                { "Dash", "Lướt Nhanh" },
                { "Dash_des", "Dash_des" },
                { "Dash_des_template", "Dash_des_template" },
                { "LifeDrain", "LifeDrain" },
                { "LifeDrain_des", "LifeDrain_des" },
                { "LifeDrain_des_template", "LifeDrain_des_template" },
                { "ManaBoost", "ManaBoost" },
                { "ManaBoost_des", "ManaBoost_des" },
                { "ManaBoost_des_template", "ManaBoost_des_template" },
                { "Invincible", "Invincible" },
                { "Invincible_des", "Invincible_des" },
                { "Invincible_des_template", "Invincible_des_template" },
                { "Slash", "Vết Chém" },
                { "Slash_des", "Slash_des" },
                { "Slash_des_template", "Slash_des_template" },
                { "ShurikenThrow", "ShurikenThrow" },
                { "ShurikenThrow_des", "ShurikenThrow_des" },
                { "ShurikenThrow_des_template", "ShurikenThrow_des_template" },
            }
        }
    };

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }

        LoadResourceTranslations();
        LoadLanguage();
    }

    private void Start()
    {
        RequestRefresh();
    }

    private void Update()
    {
        if (_refreshFramesRemaining <= 0) return;

        _refreshFramesRemaining--;
        RefreshAllTexts();
    }

    public void SetLanguage(string lang)
    {
        if (lang != "en" && lang != "vi") return;

        _currentLanguage = lang;
        PlayerPrefs.SetString(LANGUAGE_PREF_KEY, lang);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
        RequestRefresh();
    }

    private void LoadLanguage()
    {
        _currentLanguage = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, "en");
    }

    private void LoadResourceTranslations()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Localization");
        foreach (TextAsset file in files)
        {
            if (file == null || string.IsNullOrWhiteSpace(file.text))
                continue;

            TranslationFile translationFile = null;
            try
            {
                translationFile = JsonUtility.FromJson<TranslationFile>(file.text);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LanguageManager] Could not parse localization file '{file.name}': {ex.Message}");
            }

            if (translationFile?.entries == null)
                continue;

            string language = string.IsNullOrWhiteSpace(translationFile.language)
                ? file.name
                : translationFile.language.Trim();

            if (!_translations.TryGetValue(language, out var languageTable))
            {
                languageTable = new Dictionary<string, string>();
                _translations[language] = languageTable;
            }

            foreach (TranslationEntry entry in translationFile.entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    continue;

                languageTable[entry.key.Trim()] = entry.value ?? string.Empty;
            }
        }
    }

    public string GetTranslation(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        string hardcoded = GetCoreTranslation(key);
        if (!string.IsNullOrEmpty(hardcoded))
        {
            return hardcoded;
        }

        if (_translations.TryGetValue(_currentLanguage, out var langDict))
        {
            if (langDict.TryGetValue(key, out string val))
            {
                return val;
            }
        }

        if (_currentLanguage != "en"
            && _translations.TryGetValue("en", out var englishDict)
            && englishDict.TryGetValue(key, out string englishValue))
        {
            return englishValue;
        }

        return key; // Fallback to returning key
    }

    public bool HasTranslation(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (!string.IsNullOrEmpty(GetCoreTranslation(key)))
            return true;

        return _translations.TryGetValue(_currentLanguage, out var langDict)
            && langDict.ContainsKey(key);
    }

    public string GetTranslationOrFallback(string key, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            string translated = GetTranslation(key);
            if (!string.IsNullOrWhiteSpace(translated) && translated != key)
                return translated;
        }

        if (!string.IsNullOrWhiteSpace(fallback))
            return TranslateText(fallback);

        return string.IsNullOrWhiteSpace(key) ? string.Empty : key;
    }

    public void RequestRefresh()
    {
        _refreshFramesRemaining = 30;
        RefreshAllTexts();
    }

    public void RefreshAllTexts()
    {
        EnsureTextLookup();

        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text text in texts)
        {
            if (text == null || string.IsNullOrEmpty(text.text)) continue;
            if (IsManagedByEnemyInfoPopup(text)) continue;

            ApplyDefaultFont(text);
            PrepareStatTextIfNeeded(text);
            string translated = TranslateText(text.text);
            if (translated != text.text)
            {
                text.text = translated;
                PrepareStatTextIfNeeded(text);
            }
        }

        Text[] legacyTexts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Text text in legacyTexts)
        {
            if (text == null || string.IsNullOrEmpty(text.text)) continue;

            ApplyDefaultFont(text);
            string translated = TranslateText(text.text);
            if (translated != text.text)
            {
                text.text = translated;
            }
        }
    }

    public string TranslateText(string source)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        EnsureTextLookup();

        string trimmed = source.Trim().Trim('\'');
        if (_textToKey.TryGetValue(trimmed, out string key))
        {
            string translated = GetTranslation(key);
            if (source.Trim() == source.Trim().ToUpperInvariant())
            {
                translated = translated.ToUpperInvariant();
            }

            return PreserveEdgeWhitespace(source, translated);
        }

        if (IsStatText(source))
        {
            return TranslateStatText(source);
        }

        string result = source;
        ReplaceToken(ref result, "Not enough Gold", "not_enough_gold");
        ReplaceToken(ref result, "Upgrade successful!", "upgrade_successful");
        ReplaceToken(ref result, "Upgrade successful", "upgrade_successful");
        ReplaceToken(ref result, "Sold successfully", "sold_successfully");
        ReplaceToken(ref result, "Unequip", "unequip");
        ReplaceToken(ref result, "Upgrade", "upgrade");
        ReplaceToken(ref result, "Confirm", "confirm");
        ReplaceToken(ref result, "Close", "close");
        ReplaceToken(ref result, "Locked", "locked");
        ReplaceToken(ref result, "Learn", "learn");
        ReplaceToken(ref result, "Claim", "claim");
        ReplaceToken(ref result, "Equip", "equip");
        ReplaceToken(ref result, "Sell", "sell");
        ReplaceToken(ref result, "Buy", "buy");
        ReplaceToken(ref result, "Use", "use");
        ReplaceToken(ref result, "Skill Point", "skill_point");
        ReplaceToken(ref result, "Level Up", "level_up");
        ReplaceToken(ref result, "Level", "level");
        ReplaceToken(ref result, "Current", "current");
        ReplaceToken(ref result, "Price", "price");
        ReplaceToken(ref result, "Power", "power");
        ReplaceToken(ref result, "Stage", "stage");
        ReplaceToken(ref result, "Wave", "wave");
        ReplaceToken(ref result, "Chapter", "chapter");
        ReplaceToken(ref result, "Text Hp", "text_hp");
        ReplaceToken(ref result, "HP", "hp");
        ReplaceToken(ref result, "Hp", "hp");
        ReplaceToken(ref result, "hp", "hp");
        ReplaceToken(ref result, "x1 speed", "speed_x1");
        ReplaceToken(ref result, "x3 speed", "speed_x3");
        ReplaceToken(ref result, "Receive", "receive");
        ReplaceToken(ref result, "items", "items");
        ReplaceToken(ref result, "item", "item");
        ReplaceToken(ref result, "Quest Failed", "quest_failed");
        ReplaceToken(ref result, "TAP TO START", "tap_to_start");
        ReplaceToken(ref result, "You are not ready yet", "not_ready");
        ReplaceToken(ref result, "NextWave", "next_wave");
        ReplaceToken(ref result, "Quest", "quest");
        ReplaceToken(ref result, "Reward", "reward");
        ReplaceToken(ref result, "Received", "received");
        ReplaceToken(ref result, "Completed", "completed");
        ReplaceToken(ref result, "Hero", "hero");
        ReplaceToken(ref result, "Empty", "empty");
        ReplaceToken(ref result, "Button", "button");
        ReplaceToken(ref result, "New Text", "new_text");
        ReplaceToken(ref result, "Dialog", "dialog");
        ReplaceToken(ref result, "Boss", "boss");
        ReplaceToken(ref result, "Cheat", "cheat");
        ReplaceToken(ref result, "ROLL X5", "roll_x5");
        ReplaceToken(ref result, "ROLL", "roll");
        ReplaceToken(ref result, "Lv", "lv");
        ReplaceToken(ref result, "ATK", "atk");
        ReplaceToken(ref result, "DEF", "def");
        ReplaceToken(ref result, "SPD", "spd");
        ReplaceToken(ref result, "Attack", "attack");
        ReplaceToken(ref result, "Defense", "defense");
        ReplaceToken(ref result, "Crit", "crit");
        ReplaceToken(ref result, "LifeSteal", "life_steal");
        ReplaceToken(ref result, "AtkSpeed", "attack_speed");
        ReplaceToken(ref result, "AttSpeed", "attack_speed");
        ReplaceToken(ref result, "Speed", "speed");
        ReplaceToken(ref result, "Gold", "gold");
        ReplaceToken(ref result, "Gems", "gem");
        ReplaceToken(ref result, "Gem", "gem");
        return result;
    }

    private bool IsManagedByEnemyInfoPopup(TMP_Text text)
    {
        return text != null && text.GetComponentInParent<EnemyInfoPopupUI>(true) != null;
    }

    private void ApplyDefaultFont(TMP_Text text)
    {
        if (text == null || TMP_Settings.defaultFontAsset == null) return;

        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
        if (text.font != defaultFont)
        {
            text.font = defaultFont;
        }

        if (defaultFont.material != null)
        {
            text.fontSharedMaterial = defaultFont.material;
        }
    }

    private void ApplyDefaultFont(Text text)
    {
        if (text == null) return;

        _defaultLegacyFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_defaultLegacyFont != null)
        {
            text.font = _defaultLegacyFont;
        }
    }

    private void PrepareStatTextIfNeeded(TMP_Text text)
    {
        if (text == null || !IsStatText(text.text)) return;

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private bool IsStatText(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        return text.Contains("attack_icon")
            || text.Contains("defense_icon")
            || text.Contains("speed_icon")
            || text.Contains("crit_icon")
            || text.Contains("lifesteal_icon")
            || text.Contains("attackspeed_icon")
            || text.Contains("health_icon")
            || text.Contains("mana_icon");
    }

    private string TranslateStatText(string source)
    {
        string text = source;

        if (_currentLanguage == "vi")
        {
            ReplaceStatLabel(ref text, "Attack", "T\u1ea5n c\u00f4ng");
            ReplaceStatLabel(ref text, "T\u1ea5n C\u00f4ng", "T\u1ea5n c\u00f4ng");
            ReplaceStatLabel(ref text, "Defense", "Ph\u00f2ng th\u1ee7");
            ReplaceStatLabel(ref text, "Ph\u00f2ng Th\u1ee7", "Ph\u00f2ng th\u1ee7");
            ReplaceStatLabel(ref text, "Speed", "T\u1ed1c \u0111\u1ed9");
            ReplaceStatLabel(ref text, "T\u1ed1c \u0110\u1ed9", "T\u1ed1c \u0111\u1ed9");
            ReplaceStatLabel(ref text, "Crit", "Ch\u00ed m\u1ea1ng");
            ReplaceStatLabel(ref text, "Ch\u00ed M\u1ea1ng", "Ch\u00ed m\u1ea1ng");
            ReplaceStatLabel(ref text, "LifeSteal", "H\u00fat m\u00e1u");
            ReplaceStatLabel(ref text, "H\u00fat M\u00e1u", "H\u00fat m\u00e1u");
            ReplaceStatLabel(ref text, "AtkSpeed", "T\u1ed1c \u0111\u00e1nh");
            ReplaceStatLabel(ref text, "AttSpeed", "T\u1ed1c \u0111\u00e1nh");
            ReplaceStatLabel(ref text, "Atk Speed", "T\u1ed1c \u0111\u00e1nh");
            ReplaceStatLabel(ref text, "HP", "M\u00e1u");
            ReplaceStatLabel(ref text, "Mana", "Mana");
        }
        else
        {
            ReplaceStatLabel(ref text, "T\u1ea5n c\u00f4ng", "Attack");
            ReplaceStatLabel(ref text, "Ph\u00f2ng th\u1ee7", "Defense");
            ReplaceStatLabel(ref text, "T\u1ed1c \u0111\u1ed9", "Speed");
            ReplaceStatLabel(ref text, "Ch\u00ed m\u1ea1ng", "Crit");
            ReplaceStatLabel(ref text, "H\u00fat M\u00e1u", "LifeSteal");
            ReplaceStatLabel(ref text, "H\u00fat m\u00e1u", "LifeSteal");
            ReplaceStatLabel(ref text, "T\u1ed1c \u0110\u00e1nh", "AttSpeed");
            ReplaceStatLabel(ref text, "T\u1ed1c \u0111\u00e1nh", "AttSpeed");
            ReplaceStatLabel(ref text, "M\u00e1u", "HP");
        }

        return text;
    }

    private void ReplaceStatLabel(ref string text, string from, string to)
    {
        text = text.Replace($">{from}:</color>", $">{to}:</color>");
        text = text.Replace($">{from}: </color>", $">{to}: </color>");
    }

    private void ReplaceToken(ref string text, string token, string key)
    {
        string translated = GetTranslation(key);
        text = text.Replace(token, translated);
        text = text.Replace(GetCoreTranslation(key, "en"), translated);
        text = text.Replace(GetCoreTranslation(key, "vi"), translated);
    }

    private void EnsureTextLookup()
    {
        if (_textToKey != null) return;

        _textToKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var language in _translations)
        {
            foreach (var entry in language.Value)
            {
                RegisterTextAlias(entry.Value, entry.Key);
            }
        }

        string[] coreKeys =
        {
            "shop", "buy", "sell", "upgrade", "unequip", "close", "inventory", "all", "equip", "use",
            "stats", "level_up", "skill", "assign", "learn", "yes", "no", "roll", "roll_x5", "ok", "confirm", "reroll", "congratulations",
            "setting_title", "bgm", "sfx", "language", "purchased", "unequip_all", "sell_all",
            "auto_equip", "melee", "ranged", "heavy_melee", "claim", "next", "map", "gacha",
            "equipment", "locked", "skill_point", "level", "current", "price", "power", "stage",
            "wave", "chapter", "attack", "defense", "speed", "crit", "life_steal", "attack_speed",
            "hp", "mana", "gold", "gem", "quest", "quest_failed", "reward", "received", "completed", "feature_requires_level",
            "hero", "empty", "button", "new_text", "dialog", "boss", "cheat", "tap_to_start",
            "not_ready", "next_wave", "xp", "lv", "atk", "def", "spd",
            "choose_passive_skill", "skill_applied_immediately", "choose_skill_first",
            "not_enough_gold", "upgrade_successful", "sold_successfully", "receive", "item", "items",
            "UpgradeItem", "UpgradeItemLevel", "KillEnemies", "name", "description", "tier", "legendary",
            "text_hp", "speed_x1", "speed_x3",
            "AttackSpeedBoost", "CritChanceBoost", "DamageBoost", "DefenseBoost", "HealthBoost", "SpeedBoost",
            "Dash", "LifeDrain", "ManaBoost", "Invincible", "Slash", "ShurikenThrow",
            "AttackSpeedBoost_des", "CritChanceBoost_des", "DamageBoost_des", "DefenseBoost_des", "HealthBoost_des", "SpeedBoost_des",
            "Dash_des", "LifeDrain_des", "ManaBoost_des", "Invincible_des", "Slash_des", "ShurikenThrow_des",
            "AttackSpeedBoost_des_template", "CritChanceBoost_des_template", "DamageBoost_des_template", "DefenseBoost_des_template",
            "HealthBoost_des_template", "SpeedBoost_des_template", "Dash_des_template", "LifeDrain_des_template",
            "ManaBoost_des_template", "Invincible_des_template", "Slash_des_template", "ShurikenThrow_des_template"
        };

        foreach (string key in coreKeys)
        {
            RegisterTextAlias(GetCoreTranslation(key, "en"), key);
            RegisterTextAlias(GetCoreTranslation(key, "vi"), key);
        }

        RegisterTextAlias("Shop", "shop");
        RegisterTextAlias("Buy", "buy");
        RegisterTextAlias("Sell", "sell");
        RegisterTextAlias("Upgrade", "upgrade");
        RegisterTextAlias("Unequip", "unequip");
        RegisterTextAlias("Close", "close");
        RegisterTextAlias("Inventory", "inventory");
        RegisterTextAlias("INVENTORY", "inventory");
        RegisterTextAlias("All", "all");
        RegisterTextAlias("all", "all");
        RegisterTextAlias("Equip", "equip");
        RegisterTextAlias("Use", "use");
        RegisterTextAlias("Stats", "stats");
        RegisterTextAlias("STATS", "stats");
        RegisterTextAlias("Skill", "skill");
        RegisterTextAlias("SKILL", "skill");
        RegisterTextAlias("Assign", "assign");
        RegisterTextAlias("assign", "assign");
        RegisterTextAlias("Learn", "learn");
        RegisterTextAlias("Yes", "yes");
        RegisterTextAlias("No", "no");
        RegisterTextAlias("OK", "ok");
        RegisterTextAlias("ROLL", "roll");
        RegisterTextAlias("ROLL X5", "roll_x5");
        RegisterTextAlias("Confirm", "confirm");
        RegisterTextAlias("Reroll", "reroll");
        RegisterTextAlias("Setting", "setting_title");
        RegisterTextAlias("Language", "language");
        RegisterTextAlias("Purchased", "purchased");
        RegisterTextAlias("Sell All", "sell_all");
        RegisterTextAlias("Auto Equip", "auto_equip");
        RegisterTextAlias("Unequip All", "unequip_all");
        RegisterTextAlias("Melee", "melee");
        RegisterTextAlias("Ranged", "ranged");
        RegisterTextAlias("Heavy Melee", "heavy_melee");
        RegisterTextAlias("Claim", "claim");
        RegisterTextAlias("Next", "next");
        RegisterTextAlias("Map", "map");
        RegisterTextAlias("GaCha", "gacha");
        RegisterTextAlias("Gacha", "gacha");
        RegisterTextAlias("Equipment", "equipment");
        RegisterTextAlias("Locked", "locked");
        RegisterTextAlias("Hero", "hero");
        RegisterTextAlias("Quest", "quest");
        RegisterTextAlias("Quest Failed", "quest_failed");
        RegisterTextAlias("Empty", "empty");
        RegisterTextAlias("Button", "button");
        RegisterTextAlias("New Text", "new_text");
        RegisterTextAlias("Dialog", "dialog");
        RegisterTextAlias("Boss", "boss");
        RegisterTextAlias("Cheat", "cheat");
        RegisterTextAlias("TAP TO START", "tap_to_start");
        RegisterTextAlias("You are not ready yet", "not_ready");
        RegisterTextAlias("NextWave", "next_wave");
        RegisterTextAlias("XP", "xp");
        RegisterTextAlias("Lv", "lv");
        RegisterTextAlias("ATK", "atk");
        RegisterTextAlias("DEF", "def");
        RegisterTextAlias("SPD", "spd");
        RegisterTextAlias("Choose 1 new Passive skill", "choose_passive_skill");
        RegisterTextAlias("The skill will be applied immediately", "skill_applied_immediately");
        RegisterTextAlias("des", "description");
        RegisterTextAlias("Des", "description");
        RegisterTextAlias("tier", "tier");
        RegisterTextAlias("Legendaly", "legendary");
        RegisterTextAlias("xin chuc mung tk gay", "congratulations");
        RegisterTextAlias("xin chuc mung tk gay \\n", "congratulations");
    }

    private void RegisterTextAlias(string text, string key)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        _textToKey[text.Trim().Trim('\'')] = key;
    }

    private string PreserveEdgeWhitespace(string original, string translated)
    {
        int start = 0;
        while (start < original.Length && char.IsWhiteSpace(original[start])) start++;

        int end = original.Length - 1;
        while (end >= 0 && char.IsWhiteSpace(original[end])) end--;

        string prefix = original.Substring(0, start);
        string suffix = end + 1 < original.Length ? original.Substring(end + 1) : string.Empty;
        return prefix + translated + suffix;
    }

    private string GetCoreTranslation(string key)
    {
        return GetCoreTranslation(key, _currentLanguage);
    }

    private string GetCoreTranslation(string key, string language)
    {
        if (language == "en")
        {
            return key switch
            {
                "shop" => "Shop",
                "buy" => "Buy",
                "sell" => "Sell",
                "upgrade" => "Upgrade",
                "unequip" => "Unequip",
                "close" => "Close",
                "inventory" => "Inventory",
                "all" => "All",
                "equip" => "Equip",
                "use" => "Use",
                "stats" => "Stats",
                "level_up" => "Level Up",
                "skill" => "Skill",
                "assign" => "Assign",
                "learn" => "Learn",
                "yes" => "Yes",
                "no" => "No",
                "roll" => "ROLL",
                "roll_x5" => "ROLL X5",
                "ok" => "OK",
                "confirm" => "Confirm",
                "reroll" => "Reroll",
                "congratulations" => "Congratulations!",
                "setting_title" => "Setting",
                "bgm" => "BGM",
                "sfx" => "SFX",
                "language" => "Language",
                "purchased" => "Purchased",
                "unequip_all" => "Unequip All",
                "sell_all" => "Sell All",
                "auto_equip" => "Auto Equip",
                "melee" => "Melee",
                "ranged" => "Ranged",
                "heavy_melee" => "Heavy Melee",
                "claim" => "Claim",
                "next" => "Next",
                "map" => "Map",
                "gacha" => "Gacha",
                "equipment" => "Equipment",
                "locked" => "Locked",
                "skill_point" => "Skill Point",
                "level" => "Level",
                "current" => "Current",
                "price" => "Price",
                "power" => "Power",
                "stage" => "Stage",
                "wave" => "Wave",
                "chapter" => "Chapter",
                "attack" => "Attack",
                "defense" => "Defense",
                "speed" => "Speed",
                "crit" => "Crit",
                "life_steal" => "LifeSteal",
                "attack_speed" => "AttSpeed",
                "hp" => "HP",
                "mana" => "Mana",
                "gold" => "Gold",
                "gem" => "Gem",
                "quest" => "Quest",
                "quest_failed" => "Quest Failed",
                "no_active_quest" => "No active quest",
                "find_npc_for_quest" => "Find an NPC for a quest",
                "achievement_rewards" => "Achievement Rewards!",
                "quest_completed_message" => "You completed the quest!",
                "reward" => "Reward",
                "received" => "Received",
                "completed" => "Completed",
                "feature_requires_level" => "Requires level {0}",
                "hero" => "Hero",
                "empty" => "Empty",
                "button" => "Button",
                "new_text" => "New Text",
                "dialog" => "Dialog",
                "boss" => "Boss",
                "cheat" => "Cheat",
                "tap_to_start" => "TAP TO START",
                "not_ready" => "You are not ready yet",
                "next_wave" => "NextWave",
                "xp" => "XP",
                "lv" => "Lv",
                "atk" => "ATK",
                "def" => "DEF",
                "spd" => "SPD",
                "not_enough_gold" => "Not enough Gold",
                "upgrade_successful" => "Upgrade successful!",
                "sold_successfully" => "Sold successfully",
                "receive" => "Receive",
                "item" => "item",
                "items" => "items",
                "UpgradeItem" => "Upgrade Item",
                "UpgradeItemLevel" => "Upgrade Item Level",
                "KillEnemies" => "Kill Enemies",
                "name" => "Name",
                "description" => "Description",
                "tier" => "Tier",
                "legendary" => "Legendary",
                "text_hp" => "HP Text",
                "speed_x1" => "x1 Speed",
                "speed_x3" => "x3 Speed",
                "AttackSpeedBoost" => "Attack Speed Boost",
                "AttackSpeedBoost_des" => "Increase attack speed.",
                "AttackSpeedBoost_des_template" => "Increase attack speed by {value}.",
                "CritChanceBoost" => "Critical Boost",
                "CritChanceBoost_des" => "Increase critical chance.",
                "CritChanceBoost_des_template" => "Increase critical chance by {value}%.",
                "DamageBoost" => "Damage Boost",
                "DamageBoost_des" => "Increase attack damage.",
                "DamageBoost_des_template" => "Increase damage by {value}.",
                "DefenseBoost" => "Defense Boost",
                "DefenseBoost_des" => "Increase defense.",
                "DefenseBoost_des_template" => "Increase defense by {value}.",
                "HealthBoost" => "Health Boost",
                "HealthBoost_des" => "Increase maximum health.",
                "HealthBoost_des_template" => "Increase maximum health by {value}.",
                "SpeedBoost" => "Speed Boost",
                "SpeedBoost_des" => "Increase movement speed.",
                "SpeedBoost_des_template" => "Increase movement speed by {value}.",
                "Dash" => "Dash",
                "Dash_des" => "Dash quickly in the moving direction.",
                "Dash_des_template" => "Dash quickly. Mana: {mana}. Cooldown: {cooldown}s.",
                "LifeDrain" => "Life Drain",
                "LifeDrain_des" => "Steal health from enemies.",
                "LifeDrain_des_template" => "Steal {value}% health from damage dealt.",
                "ManaBoost" => "Mana Boost",
                "ManaBoost_des" => "Increase maximum mana.",
                "ManaBoost_des_template" => "Increase maximum mana by {value}.",
                "Invincible" => "Invincible",
                "Invincible_des" => "Become invincible for a short time.",
                "Invincible_des_template" => "Become invincible for {duration}s. Cooldown: {cooldown}s.",
                "Slash" => "Slash",
                "Slash_des" => "Slash enemies in front of you.",
                "Slash_des_template" => "Deal {value} slash damage. Mana: {mana}. Cooldown: {cooldown}s.",
                "ShurikenThrow" => "Shuriken Throw",
                "ShurikenThrow_des" => "Throw a shuriken at enemies.",
                "ShurikenThrow_des_template" => "Throw a shuriken dealing {value} damage. Mana: {mana}. Cooldown: {cooldown}s.",
                "choose_passive_skill" => "Choose 1 new Passive skill",
                "skill_applied_immediately" => "The skill will be applied immediately",
                "choose_skill_first" => "Choose 1 skill first",
                "quest.NV1.name" => "First Slimes",
                "quest.NV1.description" => "Defeat 6 Small Slimes",
                "quest.NV1.objective.1" => "Defeat 6 Small Slimes",
                "quest.NV2.name" => "First Clear",
                "quest.NV2.description" => "Clear Stage 1",
                "quest.NV2.objective.1" => "Clear Stage 1",
                "quest.NV3.name" => "Train Your Body",
                "quest.NV3.description" => "Upgrade any stat 3 times",
                "quest.NV3.objective.1" => "Upgrade any stat 3 times",
                "quest.NV4.name" => "First Equipment",
                "quest.NV4.description" => "Buy any 1 equipment item",
                "quest.NV4.objective.1" => "Buy any 1 equipment item",
                "quest.NV5.name" => "Wear Your Power",
                "quest.NV5.description" => "Equip 1 item",
                "quest.NV5.objective.1" => "Equip 1 item",
                "quest.NV6.name" => "Goblin Camp",
                "quest.NV6.description" => "Defeat 8 Goblins",
                "quest.NV6.objective.1" => "Defeat 8 Goblins",
                "quest.NV7.name" => "Slime Crown",
                "quest.NV7.description" => "Defeat King Slime",
                "quest.NV7.objective.1" => "Defeat King Slime",
                "quest.NV8.name" => "Try Your Luck",
                "quest.NV8.description" => "Roll Gacha 1 time",
                "quest.NV8.objective.1" => "Roll Gacha 1 time",
                "quest.NV9.name" => "Small Collection",
                "quest.NV9.description" => "Own 3 different equipment items",
                "quest.NV9.objective.1" => "Own 3 different equipment items",
                "quest.NV10.name" => "Sharpen Gear",
                "quest.NV10.description" => "Upgrade equipment 2 times",
                "quest.NV10.objective.1" => "Upgrade equipment 2 times",
                "quest.NV11.name" => "Bones Ahead",
                "quest.NV11.description" => "Defeat 5 Skeletons and 5 Skeleton 1",
                "quest.NV11.objective.1" => "Defeat 5 Skeletons",
                "quest.NV11.objective.2" => "Defeat 5 Skeleton 1",
                "quest.NV12.name" => "Goblin King",
                "quest.NV12.description" => "Defeat Goblin King Boss",
                "quest.NV12.objective.1" => "Defeat Goblin King Boss",
                "quest.NV13.name" => "Stage Climber",
                "quest.NV13.description" => "Clear Stage 3",
                "quest.NV13.objective.1" => "Clear Stage 3",
                "quest.NV14.name" => "Rogue Trouble",
                "quest.NV14.description" => "Defeat 10 Rogues",
                "quest.NV14.objective.1" => "Defeat 10 Rogues",
                "quest.NV15.name" => "Better Blade",
                "quest.NV15.description" => "Upgrade 1 equipment item to +3",
                "quest.NV15.objective.1" => "Upgrade 1 equipment item to +3",
                "quest.NV16.name" => "Axe Line",
                "quest.NV16.description" => "Defeat 10 Axemen",
                "quest.NV16.objective.1" => "Defeat 10 Axemen",
                "quest.NV17.name" => "Bone Captain",
                "quest.NV17.description" => "Defeat Skeleton Boss",
                "quest.NV17.objective.1" => "Defeat Skeleton Boss",
                "quest.NV18.name" => "Gravok Hunt",
                "quest.NV18.description" => "Defeat 12 Gravok",
                "quest.NV18.objective.1" => "Defeat 12 Gravok",
                "quest.NV19.name" => "Warlock Lesson",
                "quest.NV19.description" => "Defeat 12 Warlocks",
                "quest.NV19.objective.1" => "Defeat 12 Warlocks",
                "quest.NV20.name" => "Build Momentum",
                "quest.NV20.description" => "Upgrade any stat 8 more times",
                "quest.NV20.objective.1" => "Upgrade any stat 8 more times",
                "quest.NV21.name" => "Rare Attempt",
                "quest.NV21.description" => "Roll Gacha 5 times",
                "quest.NV21.objective.1" => "Roll Gacha 5 times",
                "quest.NV22.name" => "Fuller Bag",
                "quest.NV22.description" => "Own 5 different equipment items",
                "quest.NV22.objective.1" => "Own 5 different equipment items",
                "quest.NV23.name" => "Silent Hunt",
                "quest.NV23.description" => "Defeat 12 Silentmaw",
                "quest.NV23.objective.1" => "Defeat 12 Silentmaw",
                "quest.NV24.name" => "Elfblade Trial",
                "quest.NV24.description" => "Defeat 12 Elfblades",
                "quest.NV24.objective.1" => "Defeat 12 Elfblades",
                "quest.NV25.name" => "Face the Executioner",
                "quest.NV25.description" => "Defeat Undead Executioner",
                "quest.NV25.objective.1" => "Defeat Undead Executioner",
                "quest.NV26.name" => "Heavy Road",
                "quest.NV26.description" => "Defeat 8 Ravageron and 8 Fangriders",
                "quest.NV26.objective.1" => "Defeat 8 Ravageron",
                "quest.NV26.objective.2" => "Defeat 8 Fangriders",
                "quest.NV27.name" => "Stone Machine",
                "quest.NV27.description" => "Defeat Mecha Stone Golem",
                "quest.NV27.objective.1" => "Defeat Mecha Stone Golem",
                "quest.NV28.name" => "Creature Check",
                "quest.NV28.description" => "Defeat Boss Creature",
                "quest.NV28.objective.1" => "Defeat Boss Creature",
                "quest.NV29.name" => "Late Game Ready",
                "quest.NV29.description" => "Clear Stage 8",
                "quest.NV29.objective.1" => "Clear Stage 8",
                _ => string.Empty
            };
        }

        return key switch
        {
            "shop" => "C\u1eeda H\u00e0ng",
            "buy" => "Mua",
            "sell" => "B\u00e1n",
            "upgrade" => "N\u00e2ng C\u1ea5p",
            "unequip" => "Th\u00e1o",
            "close" => "\u0110\u00f3ng",
            "inventory" => "H\u00e0nh Trang",
            "all" => "T\u1ea5t C\u1ea3",
            "equip" => "Trang B\u1ecb",
            "use" => "S\u1eed D\u1ee5ng",
            "stats" => "Ch\u1ec9 S\u1ed1",
            "level_up" => "L\u00ean C\u1ea5p",
            "skill" => "K\u1ef9 N\u0103ng",
            "assign" => "G\u00e1n",
            "learn" => "H\u1ecdc",
            "yes" => "C\u00f3",
            "no" => "Kh\u00f4ng",
            "roll" => "Quay",
            "roll_x5" => "Quay x5",
            "ok" => "OK",
            "confirm" => "X\u00e1c Nh\u1eadn",
            "reroll" => "Quay L\u1ea1i",
            "congratulations" => "Ch\u00fac M\u1eebng!",
            "setting_title" => "C\u00e0i \u0110\u1eb7t",
            "bgm" => "Nh\u1ea1c N\u1ec1n",
            "sfx" => "\u00c2m Thanh",
            "language" => "Ng\u00f4n Ng\u1eef",
            "purchased" => "\u0110\u00e3 Mua",
            "unequip_all" => "Th\u00e1o H\u1ebft",
            "sell_all" => "B\u00e1n H\u1ebft",
            "auto_equip" => "T\u1ef1 Trang B\u1ecb",
            "melee" => "C\u1eadn Chi\u1ebfn",
            "ranged" => "T\u1ea7m Xa",
            "heavy_melee" => "C\u1eadn Chi\u1ebfn N\u1eb7ng",
            "claim" => "Nh\u1eadn",
            "next" => "Ti\u1ebfp",
            "map" => "B\u1ea3n \u0110\u1ed3",
            "gacha" => "Gacha",
            "equipment" => "Trang B\u1ecb",
            "locked" => "\u0110\u00e3 Kh\u00f3a",
            "skill_point" => "\u0110i\u1ec3m K\u1ef9 N\u0103ng",
            "level" => "C\u1ea5p",
            "current" => "Hi\u1ec7n T\u1ea1i",
            "price" => "Gi\u00e1",
            "power" => "S\u1ee9c M\u1ea1nh",
            "stage" => "M\u00e0n",
            "wave" => "\u0110\u1ee3t",
            "chapter" => "Ch\u01b0\u01a1ng",
            "attack" => "T\u1ea5n C\u00f4ng",
            "defense" => "Ph\u00f2ng Th\u1ee7",
            "speed" => "T\u1ed1c \u0110\u1ed9",
            "crit" => "Ch\u00ed M\u1ea1ng",
            "life_steal" => "H\u00fat M\u00e1u",
            "attack_speed" => "T\u1ed1c \u0110\u00e1nh",
            "hp" => "M\u00e1u",
            "mana" => "Mana",
            "gold" => "V\u00e0ng",
            "gem" => "Ng\u1ecdc",
            "quest" => "Nhi\u1ec7m V\u1ee5",
            "quest_failed" => "Nhi\u1ec7m V\u1ee5 Th\u1ea5t B\u1ea1i",
            "no_active_quest" => "Ch\u01b0a c\u00f3 nhi\u1ec7m v\u1ee5",
            "find_npc_for_quest" => "H\u00e3y t\u00ecm NPC \u0111\u1ec3 nh\u1eadn nhi\u1ec7m v\u1ee5",
            "achievement_rewards" => "Ph\u1ea7n Th\u01b0\u1edfng!",
            "quest_completed_message" => "B\u1ea1n \u0111\u00e3 ho\u00e0n th\u00e0nh nhi\u1ec7m v\u1ee5!",
            "reward" => "Ph\u1ea7n Th\u01b0\u1edfng",
            "received" => "\u0110\u00e3 Nh\u1eadn",
            "completed" => "Ho\u00e0n Th\u00e0nh",
            "feature_requires_level" => "Y\u00eau c\u1ea7u c\u1ea5p {0}",
            "hero" => "Anh H\u00f9ng",
            "empty" => "Tr\u1ed1ng",
            "button" => "N\u00fat",
            "new_text" => "Ch\u1eef M\u1edbi",
            "dialog" => "H\u1ed9i Tho\u1ea1i",
            "boss" => "Boss",
            "cheat" => "Gian L\u1eadn",
            "tap_to_start" => "CH\u1ea0M \u0110\u1ec2 B\u1eaeT \u0110\u1ea6U",
            "not_ready" => "B\u1ea1n ch\u01b0a s\u1eb5n s\u00e0ng",
            "next_wave" => "\u0110\u1ee3t Ti\u1ebfp",
            "xp" => "EXP",
            "lv" => "C\u1ea5p",
            "atk" => "TC",
            "def" => "PT",
            "spd" => "T\u0110",
            "not_enough_gold" => "Kh\u00f4ng \u0111\u1ee7 V\u00e0ng",
            "upgrade_successful" => "N\u00e2ng c\u1ea5p th\u00e0nh c\u00f4ng!",
            "sold_successfully" => "B\u00e1n th\u00e0nh c\u00f4ng",
            "receive" => "Nh\u1eadn",
            "item" => "v\u1eadt ph\u1ea9m",
            "items" => "v\u1eadt ph\u1ea9m",
            "UpgradeItem" => "N\u00e2ng c\u1ea5p trang b\u1ecb",
            "UpgradeItemLevel" => "C\u1ea5p n\u00e2ng trang b\u1ecb",
            "KillEnemies" => "Ti\u00eau di\u1ec7t k\u1ebb \u0111\u1ecbch",
            "name" => "T\u00ean",
            "description" => "M\u00f4 t\u1ea3",
            "tier" => "B\u1eadc",
            "legendary" => "Huy\u1ec1n Tho\u1ea1i",
            "text_hp" => "Ch\u1eef M\u00e1u",
            "speed_x1" => "x1 T\u1ed1c \u0110\u1ed9",
            "speed_x3" => "x3 T\u1ed1c \u0110\u1ed9",
            "AttackSpeedBoost" => "T\u0103ng T\u1ed1c \u0110\u00e1nh",
            "AttackSpeedBoost_des" => "T\u0103ng t\u1ed1c \u0111\u00e1nh.",
            "AttackSpeedBoost_des_template" => "T\u0103ng t\u1ed1c \u0111\u00e1nh th\u00eam {value}.",
            "CritChanceBoost" => "T\u0103ng Ch\u00ed M\u1ea1ng",
            "CritChanceBoost_des" => "T\u0103ng t\u1ec9 l\u1ec7 ch\u00ed m\u1ea1ng.",
            "CritChanceBoost_des_template" => "T\u0103ng t\u1ec9 l\u1ec7 ch\u00ed m\u1ea1ng th\u00eam {value}%.",
            "DamageBoost" => "T\u0103ng S\u00e1t Th\u01b0\u01a1ng",
            "DamageBoost_des" => "T\u0103ng s\u00e1t th\u01b0\u01a1ng t\u1ea5n c\u00f4ng.",
            "DamageBoost_des_template" => "T\u0103ng s\u00e1t th\u01b0\u01a1ng th\u00eam {value}.",
            "DefenseBoost" => "T\u0103ng Ph\u00f2ng Th\u1ee7",
            "DefenseBoost_des" => "T\u0103ng ph\u00f2ng th\u1ee7.",
            "DefenseBoost_des_template" => "T\u0103ng ph\u00f2ng th\u1ee7 th\u00eam {value}.",
            "HealthBoost" => "T\u0103ng M\u00e1u",
            "HealthBoost_des" => "T\u0103ng m\u00e1u t\u1ed1i \u0111a.",
            "HealthBoost_des_template" => "T\u0103ng m\u00e1u t\u1ed1i \u0111a th\u00eam {value}.",
            "SpeedBoost" => "T\u0103ng T\u1ed1c \u0110\u1ed9",
            "SpeedBoost_des" => "T\u0103ng t\u1ed1c \u0111\u1ed9 di chuy\u1ec3n.",
            "SpeedBoost_des_template" => "T\u0103ng t\u1ed1c \u0111\u1ed9 di chuy\u1ec3n th\u00eam {value}.",
            "Dash" => "L\u01b0\u1edbt Nhanh",
            "Dash_des" => "L\u01b0\u1edbt nhanh theo h\u01b0\u1edbng di chuy\u1ec3n.",
            "Dash_des_template" => "L\u01b0\u1edbt nhanh. Mana: {mana}. H\u1ed3i chi\u00eau: {cooldown}s.",
            "LifeDrain" => "H\u00fat M\u00e1u",
            "LifeDrain_des" => "H\u00fat m\u00e1u t\u1eeb k\u1ebb \u0111\u1ecbch.",
            "LifeDrain_des_template" => "H\u00fat {value}% m\u00e1u t\u1eeb s\u00e1t th\u01b0\u01a1ng g\u00e2y ra.",
            "ManaBoost" => "T\u0103ng Mana",
            "ManaBoost_des" => "T\u0103ng mana t\u1ed1i \u0111a.",
            "ManaBoost_des_template" => "T\u0103ng mana t\u1ed1i \u0111a th\u00eam {value}.",
            "Invincible" => "B\u1ea5t T\u1eed",
            "Invincible_des" => "Tr\u1edf n\u00ean b\u1ea5t t\u1eed trong th\u1eddi gian ng\u1eafn.",
            "Invincible_des_template" => "B\u1ea5t t\u1eed trong {duration}s. H\u1ed3i chi\u00eau: {cooldown}s.",
            "Slash" => "Ch\u00e9m",
            "Slash_des" => "Ch\u00e9m k\u1ebb \u0111\u1ecbch ph\u00eda tr\u01b0\u1edbc.",
            "Slash_des_template" => "G\u00e2y {value} s\u00e1t th\u01b0\u01a1ng ch\u00e9m. Mana: {mana}. H\u1ed3i chi\u00eau: {cooldown}s.",
            "ShurikenThrow" => "N\u00e9m Phi Ti\u00eau",
            "ShurikenThrow_des" => "N\u00e9m phi ti\u00eau v\u00e0o k\u1ebb \u0111\u1ecbch.",
            "ShurikenThrow_des_template" => "N\u00e9m phi ti\u00eau g\u00e2y {value} s\u00e1t th\u01b0\u01a1ng. Mana: {mana}. H\u1ed3i chi\u00eau: {cooldown}s.",
            "choose_passive_skill" => "Ch\u1ecdn 1 k\u1ef9 n\u0103ng b\u1ecb \u0111\u1ed9ng m\u1edbi",
            "skill_applied_immediately" => "K\u1ef9 n\u0103ng s\u1ebd \u0111\u01b0\u1ee3c \u00e1p d\u1ee5ng ngay",
            "choose_skill_first" => "H\u00e3y ch\u1ecdn 1 k\u1ef9 n\u0103ng tr\u01b0\u1edbc",
            "quest.NV1.name" => "Nh\u1eefng Slime \u0110\u1ea7u Ti\u00ean",
            "quest.NV1.description" => "Ti\u00eau di\u1ec7t 6 Slime Nh\u1ecf",
            "quest.NV1.objective.1" => "Ti\u00eau di\u1ec7t 6 Slime Nh\u1ecf",
            "quest.NV2.name" => "V\u01b0\u1ee3t \u1ea2i \u0110\u1ea7u",
            "quest.NV2.description" => "Ho\u00e0n th\u00e0nh M\u00e0n 1",
            "quest.NV2.objective.1" => "Ho\u00e0n th\u00e0nh M\u00e0n 1",
            "quest.NV3.name" => "R\u00e8n Luy\u1ec7n C\u01a1 B\u1ea3n",
            "quest.NV3.description" => "N\u00e2ng ch\u1ec9 s\u1ed1 b\u1ea5t k\u1ef3 3 l\u1ea7n",
            "quest.NV3.objective.1" => "N\u00e2ng ch\u1ec9 s\u1ed1 b\u1ea5t k\u1ef3 3 l\u1ea7n",
            "quest.NV4.name" => "Trang B\u1ecb \u0110\u1ea7u Ti\u00ean",
            "quest.NV4.description" => "Mua 1 m\u00f3n trang b\u1ecb b\u1ea5t k\u1ef3",
            "quest.NV4.objective.1" => "Mua 1 m\u00f3n trang b\u1ecb b\u1ea5t k\u1ef3",
            "quest.NV5.name" => "M\u1eb7c L\u00ean S\u1ee9c M\u1ea1nh",
            "quest.NV5.description" => "Trang b\u1ecb 1 m\u00f3n \u0111\u1ed3",
            "quest.NV5.objective.1" => "Trang b\u1ecb 1 m\u00f3n \u0111\u1ed3",
            "quest.NV6.name" => "Tr\u1ea1i Goblin",
            "quest.NV6.description" => "Ti\u00eau di\u1ec7t 8 Goblin",
            "quest.NV6.objective.1" => "Ti\u00eau di\u1ec7t 8 Goblin",
            "quest.NV7.name" => "V\u01b0\u01a1ng Mi\u1ec7n Slime",
            "quest.NV7.description" => "Ti\u00eau di\u1ec7t King Slime",
            "quest.NV7.objective.1" => "Ti\u00eau di\u1ec7t King Slime",
            "quest.NV8.name" => "Th\u1eed V\u1eadn May",
            "quest.NV8.description" => "Quay Gacha 1 l\u1ea7n",
            "quest.NV8.objective.1" => "Quay Gacha 1 l\u1ea7n",
            "quest.NV9.name" => "B\u1ed9 S\u01b0u T\u1eadp Nh\u1ecf",
            "quest.NV9.description" => "S\u1edf h\u1eefu 3 trang b\u1ecb kh\u00e1c nhau",
            "quest.NV9.objective.1" => "S\u1edf h\u1eefu 3 trang b\u1ecb kh\u00e1c nhau",
            "quest.NV10.name" => "M\u00e0i S\u1eafc Trang B\u1ecb",
            "quest.NV10.description" => "N\u00e2ng c\u1ea5p trang b\u1ecb 2 l\u1ea7n",
            "quest.NV10.objective.1" => "N\u00e2ng c\u1ea5p trang b\u1ecb 2 l\u1ea7n",
            "quest.NV11.name" => "X\u01b0\u01a1ng Tr\u1eafng Ph\u00eda Tr\u01b0\u1edbc",
            "quest.NV11.description" => "Ti\u00eau di\u1ec7t 5 Skeleton v\u00e0 5 Skeleton 1",
            "quest.NV11.objective.1" => "Ti\u00eau di\u1ec7t 5 Skeleton",
            "quest.NV11.objective.2" => "Ti\u00eau di\u1ec7t 5 Skeleton 1",
            "quest.NV12.name" => "Vua Goblin",
            "quest.NV12.description" => "Ti\u00eau di\u1ec7t Goblin King Boss",
            "quest.NV12.objective.1" => "Ti\u00eau di\u1ec7t Goblin King Boss",
            "quest.NV13.name" => "Leo M\u00e0n",
            "quest.NV13.description" => "Ho\u00e0n th\u00e0nh M\u00e0n 3",
            "quest.NV13.objective.1" => "Ho\u00e0n th\u00e0nh M\u00e0n 3",
            "quest.NV14.name" => "R\u1eafc R\u1ed1i Rogue",
            "quest.NV14.description" => "Ti\u00eau di\u1ec7t 10 Rogue",
            "quest.NV14.objective.1" => "Ti\u00eau di\u1ec7t 10 Rogue",
            "quest.NV15.name" => "L\u01b0\u1ee1i Ki\u1ebfm T\u1ed1t H\u01a1n",
            "quest.NV15.description" => "N\u00e2ng 1 trang b\u1ecb l\u00ean +3",
            "quest.NV15.objective.1" => "N\u00e2ng 1 trang b\u1ecb l\u00ean +3",
            "quest.NV16.name" => "H\u00e0ng R\u00ecu",
            "quest.NV16.description" => "Ti\u00eau di\u1ec7t 10 Axeman",
            "quest.NV16.objective.1" => "Ti\u00eau di\u1ec7t 10 Axeman",
            "quest.NV17.name" => "\u0110\u1ed9i Tr\u01b0\u1edfng X\u01b0\u01a1ng",
            "quest.NV17.description" => "Ti\u00eau di\u1ec7t Skeleton Boss",
            "quest.NV17.objective.1" => "Ti\u00eau di\u1ec7t Skeleton Boss",
            "quest.NV18.name" => "S\u0103n Gravok",
            "quest.NV18.description" => "Ti\u00eau di\u1ec7t 12 Gravok",
            "quest.NV18.objective.1" => "Ti\u00eau di\u1ec7t 12 Gravok",
            "quest.NV19.name" => "B\u00e0i H\u1ecdc Warlock",
            "quest.NV19.description" => "Ti\u00eau di\u1ec7t 12 Warlock",
            "quest.NV19.objective.1" => "Ti\u00eau di\u1ec7t 12 Warlock",
            "quest.NV20.name" => "T\u0103ng \u0110\u00e0 S\u1ee9c M\u1ea1nh",
            "quest.NV20.description" => "N\u00e2ng ch\u1ec9 s\u1ed1 b\u1ea5t k\u1ef3 th\u00eam 8 l\u1ea7n",
            "quest.NV20.objective.1" => "N\u00e2ng ch\u1ec9 s\u1ed1 b\u1ea5t k\u1ef3 th\u00eam 8 l\u1ea7n",
            "quest.NV21.name" => "C\u01a1 H\u1ed9i Hi\u1ebfm",
            "quest.NV21.description" => "Quay Gacha 5 l\u1ea7n",
            "quest.NV21.objective.1" => "Quay Gacha 5 l\u1ea7n",
            "quest.NV22.name" => "T\u00fai \u0110\u1ed3 \u0110\u1ea7y H\u01a1n",
            "quest.NV22.description" => "S\u1edf h\u1eefu 5 trang b\u1ecb kh\u00e1c nhau",
            "quest.NV22.objective.1" => "S\u1edf h\u1eefu 5 trang b\u1ecb kh\u00e1c nhau",
            "quest.NV23.name" => "Cu\u1ed9c S\u0103n L\u1eb7ng L\u1ebd",
            "quest.NV23.description" => "Ti\u00eau di\u1ec7t 12 Silentmaw",
            "quest.NV23.objective.1" => "Ti\u00eau di\u1ec7t 12 Silentmaw",
            "quest.NV24.name" => "Th\u1eed Th\u00e1ch Elfblade",
            "quest.NV24.description" => "Ti\u00eau di\u1ec7t 12 Elfblade",
            "quest.NV24.objective.1" => "Ti\u00eau di\u1ec7t 12 Elfblade",
            "quest.NV25.name" => "\u0110\u1ed1i M\u1eb7t \u0110ao Ph\u1ee7",
            "quest.NV25.description" => "Ti\u00eau di\u1ec7t Undead Executioner",
            "quest.NV25.objective.1" => "Ti\u00eau di\u1ec7t Undead Executioner",
            "quest.NV26.name" => "Con \u0110\u01b0\u1eddng N\u1eb7ng N\u1ec1",
            "quest.NV26.description" => "Ti\u00eau di\u1ec7t 8 Ravageron v\u00e0 8 Fangrider",
            "quest.NV26.objective.1" => "Ti\u00eau di\u1ec7t 8 Ravageron",
            "quest.NV26.objective.2" => "Ti\u00eau di\u1ec7t 8 Fangrider",
            "quest.NV27.name" => "C\u1ed7 M\u00e1y \u0110\u00e1",
            "quest.NV27.description" => "Ti\u00eau di\u1ec7t Mecha Stone Golem",
            "quest.NV27.objective.1" => "Ti\u00eau di\u1ec7t Mecha Stone Golem",
            "quest.NV28.name" => "Ki\u1ec3m Tra Qu\u00e1i V\u1eadt",
            "quest.NV28.description" => "Ti\u00eau di\u1ec7t Boss Creature",
            "quest.NV28.objective.1" => "Ti\u00eau di\u1ec7t Boss Creature",
            "quest.NV29.name" => "S\u1eb5n S\u00e0ng V\u1ec1 Sau",
            "quest.NV29.description" => "Ho\u00e0n th\u00e0nh M\u00e0n 8",
            "quest.NV29.objective.1" => "Ho\u00e0n th\u00e0nh M\u00e0n 8",
            _ => string.Empty
        };
    }
}
