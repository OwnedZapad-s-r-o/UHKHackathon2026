import json, io, os, urllib.request, urllib.parse, concurrent.futures as cf, time

rows = json.load(io.open('research/kontext/datasets.json', encoding='utf-8'))
os.makedirs('research/kontext/schemas', exist_ok=True)

def get(url, tries=3):
    last = None
    for _ in range(tries):
        try:
            req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
            return json.load(urllib.request.urlopen(req, timeout=90))
        except Exception as e:
            last = e; time.sleep(1.5)
    raise last

def probe(r):
    out = {'id': r['id'], 'nazev': r['nazev'], 'popis': r['popis'],
           'temata': r['temata'], 'periodicita': r['periodicita'],
           'pocet': r['pocet_zaznamu'], 'geometrie': r['geometrie'],
           'portal': r['portal'], 'api': r['urls'].get('FEATURESERVER'),
           'csv': r['urls'].get('CSV')}
    fs = r['urls'].get('FEATURESERVER')
    if not fs:
        out['typ'] = 'stranka_portalu'
        return out
    try:
        meta = get(fs + '?f=json')
        flds = [f for f in meta.get('fields', [])
                if f['name'] not in ('OBJECTID', 'FID', 'Shape__Area', 'Shape__Length')]
        out['pole'] = [{'name': f['name'],
                        'typ': f['type'].replace('esriFieldType', ''),
                        'alias': f.get('alias', '')} for f in flds]
        # sample rows
        s = get(fs + '/query?where=1%3D1&outFields=*&resultRecordCount=3&returnGeometry=false&f=json')
        out['vzorek'] = [a.get('attributes') for a in s.get('features', [])]
        # distinct values for low-cardinality string fields -> reveals what the data actually contains
        cats = {}
        names = [f['name'] for f in flds if f['type'] == 'esriFieldTypeString']
        for n in names:
            if any(k in n.lower() for k in ('nazev_vusc','kod_vusc','kod_obce','kod_okres','kod_orp',
                                            'wkt','www','ulice','cislo','psc','dp_id','ico','dic',
                                            'nazev_obce','nazev_ulice','id')):
                continue
            try:
                q = get(fs + '/query?where=1%3D1&outFields=' + urllib.parse.quote(n) +
                        '&returnDistinctValues=true&returnGeometry=false&resultRecordCount=25&f=json')
                vals = [f['attributes'].get(n) for f in q.get('features', [])]
                vals = [v for v in vals if v not in (None, '', ' ')]
                if 0 < len(vals) <= 20:
                    cats[n] = vals
            except Exception:
                pass
        if cats: out['ciselniky'] = cats
    except Exception as e:
        out['chyba'] = str(e)[:120]
    return out

res = []
with cf.ThreadPoolExecutor(max_workers=5) as ex:
    for i, o in enumerate(ex.map(probe, rows), 1):
        res.append(o)
        if i % 20 == 0: print('  ...', i, flush=True)

res.sort(key=lambda x: x['nazev'])
json.dump(res, io.open('research/kontext/schemas/all.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
ok = sum(1 for r in res if r.get('pole'))
print('HOTOVO: %d se schematem, %d stranek, %d chyb' %
      (ok, sum(1 for r in res if r.get('typ')), sum(1 for r in res if r.get('chyba'))))
