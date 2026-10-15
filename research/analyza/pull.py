import json, io, os, urllib.request, urllib.parse
SVC='https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services'
API={'doj':SVC+'/Doj%C3%AD%C5%BE%C4%8Fka_do_st%C5%99edn%C3%ADch_%C5%A1kol/FeatureServer/0',
     'hran':SVC+'/Hranice_obc%C3%AD_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0',
     'adm':SVC+'/Administrativn%C3%AD_%C4%8Dlen%C4%9Bn%C3%AD_obc%C3%AD/FeatureServer/0',
     'zast':SVC+'/Autobusov%C3%A9_zast%C3%A1vky_VDKHK/FeatureServer/0'}
def get(u):
    return json.load(urllib.request.urlopen(urllib.request.Request(u,headers={'User-Agent':'Mozilla/5.0'}),timeout=120))
def agg(api,grp,extra=''):
    st=urllib.parse.quote(json.dumps([{'statisticType':'sum','onStatisticField':'pocet_zaku','outStatisticFieldName':'z'}]))
    out=[];off=0
    while True:
        j=get(f"{api}/query?where=1%3D1{extra}&groupByFieldsForStatistics={urllib.parse.quote(grp)}"
              f"&outStatistics={st}&returnGeometry=false&resultRecordCount=2000&resultOffset={off}&f=json")
        f=j.get('features',[]); out+= [x['attributes'] for x in f]
        if len(f)<2000: break
        off+=2000
    return out
os.makedirs('research/analyza/data',exist_ok=True)
D={}
D['toky']  = agg(API['doj'],'izo,kod_obce_bydliste_studenta')
D['skoly'] = agg(API['doj'],'izo,nazev_skoly,x,y,nazev_obce,kod_obce,kod_orp,nazev_orp,zrizovatel')
D['obce']  = agg(API['doj'],'kod_obce_bydliste_studenta,nazev_obce_bydliste_studenta,nazev_okresu_bydliste_studenta')
D['obory'] = agg(API['doj'],'izo,obor_nazev')
D['adm']   = [f['attributes'] for f in get(API['adm']+'/query?where=1%3D1&outFields=kod_obce,nazev_obce,kod_orp,nazev_orp,nazev_okresu&returnGeometry=false&resultRecordCount=1000&f=json')['features']]
D['hran']  = get(API['hran']+'/query?where=1%3D1&outFields=nazev_obce,kod_obce&maxAllowableOffset=0.0008&f=geojson&resultRecordCount=1000')
zast=[];off=0
while True:
    j=get(API['zast']+f'/query?where=1%3D1&outFields=nazev,kod_obce,x,y&returnGeometry=false&resultRecordCount=2000&resultOffset={off}&f=json')
    f=j.get('features',[]); zast+=[x['attributes'] for x in f]
    if len(f)<2000: break
    off+=2000
D['zast']=zast
json.dump(D, io.open('research/analyza/data/all.json','w',encoding='utf-8'), ensure_ascii=False)
for k,v in D.items():
    print('%-7s %s' % (k, len(v['features']) if isinstance(v,dict) else len(v)))
