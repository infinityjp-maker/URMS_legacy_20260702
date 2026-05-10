using System;

namespace URMS.WinUI.Models
{
    /// <summary>
    /// ダッシュボードカード用のデータインターフェース。
    /// テーマ変更時でも、このインターフェースは変更されない（データ構造の安定性を保証）。
    /// </summary>
    public interface IDashboardCardModel
    {
        /// <summary>カードのタイトル</summary>
        string Title { get; }

        /// <summary>カードの主値（大きく表示される値）</summary>
        string MainValue { get; }

        /// <summary>カードのサブ情報（補足）</summary>
        string SubInfo { get; }

        /// <summary>アイコンの Glyph 文字</summary>
        string IconGlyph { get; }

        /// <summary>カードの階層クラス (hero / secondary / support)</summary>
        CardHierarchyClass HierarchyClass { get; }
    }

    /// <summary>カードの階層分類</summary>
    public enum CardHierarchyClass
    {
        /// <summary>主役：視線が最初に吸い寄せられる</summary>
        Hero = 0,

        /// <summary>準主役：補助情報</summary>
        Secondary = 1,

        /// <summary>脇役：参考情報</summary>
        Support = 2
    }

    /// <summary>
    /// ダッシュボードカードの基本実装。
    /// ViewModel が継承して利用する。
    /// </summary>
    public class DashboardCardModel : IDashboardCardModel
    {
        public string Title { get; set; } = string.Empty;
        public string MainValue { get; set; } = string.Empty;
        public string SubInfo { get; set; } = string.Empty;
        public string IconGlyph { get; set; } = "\uE946";
        public CardHierarchyClass HierarchyClass { get; set; } = CardHierarchyClass.Support;

        public DashboardCardModel() { }

        public DashboardCardModel(string title, string mainValue, string subInfo, string iconGlyph, CardHierarchyClass hierarchyClass)
        {
            Title = title;
            MainValue = mainValue;
            SubInfo = subInfo;
            IconGlyph = iconGlyph;
            HierarchyClass = hierarchyClass;
        }
    }
}
