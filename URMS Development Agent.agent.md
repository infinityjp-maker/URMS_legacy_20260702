# URMS Development Agent

## Scope
- Operate only for URMS repository tasks.
- Keep Todo, UI implementation, and specification document synchronized in real time.

## Execution Priority
1. Backup safety requirements
2. Todo real-time maintenance
3. CardControl architecture integrity
4. Dashboard hierarchy drama integrity
5. Global tone consistency
6. Specification and agent-definition zero-diff synchronization
7. Build and runtime verification

## UI Rebuild Baseline

### CardControl Mandatory Architecture
- 4-layer structure is mandatory:
	- MaterialLayer
	- OpticalLayer
	- ContentLayer
	- OverlayLayer
- Material rules:
	- Noise 1.8 to 2.4%
	- Satin reflection 6 to 9%
	- Inner reflection 6 to 10px
	- Blur 14 to 20px
	- Hero cards +40% material intensity
- Optical rules:
	- TopHighlight
	- InnerReflection
	- MidShadow
	- DeepShadow with Y 22 to 28 and Blur 36 to 48
	- AmbientFog
	- BackdropBloom (hero only)
- Content rules:
	- InfoPanel margin 52,44,52,44
	- MainValue 34 to 38px
	- Title 14px with top margin 4px
	- SubInfo 12px with line-height 26
- Overlay rules:
	- ActiveOverlay ripple opacity 0.10 to 0.14
	- Hover changes reflection by 5 to 8%
	- Hover 160ms / Active 180ms
	- Hero motion intensity +30%

### Dashboard Hierarchy Drama Rules
- Hero/secondary/support differential:
	- Hero: material +40%, optical +50%, shadow +60%
	- Secondary: material +20%, shadow +20%
	- Support: material -20%, optical -40%
- Section luminance:
	- SYSTEM 100%
	- SUBSYSTEM 55 to 60%
	- OPERATION 25 to 30%
- Density:
	- Hero high
	- Secondary medium
	- Support low
- Layout:
	- ColumnSpacing and RowSpacing 56
	- Section gap 60
	- Card margin: hero +16, secondary +8, support +4

### Global Tone Rules
- Deep blue-black background and 1.5% noise baseline.
- Reduce glow by 20 to 30%.
- Header uses cold glass signature, not warm metal.
- BootHud uses glass intro style.
- WorkflowCard uses same material family as CardControl.

## Prohibitions
- No high-emission neon overuse.
- No high-density information clutter.
- No hero-card overpopulation on the same view.
- No direct hard-coded accent spikes when ThemeResource is available.
- No spec-agent drift after UI changes.

## New Card Addition Rules
- New cards must inherit CardControl material/optical/density model.
- Every new card must explicitly define at least one of:
	- MaterialIntensity
	- OpticalDepth
	- ShadowDepth
	- InfoDensity
- New cards must declare hierarchy class: hero, secondary, or support.
- New cards must include v2 motion and shadow compatibility review.

## Synchronization Rules
- Any UI rule change must immediately update:
	- docs/URMS_Complete_System_Specification.md
	- this agent file
	- active Todo status
- Before updating this file, create backup:
	- URMS Development Agent.agent.md.bak_YYYYMMDD_HHMM
- Final state must keep zero semantic drift between specification and agent definition.

## Verification Rules
- Required validation after rebuild:
	- dotnet build
	- application launch
	- material layer verification
	- optical depth verification
	- hierarchy drama verification
	- tone consistency verification
	- agent/spec synchronization verification

---

## URMS UI 100% Completion Project (フェーズ 0-4)

### Phase 0: Safety Foundation
- Git backup branch: `backup/highend-ui-before-100pct`
- Folder backup: `URMS.WinUI_backup_100pct_YYYYMMDD_HHMM`
- Agent definition backup: `URMS Development Agent.agent.md.bak_YYYYMMDD_HHMM`
- Todo tracking: Real-time updates for all sub-phases

### Phase 1: Architecture Optimization
- **Objective**: Establish dashboard-card interface and theme separation; eliminate dead logic.
- **Key Requirements**:
  - Define ViewModel/DTO/Interface (e.g., `DashboardCardModel`) for card data.
  - Separate theme-dependent presentation from information logic.
  - Eliminate unused event handlers, properties, and legacy code.
  - Maintain interface stability across theme changes (absolute prohibition: modifying card information structure for theme changes).
- **Turing Point**: Debug EXE launch + user (拓也) confirmation that structure is sound.

### Phase 2: High-End Theme Completion
- **Objective**: Finalize material/optical/animation quality.
- **Key Requirements**:
  - Complete CardControl v2 material/optical/motion (Material 1.8-2.4%, Satin 6-9%, Motion Hover 160ms/Active 180ms).
  - Implement global UI unification (MainWindow, Header, Workflow, BootHud, Settings).
  - Maintain dashboard hierarchy drama (SYSTEM 100%, SUBSYSTEM 55-60%, OPERATION 25-30%).
  - Ensure theme system is pluggable: all visual parameters in ResourceDictionary.
- **Turing Point**: Release EXE launch + user (拓也) confirmation on high-end aesthetic direction.

### Phase 3: Product Finalization
- **Objective**: Complete documentation and specification sync.
- **Key Requirements**:
  - Ensure zero semantic drift: specification ↔ agent ↔ code.
  - Add Phase 1-3 sections to spec and agent definition.
  - Verify Git/backup/branch naming and documentation.
  - Record all deprecated elements to prevent accidental revival.
- **Turing Point**: Release EXE launch + user (拓也) final confirmation on product readiness.

### Phase 4: Completion Report
- All Todo items to completed status.
- Git status clean (all intended changes committed).
- Final summary of architecture/theme/product structure.
- Locked-in version: "URMS UI 100% Complete v3".

### Absolute Prohibitions
- **No theme-data mixing**: Theme files must not contain data logic.
- **No interface breaking**: DashboardCardModel/ViewModel interface must remain stable.
- **No dead code revival**: Any deleted code must be documented with deletion reason.
- **No documentation drift**: Spec and agent must be synchronized after every phase boundary.
- **No skipped backups**: Every phase transition requires Git branch + folder backup.
- **No quality compromises**: No lightweight/simplified/reduced-quality alternatives permitted.

### Theme System Requirements (Phase 2 continuation into Phase 3)
- Theme separation: Move all brushes/gradients/colors/noise/animation-params to Themes/HighEndMaterialTheme.xaml.
- Resource-only interface: Dashboard/CardControl reference only `StaticResource` / `ThemeResource`, never direct theme values.
- Future pluggability: Prepare for additional theme definitions (structure, not UI).
- Theme entry point: MainWindow or App level; store current theme name for future switcher.

### Glass Signature Fixed Direction
- Fixed product direction is Glass Signature.
- Do not use bronze / quiet luxury / warm metal wording or palette.
- Use cool transparency, white-blue highlights, thin strokes, and restrained luminance.
- Dashboard / Header / BootHud / Settings / CardControl must remain in the same glass temperature band.

### Interface Formalization (Phase 1 mandatory)
- DashboardCardModel interface (or equivalent): Define required properties for card data.
- Example signature: `Title`, `MainValue`, `SubInfo`, `Detail`, `HierarchyClass` (hero/secondary/support).
- Code-behind pattern: No magic strings; use ViewModel property binding throughout.
- Testing: Verify theme change does not break card information display.
