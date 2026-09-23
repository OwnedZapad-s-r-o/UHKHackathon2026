import json, io, math, random
from collections import defaultdict
import numpy as np
D=json.load(io.open('research/analyza/data/all.json',encoding='utf-8'))
cent={}; nazev={}
for f in D['hran']['features']:
    k=f['properties']['kod_obce']; g=f['geometry']; cs=g['coordinates']
    rings=cs if g['type']=='Polygon' else [r for p in cs for r in p]
    pts=[p for r in rings for p in r]
    cent[k]=(sum(p[0] for p in pts)/len(pts),sum(p[1] for p in pts)/len(pts)); nazev[k]=f['properties']['nazev_obce']
skoly={}
for s in D['skoly']:
    o=skoly.setdefault(s['izo'],{'nazev':s['nazev_skoly'],'x':s['x'],'y':s['y'],'obec':s['nazev_obce'],'z':0}); o['z']+=s['z'] or 0

# --- 1. obsluha zastavkami ---
zast_obce=defaultdict(int)
for z in D['zast']:
    if z.get('kod_obce'): zast_obce[z['kod_obce']]+=1
bez=[k for k in cent if zast_obce.get(k,0)==0]
print('AUTOBUSOVÉ ZASTÁVKY (4 538) VS 448 OBCÍ')
print('  obcí bez jediné zastávky VDKHK: %d' % len(bez))
print('  příklady: %s' % ', '.join(nazev[k] for k in bez[:8]))
tot={}
for t in D['toky']:
    k=t['kod_obce_bydliste_studenta']
    if k in cent: tot[k]=tot.get(k,0)+(t['z'] or 0)
zaci_bez=sum(tot.get(k,0) for k in bez)
print('  žáků SŠ bydlících v obci bez zastávky: %d' % zaci_bez)

# --- 2. shluky obci podle profilu dojizdky (k-means na podilech do TOP skol) ---
top=[i for i,_ in sorted(skoly.items(), key=lambda kv:-kv[1]['z'])[:25]]
idx={i:n for n,i in enumerate(top)}
prof={}
for t in D['toky']:
    k,i,z=t['kod_obce_bydliste_studenta'],t['izo'],t['z'] or 0
    if k in cent and i in idx and tot.get(k,0)>=15:
        prof.setdefault(k,np.zeros(len(top)))[idx[i]]+=z
kods=[k for k,v in prof.items() if v.sum()>0]
X=np.array([prof[k]/prof[k].sum() for k in kods])
print('\nSHLUKOVÁNÍ OBCÍ PODLE PROFILU DOJÍŽĎKY (%d obcí, %d hlavních škol)' % (X.shape[0],X.shape[1]))
random.seed(1); np.random.seed(1)
K=6; C=X[np.random.choice(len(X),K,replace=False)].copy()
for _ in range(60):
    lab=((X[:,None,:]-C[None])**2).sum(2).argmin(1)
    for j in range(K):
        if (lab==j).any(): C[j]=X[lab==j].mean(0)
adm={a['kod_obce']:a for a in D['adm']}
for j in range(K):
    mem=[kods[n] for n in range(len(kods)) if lab[n]==j]
    if not mem: continue
    dom=C[j].argmax()
    orps=defaultdict(int)
    for k in mem: orps[adm.get(k,{}).get('nazev_orp','?')]+=1
    mix=', '.join('%s(%d)'%(o,c) for o,c in sorted(orps.items(),key=lambda x:-x[1])[:3])
    print('  shluk %d: %3d obcí | dominantní škola: %-40s' % (j+1,len(mem),skoly[top[dom]]['nazev'][:40]))
    print('            ORP: %s' % mix)
print('\n-> pokud shluk míchá víc ORP, znamená to, že reálná spádovost nekopíruje správní hranice')
