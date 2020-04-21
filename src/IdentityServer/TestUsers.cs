using System.Collections.Generic;
using IdentityServer4.Test;
using Microsoft.Extensions.Configuration;

namespace IdentityServer {
	public static class TestUsers {
		public static List<TestUser> FromConfiguration(IConfiguration configuration) {
			var section = configuration.GetSection(nameof(TestUsers));

			var users = new List<TestUser>();

			section.Bind(users);

			return users;
		}
	}
}
