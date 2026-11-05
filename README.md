# CaprICE

<p align="center">
  <strong>AI security and privacy controls for Capri Desktop</strong><br>
  Inspect outgoing AI traffic, protect sensitive data, and manage local context.
</p>

<p align="center">
  <a href="https://github.com/RajareCorp/CaprICE/releases/latest/download/CaprICE-windows.zip"><img src="https://img.shields.io/badge/Download%20for%20Windows-ff2a6d?style=for-the-badge" alt="Download CaprICE for Windows"></a>

<p align="center">
  <a href="https://github.com/RajareCorp/CaprICE/releases/latest"><img src="https://img.shields.io/github/v/release/RajareCorp/CaprICE?label=latest&color=05d9e8&style=flat-square" alt="Latest release"></a>
  <a href="https://github.com/RajareCorp/CaprICE/releases"><img src="https://img.shields.io/github/downloads/RajareCorp/CaprICE/total?color=05d9e8&style=flat-square" alt="Total downloads"></a>
  <a href="https://github.com/RajareCorp/CaprICE/issues"><img src="https://img.shields.io/github/issues/RajareCorp/CaprICE?color=05d9e8&style=flat-square" alt="Open issues"></a>
  <a href="https://github.com/RajareCorp/CaprICE/blob/main/SECURITY.md"><img src="https://img.shields.io/badge/security-policy-05d9e8?style=flat-square" alt="Security policy"></a>
  <a href="https://github.com/RajareCorp/CaprICE/blob/main/CONTRIBUTING.md"><img src="https://img.shields.io/badge/contributions-welcome-05d9e8?style=flat-square" alt="Contributing guidelines"></a>
</p>

<p align="center">
  <a href="#version-francaise">Français</a> · <a href="#english">English</a> · <a href="https://github.com/lachydotmcg/capri-desktop">Capri Desktop</a>
</p>

<a id="version-francaise"></a>

## Présentation

