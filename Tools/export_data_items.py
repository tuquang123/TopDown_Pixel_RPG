#!/usr/bin/env python3
"""Export Unity ItemData assets to an Excel-compatible CSV."""
from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DATA_DIR = ROOT / "Assets" / "Data_Game" / "Item" / "DataItem"
OUT_DIR = ROOT / "Exports"
CSV_PATH = OUT_DIR / "DataItem_AllItems.csv"

ITEM_TYPES = ["Weapon", "Clother", "Consumable", "Helmet", "Boots", "Horse", "SpecialArmor", "Cloak", "Hair"]
WEAPON_CATEGORIES = ["Melee", "Ranged", "HeavyMelee", "Bow"]
ITEM_TIERS = ["Common", "Uncommon", "Rare", "Epic", "Legendary", "Mythic"]

STAT_FIELDS = ["attack", "defense", "health", "mana", "critChance", "attackSpeed", "lifeSteal", "speed"]
SCALAR_FIELDS = [
    "itemID", "itemName", "weaponCategory", "itemType", "tier", "price", "description",
    "baseUpgradeCost", "restoresHealth", "healthRestoreAmount", "restoresMana", "manaRestoreAmount",
    "percentageBased", "lockAttack", "lockDefense", "lockHealth", "lockMana", "lockCrit",
    "lockAttackSpeed", "lockLifeSteal", "lockSpeed",
]
REF_FIELDS = ["icon", "iconLeft", "iconRight", "horseData", "tierConfig", "itemDataBase", "prefab"]
COLOR_FIELDS = ["color.r", "color.g", "color.b", "color.a"]
HEADERS = ["assetFile", "assetName"] + SCALAR_FIELDS + [f"{s}.{p}" for s in STAT_FIELDS for p in ("flat", "percent")] + COLOR_FIELDS + REF_FIELDS


def parse_asset(path: Path) -> dict[str, object]:
    data: dict[str, object] = {"assetFile": str(path.relative_to(ROOT)).replace("\\", "/")}
    lines = path.read_text(encoding="utf-8-sig").splitlines()
    i = 0
    while i < len(lines):
        line = lines[i]
        if not line.startswith("  ") or line.startswith("    ") or ":" not in line:
            i += 1
            continue
        key, raw = line[2:].split(":", 1)
        value = raw.strip()
        if key == "m_Name":
            data["assetName"] = value
        elif key in SCALAR_FIELDS:
            data[key] = value
        elif key in REF_FIELDS:
            data[key] = value
        elif key == "color":
            for comp in ("r", "g", "b", "a"):
                m = re.search(rf"{comp}: ([^,}}]+)", value)
                if m:
                    data[f"color.{comp}"] = m.group(1).strip()
        elif key in STAT_FIELDS:
            if i + 2 < len(lines):
                for sub in ("flat", "percent"):
                    m = re.match(rf"    {sub}:\s*(.*)$", lines[i + (1 if sub == "flat" else 2)])
                    if m:
                        data[f"{key}.{sub}"] = m.group(1).strip()
            i += 2
        i += 1

    data["itemType"] = enum_name(data.get("itemType"), ITEM_TYPES)
    data["weaponCategory"] = enum_name(data.get("weaponCategory"), WEAPON_CATEGORIES)
    data["tier"] = enum_name(data.get("tier"), ITEM_TIERS)
    return data


def enum_name(value: object, names: list[str]) -> str:
    text = "" if value is None else str(value)
    try:
        idx = int(text)
    except ValueError:
        return text
    return names[idx] if 0 <= idx < len(names) else text


def write_csv(rows: list[dict[str, object]]) -> None:
    with CSV_PATH.open("w", encoding="utf-8-sig", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=HEADERS, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)



def main() -> None:
    OUT_DIR.mkdir(exist_ok=True)
    rows = [parse_asset(path) for path in sorted(DATA_DIR.glob("*.asset"))]
    rows.sort(key=lambda r: (str(r.get("itemType", "")), str(r.get("tier", "")), str(r.get("itemName", ""))))
    write_csv(rows)
    print(f"Exported {len(rows)} DataItem assets to {CSV_PATH.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
