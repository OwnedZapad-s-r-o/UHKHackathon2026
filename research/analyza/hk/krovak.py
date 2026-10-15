"""S-JTSK Krovak East/North (EPSG:5514) -> WGS84. Bez externich zavislosti.
Postup: inverzni Krovak -> Bessel 1841 fi/la -> geocentricke -> Helmert 7 param -> WGS84."""
import math

# Bessel 1841
A_B = 6377397.155
F_B = 1/299.1528128
E2_B = 2*F_B - F_B*F_B
E_B = math.sqrt(E2_B)
# WGS84
A_W = 6378137.0
F_W = 1/298.257223563
E2_W = 2*F_W - F_W*F_W

# Krovak parametry
PHI_C = math.radians(49.5)
LAM_0 = math.radians(24.83333333333333)   # 24 st 50 min vych. od Greenwiche
ALPHA = math.radians(30.28813972222222)
PHI_P = math.radians(78.5)
K_C   = 0.9999

_A = A_B*math.sqrt(1-E2_B)/(1-E2_B*math.sin(PHI_C)**2)
_B = math.sqrt(1 + (E2_B*math.cos(PHI_C)**4)/(1-E2_B))
_G0 = math.asin(math.sin(PHI_C)/_B)
_T0 = (math.tan(math.pi/4 + _G0/2)
       * ((1+E_B*math.sin(PHI_C))/(1-E_B*math.sin(PHI_C)))**(E_B*_B/2)
       / math.tan(math.pi/4 + PHI_C/2)**_B)
_N = math.sin(PHI_P)
_R0 = K_C*_A/math.tan(PHI_P)

# Helmert S-JTSK(Bessel) -> WGS84, bezne uzivane parametry pro CR
DX, DY, DZ = 570.8, 85.7, 462.8
RX = math.radians(4.998/3600); RY = math.radians(1.587/3600); RZ = math.radians(5.261/3600)
SC = 1 + 3.56e-6

def krovak_inv(X, Y):
    """X,Y v metrech tak, jak jsou v shapefilu (zaporne). Vraci (lat, lon) na Besselu."""
    # EPSG:5514 ma X = zaporny easting, Y = zaporny northing.
    # Krovak pracuje s kladnym jihem (southing) a zapadem (westing):
    xp, yp = -Y, -X
    r  = math.hypot(xp, yp)
    th = math.atan2(yp, xp)
    d  = th/math.sin(PHI_P)
    t  = 2*(math.atan((_R0/r)**(1/_N) * math.tan(math.pi/4 + PHI_P/2)) - math.pi/4)
    u  = math.asin(math.cos(ALPHA)*math.sin(t) - math.sin(ALPHA)*math.cos(t)*math.cos(d))
    v  = math.asin(math.cos(t)*math.sin(d)/math.cos(u))
    phi = u
    for _ in range(12):                  # iterace na zemepisnou sirku
        prev = phi
        phi = 2*(math.atan(_T0**(-1/_B)
                 * math.tan(u/2 + math.pi/4)**(1/_B)
                 * ((1+E_B*math.sin(phi))/(1-E_B*math.sin(phi)))**(E_B/2)) - math.pi/4)
        if abs(phi-prev) < 1e-13: break
    return phi, LAM_0 - v/_B

def bessel_to_wgs(phi, lam, h=250.0):
    n = A_B/math.sqrt(1-E2_B*math.sin(phi)**2)
    x = (n+h)*math.cos(phi)*math.cos(lam)
    y = (n+h)*math.cos(phi)*math.sin(lam)
    z = (n*(1-E2_B)+h)*math.sin(phi)
    x2 = SC*( x + RZ*y - RY*z) + DX
    y2 = SC*(-RZ*x +  y + RX*z) + DY
    z2 = SC*( RY*x - RX*y +  z) + DZ
    p = math.hypot(x2, y2)
    la = math.atan2(y2, x2)
    fi = math.atan2(z2, p*(1-E2_W))
    for _ in range(8):
        nn = A_W/math.sqrt(1-E2_W*math.sin(fi)**2)
        hh = p/math.cos(fi) - nn
        fi = math.atan2(z2, p*(1-E2_W*nn/(nn+hh)))
    return math.degrees(fi), math.degrees(la)

def sjtsk_to_wgs(X, Y):
    phi, lam = krovak_inv(X, Y)
    return bessel_to_wgs(phi, lam)

if __name__ == '__main__':
    # kontrolni bod: znamy prevod S-JTSK -> WGS84
    for X, Y, ocek in [(-641630.76, -1042005.84, 'ma byt Hradec ~50.21/15.83'),
                       (-742000.0, -1043000.0, 'ma byt Praha ~50.09/14.42'),
                       (-641674.07, -1042181.17, 'dalsi bod z dat')]:
        la, lo = sjtsk_to_wgs(X, Y)
        print('X=%.1f Y=%.1f -> %.5f, %.5f   (%s)' % (X, Y, la, lo, ocek))