**CaprICE** ajoute des contrôles de sécurité et de confidentialité à [Capri Desktop](https://github.com/lachydotmcg/capri-desktop). Il surveille les requêtes Brain et Voice, inspecte le contexte sortant et permet d'appliquer des règles locales. CaprICE est un projet externe : il ne modifie pas les fichiers de Capri.

### Installation rapide (Windows)

**Option recommandée — installateur et mises à jour :** téléchargez [`setup-caprice.ps1`](https://github.com/RajareCorp/CaprICE/releases/latest/download/setup-caprice.ps1) depuis la dernière release, puis exécutez-le dans PowerShell :

```powershell
powershell -ExecutionPolicy Bypass -File .\setup-caprice.ps1
```

Le script installe CaprICE pour l'utilisateur courant et crée un raccourci dans le menu Démarrer. Relancez-le pour mettre à jour. Fermez CaprICE avant installation ou mise à jour. Capri Desktop n'est pas inclus.

**Installation manuelle :** téléchargez l'[archive Windows](https://github.com/RajareCorp/CaprICE/releases/latest/download/CaprICE-windows.zip), décompressez-la, puis placez `CaprICE.exe` **et** `0Harmony.dll` dans le dossier où Capri Desktop est installé (à côté de `Capri.exe`). Lancez ensuite `CaprICE.exe`. La DLL Harmony est requise par l'application ; ne la séparez pas de l'exécutable.

Si le démarrage automatique de Capri est activé, CaprICE remplace son entrée de démarrage Windows afin que Capri se lance ensuite via CaprICE. Une entrée désactivée n'est pas modifiée. Le désinstalleur restaure l'entrée Capri d'origine si elle pointe toujours vers CaprICE.

Pour désinstaller une installation créée par le script, sans supprimer les données utilisateur :

```powershell
powershell -ExecutionPolicy Bypass -File .\setup-caprice.ps1 -Uninstall
```

### Fonctionnalités

- **Politiques réseau :** surveillance, confirmation des connexions externes, ou blocage de toutes les connexions non-loopback.
- **Prévention des fuites (DLP) :** détecte et peut masquer clés API, mots de passe, chemins Windows, emails, cartes bancaires, téléphones, IP, adresses et valeurs personnalisées.
- **Inspection :** historique des événements, cibles, motifs de décision et aperçu détaillé du contexte.
- **Gestion locale :** consultation des transcriptions, de la mémoire et des prompts personnalisés.
- **Actions locales :** associe un mot-clé à un fichier ouvert par Windows. Les fichiers exécutables, notamment les scripts, peuvent s'exécuter.

> **Limites DLP :** CaprICE inspecte le contenu textuel, pas les pixels des images. Une donnée sensible détectée dans le texte multimodal bloque l'envoi au lieu de modifier son format. La politique de sécurité active détermine quelles requêtes sont bloquées ou masquées.

### Politiques de sécurité

| Mode | Comportement |
| --- | --- |
| **MONITOR ONLY** | Observe et journalise sans bloquer les requêtes. |
| **ENFORCE DLP** | Inspecte le contexte et masque les données détectées ; bloque si le masquage est désactivé ou impossible en contexte multimodal. |
| **STRICT LOCAL** | Bloque les connexions non-loopback, y compris les connexions vocales. |
| **ASK CONFIRMATION** | Demande l'approbation avant une connexion Brain ou Voice externe et affiche l'URL et un aperçu du contexte. |

La détection des fournisseurs se base sur l'hôte réel de l'URL ; les domaines ressemblants ne sont pas assimilés aux fournisseurs reconnus. Les clés API vocales ne sont pas journalisées.

### Configuration

- **SETTINGS** permet d'activer les règles DLP et de définir les noms/adresses personnalisés. Ces valeurs ne sont pas écrites dans les libellés DLP du journal d'audit.
- **PROMPTS** permet d'adapter les sections Personality, Safety et Machinery. Les modifications sont sauvegardées dans les paramètres CaprICE.
- **MEMORY** permet de consulter et sauvegarder la mémoire. La purge des données locales efface l'historique, les notes et les transcriptions concernés ; une note du vault externe sélectionnée est préservée.
- L'action locale **Performance** affiche les mesures CPU, RAM et GPU disponibles sur cette machine. Ces mesures ne sont pas transmises au service d'IA.

### Compiler depuis les sources

Prérequis : Windows, Visual Studio Build Tools avec le compilateur C# (`csc.exe`) et `0Harmony.dll` dans `lib\`.

```powershell
powershell -ExecutionPolicy Bypass -File .\build-caprice.ps1
.\out\CaprICE.exe
```

Pour les détails de conception, consultez [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). Les instructions de contribution et de signalement de failles sont dans [CONTRIBUTING.md](CONTRIBUTING.md) et [SECURITY.md](SECURITY.md).

<a id="english"></a>

## English

**CaprICE** adds security and privacy controls to [Capri Desktop](https://github.com/lachydotmcg/capri-desktop). It monitors Brain and Voice requests, inspects outgoing context, and lets you apply local policies. CaprICE is an external project and does not modify Capri's files.

### Quick start (Windows)

**Recommended — installer and updates:** download [`setup-caprice.ps1`](https://github.com/RajareCorp/CaprICE/releases/latest/download/setup-caprice.ps1) from the latest release, then run it in PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\setup-caprice.ps1
```

The script installs CaprICE for the current user and adds a Start Menu shortcut. Run it again to update. Close CaprICE before installing or updating. Capri Desktop is not included.

**Manual install:** download the [Windows archive](https://github.com/RajareCorp/CaprICE/releases/latest/download/CaprICE-windows.zip), extract it, then place both `CaprICE.exe` **and** `0Harmony.dll` in the Capri Desktop installation folder (next to `Capri.exe`). Launch `CaprICE.exe`. The Harmony DLL is required; keep it beside the executable.

If Capri's automatic startup is enabled, CaprICE replaces its Windows startup entry so Capri will subsequently launch through CaprICE. A disabled entry is left unchanged. The uninstaller restores Capri's original entry if it still points to CaprICE.

To uninstall a script-based installation without deleting user data:

```powershell
powershell -ExecutionPolicy Bypass -File .\setup-caprice.ps1 -Uninstall
```

### Features

- **Network policies:** monitor traffic, confirm external connections, or block all non-loopback connections.
- **Data loss prevention (DLP):** detects and can mask API keys, passwords, Windows paths, email addresses, credit cards, phone numbers, IP addresses, street addresses, and custom values.
- **Inspection:** review events, targets, decision reasons, and detailed outgoing context.
- **Local data management:** view transcripts, memory, and custom prompts.
- **Local actions:** map a keyword to a file opened by Windows. Executables, including scripts, may run.

> **DLP limitations:** CaprICE inspects text, not image pixels. If sensitive data is found in multimodal text, the request is blocked rather than modifying its format. The active security policy determines which requests are masked or blocked.

### Security policies

| Mode | Behavior |
| --- | --- |
| **MONITOR ONLY** | Monitors and logs without blocking requests. |
| **ENFORCE DLP** | Inspects outgoing context and masks detected data; blocks requests if masking is disabled or cannot safely preserve multimodal content. |
| **STRICT LOCAL** | Blocks non-loopback connections, including voice connections. |
| **ASK CONFIRMATION** | Requests approval before an external Brain or Voice connection and shows its URL and a context preview. |

Provider detection uses the URL's actual host; lookalike domains are not treated as recognized providers. Voice API keys are not logged.

### Settings and local data

- **SETTINGS** lets you enable DLP rules and define custom names and addresses. Custom values are not written to DLP audit labels.
- **PROMPTS** lets you customize the Personality, Safety, and Machinery sections. Changes are saved with CaprICE settings.
- **MEMORY** lets you view and save memory. Purging local data deletes the applicable history, notes, and transcripts; a selected note in an external vault is preserved.
- The local **Performance** action reports available CPU, RAM, and GPU measurements. These measurements are not sent to the AI service.

### Mac and Linux — early preview

CaprICE is currently a Windows application built with WinForms and Windows-specific hooks. There is no macOS or Linux build yet; the button above is informational, not a download.

### Build from source

Requirements: Windows, Visual Studio Build Tools with the C# compiler (`csc.exe`), and `0Harmony.dll` in `lib\`.

```powershell
powershell -ExecutionPolicy Bypass -File .\build-caprice.ps1
.\out\CaprICE.exe
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for design details, [CONTRIBUTING.md](CONTRIBUTING.md) to contribute, and [SECURITY.md](SECURITY.md) to report a vulnerability.
