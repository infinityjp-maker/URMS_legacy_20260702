using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using URMS.WinUI.Controls;

namespace URMS.WinUI.Services
{
    // ─── データモデル ─────────────────────────────────────────────────────────

    /// <summary>カード上のデータフィールド 1 件</summary>
    public record CardField(string Label, string DefaultValue, string ColorHex = "#FF00F7FF");

    /// <summary>新カードの仕様定義</summary>
    public record CardSpec(
        string               Id,
        string               Title,
        string               AccentHex,
        IReadOnlyList<CardField> Fields,
        int GridRow    = -1,
        int GridColumn = -1);

    // ─── エンジン本体 ─────────────────────────────────────────────────────────

    /// <summary>
    /// URMS カード自律生成エンジン。
    ///  1. 必要性判断（競合回避）
    ///  2. CyberCard XAML テンプレート生成
    ///  3. ViewModel スタブ生成
    ///  4. ランタイム Card インスタンス生成 → StackPanel 末尾へ追加
    ///  5. Playwright smoke テスト生成
    /// </summary>
    public sealed class CardGeneratorEngine
    {
        public static CardGeneratorEngine Instance { get; } = new();
        private readonly List<CardSpec> _registry = [];
        private CardGeneratorEngine() { }

        // ── 1. 必要性判断 ────────────────────────────────────────────────────

        /// <summary>指定 ID のカードがまだ登録されていなければ true</summary>
        public bool ShouldAddCard(string id) => !_registry.Exists(c => c.Id == id);

        /// <summary>CardSpec を登録（整合性チェック: ID 重複は無視）</summary>
        public bool TryRegister(CardSpec spec)
        {
            if (!ShouldAddCard(spec.Id)) return false;
            _registry.Add(spec);
            return true;
        }

        // ── 2. XAML テンプレート生成 ─────────────────────────────────────────

        public string GenerateXaml(CardSpec spec)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<ctrl:CyberCard xmlns:ctrl=\"using:URMS.WinUI.Controls\" Margin=\"0\">");
            sb.AppendLine("  <StackPanel Spacing=\"0\">");
            sb.AppendLine("    <Grid>");
            sb.AppendLine("      <Grid.ColumnDefinitions>");
            sb.AppendLine("        <ColumnDefinition Width=\"14\"/>");
            sb.AppendLine("        <ColumnDefinition Width=\"6\"/>");
            sb.AppendLine("        <ColumnDefinition Width=\"Auto\"/>");
            sb.AppendLine("        <ColumnDefinition Width=\"*\"/>");
            sb.AppendLine("      </Grid.ColumnDefinitions>");
            sb.AppendLine($"      <Rectangle Grid.Column=\"0\" Height=\"1\" Fill=\"{spec.AccentHex}\" Opacity=\"0.7\" VerticalAlignment=\"Center\"/>");
            sb.AppendLine($"      <TextBlock Grid.Column=\"2\" Text=\"{spec.Title}\" FontFamily=\"Consolas\" FontSize=\"9\" FontWeight=\"Bold\" CharacterSpacing=\"390\" Foreground=\"{spec.AccentHex}\"/>");
            sb.AppendLine("    </Grid>");
            sb.AppendLine("    <Rectangle Height=\"1\" Margin=\"0,3,0,6\"/>");

            foreach (var f in spec.Fields)
            {
                sb.AppendLine($"    <StackPanel Orientation=\"Horizontal\" Spacing=\"6\" Margin=\"0,2\">");
                sb.AppendLine($"      <TextBlock Text=\"{f.Label}\" FontFamily=\"Consolas\" FontSize=\"9\" Foreground=\"{ToHalf(f.ColorHex)}\"/>");
                sb.AppendLine($"      <TextBlock x:Name=\"Txt_{spec.Id}_{Safe(f.Label)}\" Text=\"{f.DefaultValue}\" FontFamily=\"Consolas\" FontSize=\"11\" Foreground=\"{f.ColorHex}\"/>");
                sb.AppendLine($"    </StackPanel>");
            }

