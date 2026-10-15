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
  console.log("Navigating to http://localhost:3000/login...");
  await page.goto("http://localhost:3000/login", { waitUntil: "networkidle" });
  await page.screenshot({ path: "docs/images/01-login.png" });

  // Click Demo Kraj button
  console.log("Logging in as Demo Kraj...");
  const krajButton = page.locator('button:has-text("Jsem z kraje"), button[value="Kraj"]').first();
  if (await krajButton.isVisible()) {
    await krajButton.click();
    await page.waitForTimeout(3000);
  }

  // 2. Main Map Overview - Full width (panel hidden: inspekce=0)
  console.log("Navigating to Map (Full Width)...");
  await page.goto("http://localhost:3000/?inspekce=0", { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  console.log("Capturing 02-mapa-prehled.png...");
  await page.screenshot({ path: "docs/images/02-mapa-prehled.png" });

  // 3. Inspection Panel - Telemetrie Tab
  console.log("Navigating to Map with Inspection Panel (Telemetrie)...");
  await page.goto("http://localhost:3000/?panel=telemetrie", { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  console.log("Capturing 03-inspekce-telemetrie.png...");
  await page.screenshot({ path: "docs/images/03-inspekce-telemetrie.png" });

  // 4. Inspection Panel - Řez Budovou Tab
  console.log("Navigating to Inspection Panel (Řez budovou)...");
  await page.goto("http://localhost:3000/?panel=rez", { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  console.log("Capturing 04-inspekce-rez.png...");
  await page.screenshot({ path: "docs/images/04-inspekce-rez.png" });

  // 5. Inspection Panel - Karta budovy ČÚZK Tab
  console.log("Navigating to Inspection Panel (Karta budovy ČÚZK)...");
  await page.goto("http://localhost:3000/?panel=karta", { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  console.log("Capturing 05-inspekce-karta-cuzk.png...");
  await page.screenshot({ path: "docs/images/05-inspekce-karta-cuzk.png" });

  // 6. Inspection Panel - AI Analýza Tab
  console.log("Navigating to Inspection Panel (AI Analýza)...");
  await page.goto("http://localhost:3000/?panel=ai", { waitUntil: "networkidle" });
  await page.waitForTimeout(4000);
  console.log("Capturing 06-inspekce-ai-analyza.png...");
  await page.screenshot({ path: "docs/images/06-inspekce-ai-analyza.png" });

  // 7. Portfolio page
  console.log("Navigating to Portfolio...");
  await page.goto("http://localhost:3000/portfolio", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  console.log("Capturing 07-portfolio.png...");
  await page.screenshot({ path: "docs/images/07-portfolio.png" });

  // 8. Anomálie page
  console.log("Navigating to Anomálie...");
  await page.goto("http://localhost:3000/anomalie", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  console.log("Capturing 08-anomalie.png...");
  await page.screenshot({ path: "docs/images/08-anomalie.png" });

  // 9. Reporty page
  console.log("Navigating to Reporty...");
  await page.goto("http://localhost:3000/reporty", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  console.log("Capturing 09-reporty.png...");
  await page.screenshot({ path: "docs/images/09-reporty.png" });

  // 10. Otevřená data page
  console.log("Navigating to Otevřená data...");
  await page.goto("http://localhost:3000/otevrena-data", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  console.log("Capturing 10-otevrena-data.png...");
  await page.screenshot({ path: "docs/images/10-otevrena-data.png" });

  // 11. Zdroje dat page
  console.log("Navigating to Zdroje dat...");
  await page.goto("http://localhost:3000/zdroje-dat", { waitUntil: "networkidle" });
  await page.waitForTimeout(3000);
  console.log("Capturing 11-zdroje-dat.png...");
  await page.screenshot({ path: "docs/images/11-zdroje-dat.png" });

  await browser.close();
  console.log("Done taking screenshots successfully!");
})();
