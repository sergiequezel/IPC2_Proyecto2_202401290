using System;

namespace ProyectoDrones.Services
{
    using ProyectoDrones.Domain;

    public class SimulacionService
    {
        public Lista<TiempoAccion> Simular(Mensaje mensaje, SistemaDrones sistema)
        {
            Lista<TiempoAccion> timeline = new Lista<TiempoAccion>();

            int tiempo = 0;
            int[] alturas = new int[sistema.CantidadDrones];

            Nodo<Instruccion> actual = mensaje.Instrucciones.ObtenerCabeza();

            while (actual != null)
            {
                Instruccion inst = actual.Valor;
                Nodo<Instruccion> siguienteNodo = actual.Siguiente;

                int indexActivo = sistema.ObtenerIndiceDron(inst.NombreDron);
                if (indexActivo == -1)
                {
                    actual = actual.Siguiente;
                    continue;
                }

                int objetivoActivo = inst.Altura;

                int indexFuturo = -1;
                int objetivoFuturo = -1;

                if (siguienteNodo != null)
                {
                    indexFuturo = sistema.ObtenerIndiceDron(siguienteNodo.Valor.NombreDron);
                    objetivoFuturo = siguienteNodo.Valor.Altura;
                }

                bool llego = false;

                while (!llego)
                {
                    tiempo++;
                    TiempoAccion ta = new TiempoAccion(tiempo);

                    for (int i = 0; i < sistema.CantidadDrones; i++)
                    {
                        int alturaActual = alturas[i];

                        if (i == indexActivo)
                        {
                            if (alturaActual < objetivoActivo)
                            {
                                alturas[i]++;
                                ta.Acciones.Agregar(new Accion(sistema.NombresDrones[i], "SUBIR"));
                            }
                            else if (alturaActual > objetivoActivo)
                            {
                                alturas[i]--;
                                ta.Acciones.Agregar(new Accion(sistema.NombresDrones[i], "BAJAR"));
                            }
                            else
                            {
                                llego = true;
                                ta.Acciones.Agregar(new Accion(sistema.NombresDrones[i], "ESPERAR"));
                            }
                        }
                        else if (i == indexFuturo)
                        {
                            if (alturaActual < objetivoFuturo)
                            {
                                alturas[i]++;
                                ta.Acciones.Agregar(new Accion(sistema.NombresDrones[i], "SUBIR"));
                            }
                            else if (alturaActual > objetivoFuturo)
                            {
                                alturas[i]--;
                                ta.Acciones.Agregar(new Accion(sistema.NombresDrones[i], "BAJAR"));
                            }
                            else
                            {
                                ta.Acciones.Agregar(new Accion(sistema.NombresDrones[i], "ESPERAR"));
                            }
                        }
                        else
                        {
                            ta.Acciones.Agregar(new Accion(sistema.NombresDrones[i], "ESPERAR"));
                        }
                    }

                    timeline.Agregar(ta);
                }

                tiempo++;
                TiempoAccion luzOn = new TiempoAccion(tiempo);
                luzOn.Acciones.Agregar(new Accion(inst.NombreDron, "LUZ_ON"));
                timeline.Agregar(luzOn);

                tiempo++;
                TiempoAccion luzOff = new TiempoAccion(tiempo);
                luzOff.Acciones.Agregar(new Accion(inst.NombreDron, "LUZ_OFF"));
                timeline.Agregar(luzOff);

                actual = actual.Siguiente;
            }

            return timeline;
        }

        public string ReconstruirMensaje(Mensaje mensaje, SistemaDrones sistema)
        {
            string resultado = "";

            Nodo<Instruccion> actual = mensaje.Instrucciones.ObtenerCabeza();

            while (actual != null)
            {
                Instruccion inst = actual.Valor;
                char letra = sistema.ObtenerLetra(inst.NombreDron, inst.Altura);

                resultado += letra;

                actual = actual.Siguiente;
            }

            return resultado;
        }
    }
}
