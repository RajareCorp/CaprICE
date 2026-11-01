# Architecture de CaprICE

CaprICE charge `Capri.exe` en mémoire, pose des patchs Harmony sur `CapriDesktop.Brain` / `Voice`,
puis affiche une fenêtre de contrôle (Mission Control). Aucune modification du binaire Capri.

## Arborescence

```
caprice/
├─ build-caprice.ps1        Compile src/**/*.cs -> out/CaprICE.exe
├─ lib/                     0Harmony.dll (à fournir)
├─ docs/                    Documentation technique
└─ src/
   ├─ Program.cs            Point d'entrée (orchestration uniquement)
   ├─ Startup/              Démarrage de Capri
   │  ├─ CapriLocator.cs      Retrouve Capri.exe (--capri, %LocalAppData%, candidats, dialogue)
   │  └─ CapriLauncher.cs     Dossier courant, chargement de l'assembly, thread STA
   ├─ Security/             Cœur métier, sans aucune dépendance à l'interface
   │  ├─ PolicyMode.cs, InterceptStatus.cs, InterceptEvent.cs   Modèle
   │  ├─ SecurityPolicy.cs    Réglages courants + décision Evaluate(...)
   │  ├─ DlpEngine.cs         Scan() / Redact() selon les règles actives
   │  ├─ DlpRules.cs          Expressions régulières intégrées
   │  ├─ SettingsStore.cs     Lecture/écriture de caprice_settings.json
   │  ├─ AuditLog.cs          Journal d'audit thread-safe
   │  ├─ StoragePaths.cs      %LocalAppData%\Capri\CaprICE\
   │  └─ TargetDetector.cs    Local / externe, fournisseur IA reconnu
   ├─ Hooks/                Patchs Harmony
   │  ├─ CaprICEHook.cs       État partagé (proxy, prompt) + installation des patchs
   │  ├─ BrainHooks.cs        Brain.Create / Ask / Log
   │  ├─ VoiceHooks.cs        Voice.Open
   │  ├─ PersonaHooks.cs      Brain.Persona
   │  └─ BrainReflection.cs   Accès par réflexion à l'historique et à la mémoire
   ├─ Services/
   │  ├─ CapriDataPaths.cs    Chemins de données partagés de Capri (%LocalAppData%)
   │  ├─ ChatHistoryManager.cs Lecture/écriture de chat.json
   │  ├─ MemoryManager.cs     Lecture/écriture de memories.md / me.md
   │  ├─ TranscriptManager.cs Lecture/suppression des journaux de transcripts
   │  └─ PersonaReader.cs     Lit le prompt système d'origine de Capri
   └─ UI/
      ├─ CaprICEForm.cs       Cœur de la fenêtre (champs, construction, peinture)
      ├─ CaprICEForm.Header.cs / .Policy.cs / .Footer.cs / .Events.cs   Zones de la fenêtre
      ├─ Drawers/             Un fichier par tiroir latéral (DLP, Guide, Mémoire, System Prompt)
      ├─ Dialogs/ConfirmationDialog.cs   Autoriser / Bloquer
      ├─ InspectorRenderer.cs Rendu détaillé d'un événement
      ├─ CyberControls.cs     Fabriques de boutons / cases au style néon
      ├─ NativeMethods.cs     Win32 (déplacement de fenêtre)
      └─ Theme/NeonTheme.cs   Couleurs, polices, cadre et fond
```

## Règles de dépendance

`UI` -> `Hooks`, `Security`, `Services` · `Hooks` -> `Security`, `Services`, `UI` (uniquement `CaprICEForm.NotifyEvent`
et `PromptConfirmation`) · `Security` ne dépend de rien d'autre.

## Où modifier quoi

| Je veux…                                   | Fichier                                  |
|--------------------------------------------|------------------------------------------|
| Ajouter un type de donnée sensible         | `Security/DlpRules.cs` + `DlpEngine.cs` (+ case dans `UI/Drawers/...SettingsDrawer.cs`, champ dans `SecurityPolicy.cs`, clé dans `SettingsStore.cs`) |
| Reconnaître un nouveau fournisseur IA      | `Security/TargetDetector.cs`             |
| Changer la logique de décision d'une politique | `Security/SecurityPolicy.cs` (`Evaluate`) |
| Intercepter une nouvelle méthode de Capri  | `Hooks/CaprICEHook.cs` (patch) + un callback dans `Hooks/` |
| Modifier un tiroir latéral                 | `UI/Drawers/CaprICEForm.<Nom>Drawer.cs`  |
| Changer les couleurs / polices             | `UI/Theme/NeonTheme.cs`                  |

> Les méthodes de hook gardent les noms de paramètres `__instance`, `__result`, `url`, `text`… :
> Harmony les résout **par nom**, ne les renommez pas.
