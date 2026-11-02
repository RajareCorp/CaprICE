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
* **Redimensionnement** : La fenêtre sans bordure peut être redimensionnée par ses bords et ses coins ; sa taille minimale est de 1000 × 650 px.
* **Fermeture rapide** : Bouton `[X]` dédié ou touche **Échap** (`ESC`).
* **Minimisation** : Bouton `[_]` pour réduire la fenêtre dans la barre des tâches.

### 📐 Dimensions & Lisibilité

* Résolution augmentée à **1240 x 800 px** pour une consultation optimale du trafic et de l'inspecteur de payload.

### 🛡️ Paramètres DLP

Le tiroir **SETTINGS** présente les règles en deux colonnes, dans le style du Mainframe. En mode Enforce DLP, l'inspection porte sur tout le contexte textuel construit pour le modèle : instructions système, historique, note mémoire, contexte rappelé, arguments d'outils et texte multimodal. Les pixels des images jointes ne sont pas analysés par OCR ; si une donnée sensible est détectée dans le texte multimodal, l'envoi est bloqué afin de ne pas corrompre son format.

| Type de Donnée | Description / Formats Couverts |
| --- | --- |
| **Téléphones** | Formats français (`06`, `07`, `+33`...) et internationaux |
| **Adresses IP** | Détection automatique des adresses IPv4 et IPv6 |
| **Adresses postales** | Rues, avenues, boulevards, places, codes postaux |
| **Noms & Prénoms** | Champ personnalisé (ex: `Dupont, Jean, Alice`) |
| **Adresses privées** | Champ personnalisé (ex: `12 rue de la Paix, 75001 Paris`) |
| **Identifiants secrets** | Clés API, tokens secrets, mots de passe, emails, cartes bancaires & chemins Windows (`C:\Users\...`) |

* **Sauvegarde** : Le bouton **ENREGISTRER** persiste les règles dans `caprice_settings.json` pour un rechargement automatique au démarrage.
* Les noms et adresses personnalisés ne sont pas recopiés dans les libellés DLP du journal.

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
| 🛡️ ENFORCE DLP     | Masque les données sensibles du contexte sortant.  |
| 🔒 STRICT LOCAL   | Bloque les URL externes ; autorise les loopbacks.  |
| ✋ ASK CONFIRM    | Confirme chaque connexion externe avec son hôte.   |
+-------------------+---------------------------------------------------+

