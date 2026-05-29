using Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Shared.Services
{
    public class EncargadosService
    {
        private readonly HttpClient _httpClient;

        public EncargadosService()
        {
            _httpClient = new HttpClient { BaseAddress = new System.Uri("https://t1tkm4dk-7153.usw3.devtunnels.ms/api/Encargados") };
        }

        // ¡Y AQUÍ! Le faltaba el "public" al método
        public async Task<Encargados> Login(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("login", new { username, password });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Encargados>();
            }
            return null;
        }
    }
}
