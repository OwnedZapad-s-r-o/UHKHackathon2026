import json, io, urllib.request, urllib.parse, concurrent.futures as cf, sys

rows = json.load(io.open('research/kontext/datasets.json', encoding='utf-8'))
targets = sys.argv[1:]

def get(url):
    req = urllib.request.Request(url, headers={'User-Agent':'Mozilla/5.0'})
    return json.load(urllib.request.urlopen(req, timeout=60))

def probe(r):
    fs = r['urls'].get('FEATURESERVER')
    if not fs: return r['nazev'], None
    try:
        meta = get(fs + '?f=json')
        cnt = get(fs + '/query?where=1%3D1&returnCountOnly=true&f=json')
        samp = get(fs + '/query?where=1%3D1&outFields=*&resultRecordCount=2&returnGeometry=false&f=json')
        return r['nazev'], {
            'count': cnt.get('count'),
            'geom': meta.get('geometryType'),
            'fields': [(f['name'], f['type'].replace('esriFieldType',''), f.get('alias','')) for f in meta.get('fields',[])],
            'sample': [a.get('attributes') for a in samp.get('features',[])],
        }
    except Exception as e:
        return r['nazev'], {'error': str(e)}

sel = [r for r in rows if any(t.lower() in r['nazev'].lower() for t in targets)]
with cf.ThreadPoolExecutor(max_workers=6) as ex:
    for name, res in ex.map(probe, sel):
        print('=' * 70)
        print('###', name)
        if not res: print('  (no featureserver)'); continue
        if 'error' in res: print('  ERR', res['error']); continue
        print('  ROWS=%s  GEOM=%s' % (res['count'], res['geom']))
        print('  FIELDS:', ', '.join('%s:%s' % (n, t) for n, t, a in res['fields']))
        for s in res['sample'][:1]:
            print('  SAMPLE:', json.dumps(s, ensure_ascii=False)[:900])