```

1. **MONITOR ONLY (Audit Passif)**
N'interrompt jamais les requêtes. Observe et enregistre chaque échange en arrière-plan.
2. **ENFORCE DLP (Prévention de Fuite de Données)**
Scanne et masque les données sensibles dans le prompt complet avant un envoi externe. Si le masquage est désactivé et qu'une donnée sensible est détectée, la requête est bloquée.
3. **STRICT LOCAL (Verrou Cloud / Offline)**
Bloque immédiatement toute connexion vers une adresse non-loopback, y compris les connexions vocales. Les serveurs locaux loopback restent autorisés.
4. **ASK CONFIRMATION (Contrôle Manuel)**
Demande l'approbation avant chaque connexion externe de Brain ou Voice, en affichant l'URL réelle et un aperçu du contexte sortant. Les clés vocales ne sont jamais journalisées.

La détection classe les fournisseurs à partir de l'hôte URI exact (ou de ses sous-domaines), sans confondre un domaine ressemblant avec un fournisseur reconnu.

### 🧹 Purge mémoire

Depuis le tiroir **MEMORY**, **PURGER LES DONNÉES LOCALES** supprime le chat, les notes locales et les transcriptions, puis vide l'historique, les captures et le cache mémoire actifs. La note sélectionnée dans un vault externe est préservée ; elle reste active et pourra être relue lors des prochaines requêtes.

### ✍️ Lecture des prompts et de la mémoire

Les éditeurs **PROMPTS** et **MEMORY** replient les longues lignes, conservent les retours à la ligne et mettent en couleur les titres Markdown, citations et listes. La mise en couleur est uniquement visuelle : le texte brut est celui qui est appliqué ou sauvegardé.

Les trois sections du prompt personnalisé sont enregistrées avec les paramètres CaprICE lorsque **APPLIQUER LES HOOKS PROMPT** est sélectionné. Les retours à la ligne sont préservés au redémarrage.

### ⚡ Actions locales

Dans **ACTIONS**, associez un mot-clé exact à un fichier choisi localement. Quand ce mot est reconnu comme demande, CaprICE demande à Windows d'ouvrir le fichier avec l'application associée ; selon son type, certains fichiers comme les scripts peuvent s'exécuter. Le mot-clé **Performance** est réservé : il renvoie localement les mesures CPU, RAM et GPU 3D disponibles, sans transmettre ces valeurs au service d'IA. Les mesures GPU dépendent des compteurs exposés par Windows.

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

L'icône `Capr-ICE.ico` à la racine du dépôt est intégrée à l'exécutable lors de la compilation et utilisée par la fenêtre CaprICE.

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
* **Resize**: The borderless window can be resized from its edges and corners; minimum size is 1000 × 650 px.

### 📐 Expanded Dimensions

* Higher resolution at **1240 x 800 px**, offering an expanded workspace for traffic logs and payload inspection.

### 🛡️ DLP Settings

The **SETTINGS** drawer uses a two-column layout inspired by Capri Mainframe. Enforce DLP scans the complete textual model context: system instructions, conversation history, memory note, recalled context, tool arguments, and multimodal text. Image pixels are not OCR-scanned; if sensitive data is found in multimodal text, the request is blocked to avoid corrupting its format.

| Data Category | Detection & Masking Details |
| --- | --- |
| **Phone Numbers** | French (`06`, `07`, `+33`...) and international patterns |
| **IP Addresses** | Automatic IPv4 and IPv6 detection |
| **Postal Addresses** | Street names, avenues, boulevards, postal codes |
| **Custom Names** | Dedicated text field for specific names (e.g. `Doe, John, Alice`) |
| **Custom Addresses** | Dedicated field for private locations (e.g. `10 Downing Street`) |
| **Secrets & Keys** | API keys, secret tokens, passwords, emails, credit cards & Windows user paths (`C:\Users\...`) |

* **Persistence**: Click **SAVE** to write rules to `caprice_settings.json`, automatically reloaded on boot.
* Custom names and addresses are not included in DLP detection labels written to the audit log.

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
| 🛡️ ENFORCE DLP     | Masks sensitive data in the complete outgoing input. |
| 🔒 STRICT LOCAL   | Blocks non-loopback URLs; loopback stays allowed. |
| ✋ ASK CONFIRM    | Confirms each external connection and its host.    |
+-------------------+---------------------------------------------------+

```

1. **MONITOR ONLY (Passive Audit)**
Passively logs transactions without interfering with Capri operations.
2. **ENFORCE DLP (Data Loss Prevention)**
Scans and masks sensitive data in the complete input before an external request. If masking is disabled and sensitive data is found, the request is blocked.
3. **STRICT LOCAL (Offline / Cloud Lock)**
Blocks every non-loopback connection, including voice connections. Loopback servers remain allowed.
4. **ASK CONFIRMATION (Manual Control)**
Requests approval before each external Brain or Voice connection, showing its real URL and an outgoing-context preview. Voice API keys are never logged.

Provider detection uses the URI host and exact provider-domain boundaries; lookalike domains are classified as unknown external targets.

### 🧹 Memory purge

From **MEMORY**, **PURGER LES DONNÉES LOCALES** removes chat history, local notes and transcripts, then clears active history, sightings and the memory cache. A note selected from an external vault is preserved; it remains active and may be read again in future requests.

### ✍️ Prompt and memory readability

The **PROMPTS** and **MEMORY** editors wrap long lines, preserve line breaks, and color Markdown headings, quotes, and lists. Formatting is visual only; applied and saved content remains plain text.

All three custom prompt sections are stored with CaprICE settings when **APPLIQUER LES HOOKS PROMPT** is selected. Line breaks are preserved across restarts.

### ⚡ Local actions

In **ACTIONS**, map an exact keyword to a locally selected file. When the keyword is recognized as a request, CaprICE asks Windows to open it with its associated application; depending on its type, files such as scripts may execute. **Performance** is reserved and returns available CPU, RAM, and 3D GPU measurements locally, without sending them to the AI service. GPU measurements depend on counters exposed by Windows.

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