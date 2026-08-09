using Microsoft.AspNetCore.Components;

namespace GMMS.App.Components.Common;

public partial class TruncatedText : ComponentBase
{
    [Parameter, EditorRequired]
    public string? Text { get; set; }

    [Parameter]
    public int MaxLength { get; set; } = 35;

    [Parameter]
    public string? MaxWidth { get; set; }

    private string FullText => string.IsNullOrWhiteSpace(Text) ? "—" : Text;

    private string DisplayText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Text)) return "—";
            if (Text.Length <= MaxLength) return Text;
            return string.Concat(Text.AsSpan(0, MaxLength).TrimEnd(), "…");
        }
    }

    private string MaxWidthCss => string.IsNullOrWhiteSpace(MaxWidth) ? "260px" : MaxWidth;
}
