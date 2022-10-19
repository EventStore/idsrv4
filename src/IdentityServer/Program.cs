using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using IdentityServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Verbose()
	.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
	.MinimumLevel.Override("System", LogEventLevel.Warning)
	.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
	.Enrich.FromLogContext()
	.WriteTo.Console(
		outputTemplate:
		"[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
		theme: AnsiConsoleTheme.Literate)
	.CreateLogger();

using var cert = CreateSelfSignedCertificate();


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSerilog(Log.Logger);

builder.Host
	.ConfigureHostConfiguration(config => config
		.AddJsonFile("/etc/idsrv4/idsrv4.conf", false)
		.AddEnvironmentVariables("DOTNET_")
		.AddCommandLine(args));

builder.WebHost.ConfigureKestrel(kestrel => kestrel.ConfigureHttpsDefaults(https => https.ServerCertificate = cert));

builder.Services
	.AddControllersWithViews().Services
	.AddIdentityServer(options => {
		options.Events.RaiseErrorEvents = true;
		options.Events.RaiseInformationEvents = true;
		options.Events.RaiseFailureEvents = true;
		options.Events.RaiseSuccessEvents = true;
	})
	.AddTestUsers(TestUsers.FromFile())
	.AddInMemoryIdentityResources(builder.Configuration.GetSection("IdentityResources"))
	.AddInMemoryApiResources(builder.Configuration.GetSection("ApiResources"))
	.AddInMemoryClients(builder.Configuration.GetSection("Clients"))
	.AddDeveloperSigningCredential().Services
	.AddAuthentication();

await using var app = builder.Build();

app
	.UseDeveloperExceptionPage()
	.UseStaticFiles()
	.UseRouting()
	.UseIdentityServer()
	.UseAuthorization()
	.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

app.Run();

X509Certificate2 CreateSelfSignedCertificate() {
	using var rsa = RSA.Create();
	var certificateRequest =
		new CertificateRequest("cn=localhost", rsa, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1) {
			CertificateExtensions = {
				new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true)
			}
		};
	return certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
}
