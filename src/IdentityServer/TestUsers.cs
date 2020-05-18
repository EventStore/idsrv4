using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel;
using IdentityServer4.Test;

namespace IdentityServer {
	public static class TestUsers {
		public static List<TestUser> FromFile() {
			var usersFile = new FileInfo("/etc/idsrv4/users.conf");
			if (!usersFile.Exists) {
				return new List<TestUser>();
			}

			using var fileStream = usersFile.Open(FileMode.Open, FileAccess.Read);
			using var memoryStream = new MemoryStream();
			fileStream.CopyTo(memoryStream);

			return JsonSerializer.Deserialize<TestUserDto[]>(memoryStream.ToArray(), new JsonSerializerOptions {
					PropertyNameCaseInsensitive = true
				})
				.Select(x => new TestUser {
					Password = x.Password,
					Username = x.Username,
					SubjectId = x.SubjectId,
					Claims = new HashSet<Claim>(Array.ConvertAll(x.Claims, claim => new Claim(claim.Type, claim.Value)),
						new ClaimComparer())
				}).ToList();
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
