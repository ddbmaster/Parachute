# 🪂 CS2 Parachute

Ein stabiles und serverfreundliches **Parachute-Plugin für Counter-Strike 2**, basierend auf **CounterStrikeSharp**.  
Spieler können beim Fallen ein kontrolliertes Gleiten aktivieren – optional mit einem Parachute-3D-Model.

---

## ✅ Eigenschaften

- ⚙️ .NET 10
- 🔄 Hot-Reload-sicher
- 🛡️ Keine KeyNotFound- oder Disconnect-Crashes
- 🧠 Pawn-basierte Physik (CSS-konform)
- 🚀 Zukunftssicher für kommende CounterStrikeSharp-Versionen

---

## ✨ Features

- 🪂 Parachute beim Fallen über **USE-Taste (E)**
- 🎮 Optionales Parachute-3D-Model
- 🔧 Vollständig konfigurierbar
- 🔐 Admin-Flag-Unterstützung
- ♻️ Stabil bei Death, Disconnect und Reload

---

## 📦 Voraussetzungen

- 🎯 Counter-Strike 2 Server
- 🧩 CounterStrikeSharp
- ⚙️ .NET 10
- 🖥️ Windows oder Linux

---

## 🛠️ Installation

1. Plugin bauen oder Release-DLL herunterladen  
2. DLL kopieren nach:  
   ```
   csgo/addons/counterstrikesharp/plugins/Parachute/
   ```
3. Server starten  
4. Konfiguration wird automatisch erstellt unter:  
   ```
   csgo/addons/counterstrikesharp/configs/plugins/Parachute/Parachute.json
   ```

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
  "ParachuteModel": "models/props_survival/parachute/chute.vmdl"
}
```

---

## 🧩 Optionen

**Enabled**  
Plugin aktivieren oder deaktivieren  

**DecreaseVec**  
Reduzierung der Fallgeschwindigkeit pro Tick  

**Linear**  
Lineares Gleiten oder progressives Abbremsen  

**FallSpeed**  
Maximale Fallgeschwindigkeit  

**AccessFlag**  
Benötigtes Admin-Flag (leer = alle Spieler)  

**TeleportTicks**  
Tick-Intervall zur Positionsstabilisierung  

**ParachuteModelEnabled**  
Parachute-Model anzeigen  

**ParachuteModel**  
Pfad zum `.vmdl`-Model  

---

## 🎮 Benutzung

1. Springen oder fallen  
2. **USE-Taste (E)** gedrückt halten  
3. Spieler gleitet kontrolliert nach unten  
4. Taste loslassen → normales Fallen  

---

## 🧪 Build

```powershell
dotnet build -c Release
```

Ausgabe:
```
bin/Release/net8.0/Parachute.dll
```

---

## 👨‍💻 Autoren

- **Franc1sco Franug** – Originalidee  
- **ddbmaster** – Stabilisierung, .NET 8, CSS-Anpassungen  

---

## 📄 Lizenz

Open Source – Nutzung und Anpassung erlaubt.
