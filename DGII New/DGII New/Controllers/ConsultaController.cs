using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using DGII_New.Models;

namespace TuProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultaController : ControllerBase
    {
        private const string ConsultaUrl = "https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultasWeb/consultas/ncf.aspx";

        [HttpPost("Consultar")]
        public async Task<IActionResult> Consultar([FromBody] ConsultaRequest request)
        {
            var handler = new HttpClientHandler { UseCookies = true };
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                // Paso 1: Obtener el formulario inicial
                var initialResponse = await client.GetAsync(ConsultaUrl);
                var initialHtml = await initialResponse.Content.ReadAsStringAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(initialHtml);

                // Extraer los valores de __VIEWSTATE, __EVENTVALIDATION y __VIEWSTATEGENERATOR
                var viewState = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATE']")?.GetAttributeValue("value", "");
                var eventValidation = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='__EVENTVALIDATION']")?.GetAttributeValue("value", "");
                var viewStateGenerator = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATEGENERATOR']")?.GetAttributeValue("value", "");

                if (string.IsNullOrEmpty(viewState) || string.IsNullOrEmpty(eventValidation))
                {
                    return BadRequest("No se pudieron extraer los valores del formulario.");
                }

                // Paso 2: Enviar los datos del formulario
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                    new KeyValuePair<string, string>("__EVENTVALIDATION", eventValidation),
                    new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                    new KeyValuePair<string, string>("ctl00$cphMain$txtRNC", request.RNC),
                    new KeyValuePair<string, string>("ctl00$cphMain$txtNCF", request.NCF),
                    new KeyValuePair<string, string>("ctl00$cphMain$txtRncComprador", request.RncComprador ?? ""),
                    new KeyValuePair<string, string>("ctl00$cphMain$txtCodigoSeg", request.CodigoSeguridad ?? ""),
                    new KeyValuePair<string, string>("ctl00$cphMain$btnConsultar", "Buscar")
                });

                var response = await client.PostAsync(ConsultaUrl, formData);
                var htmlResult = await response.Content.ReadAsStringAsync();

                // Paso 3: Extraer los datos de la respuesta
                var resultDoc = new HtmlDocument();
                resultDoc.LoadHtml(htmlResult);

                var rncEmisor = resultDoc.DocumentNode.SelectSingleNode("//span[@id='cphMain_lblrncemisor']")?.InnerText ?? "No encontrado";
                var rncComprador = resultDoc.DocumentNode.SelectSingleNode("//span[@id='cphMain_lblrnccomprador']")?.InnerText ?? "No encontrado";
                var estado = resultDoc.DocumentNode.SelectSingleNode("//span[@id='cphMain_lblEstadoFe']")?.InnerText ?? "No encontrado";
                var montoTotal = resultDoc.DocumentNode.SelectSingleNode("//span[@id='cphMain_lblMontoTotal']")?.InnerText ?? "No encontrado";
                var fechaEmision = resultDoc.DocumentNode.SelectSingleNode("//span[@id='cphMain_lblFechaEmision']")?.InnerText ?? "No encontrado";

                // Devuelve los datos
                return Ok(new
                {
                    RncEmisor = rncEmisor,
                    RncComprador = rncComprador,
                    Estado = estado,
                    MontoTotal = montoTotal,
                    FechaEmision = fechaEmision
                });
            }
        }
    }
}
