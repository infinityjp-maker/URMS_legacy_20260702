# URMS Complete System Specification

## 1. システム概要

### 1.1 目的
URMS (Unified Resource Management System) は、運用情報・システム状態・操作アクションを単一ダッシュボードへ統合し、素早く正確な意思決定を行う WinUI ベースの統合運用 UI である。

### 1.2 設計方針
- 高級・静寂・可読性重視のサイバー UI
- 最小差分での改善と長期運用整合性の維持
- 情報の階層化（最重要 > 重要 > 詳細）
- 色ではなく構造と余白で視線誘導

### 1.3 技術基盤
- WinUI 3 / .NET 8
- ターゲット: net8.0-windows10.0.19041.0
- Windows App SDK: 1.8.260416003
- Composition API による影表現

## 2. UI 統合仕様（5フェーズ最終版）

### 2.0 Material Layer（高級素材レイヤー）
- 微細ノイズレイヤー: 0.8〜1.2%（標準 1.0%、主役 1.2%）
- サテン反射: 縦方向 2〜3% の弱光沢
- 内側反射: 1〜2px / 透明度 0.05〜0.08
- 背景ボケ: Backdrop 系の弱いブラー（4〜6px 相当）
- 主役カードは別素材: 明度 +8%、ノイズ密度 +20%、内側反射強化、影Y=10〜12

### 2.1 Phase 1: CardControl 最終仕様（強弱・余白・奥行き）

#### 2.1.1 レイアウト
- 外形: Border + CornerRadius=8
- 情報領域: InfoPanel Margin=40,32,40,32
- 情報領域間隔: Spacing=14
- TopHighlight: 2px / #42FFFFFF
- Icon色: #441EE3FF
- タイトルとアイコン間: 14px + Title 上余白 2px
- MainValue 上下余白: +12px
- SubInfo LineHeight: 24
- 素材レイヤー: BackdropBlur + NoiseLayer + SatinReflection + SoftInnerReflection

#### 2.1.2 質感
- ベースグラデーション: #0A0F1A -> #0D121E（主役カードは 5% 明度上げ）
- 枠線発光（面補助）: BorderThickness=1.5 / BorderBrush=#381BCCE6
- GlowBorder は補助光（標準 Opacity 0.13）
- Glow 強弱ルール:
  - 主役（System Health / Tasks）: 0.27（Hover 0.29, IsPrimaryTone=True）
  - 準主役（Weather / Security）: 0.20（Hover 0.21）
  - 通常: 0.18（Hover 0.19, IsPrimaryTone=False）
- ActiveOverlay: 上 #081DDEF8 / 下 #05000000
- ActiveOverlay Opacity: 0.045
- 外側シャドウ（標準）: Blur 16 / 下方向オフセット Y=9 / Color #0C1A2A
- 外側シャドウ（主役）: Blur 18 / 下方向オフセット Y=11 / Color #0C1A2A
- 内側シャドウ: 1px

#### 2.1.3 モーション（CardControl.xaml.cs）
- Hover Scale: 1.004
- Hover TranslateY: -0.5
- Hover Glow: カード種別ごとに +0.01
- Active Overlay Opacity: 0.045
- 変化時間: 100ms

### 2.2 Phase 2: Dashboard 視線誘導最適化
- 最上段 2x2:
  - 左上: SYSTEM HEALTH（主役）
  - 右上: TASKS（主役）
  - 左下: WEATHER（準主役）
  - 右下: SECURITY（準主役）
- 次段 3カラム:
  - 左: SCHEDULE
  - 中央: CALENDAR
  - 右: NETWORK
- 下段:
  - Subsystem Layer / Operation Layer を継続配置
- 余白戦略:
  - ルート StackPanel Spacing=30
  - Padding=44,36
  - Top Grid ColumnSpacing / RowSpacing=32
  - 中段/下段グリッド ColumnSpacing=32
  - Section 間余白は 34px
  - 主役カード Margin は +6px、準主役は +2px
- 情報密度ルール:
  - System Health は情報量 +5〜8%
  - Weather / Security は補助情報を 1 行削減
  - 中段（Schedule/Calendar/Network）は内部余白 +4px
  - System Health は補助情報を 1 行追加
- 階層ドラマ:
  - SYSTEM: 光量・素材強度を最大
  - SUBSYSTEM: 光量を 1 段階低下（Opacity 約0.86）
  - OPERATION: 光量と密度をさらに低下（Opacity 約0.78）

### 2.3 Phase 3: グローバルトーン最終統一

