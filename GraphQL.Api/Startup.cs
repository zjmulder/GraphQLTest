using GraphQL.EntityFrameworkCore.DynamicLinq.DependencyInjection;
using GraphQL.EntityFrameworkCore.DynamicLinq.Options;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GraphQL.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public IConfiguration _Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(o => { o.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(o => { o.AllowSynchronousIO = true; });           
            services.AddDbContext<TestDBContext>(options => options.UseSqlServer(_Configuration.GetConnectionString("GraphTest")));
            RegisterGraphQL(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseGraphQL<SchemaTest>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseGraphQLPlayground(new GraphQLPlaygroundOptions
                {
                    Path = "/ui/playground",
                    GraphQLEndPoint = "/graphql",
                    /*PlaygroundSettings = new Dictionary<string, object>
                    {
                        ["editor.theme"] = "dark",
                        ["tracing.hideTracingResponse"] = false
                    }*/
                });
            }
            app.UseHttpsRedirection();
        }

        private void RegisterGraphQL(IServiceCollection services)
        {
            services.AddScoped<IDependencyResolver>(provider => new FuncDependencyResolver(provider.GetRequiredService));
            services.Configure<QueryArgumentInfoListBuilderOptions>(_Configuration.GetSection("QueryArgumentInfoListBuilderOptions"));          
            services.AddScoped<SchemaTest>();
            services.AddGraphQL(o =>
            {
                o.EnableMetrics = true;
                o.ExposeExceptions = true;
            })
                .AddGraphTypes(ServiceLifetime.Scoped);
            services.AddGraphQLEntityFrameworkCoreDynamicLinq();
        }
    }
}
