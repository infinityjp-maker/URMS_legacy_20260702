# URMS Complete System Specification

## 1. 目的
URMS.WinUI は、運用・監視・実行・設定を 1 つのプロダクト級 UI に統合する。高級演出よりも、素材・光学・影・情報密度を二世代目仕様として体系化し、視線誘導と判断速度を最大化することを最上位目的とする。

## 2. グローバルトーン定義
- 背景基準は深青黒。基準色は #050A11 近傍。
- 画面全体に 1.5% 前後の微細ノイズを重ねる。
- 発光は従来比で 15% から 25% 減光する。
- 明るさではなく、素材差分と影深度で高級感を作る。
- Header は Acrylic を使わず、自前の素材レイヤーでガラス感を構成する。
- BootHud は映画導入のような暗い導入演出を採用する。

## 3. CardControl 4 レイヤー構造

### 3.1 MaterialLayer
- 背景ボケは 14px から 20px 相当。
- 微細ノイズは 1.8% から 2.4%。
- サテン反射は縦方向 6% から 9%。
- InnerGlow は 6px から 10px。
- 主役カードは素材強度を +40% する。

### 3.2 OpticalLayer
- TopHighlight を主役光として使う。
- SoftInnerReflection を内側反射として使う。
- MidShadow を中間深度として使う。
- DeepOuterShadow を深影として使う。
- AmbientFog を光学空気層として使う。
- BackdropBloom は主役カードにのみ適用し、準主役以下は無効化する。
- 影の深度は Y=22 から 28、Blur=36 から 48 を基準とする。

### 3.3 ContentLayer
- 情報余白は 52,44,52,44 を基準とする。
- 階層は Title / MainValue / SubInfo / Detail の順で明確に分離する。
- 主役カードの MainValue は 34px から 38px とし、準主役・脇役より明確に大きくする。

### 3.4 OverlayLayer
- ActiveOverlay は光の波紋として扱う。
- Active 時 Opacity は 0.10 から 0.14 を基準にする。
- Hover 時は素材反射量を約 5% から 8% 増やす。

### 3.5 CardControl Motion
- Composition API を前提にする。
- DropShadow 単体は禁止し、LayeredShadow を使う。
- Hover は 160ms、Active は 180ms を基準にする。
- 主役カードはアニメーション強度を +30% する。

## 4. Dashboard の階層ドラマ

### 4.1 主役 / 準主役 / 脇役
- 主役は System Health と Tasks。
- 準主役は Weather と Security。
- 脇役は Schedule / Calendar / Network と subsystem / operation の各カード。

### 4.2 素材差分ルール
- 主役: 素材 +40%、光学 +50%、影 +60%。
- 準主役: 素材 +20%、影 +20%。
- 脇役: 素材 -20%、光学 -40%。

### 4.3 光量ルール
- SYSTEM LAYER は 100%。
- SUBSYSTEM LAYER は 55% から 60%。
- OPERATION LAYER は 25% から 30%。

### 4.4 情報密度ルール
- 主役は密度高。複数ブロックを使って読み分けさせる。
- 準主役は密度中。必要情報を 2 から 3 ブロックに抑える。
- 脇役は密度低。1 画面で理解できる要点だけにする。

### 4.5 レイアウトルール
- ColumnSpacing / RowSpacing は 56 を基準とする。
- セクション間余白は 60 を基準とする。
- カード上下 Margin は主役 +16px、準主役 +8px、脇役 +4px。
- 並び順は既存構成を維持する。

### 4.6 SYSTEM 層詳細
- 情報密度を v1 比で 15% から 20% 増やす。
- 主役カードでは MainValue と Detail ブロックの階層差を明示する。

### 4.7 SUBSYSTEM 層詳細
- 光学層は 4 層で運用し、主役との差を残す。
- 情報は中密度を維持し、可視化と数値のバランスを優先する。

### 4.8 OPERATION 層詳細
- 光学層は 2 から 3 層で運用し、演出より即読性を優先する。
- カード密度は低密度を維持し、操作導線を最短にする。

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
- ノイズ・反射・境界の値は CardControl v2 と同系統レンジに合わせる。
- パイプラインは明滅ではなく深度差で現在位置を伝える。

### 5.5 SettingsPage
- 設定画面はマット素材を基準にし、主画面より演出を 1 段階落とす。
- 左カラムはカテゴリ、右カラムは詳細情報という高密度レイアウトを採用する。

### 5.6 BootHudPage
- 不透明背景で Dashboard 透過を禁止する。
- 暗いシネマティック導入、中央ロゴ、静かな状態表示を採用する。
- 光学深度 6 層を維持し、起動時だけ許容される導入演出として扱う。

## 6. 今後の UI 拡張ルール
- 新規カードは原則 CardControl を使用する。
- 新規カードは必ず MaterialIntensity / OpticalDepth / ShadowDepth / InfoDensity のいずれかを明示する。
- 同一画面で主役カードは 2 枚まで。
- 強発光色の直接指定は禁止し、ThemeResource を使用する。
- 素材を足す場合は、ノイズ・反射・影の 3 系統のうち 1 系統だけを強める。
- 光が弱いセクションほど、情報密度も落とす。
- 高級感は発光量ではなく、静かな階調差と余白で出す。

## 6.1 第二世代素材規約
- 発光の派手さではなく、素材差分を第一優先にする。
- 光学層は役割を分離し、重複演出を禁止する。

## 6.2 第二世代モーション規約
- Hover は 160ms、Active は 180ms を厳守する。
- 主役カードのみ +30% の強度補正を許可する。

