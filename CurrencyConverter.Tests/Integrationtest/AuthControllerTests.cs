using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Tests.Integrationtest
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient client;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            this.client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginRequest = new { Username = "admin", Password = "password" };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var content = await response.Content.ReadFromJsonAsync<AuthResponse>();

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            content.Should().NotBeNull();
            content!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new { Username = "wrongUser", Password = "wrongPass" };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        public class AuthResponse
        {
            public string Token { get; set; }
        }
    }
}
