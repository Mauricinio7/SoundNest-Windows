using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RESTful.Test
{
    [TestClass]
    public class AuthServiceTests
    {
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            ApiClient apiClient = new ApiClient(ApiRoutes.BaseUrl);
            _authService = new AuthService(apiClient);
        }

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var loginRequest = new LoginRequest
            {
                Username = "1",
                Password = "1"
            };

            string result = await _authService.LoginAsync(loginRequest);

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result), "The token should not be null or empty.");
        }
        [TestMethod]
        public async Task SendCodeEmail_ValidEmail_ReturnsSuccessMessage()
        {
            var request = new SendCodeRequest
            {
                Email = "user@example.com"
            };
            var result = await _authService.SendCodeEmailAsync(request);

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result), "The message should not be null or empty.");
            Console.WriteLine(result);
        }
    }
}
