using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JWTTokenStandard.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace JWTCustomHeader
{
    public class Startup
    {
        public static string SecretKey = "mysupersecret_secretkey!123";
        public static string Issuer = "infologs.in";
        public static string Audience = "global";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            }).AddXmlSerializerFormatters();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Issuer,

                        ValidateAudience = true,
                        ValidAudience = Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                    };
                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            if(context.Request.Headers.ContainsKey("JWTToken"))
                            {
                                var token = context.Request.Headers["JWTToken"].ToString();
                                if(token.StartsWith("Bearer"))
                                {
                                    context.Token = token.Substring(7).ToString();
                                }
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = context.Request.Headers["Accept"].ToString();
                            string _Message = "Authentication token is invalid.";

                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                //context.Response.Headers.Add("Token-Expired", "true");
                                //OR
                                _Message = "Token has expired.";

                                return context.Response.WriteAsync(JsonConvert.SerializeObject(new
                                ProjectResponse
                                {
                                    StatusCode = (int)HttpStatusCode.Unauthorized,
                                    Message = _Message
                                }));
                            }

                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new
                            ProjectResponse
                            {
                                StatusCode = (int)HttpStatusCode.Unauthorized,
                                Message = _Message
                            }));
                        }
                    };
                });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
