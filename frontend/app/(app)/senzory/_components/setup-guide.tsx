import { Download, KeyRound, ShieldCheck } from "lucide-react";
import type { Broker } from "@/types/api";
import { CopyButton } from "../../_components/copy-button";

type SetupGuideProps = {
  brokerHost: string;
  broker: Broker;
  gatewayId: string;
  // Jméno a heslo gatewaye k brokeru — bez nich broker gateway nepustí
  credentials: { username: string; password: string } | null;
};

// Cesta k certifikátu CA v gatewayi (IQRF Gateway Webapp ho nahraje do této složky)
const TRUST_STORE = "/etc/iqrf-gateway-daemon/certs/smartaudit-ca.crt";

// Stažení souboru přes route handler (ne stránka) — obyčejný odkaz, ne klientská navigace
const CA_CERTIFICATE_URL = "/senzory/certifikat";

// Návod pro IQRF Gateway Daemon: kam se připojit a co nastavit v IQRF Gateway Webapp → Konfigurace → Messaging → MQTT.
// Broker přijímá jen gatewaye s vlastním jménem a heslem, přes síť šifrovaně (TLS).
export function SetupGuide({ brokerHost, broker, gatewayId, credentials }: SetupGuideProps) {
  const requestTopic = `gateway/${gatewayId}/iqrf/requests`;
  const responseTopic = `gateway/${gatewayId}/iqrf/responses`;
  const address = `${broker.publicTls ? "ssl" : "tcp"}://${brokerHost}:${broker.publicPort}`;

  const daemonConfig = JSON.stringify(
    {
      component: "iqrf::MqttMessaging",
      instance: "SmartAuditKHK",
      BrokerAddr: address,
      ClientId: gatewayId,
      User: credentials?.username ?? gatewayId,
      Password: credentials?.password ?? "",
      TopicRequest: requestTopic,
      TopicResponse: responseTopic,
      Qos: 1,
      Persistence: 1,
      EnabledSSL: broker.publicTls,
      ...(broker.publicTls ? { TrustStore: TRUST_STORE, EnableServerCertAuth: true } : {}),
      acceptAsyncMsg: true,
    },
    null,
    2,
  );

  const emulator = emulatorCommand(brokerHost, gatewayId, { password: credentials?.password, tls: broker.publicTls, port: broker.publicPort });

  return (
    <ol className="space-y-4 text-[13px] text-slate-700">
      {broker.publicTls && (
        <Step number={1} title="Stáhněte certifikát brokeru do gatewaye">
          <p className="mt-1 text-slate-600">
            Spojení je šifrované (TLS). Certifikát CA nahrajte v IQRF Gateway Webapp do <code className="font-mono text-[12px]">{TRUST_STORE}</code> — gateway pak ověří,
            že mluví se skutečným brokerem kraje.
          </p>
          {broker.caAvailable ? (
            <a
              href={CA_CERTIFICATE_URL}
              download
              className="mt-2 inline-flex h-9 items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-[12.5px] font-bold text-hk-navy transition-colors hover:bg-slate-50"
            >
              <Download className="size-3.5" aria-hidden />
              Stáhnout smartaudit-ca.crt
            </a>
          ) : (
            <p className="mt-2 text-[12px] text-amber-800">Certifikát zatím není k dispozici — broker ho vytvoří při prvním startu.</p>
          )}
        </Step>
      )}

      <Step number={broker.publicTls ? 2 : 1} title="Gateway se přihlásí k brokeru MQTT této aplikace">
        <dl className="mt-2 grid gap-x-4 gap-y-1 font-mono text-[12px] sm:grid-cols-[auto_1fr]">
          <dt className="text-slate-500">broker</dt>
          <dd className="flex items-center gap-1.5">
            {broker.publicTls && <ShieldCheck className="size-3.5 text-emerald-600" aria-label="šifrováno" />}
            {address}
          </dd>
          <dt className="text-slate-500">jméno</dt>
          <dd>{credentials?.username ?? gatewayId}</dd>
          <dt className="text-slate-500">heslo</dt>
          <dd className="flex items-center gap-2">
            {credentials ? (
              <>
                <KeyRound className="size-3.5 text-hk-blue" aria-hidden />
                <span className="break-all">{credentials.password}</span>
                <CopyButton text={credentials.password} />
              </>
            ) : (
              <span className="text-amber-800">broker nemá nastavené účty</span>
            )}
          </dd>
          <dt className="text-slate-500">dotazy</dt>
          <dd className="break-all">{requestTopic}</dd>
          <dt className="text-slate-500">odpovědi</dt>
          <dd className="break-all">{responseTopic}</dd>
        </dl>
        <p className="mt-2 text-slate-600">
          Gateway smí jen na tyto dva topicy — cizí data nepřečte a za jinou budovu nic nepošle. V IQRF Gateway Webapp:{" "}
          <b>Konfigurace → Messaging → MQTT</b> → nová instance s těmito hodnotami (nebo soubor <code className="font-mono text-[12px]">iqrf__MqttMessaging.json</code>):
        </p>
        <Code code={daemonConfig} />
      </Step>

      <Step number={broker.publicTls ? 3 : 2} title="Aplikace sama najde uzly a jejich senzory">
        <p className="mt-1 text-slate-600">
          Zeptá se koordinátoru na spárované uzly a každého uzlu na jeho senzory (IQRF Standard Sensor). Typ senzoru určí veličinu —
          teplota, CO₂, vlhkost, osvětlení, hluk, pohyb… Hodnoty pak sbírá hromadně příkazem FRC: jedna veličina ze všech uzlů naráz.
        </p>
      </Step>

      <Step number={broker.publicTls ? 4 : 3} title="Přiřaďte kanál do místnosti">
        <p className="mt-1 text-slate-600">
          Od té chvíle se hodnoty ukládají, grafy místnosti pokračují skutečnými daty a simulace pro ni skončí. Audit objektu se přepočítá.
        </p>
      </Step>

      <Step number={broker.publicTls ? 5 : 4} title="Bez hardwaru: emulátor gatewaye">
        <Code code={emulator} />
      </Step>
    </ol>
  );
}

