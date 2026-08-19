using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acidoinmetal_Trading_Tracker.Services
{
    /// <summary>
    /// Dado un link de TradingView (ej: https://www.tradingview.com/x/4NblYUUq/),
    /// busca dentro del HTML de esa página el meta-tag "og:image" y devuelve la
    /// URL real de la imagen .png (ej: https://s3.tradingview.com/snapshots/4/4NblYUUq.png).
    /// Es el mismo dato que usan Twitter/WhatsApp para mostrar la previsualización del link.
    /// </summary>
    public class TradingViewImageService
    {
        private static readonly HttpClient _http = CrearHttpClient();

        // Cubre los dos posibles ordenes de atributos dentro del <meta>:
        // property antes de content, o content antes de property.
        private static readonly Regex _ogImageRegexA = new Regex(
            "<meta[^>]*property=[\"']og:image[\"'][^>]*content=[\"'](?<url>[^\"']+)[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _ogImageRegexB = new Regex(
            "<meta[^>]*content=[\"'](?<url>[^\"']+)[\"'][^>]*property=[\"']og:image[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static HttpClient CrearHttpClient()
        {
            // CheckCertificateRevocationList=false evita que la primera conexión
            // HTTPS del proceso se quede esperando la validación online del
            // certificado (puede tardar mucho más que el timeout en algunas redes).
            // Es un trade-off aceptable acá: solo bajamos una imagen pública.
            var handler = new HttpClientHandler
            {
                CheckCertificateRevocationList = false
            };

            var cliente = new HttpClient(handler);
            // Sin un User-Agent de navegador, algunos sitios (TradingView incluido)
            // devuelven error o una página distinta.
            cliente.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AcidoInMetalTradingTracker/1.0");
            cliente.Timeout = TimeSpan.FromSeconds(20);
            return cliente;
        }

        /// <summary>
        /// Dispara una conexión liviana apenas arranca la app, para que la
        /// primera carga de imagen real del usuario no pague el costo de la
        /// conexión "en frío" (DNS + TLS). Se llama en segundo plano, sin esperar.
        /// </summary>
        public static async Task PrecalentarAsync()
        {
            try
            {
                await _http.GetAsync("https://www.tradingview.com");
            }
            catch
            {
                // Si falla el precalentamiento no pasa nada: la carga real
                // simplemente va a tardar un poco más la primera vez.
            }
        }

        /// <summary>
        /// Devuelve la URL directa de la imagen .png, o null si no se pudo obtener
        /// (link inválido, sin conexión, o la página no tiene el meta-tag esperado).
        /// </summary>
        public async Task<string?> ObtenerUrlImagenAsync(string urlTradingView)
        {
            if (string.IsNullOrWhiteSpace(urlTradingView))
                return null;

            if (!Uri.TryCreate(urlTradingView, UriKind.Absolute, out var uri))
                return null;

            try
            {
                string html = await _http.GetStringAsync(uri);

                var match = _ogImageRegexA.Match(html);
                if (!match.Success)
                    match = _ogImageRegexB.Match(html);

                return match.Success ? match.Groups["url"].Value : null;
            }
            catch
            {
                // Sin conexión, timeout, link roto, etc. Se maneja como "no se pudo cargar".
                return null;
            }
        }
    }
}
