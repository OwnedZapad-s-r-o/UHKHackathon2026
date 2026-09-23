import "server-only";

// Přepošle PDF z API prohlížeči; chybu API převede na čitelnou českou odpověď
export async function pdfResponse(upstream: Response) {
  if (!upstream.ok) {
    const error = (await upstream.json().catch(() => null)) as { message?: string } | null;
    return new Response(error?.message ?? "Report se nepodařilo vytvořit.", {
      status: upstream.status,
      headers: { "Content-Type": "text/plain; charset=utf-8" },
    });
  }

  return new Response(upstream.body, {
    status: 200,
    headers: {
      "Content-Type": "application/pdf",
      "Content-Disposition": upstream.headers.get("Content-Disposition") ?? 'attachment; filename="smartaudit-report.pdf"',
      "Cache-Control": "no-store",
    },
  });
}
