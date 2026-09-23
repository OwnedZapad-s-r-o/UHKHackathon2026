const path = require("path");
const playwright = require(path.resolve(__dirname, "../frontend/node_modules/playwright"));
const { chromium } = playwright;

(async () => {
  console.log("Launching browser for screenshot capture...");
  const browser = await chromium.launch({
    channel: "msedge",
    headless: true,
  });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
  });
  const page = await context.newPage();

  // 1. Login Page
  console.log("Capturing 01-login.png...");
  await page.goto("http://localhost:3000/login", { waitUntil: "networkidle" });
  await page.screenshot({ path: "docs/images/01-login.png" });

  // Click Demo Kraj button
  console.log("Logging in as Demo Kraj...");
  const krajButton = page.locator('button:has-text("Jsem z kraje"), button[value="Kraj"]').first();
  if (await krajButton.isVisible()) {
    await krajButton.click();
    await page.waitForTimeout(3000);
  }

  // 2. 2D Mapa celého kraje (panel skrytý pro čistý pohled)
  console.log("Capturing 02-mapa-2d-prehled.png...");
  await page.goto("http://localhost:3000/?inspekce=0", { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  await page.screenshot({ path: "docs/images/02-mapa-2d-prehled.png" });

  // Střední uměleckoprůmyslová škola hudebních nástrojů a nábytku, Hradec Králové (u UHK)
  const hudebniNastrojeId = "01a0cd79-08d2-7178-8369-46292a8d2b61";

  // 3. 3D pohled na budovu hudebních nástrojů na mapě
  console.log("Capturing 03-mapa-3d-budova.png...");
  await page.goto(`http://localhost:3000/?objekt=${hudebniNastrojeId}&inspekce=0`, { waitUntil: "networkidle" });
  await page.waitForTimeout(4500); // Wait for flyTo and 3D rendering
  await page.screenshot({ path: "docs/images/03-mapa-3d-budova.png" });

  // 4. Inspekční panel - Telemetrie budovy hudebních nástrojů
  console.log("Capturing 04-inspekce-telemetrie.png...");
  await page.goto(`http://localhost:3000/?objekt=${hudebniNastrojeId}&panel=telemetrie`, { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  await page.screenshot({ path: "docs/images/04-inspekce-telemetrie.png" });

  // 5. Inspekční panel - Karta budovy ČÚZK a Schematický řez budovou hudebních nástrojů
  console.log("Capturing 05-inspekce-budova-rez-cuzk.png...");
  await page.goto(`http://localhost:3000/?objekt=${hudebniNastrojeId}&panel=budova`, { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  await page.screenshot({ path: "docs/images/05-inspekce-budova-rez-cuzk.png" });

  // 6. Inspekční panel - AI Analýza a Audit budovy hudebních nástrojů
  console.log("Capturing 06-inspekce-audit-ai.png...");
  await page.goto(`http://localhost:3000/?objekt=${hudebniNastrojeId}&panel=audit`, { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  await page.screenshot({ path: "docs/images/06-inspekce-audit-ai.png" });

  // 7. Portfolio page
  console.log("Capturing 07-portfolio.png...");
  await page.goto("http://localhost:3000/portfolio", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  await page.screenshot({ path: "docs/images/07-portfolio.png" });

  // 8. Anomálie page
  console.log("Capturing 08-anomalie.png...");
  await page.goto("http://localhost:3000/anomalie", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  await page.screenshot({ path: "docs/images/08-anomalie.png" });

  // 9. Reporty page
  console.log("Capturing 09-reporty.png...");
  await page.goto("http://localhost:3000/reporty", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  await page.screenshot({ path: "docs/images/09-reporty.png" });

  // 10. Otevřená data page
  console.log("Capturing 10-otevrena-data.png...");
  await page.goto("http://localhost:3000/otevrena-data", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  await page.screenshot({ path: "docs/images/10-otevrena-data.png" });

  // 11. Zdroje dat page
  console.log("Capturing 11-zdroje-dat.png...");
  await page.goto("http://localhost:3000/zdroje-dat", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  await page.screenshot({ path: "docs/images/11-zdroje-dat.png" });

  await browser.close();
  console.log("All screenshots for Hudební nástroje u UHK successfully captured!");
})();
