import json, io
from collections import defaultdict
D=json.load(io.open('research/analyza/data/all.json',encoding='utf-8'))
R=D['rada']
ROKY=['2021/2022','2022/2023','2023/2024','2024/2025','2025/2026']
per=defaultdict(lambda: defaultdict(int))
for r in R:
    if r['skolni_rok'] and r['obor_nazev']:
        per[r['obor_nazev']][r['skolni_rok']] += r['pocet_zaku_celkem'] or 0
tot={y:sum(v.get(y,0) for v in per.values()) for y in ROKY}
print('CELKEM ŽÁKŮ SŠ V KRAJI PODLE ŠKOLNÍHO ROKU')
for y in ROKY: print('  %-10s %6d' % (y, tot[y]))
print('  změna 21/22 -> 25/26: %+.1f %%' % ((tot[ROKY[-1]]/tot[ROKY[0]]-1)*100))
chg=[]
for ob,v in per.items():
    a,b=v.get(ROKY[0],0), v.get(ROKY[-1],0)
    if a>=40: chg.append(((b-a)/a*100, a, b, ob))
chg.sort()
print('\n--- OBORY, KTERÉ NEJVÍC MIZÍ (min. 40 žáků v 21/22) ---')
for p,a,b,ob in chg[:10]: print('  %-42s %4d -> %4d   %+6.0f %%' % (ob[:42],a,b,p))
print('\n--- OBORY, KTERÉ NEJVÍC ROSTOU ---')
for p,a,b,ob in chg[-8:]: print('  %-42s %4d -> %4d   %+6.0f %%' % (ob[:42],a,b,p))
zan=[ob for ob,v in per.items() if v.get(ROKY[0],0)>0 and v.get(ROKY[-1],0)==0]
print('\n--- OBORY, KTERÉ ZANIKLY ÚPLNĚ (%d) ---' % len(zan))
for ob in zan[:12]: print('  %s' % ob)