#### 2.3.1 App.xaml テーマ
- Base: #090F1A を基準背景として統一
- Label: #6E8097
- Value: #C7D2E0
- Accent Cyan: #4DAFD4（過度ネオン禁止）
- Border: #25384F（主張を抑えた境界）
- GlobalNoiseBrush: 0.8〜1.2% ノイズ表現の共通ブラシ

#### 2.3.2 HeaderControl
- シアン発光を最終で -15% 減光
- 日付/時計/ステータスを #8FB5C9 系へ調整
- ボトムラインを #6EAFC8 系に統一
- ヘッダー光沢は 2 層（Top Gloss + Directional Gloss）

#### 2.3.3 MainWindow 背景
- Nebula を寒色低彩度へ再配色
- Matrix レイヤー Opacity を 0.14 -> 0.05
- コーナーブラケットと ScanLine を淡色化

#### 2.3.4 Workflow / Settings / BootHud
- WorkflowCard の強発光色を ThemeResource 経由へ統一
- WorkflowCard の矢印色は #6A7C94 に固定
- Settings / Header / MainWindow / BootHud の余白を追加で +2px
- BootHud の導入演出を 20% 減光
- WorkflowCard はカード素材を統一（ノイズ・サテン反射・内側反射）
- MainWindow / Settings は全画面ノイズレイヤーを適用

### 2.4 Phase 4: 仕様同期ルール
- 新規カードは CardControl 以外を原則禁止
- 追加カードは TitleText / MainValueText / SubInfoText / IconGlyph を定義
- 強発光色（#00F7FF 近傍）を直接指定せず、ThemeResource を使用
- 余白と階層は既存導線を踏襲し、情報密度だけを増やさない

### 2.5 Phase 5: 最終品質化ルール
- 強弱ルール: 同一画面で主役は最大2カードまで
- 余白ルール: セクション間は 24px 未満禁止
- 光ルール: TopHighlight を主役、GlowBorder は補助
- 影ルール: Y方向オフセットで浮きを作り、横方向の強い影は禁止
- 密度ルール: 高/中/低 の3段階を維持する
- 視線導線ルール: 主役 -> 準主役 -> 詳細 の順で情報解像度を落とす
- 光学的深度 3 層:
  - TopHighlight（主役光）
  - SoftInnerReflection（内側反射）
  - DeepOuterShadow（深い外影）

## 3. 画面仕様

### 3.1 MainWindow
- 標準タイトルバー非表示
- HeaderControl 内に最小化/最大化/終了を配置
- SetWindowSubclass による WM_CLOSE / WM_SYSCOMMAND 抑制
- DoubleTapped で ToggleMaximizeWindow を実行

### 3.2 Dashboard
- 3層構造（System / Subsystem / Operation）
- System 層は Phase 2 配置を厳守
- CardControl による視覚統一

### 3.3 Settings
- General / UI / System / Developer
- UI 指定値は ThemeResource 経由で反映

### 3.4 BootHud
- 不透明背景で Dashboard 透過防止
- TransitionComplete 後にオーバーレイ除去

## 4. 実装ルール
- XAML 要素名は PascalCase
- UI 色は App.xaml のテーマブラシを優先
- obj 配下生成物の手編集禁止
- 既存の WinUI 固定仕様（アイコン、背景レイヤ順、ScanLine 高さ3）を維持

## 5. ビルド運用

### 5.1 ビルド
- 作業ディレクトリ: WinUI/URMS.WinUI
- コマンド: dotnet build -c Debug -p:Platform=x64

### 5.2 起動確認
- 実行: bin/x64/Debug/net8.0-windows10.0.19041.0/URMS.WinUI.exe
- Header, Dashboard, Card Hover, BootHud を確認

## 6. Copilot 変更ガイド
- 変更優先順: CardControl -> Dashboard -> Theme -> 周辺画面
- 目的は視認性と運用性、装飾過多は禁止
- 大規模改修時も最小差分で適用
- 変更後は必ずビルド検証を実施
- 追加カード実装時は主役カードを複数作らず、同一画面で強発光領域は1箇所まで
- 新規カード追加時は BaseGlowOpacity/HoverGlowOpacity を必ず明示する
- 主役カード以外で IsPrimaryTone=True を使用しない
- 新規カード追加時の初期値: 通常 0.18/0.19、準主役 0.20/0.21、主役 0.27/0.29
- 質感基準: TopHighlight は主役光、Glow は補助光、影はY方向のみ強化
- 拡張時質感基準:
  - 主役カード以外で主役素材設定を使わない
  - ノイズ強度は 0.8〜1.2% 範囲を維持
  - 反射は 2〜3% を超えない
  - 階層が下がるほど光量・密度・素材主張を段階的に落とす

本仕様書は URMS WinUI の現行統一UIを再現・拡張するための基準文書である。
