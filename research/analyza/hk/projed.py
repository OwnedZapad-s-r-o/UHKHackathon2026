import sys, json, io
sys.path.insert(0, 'research/analyza/hk')
from dbf import stahni, cti_dbf, cti_shp_body

B='https://ckan.mmhk.cz/dataset/ab3f5e87-c232-4cf7-88ca-b7a9c6253c96/resource/'
VRSTVY=[('Chodníky - nevidomí','abccf5e3-4d71-4005-a51f-11d0af0e93ec/download/chodnikynevidomi.zip'),
 ('Indukční smyčky','7f896379-a2fa-4e1f-8708-19c6ca4556e8/download/indukcni-smycky.zip'),
 ('Objekty - nevidomí','3fb3b874-fac8-4c1f-a677-e347069f746c/download/objektynevidomi.zip'),
 ('Objekty - vozíčky','a4791a0a-ccef-4ccb-ae29-dd579da3c120/download/objektyvozicky.zip'),
 ('Přechody - nevidomí','3f429e7a-f2b4-4111-baf7-4586686276e4/download/prechodynevidomi.zip'),
 ('Přechody - vozíčky','ee189b4f-afb3-49b0-bfab-709da24a9da6/download/prechodyvozicky.zip'),
 ('Trasy','2d1a4eb6-43e0-46ff-aa77-3c1970130c64/download/trasy.zip'),
 ('Úseky - nevidomí','f7cc161e-716a-4dc0-bf03-697cffd1d190/download/usekynevidomil.zip'),
 ('Zastávky - nevidomí','ea4c388a-0c91-4c33-a290-b82330c57b8b/download/zastavkynevidomi.zip'),
 ('Zastávky - vozíčky','8bf9de82-99ff-4fbc-a0cd-451fac743ef9/download/zastavkyvozicky.zip')]
GT={1:'bod',3:'linie',5:'plocha',8:'multibod'}
out={}
for nazev, cesta in VRSTVY:
    try:
        z=stahni(B+cesta)
        dbf=[n for n in z.namelist() if n.lower().endswith('.dbf')][0]
        shp=[n for n in z.namelist() if n.lower().endswith('.shp')]
        pole, rows = cti_dbf(z.read(dbf))
        _, typ = cti_shp_body(z.read(shp[0])) if shp else ([],0)
        print('### %-22s n=%-5d %s' % (nazev, len(rows), GT.get(typ,'?')))
        print('    pole:', ', '.join(pole))
        # hodnoty kratkych textovych poli
        for p in pole:
            vals=sorted({r[p] for r in rows if r[p]})
            if 0 < len(vals) <= 12 and max((len(v) for v in vals), default=0) < 42:
                print('      %-12s = %s' % (p, ' | '.join(vals)[:150]))
        if rows:
            print('    vzorek:', json.dumps({k:v for k,v in list(rows[0].items())[:9]}, ensure_ascii=False)[:260])
        out[nazev]={'n':len(rows),'geom':GT.get(typ,'?'),'pole':pole,'vzorek':rows[:2]}
    except Exception as e:
        print('### %-22s CHYBA %s' % (nazev, str(e)[:70]))
    print()
json.dump(out, io.open('research/analyza/hk/bezbarierovy.json','w',encoding='utf-8'), ensure_ascii=False, indent=1)
