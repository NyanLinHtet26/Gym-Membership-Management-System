using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Components.Layout
{
    public partial class LogoutConfirm : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void Confirm()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}
