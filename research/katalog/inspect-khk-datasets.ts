import fs from 'fs';
import path from 'path';

interface Manifest { id: string; title: string; description: string; keywords: string[]; distributions: { title?: string; downloadUrl?: string }[] }
interface Inspected {
  id: string; title: string; description: string; keywords: string[];
  serviceUrl?: string; geometryType?: string; recordCount?: number;
  fields?: { name: string; alias: string; type: string }[];
  sample?: Record<string, unknown>[];
  error?: string;
}

const manifest: Manifest[] = JSON.parse(fs.readFileSync('research/katalog/khk-datasets-manifest.json', 'utf-8'));
const j = async (u: string) => { const r = await fetch(u, { signal: AbortSignal.timeout(15000) }); if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); };
const SKIP = /^(esriFieldTypeOID|esriFieldTypeGeometry)$/;

async function inspect(m: Manifest): Promise<Inspected> {
  const base = { id: m.id, title: m.title, description: m.description, keywords: m.keywords };
  const svc = m.distributions.find(d => /FeatureServer\/\d+$/.test(d.downloadUrl ?? ''))?.downloadUrl;
  if (!svc) return { ...base, error: 'bez ArcGIS GeoService (pravděpodobně webová stránka)' };
  try {
    const meta = await j(`${svc}?f=pjson`);
    const fields = (meta.fields ?? []).filter((f: any) => !SKIP.test(f.type) && !/^Shape__/.test(f.name))
      .map((f: any) => ({ name: f.name, alias: f.alias, type: String(f.type).replace('esriFieldType', '') }));
    const cnt = await j(`${svc}/query?where=1%3D1&returnCountOnly=true&f=json`);
    const q = await j(`${svc}/query?where=1%3D1&outFields=*&returnGeometry=false&resultRecordCount=3&f=json`);
    return { ...base, serviceUrl: svc, geometryType: meta.geometryType, recordCount: cnt.count, fields, sample: (q.features ?? []).map((f: any) => f.attributes) };
  } catch (e: any) { return { ...base, serviceUrl: svc, error: e.message }; }
}

async function run() {
  const out: Inspected[] = [];
  for (let i = 0; i < manifest.length; i += 8) {
    const batch = await Promise.all(manifest.slice(i, i + 8).map(inspect));
    for (const r of batch) console.log(r.error ? `[--] ${r.title}: ${r.error}` : `[OK] ${r.title}: ${r.recordCount} záznamů, ${r.fields!.length} polí`);
    out.push(...batch);
  }
  fs.writeFileSync('research/katalog/khk-datasets-schema.json', JSON.stringify(out, null, 2), 'utf-8');

  const md: string[] = ['# Přehled datových sad KHK – struktura a ukázka dat', ''];
  for (const r of out) {
    md.push(`## ${r.title}`, '', r.description ? `> ${r.description}` : '', '');
    if (r.keywords.length) md.push(`Klíčová slova: ${r.keywords.join(', ')}`, '');
    if (r.error) { md.push(`_${r.error}_`, ''); continue; }
    md.push(`- Geometrie: ${r.geometryType ?? '—'} | Záznamů: **${r.recordCount}** | Služba: ${r.serviceUrl}`, '');
    md.push('| Pole | Alias | Typ | Ukázka |', '|---|---|---|---|');
    for (const f of r.fields!) {
      const ex = r.sample?.[0]?.[f.name];
      md.push(`| ${f.name} | ${f.alias} | ${f.type} | ${ex == null ? '' : String(ex).replace(/\|/g, '\|').slice(0, 60)} |`);
    }
    md.push('');
  }
  fs.writeFileSync('research/katalog/khk-datasets-overview.md', md.join('\n'), 'utf-8');
  console.log(`\nHotovo: ${out.filter(x => !x.error).length} sad s daty, ${out.filter(x => x.error).length} bez. -> research/katalog/khk-datasets-schema.json, research/katalog/khk-datasets-overview.md`);
}
run().catch(e => { console.error(e); process.exit(1); });
