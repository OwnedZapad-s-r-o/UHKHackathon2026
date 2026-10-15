import json, os, sys, urllib.request, concurrent.futures as cf

urls = open('research/kontext/urls.txt', encoding='utf-8').read().split()
os.makedirs('research/kontext/raw', exist_ok=True)

def fetch(u):
    fn = 'research/kontext/raw/' + u.rsplit('/',1)[-1]
    if os.path.exists(fn) and os.path.getsize(fn) > 10:
        return 'cached'
    try:
        req = urllib.request.Request(u, headers={'User-Agent':'Mozilla/5.0','Accept':'application/ld+json,application/json'})
        with urllib.request.urlopen(req, timeout=45) as r:
            data = r.read()
        open(fn,'wb').write(data)
        return 'ok'
    except Exception as e:
        return 'ERR %s %s' % (u, e)

with cf.ThreadPoolExecutor(max_workers=8) as ex:
    res = list(ex.map(fetch, urls))
from collections import Counter
c = Counter(x if x in ('ok','cached') else 'err' for x in res)
print(dict(c))
for x in res:
    if x.startswith('ERR'): print(x)
