"""Stahne 10 vrstev Bezbarieroveho Hradce, prevede S-JTSK -> WGS84 a ulozi GeoJSON.
Duvod: ckan.mmhk.cz neposila CORS hlavicky, takze shapefile nejde nacist primo z prohlizece.
Spusteni: python Analyza/hk/prevod.py     (trva ~20 s)"""
import sys, os, json, io, struct
sys.path.insert(0, os.path.dirname(__file__))
from dbf import stahni, cti_dbf
from krovak import sjtsk_to_wgs

B = 'https://ckan.mmhk.cz/dataset/ab3f5e87-c232-4cf7-88ca-b7a9c6253c96/resource/'
VRSTVY = [
 ('chodniky_nevidomi','Chodníky – nevidomí','abccf5e3-4d71-4005-a51f-11d0af0e93ec/download/chodnikynevidomi.zip'),
 ('indukcni_smycky','Indukční smyčky','7f896379-a2fa-4e1f-8708-19c6ca4556e8/download/indukcni-smycky.zip'),
 ('objekty_nevidomi','Objekty – nevidomí','3fb3b874-fac8-4c1f-a677-e347069f746c/download/objektynevidomi.zip'),
 ('objekty_vozicky','Objekty – vozíčky','a4791a0a-ccef-4ccb-ae29-dd579da3c120/download/objektyvozicky.zip'),
 ('prechody_nevidomi','Přechody – nevidomí','3f429e7a-f2b4-4111-baf7-4586686276e4/download/prechodynevidomi.zip'),
 ('prechody_vozicky','Přechody – vozíčky','ee189b4f-afb3-49b0-bfab-709da24a9da6/download/prechodyvozicky.zip'),
 ('trasy','Trasy','2d1a4eb6-43e0-46ff-aa77-3c1970130c64/download/trasy.zip'),
 ('useky_nevidomi','Úseky – nevidomí','f7cc161e-716a-4dc0-bf03-697cffd1d190/download/usekynevidomil.zip'),
 ('zastavky_nevidomi','Zastávky – nevidomí','ea4c388a-0c91-4c33-a290-b82330c57b8b/download/zastavkynevidomi.zip'),
 ('zastavky_vozicky','Zastávky – vozíčky','8bf9de82-99ff-4fbc-a0cd-451fac743ef9/download/zastavkyvozicky.zip')]

def cti_shp(raw):
    """vrati seznam geometrii: ('Point',[lon,lat]) nebo ('LineString',[[lon,lat],...])"""
    out, off = [], 100
    while off < len(raw):
        _, dl = struct.unpack('>ii', raw[off:off+8])
        st = struct.unpack('<i', raw[off+8:off+12])[0]
        body = raw[off+12: off+8+dl*2]
        if st == 1:                                    # Point
            x, y = struct.unpack('<dd', body[:16])
            la, lo = sjtsk_to_wgs(x, y)
            out.append(('Point', [round(lo,6), round(la,6)]))
        elif st in (3, 5):                             # Polyline / Polygon
            nparts, npts = struct.unpack('<ii', body[32:40])
            parts = list(struct.unpack('<%di' % nparts, body[40:40+4*nparts]))
            pb = 40 + 4*nparts
            pts = [struct.unpack('<dd', body[pb+16*i: pb+16*i+16]) for i in range(npts)]
            wgs = []
            for i, p in enumerate(parts):
                konec = parts[i+1] if i+1 < nparts else npts
                seg = [sjtsk_to_wgs(*pts[j]) for j in range(p, konec)]
                wgs.append([[round(lo,6), round(la,6)] for la, lo in seg])
            out.append(('LineString', wgs[0]) if len(wgs) == 1 else ('MultiLineString', wgs))
        else:
            out.append((None, None))
        off += 8 + dl*2
    return out

os.makedirs('research/prototypy/data', exist_ok=True)
souhrn = {}
for klic, nazev, cesta in VRSTVY:
    z = stahni(B + cesta)
    dbf = [n for n in z.namelist() if n.lower().endswith('.dbf')][0]
    shp = [n for n in z.namelist() if n.lower().endswith('.shp')][0]
    _, rows = cti_dbf(z.read(dbf))
    geoms = cti_shp(z.read(shp))
    feats = []
    for r, (gt, gc) in zip(rows, geoms):
        if not gt: continue
        props = {k: v for k, v in r.items() if v not in ('', None)}
        props['_vrstva'] = nazev
        feats.append({'type':'Feature','geometry':{'type':gt,'coordinates':gc},'properties':props})
    fc = {'type':'FeatureCollection','name':nazev,
          'zdroj':'https://ckan.mmhk.cz/dataset/bezbarierovy-hradec (CC0)','features':feats}
    json.dump(fc, io.open('research/prototypy/data/%s.geojson' % klic,'w',encoding='utf-8'), ensure_ascii=False)
    # kontrola: lezi body v Hradci?
    lat = [f['geometry']['coordinates'][1] for f in feats if f['geometry']['type']=='Point']
    lon = [f['geometry']['coordinates'][0] for f in feats if f['geometry']['type']=='Point']
    rozsah = 'lat %.4f–%.4f lon %.4f–%.4f' % (min(lat),max(lat),min(lon),max(lon)) if lat else 'linie'
    ok = 'OK' if (not lat or (50.10 < min(lat) and max(lat) < 50.32 and 15.70 < min(lon) and max(lon) < 15.95)) else 'MIMO HRADEC!'
    print('%-20s %4d prvků  %-34s %s' % (klic, len(feats), rozsah, ok))
    souhrn[klic] = {'nazev':nazev,'n':len(feats)}
json.dump(souhrn, io.open('research/prototypy/data/_souhrn.json','w',encoding='utf-8'), ensure_ascii=False, indent=1)
print('\ncelkem prvků:', sum(v['n'] for v in souhrn.values()))
