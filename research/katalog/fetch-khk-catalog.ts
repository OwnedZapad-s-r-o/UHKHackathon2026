import fs from 'fs';
import path from 'path';

interface Distribution {
  title?: string;
  format?: string;
  downloadUrl?: string;
}

interface DatasetMetadata {
  id: string;
  url: string;
  title: string;
  description: string;
  keywords: string[];
  themes: string[];
  distributions: Distribution[];
}

const CATALOG_URL = 'https://open.datakhk.cz/katalog.jsonld';
const REPORT_KEYWORDS = ['školy', 'investice', 'ovzduší', 'sociální', 'zdravotnictví', 'obce', 'hasiči', 'dotace'];

const cs = (v: any): string | undefined =>
  typeof v === 'string' ? v : v?.cs ?? undefined;

const asArray = (v: any): any[] => (Array.isArray(v) ? v : v == null ? [] : [v]);

const toList = (v: any): string[] =>
  asArray(v?.cs ?? v).map((x) => (typeof x === 'string' ? x : cs(x))).filter(Boolean) as string[];

async function fetchJson(url: string, timeoutMs: number) {
  const res = await fetch(url, { signal: AbortSignal.timeout(timeoutMs) });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function run() {
  console.log(`[1/3] Stahuji hlavní katalog: ${CATALOG_URL}`);
  const catalog = await fetchJson(CATALOG_URL, 15000);
  const datasetUrls: string[] = asArray(catalog['datová_sada']);
  console.log(`[2/3] Nalezeno ${datasetUrls.length} datových sad. Zahajuji inspekci...`);

  const manifest: DatasetMetadata[] = [];
  let counter = 0;
  let skipped = 0;

  for (const url of datasetUrls) {
    counter++;
    const id = url.split('/').pop()?.replace('.jsonld', '') || '';
    try {
      const d = await fetchJson(url, 8000);

      const title = cs(d.název) || cs(d.title) || 'Bez názvu';
      const description = cs(d.popis) || cs(d.description) || '';
      const keywords = toList(d['klíčové_slovo']);
      const themes = asArray(d.téma).map(String);

      const distributions: Distribution[] = asArray(d.distribuce).map((dist: any) => ({
        title: cs(dist.název) || cs(dist.title),
        format: dist.formát,
        downloadUrl: dist.přístupové_url || dist.soubor_ke_stažení,
      }));

      manifest.push({ id, url, title, description, keywords, themes, distributions });
      console.log(`[OK ${counter}/${datasetUrls.length}] ${title}`);
    } catch (err: any) {
      skipped++;
      console.error(`[ERR ${counter}/${datasetUrls.length}] ${id}: ${err.message}`);
    }
  }

  const outDir = path.join(process.cwd(), 'research', 'katalog');
  fs.mkdirSync(outDir, { recursive: true });
  const outFile = path.join(outDir, 'khk-datasets-manifest.json');
  fs.writeFileSync(outFile, JSON.stringify(manifest, null, 2), 'utf-8');
  console.log(`[3/3] Hotovo! ${manifest.length} sad uloženo (${skipped} přeskočeno) do: ${outFile}`);

  console.log('\n=== Sady podle sledovaných klíčových slov ===');
  for (const kw of REPORT_KEYWORDS) {
    const needle = kw.toLowerCase();
    const hits = manifest.filter((m) =>
      [m.title, m.description, ...m.keywords].some((s) => s.toLowerCase().includes(needle)),
    );
    console.log(`\n## ${kw} (${hits.length})`);
    for (const h of hits) console.log(`  - ${h.title}  [${h.id}]  dist: ${h.distributions.length}`);
  }
}

run().catch((e) => {
  console.error(e);
  process.exit(1);
});
