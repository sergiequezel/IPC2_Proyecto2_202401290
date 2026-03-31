// =========================
// PROYECTO: Interfaz Web Básica
// Requiere: .NET 6 o superior
// =========================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ProyectoDrones.Services;
using ProyectoDrones.Domain;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

// =========================
// ENDPOINT: SUBIR XML
// =========================
app.MapPost("/upload", async (HttpRequest request) =>
{
    var file = request.Form.Files[0];

    var path = Path.Combine("wwwroot", "entrada.xml");

    using (var stream = new FileStream(path, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    return Results.Ok("Archivo cargado");
});

// =========================
// ENDPOINT: PROCESAR
// =========================
app.MapGet("/procesar", async (HttpContext context) =>
{
    XMLService xml = new XMLService();
    xml.CargarXML("wwwroot/entrada.xml");

    var nodoMensaje = xml.Mensajes.ObtenerCabeza();
    if (nodoMensaje == null)
        return Results.BadRequest("No hay mensajes");

    var mensaje = nodoMensaje.Valor;
    var sistema = xml.BuscarSistema(mensaje.Sistema);

    if (sistema == null)
        return Results.BadRequest("Sistema no encontrado");

    SimulacionService sim = new SimulacionService();
    var timeline = sim.Simular(mensaje, sistema);
    string texto = sim.ReconstruirMensaje(mensaje, sistema);

    OutputService output = new OutputService();
    output.GenerarXML("wwwroot/salida.xml", mensaje.Nombre, sistema.Nombre, texto, timeline);

    GraphvizService graph = new GraphvizService();
    graph.GenerarSistemaDot("wwwroot/sistema.dot", sistema);
    graph.GenerarInstruccionesDot("wwwroot/timeline.dot", timeline);

    graph.GenerarImagen("wwwroot/sistema.dot", "wwwroot/sistema.png");
    graph.GenerarImagen("wwwroot/timeline.dot", "wwwroot/timeline.png");

    return Results.Ok(new
    {
        mensaje = texto,
        xml = "/salida.xml",
        sistema = "/sistema.png",
        timeline = "/timeline.png",
    });
});



app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();

