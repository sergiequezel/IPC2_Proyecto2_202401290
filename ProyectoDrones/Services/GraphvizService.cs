using System;
using System.IO;

namespace ProyectoDrones.Services
{
    using ProyectoDrones.Domain;

    public class GraphvizService
    {
        public void GenerarSistemaDot(string ruta, SistemaDrones sistema)
        {
            using (StreamWriter sw = new StreamWriter(ruta))
            {
                sw.WriteLine("digraph SistemaDrones {");
                sw.WriteLine("rankdir=LR;");
                sw.WriteLine("node [shape=box, style=filled, color=lightblue];");

                for (int i = 0; i < sistema.CantidadDrones; i++)
                {
                    string dron = sistema.NombresDrones[i];

                    sw.WriteLine($"{dron} [label=\"{dron}\"];");

                    for (int h = 1; h <= sistema.AlturaMaxima; h++)
                    {
                        char letra = sistema.Mapa[i, h];

                        if (letra != '\0')
                        {
                            string nodoAltura = $"{dron}_{h}";
                            sw.WriteLine($"{nodoAltura} [label=\"{letra} ({h})\", shape=ellipse, color=lightgreen];");
                            sw.WriteLine($"{dron} -> {nodoAltura};");
                        }
                    }
                }

                sw.WriteLine("}");
            }
        }

        public void GenerarInstruccionesDot(string ruta, Lista<TiempoAccion> timeline)
        {
            using (StreamWriter sw = new StreamWriter(ruta))
            {
                sw.WriteLine("digraph Instrucciones {");
                sw.WriteLine("rankdir=LR;");
                sw.WriteLine("node [shape=box, style=filled, color=lightyellow];");

                Nodo<TiempoAccion> actual = timeline.ObtenerCabeza();
                TiempoAccion anterior = null;

                while (actual != null)
                {
                    TiempoAccion ta = actual.Valor;

                    string nodoTiempo = $"T{ta.Tiempo}";

                    sw.WriteLine($"{nodoTiempo} [label=\"Tiempo {ta.Tiempo}\"];");

                    Nodo<Accion> accionNodo = ta.Acciones.ObtenerCabeza();

                    while (accionNodo != null)
                    {
                        Accion acc = accionNodo.Valor;

                        string nodoAccion = $"{nodoTiempo}_{acc.Dron}_{accionNodo.GetHashCode()}";

                        sw.WriteLine($"{nodoAccion} [label=\"{acc.Dron}: {acc.Tipo}\", shape=ellipse, color=lightpink];");
                        sw.WriteLine($"{nodoTiempo} -> {nodoAccion};");

                        accionNodo = accionNodo.Siguiente;
                    }

                    if (anterior != null)
                    {
                        sw.WriteLine($"T{anterior.Tiempo} -> {nodoTiempo};");
                    }

                    anterior = ta;
                    actual = actual.Siguiente;
                }

                sw.WriteLine("}");
            }
        }

        public void GenerarImagen(string rutaDot, string rutaSalida)
        {
            string comando = $"dot -Tpng {rutaDot} -o {rutaSalida}";

            System.Diagnostics.Process proceso = new System.Diagnostics.Process();
            proceso.StartInfo.FileName = "cmd.exe";
            proceso.StartInfo.Arguments = "/C " + comando;
            proceso.StartInfo.CreateNoWindow = true;
            proceso.StartInfo.UseShellExecute = false;

            proceso.Start();
            proceso.WaitForExit();
        }
    }
}
