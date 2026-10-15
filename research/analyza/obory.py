import json, io, math
from collections import defaultdict
D=json.load(io.open('research/analyza/data/all.json',encoding='utf-8'))
cent={}
for f in D['hran']['features']:
    kod=f['properties']['kod_obce']; g=f['geometry']; cs=g['coordinates']
    rings=cs if g['type']=='Polygon' else [r for p in cs for r in p]
    pts=[p for r in rings for p in r]
    cent[kod]=(sum(p[0] for p in pts)/len(pts),sum(p[1] for p in pts)/len(pts))
nazev_obce={f['properties']['kod_obce']:f['properties']['nazev_obce'] for f in D['hran']['features']}
skoly={}
for s in D['skoly']:
    o=skoly.setdefault(s['izo'],{'nazev':s['nazev_skoly'],'x':s['x'],'y':s['y'],'obec':s['nazev_obce'],'z':0}); o['z']+=s['z'] or 0
obory=defaultdict(set)
for r in D['obory']:
    if r['obor_nazev']: obory[r['obor_nazev']].add(r['izo'])
def hav(a,b):
    p=math.pi/180
    return 2*6371*math.asin(math.sqrt(math.sin((b[1]-a[1])*p/2)**2+math.cos(a[1]*p)*math.cos(b[1]*p)*math.sin((b[0]-a[0])*p/2)**2))

print('OBORY V KRAJI: %d' % len(obory))
rare=sorted(obory.items(), key=lambda kv: len(kv[1]))
print('\n--- OBORY, KTERÉ UČÍ JEN JEDNA ŠKOLA V CELÉM KRAJI (%d z %d) ---'
      % (sum(1 for k,v in obory.items() if len(v)==1), len(obory)))
for k,v in rare[:14]:
    if len(v)==1:
        izo=list(v)[0]
        print('  %-44s  jen %s' % (k[:44], skoly[izo]['obec']))

# oborova pous: pro kazdy obor prumerna a maximalni vzdalenost obce k nejblizsi skole s tim oborem
res=[]
for ob,izos in obory.items():
    pts=[(skoly[i]['x'],skoly[i]['y']) for i in izos if skoly.get(i) and skoly[i]['x']]
    if not pts: continue
    ds=[min(hav(c,p) for p in pts) for c in cent.values()]
    res.append((sum(ds)/len(ds), max(ds), ob, len(izos)))
res.sort(reverse=True)
print('\n--- NEJHŮŘE DOSTUPNÉ OBORY (průměr přes všech 448 obcí) ---')
print('  %-42s %6s %6s %s' % ('obor','prům.','max','škol'))
for a,m,ob,n in res[:12]: print('  %-42s %5.1f  %5.1f  %d' % (ob[:42],a,m,n))
print('\n--- NEJLÉPE DOSTUPNÉ ---')
for a,m,ob,n in res[-5:]: print('  %-42s %5.1f  %5.1f  %d' % (ob[:42],a,m,n))

# nejhur obsluhovana obec: prumer pres vsechny obory
per=[]
for kod,c in cent.items():
    ds=[]
    for ob,izos in obory.items():
        pts=[(skoly[i]['x'],skoly[i]['y']) for i in izos if skoly.get(i) and skoly[i]['x']]
        if pts: ds.append(min(hav(c,p) for p in pts))
    per.append((sum(ds)/len(ds), nazev_obce[kod], kod))
per.sort(reverse=True)
print('\n--- OBCE NEJDÁL OD NABÍDKY OBORŮ (průměr přes 87 oborů) ---')
for d,n,k in per[:8]: print('  %-30s %5.1f km' % (n[:30],d))
print('  ...')
for d,n,k in per[-3:]: print('  %-30s %5.1f km' % (n[:30],d))
