// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;

namespace IdentityServer {
	public class Program {
		public static async Task<int> Main(string[] args) {
			var cts = new CancellationTokenSource();

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

			try {
				Log.Information("Starting host...");
				await CreateHostBuilder(args)
					.RunConsoleAsync(options => options.SuppressStatusMessages = true, cts.Token);

				return 0;
			} catch (Exception ex) {
				Log.Fatal(ex, "Host terminated unexpectedly.");
				return 1;
			} finally {
				Log.CloseAndFlush();
			}
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			new HostBuilder()
				.ConfigureHostConfiguration(builder => builder
					.AddJsonFile("/etc/idsrv4/idsrv4.conf", true)
					.AddEnvironmentVariables("DOTNET_")
					.AddCommandLine(args ?? Array.Empty<string>()))
				.ConfigureLogging(builder => builder
					.AddSerilog())
				.ConfigureWebHostDefaults(builder => builder
					.UseStartup<Startup>()
					.UseKestrel(ConfigureKestrel));

		private static void ConfigureKestrel(KestrelServerOptions kestrel) {
			kestrel.Listen(IPAddress.Any, 5000);
			kestrel.Listen(IPAddress.Any, 5001, options => options.UseHttps(CreateSelfSignedCertificate()));
		}

		private static X509Certificate2 CreateSelfSignedCertificate() {
			using var rsa = RSA.Create();
			var certificateRequest =
				new CertificateRequest("cn=https://localhost:5001", rsa, HashAlgorithmName.SHA512,
					RSASignaturePadding.Pkcs1) {
					CertificateExtensions = {
						new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true)
					}
				};

			return certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
		}
	}
}
