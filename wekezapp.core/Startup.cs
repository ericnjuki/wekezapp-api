using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using wekezapp.business.Contracts;
using wekezapp.business.Services;
using wekezapp.core.Mapping;
using wekezapp.data.Persistence;

namespace wekezapp.core {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IFlowService, FlowService>();
            services.AddTransient<IChamaService, ChamaService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ILedgerService, LedgerService>();
            services.AddTransient<IAtomicProcedures, AtomicProcedures>();

            // set up DB
            var connection = "Server=.;Database=Wekezapp;Integrated Security=true";
            services.AddDbContext<WekezappContext>
                (options => options.UseSqlServer(connection));

            // enabling CORS
            services.AddCors(
                options => {
                    options.AddPolicy("AllowAll",
                        builder =>
                        {
                            builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                });

            services.Configure<MvcOptions>(options => {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAll"));
            });

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc => {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseMvc(
            //    routes => {
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Users}/{action=GetUsers}");
            //}
                );
        }
    }
}
