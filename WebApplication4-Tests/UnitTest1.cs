using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication4;
using Xunit;

namespace WebApplication4_Tests
{
    public class UnitTest1
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public UnitTest1()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
               .UseStartup<Startup>());
            _client = _server.CreateClient();
        }


        [Fact]
        public async Task Test1()
        {
            // Act
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.Contains("Welcome", responseString);
        }

        [Fact]
        public async Task Test2()
        {
            // Act
            var response = await _client.GetAsync("/abcd");
            var statusCode = response.IsSuccessStatusCode;
            // Assert
            Assert.False(statusCode);
        }

        [Fact]
        public async Task Test3()
        {
            // Act
            var response = await _client.GetAsync("/");
            var statusCode = response.IsSuccessStatusCode;
            // Assert
            Assert.True(statusCode);
        }

        [Fact]
        public void TestSum()
        {
            var service = new Service();
            int result = service.Sum(10, 2);
            Assert.True(result == 12);
        }
    }
}
