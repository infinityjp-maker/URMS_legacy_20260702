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
	- Noise 1.5 to 2.0%
	- Satin reflection 4 to 6%
	- Inner reflection 4 to 6px
	- Blur 10 to 14px
	- Hero cards +40% material intensity
- Optical rules:
	- TopHighlight
	- InnerReflection
	- MidShadow
	- DeepShadow with Y 16 to 20 and Blur 24 to 32
- Content rules:
	- InfoPanel margin 52,44,52,44
	- MainValue 34 to 38px
	- Title 14px with top margin 4px
	- SubInfo 12px with line-height 26
- Overlay rules:
	- ActiveOverlay ripple opacity 0.06 to 0.10
	- Hover changes reflection by 3 to 5%
	- Animation baseline 140ms

### Dashboard Hierarchy Drama Rules
- Hero/secondary/support differential:
	- Hero: material +40%, optical +50%, shadow +60%
	- Secondary: material +20%, shadow +20%
	- Support: material -20%, optical -40%
- Section luminance:
	- SYSTEM 100%
	- SUBSYSTEM 60%
	- OPERATION 30%
- Density:
	- Hero high
	- Secondary medium
	- Support low
- Layout:
	- ColumnSpacing and RowSpacing 48
	- Section gap 52
	- Card margin: hero +12, secondary +6, support +2

### Global Tone Rules
- Deep blue-black background and 1.5% noise baseline.
- Reduce glow by 20 to 30%.
- Header uses custom glass material.
- BootHud uses cinematic intro style.
- WorkflowCard uses same material family as CardControl.

## Prohibitions
- No high-emission neon overuse.
- No high-density information clutter.
- No hero-card overpopulation on the same view.
- No direct hard-coded accent spikes when ThemeResource is available.

## New Card Addition Rules
- New cards must inherit CardControl material/optical/density model.
- Every new card must explicitly define at least one of:
	- MaterialIntensity
	- OpticalDepth
	- ShadowDepth
	- InfoDensity
- New cards must declare hierarchy class: hero, secondary, or support.

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
