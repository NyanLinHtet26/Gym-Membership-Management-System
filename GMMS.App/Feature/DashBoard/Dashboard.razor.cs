using System.Globalization;
using ApexCharts;
using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.DashBoard.Models;
using Microsoft.AspNetCore.Components;

namespace GMMS.App.Feature.DashBoard
{
    public partial class Dashboard : ComponentBase
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        private DashboardResponseModel? _model;
        private bool _isLoading = true;
        private string? _errorMessage;

        private List<ChartPoint> _memberGrowthPoints = [];
        private List<ChartPoint> _revenueTrendPoints = [];
        private List<ChartPoint> _planUsagePoints = [];

        private ApexChartOptions<ChartPoint>? _memberGrowthOptions;
        private ApexChartOptions<ChartPoint>? _revenueTrendOptions;
        private ApexChartOptions<ChartPoint>? _planUsageOptions;

        private int _memberDelta;
        private decimal? _revenueTrendPercent;
        private string _topPlanName = "";
        private int _topPlanCount;

        private const string Violet = "#7C3AED";
        private const string VioletLight = "#A78BFA";
        private const string Blue = "#3B82F6";
        private const string BlueLight = "#60A5FA";
        private const string Amber = "#F59E0B";

        protected override async Task OnInitializedAsync()
        {
            if (AuthTokenStore.CurrentUser?.Role != AuthTokenStore.RoleOwner)
            {
                Navigation.NavigateTo("/member-list");
                return;
            }

            await LoadAsync();
        }

        private async Task RetryAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            _isLoading = true;
            _errorMessage = null;

            try
            {
                var result = await ApiService.GetDashboardAsync<Result<DashboardResponseModel>>();
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    _model = result.Data;
                    ComputeTrends();
                    BuildCharts();
                }
                else
                {
                    _errorMessage = result?.Message ?? "Failed to load dashboard data.";
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ComputeTrends()
        {
            if (_model is null) return;

            if (_model.MemberGrowths.Count >= 2)
            {
                _memberDelta = _model.MemberGrowths[^1].TotalMembers - _model.MemberGrowths[^2].TotalMembers;
            }
            else
            {
                _memberDelta = 0;
            }

            if (_model.RevenueTrends.Count >= 2 && _model.RevenueTrends[^2].Revenue > 0)
            {
                var previous = _model.RevenueTrends[^2].Revenue;
                _revenueTrendPercent = (_model.RevenueTrends[^1].Revenue - previous) / previous * 100m;
            }
            else
            {
                _revenueTrendPercent = null;
            }

            var topPlan = _model.MostUsedPlans.FirstOrDefault();
            _topPlanName = topPlan?.PlanName ?? "—";
            _topPlanCount = topPlan?.TotalMembers ?? 0;
        }

        private string GetMemberTrendClass()
        {
            return _memberDelta > 0
                ? "gmm-dash__trend gmm-dash__trend--up"
                : "gmm-dash__trend gmm-dash__trend--down";
        }

        private string GetRevenueTrendClass()
        {
            return _revenueTrendPercent is >= 0
                ? "gmm-dash__trend gmm-dash__trend--up"
                : "gmm-dash__trend gmm-dash__trend--down";
        }

        private static string FormatMonth(string month)
        {
            return DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date.ToString("MMM yyyy")
                : month;
        }

        private void BuildCharts()
        {
            if (_model is null) return;

            _memberGrowthPoints = _model.MemberGrowths
                .Select(g => new ChartPoint(FormatMonth(g.Month), (decimal)g.TotalMembers))
                .ToList();

            _revenueTrendPoints = _model.RevenueTrends
                .Select(t => new ChartPoint(FormatMonth(t.Month), t.Revenue))
                .ToList();

            _planUsagePoints = _model.MostUsedPlans
                .Select(p => new ChartPoint(p.PlanName, (decimal)p.TotalMembers))
                .ToList();

            _memberGrowthOptions = new ApexChartOptions<ChartPoint>
            {
                Chart = new Chart { Toolbar = new Toolbar { Show = false }, Background = "transparent" },
                Stroke = new Stroke { Curve = Curve.Smooth, Width = 3 },
                DataLabels = new DataLabels { Enabled = false },
                Colors = [Violet],
                Legend = new Legend { Show = false },
                NoData = new NoData { Text = "No data available" },
                Fill = new Fill
                {
                    Type = new List<FillType> { FillType.Gradient },
                    Gradient = new FillGradient
                    {
                        Shade = GradientShade.Dark,
                        ShadeIntensity = 0.35,
                        GradientToColors = [VioletLight],
                        OpacityFrom = 0.4,
                        OpacityTo = 0.05,
                        Stops = [0, 90, 100]
                    }
                },
                Markers = new Markers
                {
                    Size = 4,
                    Colors = [Violet],
                    StrokeColors = ["#FFFFFF"],
                    StrokeWidth = 2
                },
                Grid = new Grid { BorderColor = "rgba(255,255,255,0.08)", StrokeDashArray = 4 }
            };

            _revenueTrendOptions = new ApexChartOptions<ChartPoint>
            {
                Chart = new Chart { Toolbar = new Toolbar { Show = false }, Background = "transparent" },
                Stroke = new Stroke { Curve = Curve.Smooth, Width = 3 },
                DataLabels = new DataLabels { Enabled = false },
                Colors = [Blue],
                Legend = new Legend { Show = false },
                NoData = new NoData { Text = "No data available" },
                Fill = new Fill
                {
                    Type = new List<FillType> { FillType.Gradient },
                    Gradient = new FillGradient
                    {
                        Shade = GradientShade.Dark,
                        ShadeIntensity = 0.35,
                        GradientToColors = [BlueLight],
                        OpacityFrom = 0.4,
                        OpacityTo = 0.05,
                        Stops = [0, 90, 100]
                    }
                },
                Markers = new Markers
                {
                    Size = 4,
                    Colors = [Blue],
                    StrokeColors = ["#FFFFFF"],
                    StrokeWidth = 2
                },
                Grid = new Grid { BorderColor = "rgba(255,255,255,0.08)", StrokeDashArray = 4 },
                Yaxis =
                [
                    new YAxis
                    {
                        Labels = new YAxisLabels
                        {
                            Formatter = "function(val) { return '$' + Number(val).toLocaleString('en-US'); }"
                        }
                    }
                ]
            };

            _planUsageOptions = new ApexChartOptions<ChartPoint>
            {
                Chart = new Chart { Toolbar = new Toolbar { Show = false }, Background = "transparent" },
                DataLabels = new DataLabels { Enabled = true },
                Colors = [Amber],
                Legend = new Legend { Show = false },
                NoData = new NoData { Text = "No data available" },
                PlotOptions = new PlotOptions
                {
                    Bar = new PlotOptionsBar { Horizontal = true }
                },
                Grid = new Grid { BorderColor = "rgba(255,255,255,0.08)", StrokeDashArray = 4 }
            };
        }

        private sealed record ChartPoint(string X, decimal Y);
    }
}
