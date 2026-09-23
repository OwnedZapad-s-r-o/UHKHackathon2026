"""Minimalni cteni shapefile (.dbf atributy + .shp body/linie) bez zavislosti."""
import struct, io, zipfile, urllib.request

def stahni(url):
    b = urllib.request.urlopen(urllib.request.Request(url, headers={'User-Agent':'Mozilla/5.0'}), timeout=120).read()
    return zipfile.ZipFile(io.BytesIO(b))

def cti_dbf(raw, enc='cp1250'):
    n, hlen, rlen = struct.unpack('<I H H', raw[4:12])
    pole = []
    off = 32
    while raw[off] != 0x0D:
        nm = raw[off:off+11].split(b'\0')[0].decode(enc, 'replace')
        typ = chr(raw[off+11]); dl = raw[off+16]
        pole.append((nm, typ, dl)); off += 32
    rows = []
    for i in range(n):
        p = hlen + i*rlen
        if raw[p:p+1] == b'*': continue      # smazany zaznam
        q = p + 1; r = {}
        for nm, typ, dl in pole:
            v = raw[q:q+dl].decode(enc, 'replace').strip(); q += dl
            r[nm] = v
        rows.append(r)
    return [p[0] for p in pole], rows

def cti_shp_body(raw):
    """vrati [(x,y)] pro bodove vrstvy, [] pro ostatni"""
    typ = struct.unpack('<i', raw[32:36])[0]
    if typ != 1: return [], typ
    pts, off = [], 100
    while off < len(raw):
        _, dl = struct.unpack('>ii', raw[off:off+8])
        st = struct.unpack('<i', raw[off+8:off+12])[0]
        if st == 1:
            x, y = struct.unpack('<dd', raw[off+12:off+28]); pts.append((x, y))
        off += 8 + dl*2
    return pts, typ