## 6.3 第二世代影規約
- DeepShadow は Y=22 から 28、Blur=36 から 48 を維持する。
- Hover 時は中間影 Blur を +6 し、情報可読性を崩さない。

## 6.4 第二世代反射規約
- Hover 時は InnerReflection を +4px 相当で強化する。
- Active 時は Satin を +10% まで強化できる。

## 6.5 第二世代共通画面規約
- MainWindow / Header / Workflow / Settings / BootHud は CardControl v2 との色温度差を最小化する。
- Settings はマット方向、BootHud は導入演出方向で差分運用する。

## 6.6 追加カード規約
- 追加カードは hero / secondary / support の分類を宣言する。
- 分類未宣言のカード追加は禁止する。

## 6.7 同期規約
- UI 改修と同時に spec と agent を必ず更新する。
- どちらか一方のみ更新する運用を禁止する。

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
  - OpticalLayer の 6 層深度が主役カードで明確であること
  - 主役カードが準主役・脇役より別素材に見えること
  - Dashboard の 3 層ドラマが視線導線として成立していること
  - Header / Settings / Workflow / BootHud までトーンが統一されていること

---

## 9. フェーズ 0-4: URMS UI 完成度 100% プロジェクト

### 9.1 フェーズ 0: 安全基盤
- Git バックアップブランチ: `backup/highend-ui-before-100pct`
- フォルダバックアップ: `URMS.WinUI_backup_100pct_YYYYMMDD_HHMM`
- エージェント定義バックアップ: `URMS Development Agent.agent.md.bak_YYYYMMDD_HHMM`
- Todo 運用: すべてのサブフェーズをリアルタイム更新

### 9.2 フェーズ 1: アーキテクチャ整理
- **目的**: ダッシュボード-カード間インターフェース確立、テーマ分離、デッドロジック削減。
- **要件**:
  - 情報構造: DashboardCardModel などの ViewModel/DTO/Interface を定義。
  - テーマ分離: テーマ依存の見た目とテーマ非依存の情報ロジックを明確に分離。
  - デッドロジック削減: 未使用イベントハンドラ、プロパティ、レガシーコードを削除。
  - インターフェース安定性: テーマ変更時にカード情報構造を変更しない（絶対禁止）。
- **ターニングポイント**: Debug EXE 起動 + ユーザー（拓也）が構造健全性を確認。

### 9.3 フェーズ 2: 高級UI テーマ完成
- **目的**: 素材・光学・アニメーション品質を完成させる。
- **要件**:
  - CardControl v2 完成（Material 1.8-2.4%, Satin 6-9%, Motion Hover 160ms/Active 180ms）。
  - Global UI 統一（MainWindow, Header, Workflow, BootHud, Settings）。
  - ダッシュボード階層ドラマ維持（SYSTEM 100%, SUBSYSTEM 55-60%, OPERATION 25-30%）。
  - テーマシステム完全性: すべての視覚パラメータを ResourceDictionary に集約。
- **ターニングポイント**: Release EXE 起動 + ユーザー（拓也）が高級UI方向性を確認。

### 9.4 フェーズ 3: プロダクト化・仕上げ
- **目的**: ドキュメント・エージェント・仕様書を完全同期。
- **要件**:
  - Zero semantic drift: 仕様書 ↔ エージェント ↔ コード が完全一致。
  - フェーズ 1-3 セクションを仕様書とエージェント定義に追加。
  - Git/バックアップ/ブランチ命名規則と対応を記録。
  - 廃止要素をドキュメント化（誤った復活を防止）。
- **ターニングポイント**: Release EXE 起動 + ユーザー（拓也）が製品完成度を確認。

### 9.5 フェーズ 4: 完了報告
- Todo すべてを完了状態に。
- Git ステータス clean（意図した変更がすべてコミット済み）。
- アーキテクチャ・テーマ・プロダクト構造の最終サマリー記述。
- バージョン確定: "URMS UI 100% Complete v3"。

### 9.6 絶対禁止事項
- **テーマとデータの混在**: テーマファイルにデータロジックを含めない。
- **インターフェース破損**: DashboardCardModel/ViewModel インターフェースの安定性を破さない。
- **デッドコード復活**: 削除されたコードは削除理由をドキュメント化。
- **ドキュメント乖離**: 仕様書とエージェント定義はすべてのフェーズ境界で同期。
- **バックアップ省略**: フェーズ遷移のたびに Git ブランチとフォルダバックアップを実施。
- **品質低下**: 簡略版・軽量版・品質低下代替案は一切認めない。

### 9.7 テーマシステム要件（フェーズ 2 継続 → フェーズ 3）
- テーマ分離: ブラシ・グラデーション・色・ノイズ・アニメーションパラメータを Themes/HighEndMaterialTheme.xaml に集約。
- リソースのみインターフェース: Dashboard / CardControl は `StaticResource` / `ThemeResource` のみを参照、直接テーマ値は参照しない。
- 将来のプラグイン性: 追加テーマ定義の準備（UI 切り替え機能の実装ではなく、構造として対応可能にする）。
- テーマエントリポイント: MainWindow または App レベルに現在テーマ名を保存する仕組みを用意。

### 9.8 インターフェース形式化（フェーズ 1 必須）
- DashboardCardModel インターフェース（または同等）: カードデータに必要なプロパティを定義。
- 例署名: `Title`, `MainValue`, `SubInfo`, `Detail`, `HierarchyClass` (hero/secondary/support).
- Code-behind パターン: 魔法の文字列を使わず、ViewModel プロパティバインディングで全面対応。
- 検証: テーマ変更時にカード情報表示が破損しないことを確認。
