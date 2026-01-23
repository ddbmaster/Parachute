# 🪂 CS2 Parachute

Ein stabiles und serverfreundliches **Parachute-Plugin für Counter-Strike 2**, basierend auf **CounterStrikeSharp**.  
Spieler können beim Fallen ein Fallschirm-ähnliches Gleiten aktivieren, inkl. optionalem Parachute-Model.

✔ .NET 8  
✔ Hot-Reload-sicher  
✔ Keine KeyNotFound-Crashes  
✔ Zukunftssicher (Pawn-Teleport)  

---

## ✨ Features

- 🪂 Parachute beim Fallen per **USE-Taste**
- 🎮 Optionales **Parachute-3D-Model**
- ⚙️ Vollständig konfigurierbar (Fallgeschwindigkeit, Linearität, Tick-Teleport)
- 🔐 **Admin-Flag Support**
- ♻️ Hot-Reload & Disconnect-sicher
- 🚫 Keine Dictionary-Race-Conditions
- 🧠 Pawn-basierte Physik (CSS-konform)

---

## 📦 Voraussetzungen

- Counter-Strike 2 Server
- **CounterStrikeSharp**
- .NET **8.0**
- Windows oder Linux Server

---

## 🛠 Installation

1. Plugin bauen oder Release-DLL herunterladen
2. DLL kopieren nach:
csgo/addons/counterstrikesharp/plugins/Parachute/
3. Server starten
4. Config wird automatisch generiert unter:

csgo/addons/counterstrikesharp/configs/plugins/Parachute/Parachute.json


---

## ⚙️ Konfiguration (`Parachute.json`)

```json
{
"Enabled": true,
"DecreaseVec": 50,
"Linear": true,
"FallSpeed": 100,
"AccessFlag": "",
"TeleportTicks": 300,
"ParachuteModelEnabled": true,
"ParachuteModel": "models/props_survival/parachute/chute.vmdl"
}

🔧 Config-Optionen erklärt

| Option                  | Beschreibung                                                  |
| ----------------------- | ------------------------------------------------------------- |
| `Enabled`               | Plugin aktivieren/deaktivieren                                |
| `DecreaseVec`           | Wie stark die Fallgeschwindigkeit pro Tick reduziert wird     |
| `Linear`                | Lineares Gleiten (true) oder progressives Abbremsen           |
| `FallSpeed`             | Maximale Fallgeschwindigkeit                                  |
| `AccessFlag`            | Benötigtes Admin-Flag (leer = alle Spieler)                   |
| `TeleportTicks`         | Nach wie vielen Ticks ein Teleport zur Stabilisierung erfolgt |
| `ParachuteModelEnabled` | Parachute-Model anzeigen                                      |
| `ParachuteModel`        | Pfad zum `.vmdl`-Model                                        |


🎮 Benutzung
Springen / Fallen
USE-Taste (E) gedrückt halten
Spieler gleitet kontrolliert nach unten
Loslassen → normales Fallen

Ende
