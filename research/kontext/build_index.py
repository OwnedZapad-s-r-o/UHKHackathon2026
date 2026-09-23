import json, io, os, glob
from collections import Counter

def L(x):
    if isinstance(x, dict):
        v = x.get('cs') or x.get('en') or ''
        return v if isinstance(v, str) else (', '.join(v) if isinstance(v, list) else str(v))
    if isinstance(x, list): return ', '.join(L(i) for i in x)
    return x or ''

rows = []
for fn in sorted(glob.glob('research/kontext/raw/*.jsonld')):
    d = json.load(io.open(fn, encoding='utf-8'))
    kw = d.get('klíčové_slovo', {})
    kws = kw.get('cs', []) if isinstance(kw, dict) else []
    dist = d.get('distribuce', []) or []
    fmts, urls = [], {}
    for di in dist:
        f = (di.get('formát') or '').rsplit('/', 1)[-1]
        if f: fmts.append(f)
        nm = L(di.get('název'))
        u = di.get('soubor_ke_stažení') or di.get('přístupové_url') or ''
        if f and f not in urls: urls[f] = u
        if nm == 'ArcGIS GeoService': urls['FEATURESERVER'] = u
    themes = [t.rsplit('/', 1)[-1] for t in (d.get('téma') or []) if isinstance(t, str)]
    rows.append({
        'id': os.path.basename(fn).replace('.jsonld', ''),
        'nazev': L(d.get('název')),
        'popis': L(d.get('popis')),
        'klicova_slova': kws,
        'temata': themes,
        'periodicita': (d.get('periodicita_aktualizace') or '').rsplit('/', 1)[-1],
        'formaty': sorted(set(fmts)),
        'urls': urls,
        'portal': 'https://www.datakhk.cz/datasets/' + os.path.basename(fn).replace('.jsonld', ''),
    })

rows.sort(key=lambda r: r['nazev'])
json.dump(rows, io.open('research/kontext/datasets.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=1)

print('DATASETS:', len(rows))
print('\nTHEMES:', Counter(t for r in rows for t in r['temata']).most_common())
print('\nFORMATS:', Counter(f for r in rows for f in r['formaty']).most_common())
print('\nPERIODICITA:', Counter(r['periodicita'] for r in rows).most_common())
