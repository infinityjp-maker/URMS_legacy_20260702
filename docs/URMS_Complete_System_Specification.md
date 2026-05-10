# URMS Complete System Specification

## 1. 目的
URMS.WinUI は、運用・監視・実行・設定を 1 つの静かな高級 UI に統合する。単なるサイバー装飾ではなく、素材・光・影・情報密度で優先度を伝えることを最上位目的とする。

## 2. グローバルトーン定義
- 背景基準は深青黒。基準色は #050A11 近傍。
- 画面全体に 1.2% 前後の微細ノイズを重ねる。
- 発光は従来比で 15% から 25% 減光する。
- 明るさではなく、素材差分と影深度で高級感を作る。
- Header は Acrylic を使わず、自前の素材レイヤーでガラス感を構成する。
- BootHud は映画導入のような暗い導入演出を採用する。

## 3. CardControl 4 レイヤー構造

### 3.1 MaterialLayer
- 背景ボケは 6px から 10px 相当。
- 微細ノイズは 1.2% から 1.8%。
- サテン反射は縦方向 3% から 5%。
- InnerGlow は 2px から 4px。
- 主役カードは素材強度を +20% する。

### 3.2 OpticalLayer
- TopHighlight を主役光として使う。
- SoftInnerReflection を内側反射として使う。
- DeepOuterShadow を深影として使う。
- 影の深度は Y=12 から 16、Blur=18 から 24 を基準とする。

### 3.3 ContentLayer
- 情報余白は 52,44,52,44 を基準とする。
- 階層は Title / MainValue / SubInfo / Detail の順で明確に分離する。
- 主役カードの MainValue は 34px から 38px とし、準主役・脇役より明確に大きくする。

### 3.4 OverlayLayer
- ActiveOverlay は光の波紋として扱う。
- Active 時 Opacity は 0.05 から 0.08 を基準にする。
- Hover 時は素材反射量を約 3% 増やす。

### 3.5 CardControl Motion
- Composition API を前提にする。
- DropShadow 単体は禁止し、LayeredShadow を使う。
- Hover / Active アニメーション時間は 140ms を基準にする。
- 主役カードはアニメーション強度を +15% する。

## 4. Dashboard の階層ドラマ

### 4.1 主役 / 準主役 / 脇役
- 主役は System Health と Tasks。
- 準主役は Weather と Security。
- 脇役は Schedule / Calendar / Network と subsystem / operation の各カード。

### 4.2 素材差分ルール
- 主役: 素材強度 +20%、光学深度 +30%、影深度 +40%。
- 準主役: 素材強度 +10%、影深度 +10%。
- 脇役: 素材強度 -10%、光学深度 -20%。

### 4.3 光量ルール
- SYSTEM LAYER は 100%。
- SUBSYSTEM LAYER は 70%。
- OPERATION LAYER は 40%。

### 4.4 情報密度ルール
- 主役は密度高。複数ブロックを使って読み分けさせる。
- 準主役は密度中。必要情報を 2 から 3 ブロックに抑える。
- 脇役は密度低。1 画面で理解できる要点だけにする。

### 4.5 レイアウトルール
- ColumnSpacing / RowSpacing は 48 を基準とする。
- セクション間余白は 52 を基準とする。
- カード上下 Margin は主役 +10px、準主役 +6px、脇役 +2px。
- 並び順は既存構成を維持する。

## 5. グローバル素材基準

### 5.1 App.xaml
- 全テーマに共通して深青黒、低彩度アクセント、細い境界線を使う。
- Resource は ThemeResource 経由で参照する。
- グローバルノイズと深度グラデーションを定義する。

### 5.2 MainWindow
- 背景レイヤー順は BgStarsCanvas → NebulaGrid → BgGridCanvas → BgPerspCanvas → BgMatrixCanvas を維持する。
- Matrix は補助雰囲気であり、可読性を壊さない低 opacity を維持する。
- ScanLine は高さ 3px、全画面、Opacity 0.6 を維持する。

### 5.3 HeaderControl
- 透明ではなく、暗いガラス素材として構成する。
- 上面ハイライト、細いガラスライン、薄いノイズを持つ。
- 時刻・日付・状態表示は高輝度ではなく静かな明色を使う。

### 5.4 WorkflowCard
- MaterialLayer と同系統の質感を持つ独立カードとする。
- パイプラインは明滅ではなく深度差で現在位置を伝える。

### 5.5 SettingsPage
- 設定画面もダッシュボードと同じ素材基準で統一する。
- 左カラムはカテゴリ、右カラムは詳細情報という高密度レイアウトを採用する。

### 5.6 BootHudPage
- 不透明背景で Dashboard 透過を禁止する。
- 暗いシネマティック導入、中央ロゴ、静かな状態表示を採用する。

## 6. 今後の UI 拡張ルール
- 新規カードは原則 CardControl を使用する。
- 新規カードは必ず MaterialIntensity / OpticalDepth / ShadowDepth / InfoDensity のいずれかを明示する。
- 同一画面で主役カードは 2 枚まで。
- 強発光色の直接指定は禁止し、ThemeResource を使用する。
- 素材を足す場合は、ノイズ・反射・影の 3 系統のうち 1 系統だけを強める。
- 光が弱いセクションほど、情報密度も落とす。
- 高級感は発光量ではなく、静かな階調差と余白で出す。

## 7. 実装契約
- 既存 code-behind が参照する x:Name は維持する。
- Window の OS 既定タイトルバーは使用しない。
- SetWindowSubclass による WM_CLOSE / WM_SYSCOMMAND 抑制を維持する。
- DashboardPage の ScrollViewer は HorizontalScrollBarVisibility="Disabled" を維持する。
- DashboardPage の中央寄せレイアウトを維持する。

## 8. 検証
- ビルド: dotnet build -c Debug -p:Platform=x64
- 起動後確認項目:
  - MaterialLayer が全画面に一貫して見えること
  - OpticalLayer の 3 層深度が主役カードで明確であること
  - 主役カードが準主役・脇役より別素材に見えること
  - Dashboard の 3 層ドラマが視線導線として成立していること
  - Header / Settings / Workflow / BootHud までトーンが統一されていること
