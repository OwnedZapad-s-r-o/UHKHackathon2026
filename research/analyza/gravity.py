import json, io, math
import numpy as np

D = json.load(io.open('research/analyza/data/all.json', encoding='utf-8'))

# --- centroidy obci z hranic ---
cent = {}
for f in D['hran']['features']:
    kod = f['properties']['kod_obce']
    g = f['geometry']; cs = g['coordinates']
    rings = cs if g['type'] == 'Polygon' else [r for poly in cs for r in poly]
    pts = [p for r in rings for p in r]
    cent[kod] = (sum(p[0] for p in pts)/len(pts), sum(p[1] for p in pts)/len(pts))

# --- skoly ---
skoly = {}
for s in D['skoly']:
    o = skoly.setdefault(s['izo'], {'izo':s['izo'],'nazev':s['nazev_skoly'],'x':s['x'],'y':s['y'],
                                    'obec':s['nazev_obce'],'orp':s['nazev_orp'],'kod_orp':s['kod_orp'],
                                    'zriz':s['zrizovatel'],'z':0})
    o['z'] += s['z'] or 0
obory = {}
for r in D['obory']:
    obory.setdefault(r['izo'], set()).add(r['obor_nazev'])

obce = {o['kod_obce_bydliste_studenta']: o for o in D['obce']}
orp  = {a['kod_obce']: a for a in D['adm']}

def hav(a, b):
    lon1,lat1 = a; lon2,lat2 = b
    p = math.pi/180
    return 2*6371*math.asin(math.sqrt(
        math.sin((lat2-lat1)*p/2)**2 + math.cos(lat1*p)*math.cos(lat2*p)*math.sin((lon2-lon1)*p/2)**2))

# --- toky jen pro obce KHK, ktere mame na mape ---
rows = []
tot_by_obec = {}
for t in D['toky']:
    k = t['kod_obce_bydliste_studenta']
    if k in cent: tot_by_obec[k] = tot_by_obec.get(k, 0) + (t['z'] or 0)
for t in D['toky']:
    k, izo, z = t['kod_obce_bydliste_studenta'], t['izo'], t['z'] or 0
    s = skoly.get(izo)
    if k not in cent or not s or not s['x'] or z <= 0: continue
    d = hav(cent[k], (s['x'], s['y']))
    if d < 0.5: d = 0.5
    rows.append({'kod':k,'izo':izo,'z':z,'d':d,'M':s['z'],'share':z/tot_by_obec[k]})

print('toků v modelu: %d (obce KHK)' % len(rows))
print('celkem žáků  : %d' % sum(r['z'] for r in rows))

# --- gravitacni model na podilech ---
X = np.column_stack([np.ones(len(rows)),
                     np.log([r['M'] for r in rows]),
                     np.log([r['d'] for r in rows])])
y = np.log([r['share'] for r in rows])
beta, *_ = np.linalg.lstsq(X, y, rcond=None)
pred = X @ beta
ss_res = ((y-pred)**2).sum(); ss_tot = ((y-y.mean())**2).sum()
print('\nlog(podíl) = %.3f + %.3f*log(velikost školy) %.3f*log(vzdálenost)' % (beta[0],beta[1],beta[2]))
print('R² = %.3f   (n=%d)' % (1-ss_res/ss_tot, len(rows)))

for i,r in enumerate(rows): r['resid'] = y[i]-pred[i]

def nm(k): return obce[k]['nazev_obce_bydliste_studenta'] if k in obce else k
srt = sorted(rows, key=lambda r:-r['resid'])
print('\n--- ŠKOLY, KTERÉ PŘITAHUJÍ VÝRAZNĚ VÍC, NEŽ BY MĚLY (top 10 reziduí) ---')
for r in srt[:10]:
    print('  %-22s -> %-52s %3d žáků, %4.1f km (%.1fx víc)' % (
        nm(r['kod'])[:22], skoly[r['izo']]['nazev'][:52], r['z'], r['d'], math.exp(r['resid'])))
print('\n--- A KDE JE TO NAOPAK SLABÉ (bottom 5) ---')
for r in srt[-5:]:
    print('  %-22s -> %-52s %3d žáků, %4.1f km (%.2fx)' % (
        nm(r['kod'])[:22], skoly[r['izo']]['nazev'][:52], r['z'], r['d'], math.exp(r['resid'])))

# --- prumerna dojezdova vzdalenost podle obce ---
wd = {}
for r in rows:
    a = wd.setdefault(r['kod'], [0,0]); a[0]+=r['d']*r['z']; a[1]+=r['z']
avg = sorted(((v[0]/v[1], k, v[1]) for k,v in wd.items() if v[1]>=20), reverse=True)
print('\n--- OBCE S NEJDELŠÍ PRŮMĚRNOU DOJÍŽĎKOU (min. 20 žáků) ---')
for d,k,n in avg[:8]: print('  %-26s %5.1f km  (%d žáků)' % (nm(k)[:26], d, n))
print('  ...')
for d,k,n in avg[-3:]: print('  %-26s %5.1f km  (%d žáků)' % (nm(k)[:26], d, n))
allw = sum(r['d']*r['z'] for r in rows)/sum(r['z'] for r in rows)
print('\nkrajský vážený průměr: %.1f km' % allw)
json.dump({'rows':rows,'beta':list(beta)}, io.open('research/analyza/data/gravity.json','w',encoding='utf-8'))