function Step({ number, title, children }: { number: number; title: string; children: React.ReactNode }) {
  return (
    <li className="flex gap-3">
      <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-hk-navy font-mono text-[11px] font-bold text-white">{number}</span>
      <div className="min-w-0 flex-1">
        <p className="font-bold text-hk-navy">{title}</p>
        {children}
      </div>
    </li>
  );
}

type EmulatorOptions = { password?: string; tls?: boolean; port?: number };

// Emulátor IQRF gatewaye z repozitáře — vyzkoušení celého postupu bez hardwaru.
// Přes TLS ověří certifikát brokeru (stažený smartaudit-ca.crt vedle příkazu).
export function emulatorCommand(brokerHost: string, gatewayId: string, { password, tls = false, port = 1883 }: EmulatorOptions = {}) {
  const connection = tls ? `--host ${brokerHost} --port ${port} --tls --cafile smartaudit-ca.crt` : `--host ${brokerHost} --port ${port}`;

  return `pip install -r tools/iqrf-emulator/requirements.txt\npython tools/iqrf-emulator/emulator.py ${connection} --gateway ${gatewayId} --password ${password ?? "<heslo gatewaye>"}`;
}

export function Code({ code }: { code: string }) {
  return (
    <div className="mt-2 overflow-hidden rounded-xl bg-hk-navy">
      <div className="flex justify-end border-b border-white/10 px-2 py-1.5">
        <CopyButton text={code} />
      </div>
      <pre className="overflow-x-auto p-3 font-mono text-[11.5px] leading-relaxed text-white/90">
        <code>{code}</code>
      </pre>
    </div>
  );
}
