# 🪂 CS2 Parachute

Ein stabiles und serverfreundliches **Parachute-Plugin für Counter-Strike 2**, basierend auf **CounterStrikeSharp**.  
Spieler können beim Fallen ein kontrolliertes Gleiten aktivieren – optional mit einem Parachute-3D-Model.

**Aktuelle Version:** 1.6.1

---

## ✅ Eigenschaften

- ⚙️ Gebaut für .NET 10
- 🔄 Hot-Reload-sicher
- 🛡️ Keine KeyNotFound-, Disconnect- oder Respawn-Crashes
- 🧠 Pawn-basierte Physik mit Gültigkeitsprüfung
- 🧹 Sauberes Aufräumen bei Rundenstart, Mapwechsel und Plugin-Unload
- 🪶 Ressourcenschonend: Rechteprüfung nur beim Öffnen und alle 64 Ticks

---

## ✨ Features

- 🪂 Parachute beim Fallen über **USE-Taste (E)**
- 🎮 Optionales Parachute-3D-Model, das dem Spieler folgt
- 🔧 Vollständig konfigurierbar
- 🔐 Admin-Flag-Unterstützung
- 🪜 Kein Auslösen auf Leitern, im Wasser oder im Noclip
- ♻️ Stabil bei Death, Disconnect, Rundenwechsel und Reload

---

## 📦 Voraussetzungen

- 🎯 Counter-Strike 2 Server
- 🧩 CounterStrikeSharp (API-Version 358 oder neuer)
- ⚙️ .NET-Laufzeit von CounterStrikeSharp (.NET 10 oder neuer)
- 🖥️ Windows oder Linux

---

## 🛠️ Installation

1. Plugin bauen oder Release-DLL herunterladen  
2. `Parachute.dll` kopieren nach:  
   ```
   csgo/addons/counterstrikesharp/plugins/Parachute/
   ```
3. Server starten  
4. Konfiguration wird automatisch erstellt unter:  
   ```
   csgo/addons/counterstrikesharp/configs/plugins/Parachute/Parachute.json
   ```

### Update von 1.6.0

Einfach die `Parachute.dll` ersetzen. Die bestehende `Parachute.json` bleibt unverändert gültig.  
Eine eventuell vorhandene `Parachute.deps.json` im Plugin-Ordner kann gelöscht werden – sie wird nicht mehr benötigt.

---

## ⚙️ Konfiguration (Parachute.json)

```json
{
  "Enabled": true,
  "DecreaseVec": 50,
  "Linear": true,
  "FallSpeed": 100,
  "AccessFlag": "",
  "TeleportTicks": 300,
  "ParachuteModelEnabled": true,
  "ParachuteModel": "models/props_survival/parachute/chute.vmdl",
  "ConfigVersion": 1
}
```

---

## 🧩 Optionen

**Enabled**  
Plugin aktivieren oder deaktivieren  

**DecreaseVec**  
Abbremsung der Fallgeschwindigkeit pro Tick (bei `0` wird sofort auf `FallSpeed` gesetzt)  

**Linear**  
`true` = konstantes Gleiten mit `FallSpeed`, `false` = progressives Abbremsen über `DecreaseVec`  

**FallSpeed**  
Maximale Fallgeschwindigkeit beim Gleiten  

**AccessFlag**  
Benötigtes Admin-Flag, z. B. `@css/vip` (leer = alle Spieler)  

**TeleportTicks**  
Tick-Intervall zur Positionsstabilisierung  

**ParachuteModelEnabled**  
Parachute-Model anzeigen  

**ParachuteModel**  
Pfad zum `.vmdl`-Model  

**ConfigVersion**  
Wird von CounterStrikeSharp verwaltet – nicht ändern  

---

## 🎮 Benutzung

1. Springen oder fallen  
2. **USE-Taste (E)** gedrückt halten  
3. Spieler gleitet kontrolliert nach unten  
4. Taste loslassen oder landen → normales Verhalten  

---


> Tipp: In der `Parachute.csproj` die Version von `CounterStrikeSharp.API` auf die Version setzen, die auf deinem Server installiert ist.

---

## 📝 Changelog

**1.6.1**
- Möglichen Absturz beim Respawn behoben (Pawn-Prüfung)
- Fallschirm-Model über Entity-Handle verwaltet – sicher bei Runden- und Mapwechsel
- Kein Fallschirm mehr auf Leitern, im Wasser oder im Noclip
- Model folgt dem Spieler auch beim Steigen (z. B. Jumppads)
- Rechteprüfung nur beim Öffnen und alle 64 Ticks
- Wirkungslose `GravityScale`-Änderung entfernt (Fallverhalten unverändert)
- DLL-Version entspricht der Plugin-Version, keine `deps.json` mehr

---

## 👨‍💻 Autoren

- **Franc1sco Franug** – Originalidee  
- **ddbmaster** – Stabilisierung, .NET 10, CSS-Anpassungen  

---

## 📄 Lizenz

Open Source – Nutzung und Anpassung erlaubt.
