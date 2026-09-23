import json, io
rows = json.load(io.open('research/kontext/datasets.json', encoding='utf-8'))

TH = {'EDUC':'Vzdělávání/kultura','GOVE':'Veřejná správa','REGI':'Regiony','TRAN':'Doprava','ENVI':'Životní prostředí',
      'ECON':'Ekonomika','HEAL':'Zdravotnictví','JUST':'Bezpečnost/justice','SOCI':'Sociální','TECH':'Technologie',
      'OP_DATPRO':'Metadata portálu'}

# manual thematic grouping by name keywords -> more useful than EU themes
GROUPS = [
 ('ŠKOLSTVÍ A VZDĚLÁVÁNÍ', ['Dojížďka','Přijímací','Počet žáků','Počet student','Počet ubytovan','Školy a školská','Právní subjekty vykon','Absolventi vysok','Studenti vysok','Základní umělecké','Domy dětí']),
 ('DOPRAVA A SILNICE', ['Autobusov','Železnič','zaručených','zimní údržby','Stav povrchu','Opravy silniční','Letiště','Informační centra autobus','sportovní létající']),
 ('ZDRAVOTNICTVÍ', ['Nemocnice','Praktičtí','Zubní','Ambulantní gyn','Lékařská pohotov','Následná lůžkov','Odborné léčebné','Lázně']),
 ('IZS A BEZPEČNOST', ['Služebny Policie','hasičského','Výjezdové základny']),
 ('SOCIÁLNÍ OBLAST', ['Poskytovatelé sociál','Veřejné sbírky']),
 ('ŽIVOTNÍ PROSTŘEDÍ A RIZIKA', ['Záplavov','Aktivní zóna','Přírodní pam','Přírodní park','Přírodní rezerv','Přírodní zajím','ÚSES','Vymezení krajin']),
 ('DOTACE A ROZVOJ', ['Schválené','Hospodářsky a sociáln','kanalizaci','rozvojov','specifických oblast','Územní rezervy','Významné oborové','Plochy a koridory','Plochy jejichž']),
 ('EKONOMIKA A PRÁCE', ['Inovativní firmy','Inovační infra','Investiční pobídky','Průmyslové zóny','Klastry','Právnické osoby v klast']),
 ('OBCE A SPRÁVA', ['Hranice','Adresář obcí','Administrativní člen','Organizace Královéhrad','Místní akční','Dobrovolné svazky','Turistické regiony']),
 ('CESTOVNÍ RUCH A KULTURA', ['Hrady','Zámky','Rozhledny','Muzea','Kina','Divadla','Cyklovýlety','Letní koupán','Lanovky','ZOO','Pivovary','Regionální potraviny','Řemeslné','Infocentra','Solné','Golf','Jezdectv','Rybaře','Botanick','Židovské','Církevní','Technické pam','Architektonické','Válečné hroby','Pevnosti','Zábavní','Knihovny','Hudební kluby','Kulturní dom','Kulturní akce','Kulturní památky','Národní kulturní','Ostatní','Památkov','Letecké sporty','Soln']),
]

def grp(n):
    for g, kws in GROUPS:
        if any(k.lower() in n.lower() for k in kws): return g
    return 'METADATA PORTÁLU / OSTATNÍ'

out = io.open('research/kontext/KONTEXT_datove_sady.md','w',encoding='utf-8')
w = out.write
w('# Kontext: Otevřená data Královéhradeckého kraje\n\n')
w('Zdroj: https://open.datakhk.cz/katalog.jsonld — **%d datových sad**, staženo a ověřeno %s.\n\n' % (len(rows),'2026-09-03'))
w('Každá geodatová sada má distribuce: **CSV, GeoJSON, KML, SHP(ZIP), JSON** + živý **ArcGIS FeatureServer** (REST API s `/query?where=1=1&outFields=*&f=geojson`).\n\n')
w('Legenda: `n=` počet záznamů, `[bod]/[linie]/[plocha]` geometrie, `–` netabulková (HTML stránka portálu).\n\n')

buckets = {}
for r in rows: buckets.setdefault(grp(r['nazev']), []).append(r)
order = [g for g,_ in GROUPS] + ['METADATA PORTÁLU / OSTATNÍ']
GT = {'esriGeometryPoint':'bod','esriGeometryPolyline':'linie','esriGeometryPolygon':'plocha'}
for g in order:
    rs = buckets.get(g, [])
    if not rs: continue
    w('\n## %s (%d sad)\n\n' % (g, len(rs)))
    for r in sorted(rs, key=lambda x: -(x['pocet_zaznamu'] if isinstance(x['pocet_zaznamu'],int) else -1)):
        c = r['pocet_zaznamu']; gt = GT.get(r['geometrie'],'')
        head = '**%s** — ' % r['nazev']
        head += ('n=%s [%s]' % (c,gt)) if isinstance(c,int) else '–'
        w('- %s\n' % head)
        w('  - %s\n' % r['popis'].replace('\n',' ')[:300])
        if r['urls'].get('FEATURESERVER'):
            w('  - API: `%s`\n' % r['urls']['FEATURESERVER'])
        w('  - Portál: %s\n' % r['portal'])
out.close()
print('OK')
