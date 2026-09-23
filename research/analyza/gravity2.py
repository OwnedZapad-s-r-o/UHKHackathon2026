import json, io, math
import numpy as np
D = json.load(io.open('research/analyza/data/all.json', encoding='utf-8'))
cent={}
for f in D['hran']['features']:
    kod=f['properties']['kod_obce']; g=f['geometry']; cs=g['coordinates']
    rings = cs if g['type']=='Polygon' else [r for p in cs for r in p]
    pts=[p for r in rings for p in r]
    cent[kod]=(sum(p[0] for p in pts)/len(pts), sum(p[1] for p in pts)/len(pts))
skoly={}
for s in D['skoly']:
    o=skoly.setdefault(s['izo'],{'nazev':s['nazev_skoly'],'x':s['x'],'y':s['y'],'obec':s['nazev_obce'],
                                 'orp':s['nazev_orp'],'kod_orp':s['kod_orp'],'z':0})
    o['z']+=s['z'] or 0
obce={o['kod_obce_bydliste_studenta']:o for o in D['obce']}
adm={a['kod_obce']:a for a in D['adm']}
def hav(a,b):
    p=math.pi/180
    return 2*6371*math.asin(math.sqrt(math.sin((b[1]-a[1])*p/2)**2+math.cos(a[1]*p)*math.cos(b[1]*p)*math.sin((b[0]-a[0])*p/2)**2))
# jmenovatel podilu: jen toky, ktere umime modelovat (skola ma souradnice)
tot={}
for t in D['toky']:
    k=t['kod_obce_bydliste_studenta']; s=skoly.get(t['izo'])
    if k in cent and s and s['x'] and (t['z'] or 0)>0:
        tot[k]=tot.get(k,0)+t['z']
rows=[]
for t in D['toky']:
    k,izo,z=t['kod_obce_bydliste_studenta'],t['izo'],t['z'] or 0
    s=skoly.get(izo)
    if k not in cent or not s or not s['x'] or z<=0: continue
    d=max(hav(cent[k],(s['x'],s['y'])),0.5)
    same=1.0 if adm.get(k,{}).get('kod_orp')==s['kod_orp'] else 0.0
    rows.append({'kod':k,'izo':izo,'z':z,'d':d,'M':s['z'],'share':z/tot[k],'same':same})

X=np.column_stack([np.ones(len(rows)),np.log([r['M'] for r in rows]),
                   np.log([r['d'] for r in rows]),[r['same'] for r in rows]])
y=np.log([r['share'] for r in rows]); w=np.array([r['z'] for r in rows],float)
sw=np.sqrt(w)
beta,*_=np.linalg.lstsq(X*sw[:,None], y*sw, rcond=None)
pred=X@beta
r2=1-(w*(y-pred)**2).sum()/(w*(y-np.average(y,weights=w))**2).sum()
print('VÁŽENÝ GRAVITAČNÍ MODEL (váha = počet žáků)')
print('log(podíl) = %.2f + %.2f*log(velikost) %.2f*log(km) + %.2f*stejné_ORP' % tuple(beta))
print('vážené R2 = %.3f   n=%d toků' % (r2, len(rows)))
print('  -> vzdálenost: každé zdvojnásobení km sníží podíl na %.0f %%' % (100*2**beta[2]))
print('  -> stejné ORP: %.1fx vyšší podíl při stejné vzdálenosti' % math.exp(beta[3]))
for i,r in enumerate(rows): r['resid']=y[i]-pred[i]
def nm(k): return obce[k]['nazev_obce_bydliste_studenta'] if k in obce else k
big=[r for r in rows if r['z']>=10]
srt=sorted(big,key=lambda r:-r['resid'])
print('\n--- NEČEKANĚ SILNÉ VAZBY (jen toky >=10 žáků, n=%d) ---' % len(big))
for r in srt[:8]:
    print('  %-24s -> %-46s %3d ž, %4.1f km  %.1fx' % (nm(r['kod'])[:24],skoly[r['izo']]['nazev'][:46],r['z'],r['d'],math.exp(r['resid'])))
print('\n--- NEČEKANĚ SLABÉ VAZBY (škola blízko, ale nejezdí se tam) ---')
for r in srt[-8:]:
    print('  %-24s -> %-46s %3d ž, %4.1f km  %.2fx' % (nm(r['kod'])[:24],skoly[r['izo']]['nazev'][:46],r['z'],r['d'],math.exp(r['resid'])))
json.dump({'rows':rows,'beta':list(beta),'r2':r2},io.open('research/analyza/data/gravity.json','w',encoding='utf-8'))
