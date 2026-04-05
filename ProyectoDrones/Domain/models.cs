using System;

namespace ProyectoDrones.Domain
{
    public class Nodo<T>
    {
        public T Valor;
        public Nodo<T> Siguiente;

        public Nodo(T valor)
        {
            Valor = valor;
            Siguiente = null;
        }
    }

    public class Lista<T>
    {
        private Nodo<T> cabeza;

        public void Agregar(T valor)
        {
            Nodo<T> nuevo = new Nodo<T>(valor);

            if (cabeza == null)
            {
                cabeza = nuevo;
                return;
            }

            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null)
            {
                actual = actual.Siguiente;
            }

            actual.Siguiente = nuevo;
        }

        public Nodo<T> ObtenerCabeza()
        {
            return cabeza;
        }
    }

    public class Drone
    {
        public string Nombre;
        public int AlturaActual;
        public bool LuzEncendida;

        public Drone(string nombre)
        {
            Nombre = nombre;
            AlturaActual = 0;
            LuzEncendida = false;
        }
    }

    public class Instruccion
    {
        public string NombreDron;
        public int Altura;

        public Instruccion(string dron, int altura)
        {
            NombreDron = dron;
            Altura = altura;
        }
    }

    public class Mensaje
    {
        public string Nombre;
        public string Sistema;
        public Lista<Instruccion> Instrucciones;

        public Mensaje(string nombre, string sistema)
        {
            Nombre = nombre;
            Sistema = sistema;
            Instrucciones = new Lista<Instruccion>();
        }
    }

    public class SistemaDrones
    {
        public string Nombre;
        public int AlturaMaxima;
        public int CantidadDrones;

        public char[,] Mapa;

        public string[] NombresDrones;

        public SistemaDrones(string nombre, int cantidad, int alturaMax)
        {
            Nombre = nombre;
            CantidadDrones = cantidad;
            AlturaMaxima = alturaMax;

            Mapa = new char[cantidad, alturaMax + 1];
            NombresDrones = new string[cantidad];
        }

        public int ObtenerIndiceDron(string nombre)
        {
            for (int i = 0; i < NombresDrones.Length; i++)
            {
                if (NombresDrones[i] == nombre)
                    return i;
            }
            return -1;
        }

        public char ObtenerLetra(string dron, int altura)
        {
            int i = ObtenerIndiceDron(dron);
            if (i == -1) return '?';

            return Mapa[i, altura];
        }
    }

    public class Accion
    {
        public string Dron;
        public string Tipo;

        public Accion(string dron, string tipo)
        {
            Dron = dron;
            Tipo = tipo;
        }
    }

    public class TiempoAccion
    {
        public int Tiempo;
        public Lista<Accion> Acciones;

        public TiempoAccion(int tiempo)
        {
            Tiempo = tiempo;
            Acciones = new Lista<Accion>();
        }
    }
}
