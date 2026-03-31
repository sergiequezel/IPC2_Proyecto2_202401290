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

List<string> dronesGlobal = new List<string>();

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

app.MapGet("/drones", () =>
{
    XMLService xml = new XMLService();
    xml.CargarXML("wwwroot/entrada.xml");

    List<string> drones = new List<string>();

    // 🔹 XML
    if (xml.Drones != null)
    {
        Nodo<Drone> actual = xml.Drones.ObtenerCabeza();

        while (actual != null)
        {
            if (actual.Valor != null)
                drones.Add(actual.Valor.Nombre);

            actual = actual.Siguiente;
        }
    }

    // 🔹 MEMORIA
    drones.AddRange(dronesGlobal);

    // 🔥 quitar duplicados + ordenar
    drones = drones.Distinct().OrderBy(d => d).ToList();

    return Results.Ok(drones);
});

app.MapPost("/drones", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    string nombre = form["nombre"];

    if (string.IsNullOrWhiteSpace(nombre))
        return Results.BadRequest("Nombre vacío");

    // 🔥 validar contra TODO (XML + memoria)
    XMLService xml = new XMLService();
    xml.CargarXML("wwwroot/entrada.xml");

    List<string> existentes = new List<string>();

    if (xml.Drones != null)
    {
        Nodo<Drone> actual = xml.Drones.ObtenerCabeza();

        while (actual != null)
        {
            existentes.Add(actual.Valor.Nombre);
            actual = actual.Siguiente;
        }
    }

    existentes.AddRange(dronesGlobal);

    if (existentes.Contains(nombre))
        return Results.BadRequest("El dron ya existe");

    dronesGlobal.Add(nombre);

    return Results.Ok("Dron agregado");
});

app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();

