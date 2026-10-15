# Kontext pro Hackathon KHK 2026

Vygenerováno 3. 9. 2026. Všech 165 datových sad z katalogu bylo staženo a ověřeno proti živému API.

| Soubor | Co obsahuje |
|---|---|
| **NAPADY.md** | 10 nápadů na aplikaci + šance na TOP 3, obtížnost, rizika, doporučení |
| **KONTEXT_datove_sady.md** | Všech 165 sad: popis, počet záznamů, **všechna pole a hodnoty číselníků**, URL API |
| **KONTEXT_minule_rocniky.md** | ~85 projektů z pěti ročníků + tabulka nasycení témat |
| **KONTEXT_pravidla.md** | Výtah z pravidel: hodnoticí kritéria, harmonogram, tvrdé podmínky |
| `schemas/all.json` | Strojově: schémata, vzorky dat a číselníky všech 131 geodatových sad |
| `datasets.json` | Strojově čitelný index všech sad (pro skripty) |
| `pravidla.txt` | Plný text pravidel z PDF |
| `raw/` | 165 originálních JSON-LD souborů |
| `crawl.py` · `build_index.py` · `countall.py` · `mkcontext.py` | Skripty, kterými to vzniklo |
| `probe.py` | Nástroj: `python research/kontext/probe.py "název sady"` → schéma, počet řádků, vzorek dat |

## Rychlý start

```bash
python research/kontext/probe.py "Dojížďka do středních"
```

## Přehled dat

- **165 sad**, z toho **131 geodat** (CSV + GeoJSON + KML + SHP + živý ArcGIS FeatureServer)
  a 34 metadatových stránek portálu (bez využitelných dat).
- Největší sady: Dojížďka do SŠ (12 013), Stav povrchu silnic (4 888), Autobusové zastávky (4 538),
  Zaručené přípoje (3 515), ÚSES (2 924), Počet žáků dle oborů (1 754), Plán zimní údržby (1 660),
  Školy a školská zařízení (1 626), Válečné hroby (1 423).
- Tematicky: cestovní ruch a kultura 41 · dotace a rozvoj 21 · školství 14 · doprava 12 ·
  životní prostředí 12 · obce a správa 11 · zdravotnictví 9 · ekonomika 6 · IZS 3 · sociální 2.
