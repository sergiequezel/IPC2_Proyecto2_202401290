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


int CalcularTiempo(Lista<TiempoAccion> timeline)
{
    int max = 0;

    Nodo<TiempoAccion> actual = timeline.ObtenerCabeza();

    while (actual != null)
    {
        if (actual.Valor.Tiempo > max)
            max = actual.Valor.Tiempo;

        actual = actual.Siguiente;
    }

    return max;
}
// =========================
// ENDPOINT: PROCESAR
// =========================
app.MapGet("/procesar/{nombre}", (string nombre) =>
{
    XMLService xml = new XMLService();
    xml.CargarXML("wwwroot/entrada.xml");

    Nodo<Mensaje> actual = xml.Mensajes.ObtenerCabeza();
    Mensaje seleccionado = null;

    while (actual != null)
    {
        if (actual.Valor.Nombre == nombre)
        {
            seleccionado = actual.Valor;
            break;
        }

        actual = actual.Siguiente;
    }

    if (seleccionado == null)
        return Results.BadRequest("Mensaje no encontrado");

    var sistema = xml.BuscarSistema(seleccionado.Sistema);

    SimulacionService sim = new SimulacionService();
    var timeline = sim.Simular(seleccionado, sistema);
    string texto = sim.ReconstruirMensaje(seleccionado, sistema);

    OutputService output = new OutputService();
    output.GenerarXML("wwwroot/salida.xml", seleccionado.Nombre, sistema.Nombre, texto, timeline);

    GraphvizService graph = new GraphvizService();
    graph.GenerarSistemaDot("wwwroot/sistema.dot", sistema);
    graph.GenerarInstruccionesDot("wwwroot/timeline.dot", timeline);
    graph.GenerarImagen("wwwroot/sistema.dot", "wwwroot/sistema.png");
    graph.GenerarImagen("wwwroot/timeline.dot", "wwwroot/timeline.png");

    return Results.Ok(new
    {
        mensaje = texto,
        sistema = sistema.Nombre,
        tiempo = CalcularTiempo(timeline), // si no tienes, luego lo ajustamos
        xml = "/salida.xml",
        sistemaImg = "/sistema.png",
        timelineImg = "/timeline.png"
    });
});

app.MapGet("/mensajes", () =>
{
    XMLService xml = new XMLService();
    xml.CargarXML("wwwroot/entrada.xml");

    var lista = xml.Mensajes;

    var mensajes = new List<(string nombre, string sistema)>();

    Nodo<Mensaje> actual = lista.ObtenerCabeza();



    while (actual != null)
    {
        mensajes.Add((actual.Valor.Nombre, actual.Valor.Sistema));

        actual = actual.Siguiente;
    }

    mensajes = mensajes.OrderBy(m => m.nombre).ToList();

    return Results.Ok(mensajes.Select(m => new
    {
        nombre = m.nombre,
        sistema = m.sistema
    }));
});

app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();

