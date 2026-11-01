# 🛡️ CaprICE

**AI Security & Privacy Guard for Capri Desktop**

---

[Français](https://www.google.com/search?q=%23-version-fran%C3%A7aise) • [English](https://www.google.com/search?q=%23-english-version)

---

# 🇫🇷 Version Française

## 📋 Présentation

**CaprICE** est un injecteur et mod externe autonome tout-en-un conçu pour **Capri Desktop**. Il intercepte en mémoire l'ensemble des communications (locales et cloud) échangées avec les intelligences artificielles afin d'assurer la **sécurité**, la **confidentialité** et le **contrôle total des données**.

> **🔒 100% Externe & Non-Invasif**
> Aucune ligne du code source de Capri n'a été modifiée. Le binaire `Capri.exe` reste intact.

---

## 🚀 Nouveautés & Améliorations Récentes

### Contrôles de Fenêtre & Ergonomie

* **Déplacement fluide** : Glissez-déposez la fenêtre via la bannière supérieure (`ReleaseCapture` + `SendMessage`).
* **Fermeture rapide** : Bouton `[X]` dédié ou touche **Échap** (`ESC`).
* **Minimisation** : Bouton `[_]` pour réduire la fenêtre dans la barre des tâches.

### 📐 Dimensions & Lisibilité

* Résolution augmentée à **1240 x 800 px** pour une consultation optimale du trafic et de l'inspecteur de payload.

### 🛡️ Panneau "PARAMÈTRES DLP" (Masquage PII Étendu)

Affichez le panneau latéral via le bouton **PARAMÈTRES DLP** pour configurer la protection contre les fuites de données :

| Type de Donnée | Description / Formats Couverts |
| --- | --- |
| **Téléphones** | Formats français (`06`, `07`, `+33`...) et internationaux |
| **Adresses IP** | Détection automatique des adresses IPv4 et IPv6 |
| **Adresses postales** | Rues, avenues, boulevards, places, codes postaux |
| **Noms & Prénoms** | Champ personnalisé (ex: `Dupont, Jean, Alice`) |
| **Adresses privées** | Champ personnalisé (ex: `12 rue de la Paix, 75001 Paris`) |
| **Identifiants secrets** | Clés API, tokens secrets, mots de passe, emails, cartes bancaires & chemins Windows (`C:\Users\...`) |

* **Sauvegarde automatique** : Le bouton **"ENREGISTRER LES PARAMÈTRES"** persiste vos règles dans `caprice_settings.json` pour un rechargement automatique au démarrage.

### 💡 Assistance & Interface Dynamique

* **Mode d'emploi** : Bouton **`? GUIDE`** ouvrant la documentation interactive.
* **Bandeau temps réel** : Résumé explicatif sous le sélecteur de politique.

---

## 🔰 Les 4 Politiques de Sécurité

```
+-----------------------------------------------------------------------+
|                         POLITIQUES DE SÉCURITÉ                        |
+-------------------+---------------------------------------------------+
| 🔍 MONITOR ONLY   | Audit passif sans interruption des requêtes.      |
| 🛡️ ENFORCE DLP     | Masquage automatique des PII avant envoi cloud.   |
| 🔒 STRICT LOCAL   | Bloque le cloud ; seul Ollama (127.0.0.1) passe.  |
| ✋ ASK CONFIRM    | Confirmation manuelle avec prévisualisation.     |
+-------------------+---------------------------------------------------+

```

1. **MONITOR ONLY (Audit Passif)**
N'interrompt jamais les requêtes. Observe et enregistre chaque échange en arrière-plan.
2. **ENFORCE DLP (Prévention de Fuite de Données)**
Scanne chaque prompt avant l'envoi vers le cloud et masque automatiquement les données sensibles.
3. **STRICT LOCAL (Verrou Cloud / Offline)**
Bloque immédiatement toute connexion vers les IA cloud externes. Seul Ollama local (`127.0.0.1`) est autorisé.
4. **ASK CONFIRMATION (Contrôle Manuel)**
Ouvre une fenêtre d'approbation avec prévisualisation du prompt (`Autoriser` ou `Bloquer`).

---

## 🗂️ Structure du projet

```
caprice/
├─ build-caprice.ps1     Script de compilation (compile src/**/*.cs)
├─ lib/                  0Harmony.dll
├─ docs/ARCHITECTURE.md  Détail de l'architecture et « où modifier quoi »
└─ src/
   ├─ Program.cs         Point d'entrée
   ├─ Startup/           Localisation et lancement de Capri
   ├─ Security/          Politiques, moteur DLP, réglages, journal d'audit
   ├─ Hooks/             Patchs Harmony (Brain, Voice, Persona)
   ├─ Services/          Mémoire (.md) et prompt d'origine
   └─ UI/                Fenêtre, tiroirs, dialogues, thème néon
```

## 🛠️ Lancement & Compilation

### Exécution du Binaire

* **Depuis le dossier de build Windows** :
```powershell
.\out\windows\CaprICE.exe

```


* **Depuis le dossier source `caprice/**` :
```powershell
.\caprice\CaprICE.exe

```



### Recompilation

```powershell
powershell -ExecutionPolicy Bypass -File caprice\build-caprice.ps1

```

---

---

# 🇬🇧 English Version

## 📋 Overview

**CaprICE** is an all-in-one standalone external mod and injector built for **Capri Desktop**. It intercepts all in-memory AI communications (local and cloud) to guarantee **security**, **privacy**, and **full data control**.

> **🔒 100% External & Non-Invasive**
> Zero modifications to Capri's source code. The `Capri.exe` binary remains completely untouched.

---

## 🚀 Key Features & Updates

### 🪟 Window Controls & Ergonomics

* **Smooth movement**: Drag-and-drop support across the entire top banner (`ReleaseCapture` + `SendMessage`).
* **Quick close**: Dedicated `[X]` button or **Escape** (`ESC`) key.
* **Minimize**: `[_]` button to minimize directly to the taskbar.

### 📐 Expanded Dimensions

* Higher resolution at **1240 x 800 px**, offering an expanded workspace for traffic logs and payload inspection.

### 🛡️ Extended DLP Settings Panel (PII Masking)

Open the lateral configuration drawer by clicking **DLP SETTINGS**:

| Data Category | Detection & Masking Details |
| --- | --- |
| **Phone Numbers** | French (`06`, `07`, `+33`...) and international patterns |
| **IP Addresses** | Automatic IPv4 and IPv6 detection |
| **Postal Addresses** | Street names, avenues, boulevards, postal codes |
| **Custom Names** | Dedicated text field for specific names (e.g. `Doe, John, Alice`) |
| **Custom Addresses** | Dedicated field for private locations (e.g. `10 Downing Street`) |
| **Secrets & Keys** | API keys, secret tokens, passwords, emails, credit cards & Windows user paths (`C:\Users\...`) |

* **Auto-Persistence**: Click **"SAVE SETTINGS"** to write rules to `caprice_settings.json`, automatically reloaded on boot.

### 💡 Dynamic UI & In-App Guide

* **Interactive Guide**: Click **`? GUIDE`** to open detailed documentation for each mode.
* **Live Banner**: Real-time summary strip explaining the active policy right below the selector.

---

## 🔰 The 4 Security Policies

```
+-----------------------------------------------------------------------+
|                          SECURITY POLICIES                            |
+-------------------+---------------------------------------------------+
| 🔍 MONITOR ONLY   | Passive audit with zero request interruption.      |
| 🛡️ ENFORCE DLP     | Automatic PII scrubbing before cloud upload.      |
| 🔒 STRICT LOCAL   | Cloud block; only local Ollama (127.0.0.1) runs.  |
| ✋ ASK CONFIRM    | Manual prompt inspection and approval workflow.   |
+-------------------+---------------------------------------------------+

```

1. **MONITOR ONLY (Passive Audit)**
Passively logs transactions without interfering with Capri operations.
2. **ENFORCE DLP (Data Loss Prevention)**
Scans every prompt prior to cloud transmission and strips sensitive information.
3. **STRICT LOCAL (Offline / Cloud Lock)**
Instantly blocks external cloud AI traffic. Exclusively permits local Ollama (`127.0.0.1`).
4. **ASK CONFIRMATION (Manual Control)**
Displays an interactive preview window before sending prompts to external LLMs (`Allow` / `Block`).

---

## 🗂️ Project Structure

```
caprice/
├─ build-caprice.ps1     Build script (compiles src/**/*.cs)
├─ lib/                  0Harmony.dll
├─ docs/ARCHITECTURE.md  Architecture details and "where to change what"
└─ src/
   ├─ Program.cs         Entry point
   ├─ Startup/           Locating and launching Capri
   ├─ Security/          Policies, DLP engine, settings, audit log
   ├─ Hooks/             Harmony patches (Brain, Voice, Persona)
   ├─ Services/          Memory (.md) and original prompt
   └─ UI/                Window, drawers, dialogs, neon theme
```

## 🛠️ Usage & Build

### Running CaprICE

* **From the Windows build output** :
```powershell
.\out\windows\CaprICE.exe

```


* **From the `caprice/` directory** :
```powershell
.\caprice\CaprICE.exe

```



### Rebuilding from Source

```powershell
powershell -ExecutionPolicy Bypass -File caprice\build-caprice.ps1

```