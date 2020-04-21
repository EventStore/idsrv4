// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer {
	public class Startup {
		public IWebHostEnvironment Environment { get; }
		public IConfiguration Configuration { get; }

		public Startup(IWebHostEnvironment environment, IConfiguration configuration) {
			Environment = environment;
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services) =>
			services
				.AddControllersWithViews().Services
				.AddIdentityServer(options => {
					options.Events.RaiseErrorEvents = true;
					options.Events.RaiseInformationEvents = true;
					options.Events.RaiseFailureEvents = true;
					options.Events.RaiseSuccessEvents = true;
				})
				.AddTestUsers(TestUsers.FromFile())
				.AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"))
				.AddInMemoryApiResources(Configuration.GetSection("ApiResources"))
				.AddInMemoryClients(Configuration.GetSection("Clients"))
				.AddDeveloperSigningCredential().Services
				.AddAuthentication();

		public void Configure(IApplicationBuilder app) =>
			app.UseDeveloperExceptionPage()
				.UseStaticFiles()
				.UseRouting()
				.UseIdentityServer()
				.UseAuthorization()
				.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
	}
}
