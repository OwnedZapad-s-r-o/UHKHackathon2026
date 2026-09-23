import sys, json
sys.path.insert(0,'research/analyza/hk')
from dbf import stahni, cti_dbf, cti_shp_body
B='https://ckan.mmhk.cz/dataset/ab3f5e87-c232-4cf7-88ca-b7a9c6253c96/resource/'
z=stahni(B+'a4791a0a-ccef-4ccb-ae29-dd579da3c120/download/objektyvozicky.zip')
print('soubory v zip:', z.namelist())
prj=[n for n in z.namelist() if n.lower().endswith('.prj')]
if prj: print('\n.prj:', z.read(prj[0]).decode('utf-8','replace')[:220])
shp=[n for n in z.namelist() if n.lower().endswith('.shp')][0]
pts,typ=cti_shp_body(z.read(shp))
print('\nbodů:',len(pts),'| první 3 souřadnice:', pts[:3])
# volny text napric vrstvami
VR=[('Objekty - nevidomí','3fb3b874-fac8-4c1f-a677-e347069f746c/download/objektynevidomi.zip',['POZNAMKA','POZNAMKA1']),
    ('Objekty - vozíčky','a4791a0a-ccef-4ccb-ae29-dd579da3c120/download/objektyvozicky.zip',['POZN']),
    ('Přechody - nevidomí','3f429e7a-f2b4-4111-baf7-4586686276e4/download/prechodynevidomi.zip',['POZN','POZN2']),
    ('Přechody - vozíčky','ee189b4f-afb3-49b0-bfab-709da24a9da6/download/prechodyvozicky.zip',['PROBLEM']),
    ('Zastávky - nevidomí','ea4c388a-0c91-4c33-a290-b82330c57b8b/download/zastavkynevidomi.zip',['POZN','POZN1']),
    ('Zastávky - vozíčky','8bf9de82-99ff-4fbc-a0cd-451fac743ef9/download/zastavkyvozicky.zip',['PROBLEM'])]
print('\n=== VOLNÝ TEXT (materiál pro NLP) ===')
celkem=0; znaky=0
for nazev,c,pole in VR:
    zz=stahni(B+c); dbf=[n for n in zz.namelist() if n.lower().endswith('.dbf')][0]
    _,rows=cti_dbf(zz.read(dbf))
    txt=[r[p] for r in rows for p in pole if r.get(p)]
    celkem+=len(txt); znaky+=sum(len(t) for t in txt)
    print('  %-22s %3d neprázdných poznámek, %5d znaků' % (nazev,len(txt),sum(len(t) for t in txt)))
    for t in sorted(txt,key=len,reverse=True)[:2]: print('      „%s"' % t[:150])
print('\n  CELKEM %d poznámek, %d znaků volného českého textu' % (celkem,znaky))
