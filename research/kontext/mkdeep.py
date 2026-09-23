import json, io

rows = json.load(io.open('research/kontext/schemas/all.json', encoding='utf-8'))
GT = {'esriGeometryPoint': 'bod', 'esriGeometryPolyline': 'linie', 'esriGeometryPolygon': 'plocha'}

GROUPS = [
 ('ŠKOLSTVÍ A VZDĚLÁVÁNÍ', ['Dojížďka','Přijímací','Počet žáků','Počet student','Počet ubytovan','Školy a školská','Právní subjekty vykon','Absolventi vysok','Studenti vysok','Základní umělecké','Domy dětí']),
 ('DOPRAVA A SILNICE', ['Autobusov','Železnič','zaručených','zimní údržby','Stav povrchu','Opravy silniční','Letiště','Informační centra autobus','sportovní létající']),
 ('ZDRAVOTNICTVÍ', ['Nemocnice','Praktičtí','Zubní','Ambulantní gyn','Lékařská pohotov','Následná lůžkov','Odborné léčebné','Lázně']),
 ('IZS A BEZPEČNOST', ['Služebny Policie','hasičského','Výjezdové základny']),
 ('SOCIÁLNÍ OBLAST', ['Poskytovatelé sociál','Veřejné sbírky']),
 ('ŽIVOTNÍ PROSTŘEDÍ A RIZIKA', ['Záplavov','Aktivní zóna','Přírodní pam','Přírodní park','Přírodní rezerv','Přírodní zajím','ÚSES','Vymezení krajin']),
 ('ÚZEMNÍ PLÁNOVÁNÍ, DOTACE A ROZVOJ', ['Schválené','Hospodářsky a sociáln','kanalizaci','rozvojov','specifických oblast','Územní rezervy','Významné oborové','Plochy a koridory','Plochy jejichž']),
 ('EKONOMIKA A PRÁCE', ['Inovativní firmy','Inovační infra','Investiční pobídky','Průmyslové zóny','Klastry','Právnické osoby v klast']),
 ('OBCE A SPRÁVA', ['Hranice','Adresář obcí','Administrativní člen','Organizace Královéhrad','Místní akční','Dobrovolné svazky','Turistické regiony']),
 ('CESTOVNÍ RUCH, KULTURA A PAMÁTKY', ['Hrady','Zámky','Rozhledny','Muzea','Kina','Divadla','Cyklovýlety','Letní koupán','Lanovky','ZOO','Pivovary','Regionální potraviny','Řemeslné','Infocentra','Solné','Golf','Jezdectv','Rybaře','Botanick','Židovské','Církevní','Technické pam','Architektonické','Válečné hroby','Pevnosti','Zábavní','Knihovny','Hudební kluby','Kulturní dom','Kulturní akce','Kulturní památky','Národní kulturní','Ostatní','Památkov','Letecké sporty']),
]
def grp(n):
    for g, kws in GROUPS:
        if any(k.lower() in n.lower() for k in kws): return g
    return 'METADATA PORTÁLU (bez využitelných dat)'

SKIP_FLD = {'wkt','x','y','dp_id','id','nazev_vusc','kod_vusc','OBJECTID','FID'}

out = io.open('research/kontext/KONTEXT_datove_sady.md', 'w', encoding='utf-8')
w = out.write
w('# Kontext: Otevřená data Královéhradeckého kraje — hloubkový katalog\n\n')
w('Zdroj: `https://open.datakhk.cz/katalog.jsonld` — **165 datových sad**.\n')
w('U všech 131 geodatových sad ověřeno proti živému ArcGIS API: **počet záznamů, všechna pole '
  'a reálné hodnoty číselníků**. Staženo 3. 9. 2026.\n\n')
w('## Jak na data\n\n```\n<api_url>/query?where=1%3D1&outFields=*&f=geojson&resultRecordCount=2000&resultOffset=0\n```\n\n')
w('- `&returnGeometry=false` — rychlejší, když stačí atributy\n')
w('- `&returnDistinctValues=true&outFields=pole` — číselník hodnot\n')
w('- `&groupByFieldsForStatistics=pole&outStatistics=[{"statisticType":"sum",...}]` — agregace na serveru\n')
w('- Bodové sady mají souřadnice přímo v atributech jako `x` (lon) a `y` (lat)\n')
w('- Společná pole skoro všude: `nazev_obce`/`kod_obce`, `nazev_orp`/`kod_orp`, `nazev_okresu`/`kod_okresu`\n')
w('  → **`kod_obce` je spojovací klíč mezi sadami i s hranicemi obcí (448 polygonů)**\n\n')
w('---\n')

buckets = {}
for r in rows: buckets.setdefault(grp(r['nazev']), []).append(r)
order = [g for g, _ in GROUPS] + ['METADATA PORTÁLU (bez využitelných dat)']

for g in order:
    rs = buckets.get(g, [])
    if not rs: continue
    if g.startswith('METADATA'):
        w('\n## %s — %d sad\n\n' % (g, len(rs)))
        w('Stránky portálu, nikoli data: ')
        w(', '.join(r['nazev'] for r in rs) + '\n')
        continue
    w('\n## %s — %d sad\n' % (g, len(rs)))
    for r in sorted(rs, key=lambda x: -(x['pocet'] if isinstance(x['pocet'], int) else -1)):
        c, gt = r['pocet'], GT.get(r['geometrie'], '')
        w('\n### %s\n\n' % r['nazev'])
        w('`n=%s` · %s · aktualizace: %s\n\n' % (c, gt or 'tabulka', r['periodicita'].lower() or '—'))
        w('%s\n\n' % r['popis'].replace('\n', ' ').strip()[:420])
        flds = [f for f in r.get('pole', []) if f['name'] not in SKIP_FLD]
        if flds:
            w('**Pole:** ')
            w(', '.join('`%s`' % f['name'] for f in flds) + '\n\n')
        cis = {k: v for k, v in (r.get('ciselniky') or {}).items()
               if k not in ('nazev_okresu', 'nazev_orp', 'typ_cisla_domovniho', 'pozn', 'nazev_kraje', 'kod_kraje', 'Kraj', 'Zdroj', 'Jev')
               and not k.startswith(('telefon', 'email', 'fax', 'mobil', 'fotografie', 'www'))}
        if cis:
            w('**Hodnoty:**\n\n')
            for k, v in cis.items():
                w('- `%s`: %s\n' % (k, ' · '.join(str(x).strip()[:44] for x in v[:12])))
            w('\n')
        if r.get('api'):
            w('API: `%s`\n\n' % r['api'])
        w('Portál: %s\n' % r['portal'])

out.close()
print('OK')
