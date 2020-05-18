using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel;
using IdentityServer4.Test;
using Serilog;

namespace IdentityServer {
	public static class TestUsers {
		private const string UserConfPath = "/etc/idsrv4/users.conf";

		public static List<TestUser> FromFile() {
			var usersFile = new FileInfo(UserConfPath);
			if (!usersFile.Exists) {
				Log.Warning("Could not find user file at {path}. Did you forget to mount it?", UserConfPath);
				return new List<TestUser>();
			}

			using var fileStream = usersFile.Open(FileMode.Open, FileAccess.Read);
			using var memoryStream = new MemoryStream();
			fileStream.CopyTo(memoryStream);

			return new List<TestUser>(TryParseTestUsers(memoryStream, out var testUserDtos)
				? testUserDtos
					.Select(x => new TestUser {
						Password = x.Password,
						Username = x.Username,
						SubjectId = x.SubjectId,
						Claims = new HashSet<Claim>(
							Array.ConvertAll(x.Claims, claim => new Claim(claim.Type, claim.Value)),
							new ClaimComparer())
					})
				: Array.Empty<TestUser>());
		}

		private static bool TryParseTestUsers(MemoryStream memoryStream, out TestUserDto[] testUsers) {
			testUsers = default;

			try {
				testUsers = JsonSerializer.Deserialize<TestUserDto[]>(memoryStream.ToArray(), new JsonSerializerOptions {
					PropertyNameCaseInsensitive = true
				});
				return true;
			} catch (Exception ex) {
				Log.Warning(ex, "Could not parse user file at {path}. Is it valid json?", UserConfPath);
				return false;
			}
		}

		private class TestUserDto {
			public string SubjectId { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public ClaimDto[] Claims { get; set; } = Array.Empty<ClaimDto>();
		}

		private class ClaimDto {
			public string Type { get; set; }
			public string Value { get; set; }
		}
	}
}
