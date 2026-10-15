## Skutečná čidla IQRF
SmartAudit umí přijímat měření ze skutečných bezdrátových čidel IQRF (teplota, CO₂, vlhkost, osvětlení, hluk, pohyb). IQRF gateway se připojí k MQTT brokeru aplikace, ta sama najde uzly a senzory a správce je přiřadí do místností. Přiřazené čidlo nahradí simulaci místnosti skutečným měřením a zpřesní odhad ztráty.

## Jak připojit gatewayi
Na stránce Senzory je návod k připojení: broker, uživatel a heslo jako tři karty ke zkopírování. Gateway se připojuje přes MQTT s TLS na port 8883. Po připojení se objeví v seznamu gatewayí a její uzly jde přiřadit do místností — celý uzel jedním klikem „Přiřadit vše“.
→ [Senzory](/senzory)

## Párování nové gatewaye heslem z nálepky
Nová gateway z výroby se ohlásí přes sdílený párovací účet a objeví se v panelu „Nová zařízení online“. Správce vybere budovu a opíše heslo z nálepky na zařízení. Heslo nikdy nejde po síti ani do databáze — aplikace jím zašifruje nabídku přihlašovacích údajů (AES-GCM, klíč PBKDF2), kterou umí rozšifrovat jen gateway se stejnou nálepkou.

## Průvodce nastavením čidel
Interaktivní prohlídka provede nastavením čidel krok za krokem: testovací síť, připojení gatewaye, přiřazení uzlu a výsledek v mapě. Spustí se tlačítkem průvodce na stránce Senzory a pokračuje napříč stránkami.
→ [Spustit průvodce čidly](/senzory?pruvodce=1)

## Testovací síť bez hardwaru
Jedním klikem na stránce Senzory vznikne pět virtuálních IQRF gatewayí v pěti okresech. Posílají živá data celou skutečnou cestou přes MQTT, takže jde vyzkoušet párování i přiřazení místností bez fyzického zařízení. Na stránce Demo správa jdou vyrobit virtuální gatewaye a čidla s nastaveným chováním (funguje útlum, přetápí, …).
→ [Demo správa](/demo)

## Jak čidla opravila odhad ztráty
Přehled „Jak čidla opravila odhad ztráty“ v Demo správě ukáže u každé změřené budovy ztrátu před čidlem (odhad ze simulace) a teď (z měření). Skutečné čidlo ztrátu upřesňuje postupně během 7 dní.
