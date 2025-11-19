/*
    Este archivo forma parte del proyecto VeriFactuWinLib.
    Copyright (c) 2025
    Autor: Santiago Nicolás Hernández Hernández
    Empresa: Ingeniería de Desarrollo y Servicios de Canarias, S.L. (https://idssoft.net/)

    Este programa es software libre: puede redistribuirlo y/o modificarlo
    bajo los términos de la GNU Affero General Public License versión 3,
    publicada por la Free Software Foundation.

    Este programa se distribuye con la esperanza de que sea útil, pero
    SIN NINGUNA GARANTÍA; sin incluso la garantía implícita de COMERCIALIZACIÓN
    o IDONEIDAD PARA UN PROPÓSITO PARTICULAR.

    Para más detalles consulte la licencia AGPL:
    http://www.gnu.org/licenses/
*/

using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Config;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Http.Headers;

namespace VeriFactu.Net.Rest
{
    public class ApiNoverifactu
    {
        private readonly HttpClient _httpClient;
        private static string _bearerToken;
        private static DateTime _tokenExpiration; // fecha de expiración del token


        public ApiNoverifactu()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(Settings.Current.VeriFactuEndPointNoVeriFactuPrefix)
            };

            // Cargar token persistente si existe
            if (Settings.Current.NoVeriFactuToken != "")
            {

                _bearerToken = Settings.Current.NoVeriFactuToken;
                _tokenExpiration = Settings.Current.NoVeriFactuTokenTime;

            }
        }

        /// <summary>
        /// Autentica la app y guarda el token
        /// </summary>
        private async Task<bool> AuthenticateAsync()
        {
            try
            {
                var url = Settings.Current.VeriFactuEndPointNoVeriFactuPrefix + "/app-login";
                var payload = new
                {
                    nif = Settings.Current.NoVeriFactuNif,
                    key = Settings.Current.NoVeriFactuKey
                };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"❌ Url: {url}");
                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error autenticando: {response.StatusCode}");
                    return false;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                _bearerToken = doc.RootElement.GetProperty("token").GetString();

                // Opcional: si la API devuelve expiración, úsala. Si no, ponemos 1 hora por defecto
                _tokenExpiration = DateTime.UtcNow.AddHours(1);
                Settings.Current.NoVeriFactuToken = _bearerToken;
                Settings.Current.NoVeriFactuTokenTime = _tokenExpiration;
                Settings.Save();
                Console.WriteLine("✅ Token guardado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("🚨 Error autenticando: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Asegura que haya un token válido
        /// </summary>
        private async Task<bool> EnsureTokenAsync()
        {
            if (string.IsNullOrEmpty(_bearerToken) || DateTime.UtcNow >= _tokenExpiration)
            {
                Console.WriteLine("⚠️ Token no válido o expirado. Autenticando...");
                return await AuthenticateAsync();
            }
            return true;
        }

        /// <summary>
        /// Envia una petición con token Bearer
        /// </summary>
        public async Task<string> EnviarPeticionAsync(XmlDocument xmlDocument, X509Certificate2 certificate = null)
        {
            if (!await EnsureTokenAsync())
            {
                return null;
            }

            try
            {
                var url = Settings.Current.VeriFactuEndPointNoVeriFactuPrefix + "/factura"; // endpoint relativo
                string xmlString = xmlDocument.OuterXml;
                string certBase64 = certificate != null
                    ? Convert.ToBase64String(certificate.Export(X509ContentType.Pfx))
                    : null;

                var payload = new { file = xmlString, cert = certBase64 };
                string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Establecer token en el header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

                var response = await _httpClient.PostAsync(url, content);

                string mediaType = response.Content.Headers.ContentType.MediaType;

                // Si token expiró según la API, renovar y reintentar

                if (string.Equals(mediaType, "text/html", StringComparison.OrdinalIgnoreCase) || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("⚠️ Token expirado según API, renovando...");
                    if (await AuthenticateAsync())
                    {
                        Console.WriteLine("⚠️ Token renovando...");
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
                        response = await _httpClient.PostAsync(url, content);
                    }
                    else
                    {
                        return null;
                    }
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🔹 Código HTTP: {(int)response.StatusCode}");
                Console.WriteLine($"✅ Respuesta: {responseBody}");
                return responseBody;
            }
            catch (Exception ex)
            {
                Console.WriteLine("🚨 Error enviando petición: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Obtener token actual en memoria
        /// </summary>
        public static string GetToken() => _bearerToken;
    }
}