            sb.AppendLine("  </StackPanel>");
            sb.AppendLine("</ctrl:CyberCard>");
            return sb.ToString();
        }

        // ── 3. ViewModel スタブ生成 ──────────────────────────────────────────

        public string GenerateViewModelStub(CardSpec spec)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"// ── Auto-generated stub for card: {spec.Id} ({DateTime.UtcNow:O}) ──");
            sb.AppendLine("// DashboardViewModel に追加してください:");
            sb.AppendLine();
            foreach (var f in spec.Fields)
            {
                var prop = $"{spec.Id}_{Safe(f.Label)}";
                sb.AppendLine($"private string _{prop} = \"{f.DefaultValue}\";");
                sb.AppendLine($"public string {prop} {{ get => _{prop}; private set => SetProperty(ref _{prop}, value); }}");
            }
            sb.AppendLine();
            sb.AppendLine($"// RefreshAsync() に追加:");
            sb.AppendLine($"// {spec.Id}_{Safe(spec.Fields[0].Label)} = /* 実データ取得 */;");
            return sb.ToString();
        }

        // ── 4. ランタイムカード生成 ──────────────────────────────────────────

        /// <summary>
        /// CyberCard インスタンスを生成して parent StackPanel の末尾に追加。
        /// Grid レイアウト制御が必要な場合は GridRow/Column を spec に設定。
        /// </summary>
        public CyberCard AddToStackPanel(StackPanel parent, CardSpec spec)
        {
            if (!TryRegister(spec)) 
                return (CyberCard)parent.Children.First(c => c is CyberCard cc && cc.Tag?.ToString() == spec.Id);

            var card  = BuildCard(spec);
            card.Tag  = spec.Id;
            parent.Children.Add(card);
            return card;
        }

        /// <summary>既存 Grid の指定セルに CyberCard を追加</summary>
        public CyberCard AddToGrid(Grid grid, CardSpec spec)
        {
            if (!TryRegister(spec))
                return (CyberCard)grid.Children.First(c => c is CyberCard cc && cc.Tag?.ToString() == spec.Id);

            var card = BuildCard(spec);
            card.Tag = spec.Id;
            if (spec.GridRow    >= 0) Grid.SetRow(card,    spec.GridRow);
            if (spec.GridColumn >= 0) Grid.SetColumn(card, spec.GridColumn);
            grid.Children.Add(card);
            return card;
        }

        // ── 5. Playwright smoke テスト生成 ──────────────────────────────────

        public string GeneratePlaywrightTest(CardSpec spec)
        {
            var assertions = spec.Fields
                .Select(f => $"""  await expect(card.locator('[data-label="{f.Label}"]')).not.toBeEmpty();""");
            return $$"""
// Auto-generated Playwright smoke test for card: {{spec.Id}}
// Generated: {{DateTime.UtcNow:O}}
import { test, expect } from '@playwright/test';

test('card {{spec.Id}} is visible and populated', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('[data-testid="dashboard-grid"]', { state: 'visible' });
  const card = page.locator('[data-card-id="{{spec.Id}}"]');
  await expect(card).toBeVisible();
{{string.Join("\n", assertions)}}
});
""";
        }

        // ── 内部ヘルパー ─────────────────────────────────────────────────────

        private static CyberCard BuildCard(CardSpec spec)
        {
            var card  = new CyberCard();
            var stack = new StackPanel { Spacing = 4 };

            var title = new TextBlock
            {
                Text           = spec.Title,
                FontFamily     = new FontFamily("Consolas"),
                FontSize       = 9,
                FontWeight     = Microsoft.UI.Text.FontWeights.Bold,
                CharacterSpacing = 390,
                Foreground     = new SolidColorBrush(ParseHex(spec.AccentHex))
            };
            stack.Children.Add(title);

            var sep = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Height  = 1,
                Margin  = new Thickness(0, 3, 0, 6),
                Fill    = new SolidColorBrush(ParseHex(spec.AccentHex) with { A = 48 })
            };
            stack.Children.Add(sep);

            foreach (var f in spec.Fields)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                row.Children.Add(new TextBlock
                {
                    Text       = f.Label + ":",
                    FontFamily = new FontFamily("Consolas"),
                    FontSize   = 9,
                    Foreground = new SolidColorBrush(ParseHex(f.ColorHex) with { A = 128 })
                });
                var val = new TextBlock
                {
                    Text       = f.DefaultValue,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize   = 11,
                    Foreground = new SolidColorBrush(ParseHex(f.ColorHex))
                };
                row.Children.Add(val);
                stack.Children.Add(row);
            }

            card.Content = stack;
            return card;
        }

        /// "#FF00F7FF" → "#8000F7FF"（半透明版）
        private static string ToHalf(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 8) hex = hex[2..]; // strip alpha
            return "#80" + hex;
        }

        private static string Safe(string s) => s.Replace(" ", "_").Replace("/", "_").Replace("-", "_");

        private static Color ParseHex(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6) hex = "FF" + hex;
            return Color.FromArgb(
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16),
                Convert.ToByte(hex[6..8], 16));
        }
    }
}
