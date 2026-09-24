import { CopyButton } from "../../_components/copy-button";

type SetupGuideProps = {
  brokerHost: string;
  brokerPort: number;
  gatewayId?: string;
};

// Návod pro IQRF Gateway Daemon: kam se připojit a co nastavit v IQRF Gateway Webapp → Konfigurace → Messaging → MQTT
export function SetupGuide({ brokerHost, brokerPort, gatewayId = "{id-gatewaye}" }: SetupGuideProps) {
  const requestTopic = `gateway/${gatewayId}/iqrf/requests`;
  const responseTopic = `gateway/${gatewayId}/iqrf/responses`;

  const daemonConfig = JSON.stringify(
    {
      component: "iqrf::MqttMessaging",
      instance: "SmartAuditKHK",
      BrokerAddr: `tcp://${brokerHost}:${brokerPort}`,
      ClientId: gatewayId,
      TopicRequest: requestTopic,
      TopicResponse: responseTopic,
      Qos: 1,
      Persistence: 1,
      EnabledSSL: false,
      acceptAsyncMsg: true,
    },
    null,
    2,
  );

  const emulator = `pip install -r tools/iqrf-emulator/requirements.txt\npython tools/iqrf-emulator/emulator.py --host ${brokerHost} --gateway ${gatewayId}`;

  return (
    <ol className="space-y-4 text-[13px] text-slate-700">
      <Step number={1} title="Gateway se připojí k brokeru MQTT této aplikace">
        <dl className="mt-2 grid gap-x-4 gap-y-1 font-mono text-[12px] sm:grid-cols-[auto_1fr]">
          <dt className="text-slate-500">broker</dt>
          <dd>
            tcp://{brokerHost}:{brokerPort}
          </dd>
          <dt className="text-slate-500">dotazy</dt>
          <dd className="break-all">{requestTopic}</dd>
          <dt className="text-slate-500">odpovědi</dt>
          <dd className="break-all">{responseTopic}</dd>
        </dl>
        <p className="mt-2 text-slate-600">
          V IQRF Gateway Webapp: <b>Konfigurace → Messaging → MQTT</b> → nová instance s těmito hodnotami (nebo soubor{" "}
          <code className="font-mono text-[12px]">iqrf__MqttMessaging.json</code>):
        </p>
        <Code code={daemonConfig} />
      </Step>

      <Step number={2} title="Aplikace sama najde uzly a jejich senzory">
        <p className="mt-1 text-slate-600">
          Zeptá se koordinátoru na spárované uzly a pak každého uzlu na hodnoty (IQRF Standard Sensor). Typ senzoru určí veličinu —
          teplota, CO₂, vlhkost, osvětlení, hluk, pohyb…
        </p>
      </Step>

      <Step number={3} title="Přiřaďte kanál do místnosti">
        <p className="mt-1 text-slate-600">
          Od té chvíle se hodnoty ukládají, grafy místnosti pokračují skutečnými daty a simulace pro ni skončí. Audit objektu se přepočítá.
        </p>
      </Step>

      <Step number={4} title="Bez hardwaru: emulátor gatewaye">
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

function Code({ code }: { code: string }) {
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
