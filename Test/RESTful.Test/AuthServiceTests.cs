using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Test.RESTful.Test
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public async Task EnviarFormularioConImagen_DebeResponderExitosamente()
        {
            string filePath = @"C:\Users\USER\Downloads\juanalberto.jpg";
            Assert.IsTrue(File.Exists(filePath), "El archivo no existe en la ruta especificada.");

            string endpoint = "http://100.65.158.22/api/playlist/upload";
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MiwidXNlcm5hbWUiOiJuYXZpX3NpbXBsZSIsImVtYWlsIjoienMyMjAxMzY5OHBlcHBlZ3Jpw7HDsXBAZXN0dWRpYW50ZXMudXYubXgiLCJyb2xlIjoxLCJpYXQiOjE3NDc5Njg2OTEsImV4cCI6MTc0ODA1MTQ5MX0.vwAiGT0z4x09BMNuEToqwsHpXYbF3g9q0nc0JRZR1cI";
            using (var client = new HttpClient())
            using (var form = new MultipartFormDataContent())
            using (var fileStream = File.OpenRead(filePath))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

                form.Add(fileContent, "image", Path.GetFileName(filePath));
                form.Add(new StringContent("playlist 1"), "playlistName");
                form.Add(new StringContent("cumbias"), "description");

                var response = await client.PutAsync(endpoint, form);

                Assert.IsTrue(response.IsSuccessStatusCode, $"Fallo la solicitud: {response.StatusCode}");

                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Respuesta del servidor:");
                Console.WriteLine(content);
                Assert.IsNotNull(content, "La respuesta no contiene contenido.");
                System.Diagnostics.Debug.WriteLine(content);
            }
        }
    }
}
