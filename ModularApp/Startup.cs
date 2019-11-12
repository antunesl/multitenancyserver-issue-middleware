using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenancyServer;
using MultiTenancyServer.EntityFramework;
using System.Threading;
using System.Threading.Tasks;

namespace ModularApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCore().AddMvc();

            services.AddRazorPages();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("ManagementConnection"));
            });

            services.AddMultiTenancy<ApplicationTenant, string>()
                   .AddSubdomainParser(".localhost")
                   .AddEntityFrameworkStore<ApplicationDbContext, ApplicationTenant, string>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();



            app.UseMultiTenancy<ApplicationTenant>();

            //app.UseOrchardCore();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }

    public class ApplicationTenant : TenancyTenant<string>
    {
        public string DisplayName { get; set; }
    }


    public class ApplicationDbContext : DbContext, ITenantDbContext<ApplicationTenant, string>
    {
        private static TenancyModelState<string> _tenancyModelState;
        private readonly ITenancyContext<ApplicationTenant> _tenancyContext;
        private readonly ILogger _logger;

        //public ApplicationDbContext(
        //    DbContextOptions<ApplicationDbContext> options,
        //    ITenancyContext<ApplicationTenant> tenancyContext,
        //    ILogger<ApplicationDbContext> logger)
        //    : base(options)
        //{
        //    // The request scoped tenancy context.
        //    // Should not access the tenancyContext.Tenant property in the constructor yet,
        //    // as the request pipeline has not finished running yet and it will likely be null.
        //    // Use the private property wrapper above to access it later on demand.
        //    _tenancyContext = tenancyContext;
        //    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        //}

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        // MultiTenancyServer implementation.
        public DbSet<ApplicationTenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // MultiTenancyServer configuration.
            var tenantStoreOptions = new TenantStoreOptions();
            builder.ConfigureTenantContext<ApplicationTenant, string>(tenantStoreOptions);

            // Configure custom properties on ApplicationTenant.
            builder.Entity<ApplicationTenant>(b =>
            {
                b.Property(t => t.DisplayName).HasMaxLength(256);
            });


        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            // Ensure multi-tenancy for all tenantable entities.
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // Ensure multi-tenancy for all tenantable entities.
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=Mgmt.db;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
