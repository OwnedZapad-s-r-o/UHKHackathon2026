"use client";

import { useEffect, useState } from "react";
import type { DemoOverview } from "@/types/api";
import { CreateGatewayForm } from "./create-gateway-form";
import { CreateNodeForm } from "./create-node-form";
import { DemoDeviceList } from "./demo-device-list";
import { LossCorrections } from "./loss-corrections";

// Párování i přepočet auditu trvají sekundy — obnova po 3 s ukáže každý krok skoro hned
const REFRESH_MS = 3000;

// Demo správa naživo: formuláře vlevo mění zařízení, seznamy se samy obnovují, jak zařízení postupuje párováním
export function DemoWorkspace({ initial }: { initial: DemoOverview }) {
  const [overview, setOverview] = useState(initial);

  async function reload() {
    const response = await fetch("/demo/stav", { cache: "no-store" }).catch(() => null);
    if (response?.ok) setOverview((await response.json()) as DemoOverview);
  }

  useEffect(() => {
    const timer = setInterval(reload, REFRESH_MS);
    return () => clearInterval(timer);
  }, []);

  return (
    <div className="mt-6 grid gap-4 xl:grid-cols-[1fr_24rem]">
      <div className="min-w-0 space-y-4">
        <DemoDeviceList devices={overview.devices} onChanged={reload} />
        <LossCorrections corrections={overview.corrections} />
      </div>

      <div className="h-fit min-w-0 space-y-4 xl:sticky xl:top-0">
        <CreateGatewayForm templates={overview.templates} onCreated={reload} />
        <CreateNodeForm templates={overview.templates} gateways={overview.gateways} onCreated={reload} />
      </div>
    </div>
  );
}
