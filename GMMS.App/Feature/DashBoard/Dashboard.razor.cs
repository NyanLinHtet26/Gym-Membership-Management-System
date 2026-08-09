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

        private DashboardResponseModel? _model;
        private bool _isLoading = true;
        private string? _errorMessage;

        private List<ChartPoint> _memberGrowthPoints = [];
        private List<ChartPoint> _revenueTrendPoints = [];
        private List<ChartPoint> _planUsagePoints = [];

        private ApexChartOptions<ChartPoint>? _memberGrowthOptions;
        private ApexChartOptions<ChartPoint>? _revenueTrendOptions;
        private ApexChartOptions<ChartPoint>? _planUsageOptions;

        private const string Violet = "#6D28D9";
        private const string Amber = "#F59E0B";

        protected override async Task OnInitializedAsync()
        {
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
                NoData = new NoData { Text = "No data available" }
            };

            _revenueTrendOptions = new ApexChartOptions<ChartPoint>
            {
                Chart = new Chart { Toolbar = new Toolbar { Show = false }, Background = "transparent" },
                Stroke = new Stroke { Curve = Curve.Smooth, Width = 3 },
                DataLabels = new DataLabels { Enabled = false },
                Colors = [Violet],
                Legend = new Legend { Show = false },
                NoData = new NoData { Text = "No data available" },
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
                DataLabels = new DataLabels { Enabled = false },
                Colors = [Amber],
                Legend = new Legend { Show = false },
                NoData = new NoData { Text = "No data available" },
                PlotOptions = new PlotOptions
                {
                    Bar = new PlotOptionsBar { Horizontal = true }
                }
            };
        }

        private sealed record ChartPoint(string X, decimal Y);
    }
}
