import json, io, urllib.request, concurrent.futures as cf
rows = json.load(io.open('research/kontext/datasets.json', encoding='utf-8'))
def get(u):
    return json.load(urllib.request.urlopen(urllib.request.Request(u, headers={'User-Agent':'Mozilla/5.0'}), timeout=60))
def one(r):
    fs = r['urls'].get('FEATURESERVER')
    if not fs: return r, None, None
    try:
        c = get(fs + '/query?where=1%3D1&returnCountOnly=true&f=json').get('count')
        m = get(fs + '?f=json')
        return r, c, m.get('geometryType')
    except Exception as e:
        return r, 'ERR', str(e)[:40]
out = []
with cf.ThreadPoolExecutor(max_workers=8) as ex:
    for r, c, g in ex.map(one, rows):
        r['pocet_zaznamu'] = c; r['geometrie'] = g
        out.append(r)
json.dump(out, io.open('research/kontext/datasets.json','w',encoding='utf-8'), ensure_ascii=False, indent=1)
print('done', sum(1 for r in out if isinstance(r['pocet_zaznamu'], int)))
