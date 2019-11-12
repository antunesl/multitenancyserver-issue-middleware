using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MultiTenancyServer;

namespace ModularApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ITenancyContext<ApplicationTenant> _tenancyContext;

        public IndexModel(ILogger<IndexModel> logger, ITenancyContext<ApplicationTenant> tenancyContext)
        {
            _logger = logger;
            _tenancyContext = tenancyContext;
        }

        public string Tenant { get; set; }

        public void OnGet()
        {
            Tenant = _tenancyContext.Tenant?.CanonicalName ?? "-- Not Found --";
        }
    }
}
