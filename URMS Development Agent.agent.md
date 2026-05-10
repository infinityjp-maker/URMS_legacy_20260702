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
- Header uses glass plus metal hybrid.
- BootHud uses cinematic intro style.
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
