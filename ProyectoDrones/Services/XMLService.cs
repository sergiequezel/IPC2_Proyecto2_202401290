using System;

// =========================
// SERVICES - XML SERVICE
// =========================
using System.Xml;

namespace ProyectoDrones.Services
{
    using ProyectoDrones.Domain;

    public class XMLService
    {
        public Lista<SistemaDrones> Sistemas = new Lista<SistemaDrones>();
        public Lista<Mensaje> Mensajes = new Lista<Mensaje>();
        public Lista<Drone> Drones;

        public void CargarXML(string ruta)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ruta);

            XmlNode root = doc.SelectSingleNode("config");

            Drones = new Lista<Drone>();

            CargarDrones(root.SelectSingleNode("listaDrones"));
            CargarSistemas(root.SelectSingleNode("listaSistemasDrones"));
            CargarMensajes(root.SelectSingleNode("listaMensajes"));
        }

        private void CargarDrones(XmlNode nodoDrones)
        {
            if (nodoDrones == null) return;

            foreach (XmlNode dronNode in nodoDrones.SelectNodes("dron"))
            {
                string nombre = dronNode.InnerText.Trim();

                Drones.Agregar(new Drone(nombre));
            }
        }

        private void CargarSistemas(XmlNode nodoSistemas)
        {
            if (nodoSistemas == null) return;

            foreach (XmlNode sistemaNode in nodoSistemas.SelectNodes("sistemaDrones"))
            {
                string nombre = sistemaNode.Attributes["nombre"].Value;
                int alturaMax = int.Parse(sistemaNode["alturaMaxima"].InnerText);
                int cantidad = int.Parse(sistemaNode["cantidadDrones"].InnerText);

                SistemaDrones sistema = new SistemaDrones(nombre, cantidad, alturaMax);

                XmlNode contenido = sistemaNode.SelectSingleNode("contenido");

                int index = 0;

                // 🔥 RECORRIDO CORRECTO: dron → alturas
                XmlNode actual = contenido.FirstChild;

                while (actual != null)
                {
                    if (actual.Name == "dron")
                    {
                        string nombreDron = actual.InnerText.Trim();
                        sistema.NombresDrones[index] = nombreDron;

                        // Buscar alturas asociadas a ESTE dron
                        XmlNode alturasNode = actual.NextSibling;

                        while (alturasNode != null && alturasNode.Name != "alturas")
                        {
                            alturasNode = alturasNode.NextSibling;
                        }

                        if (alturasNode != null)
                        {
                            foreach (XmlNode alturaNode in alturasNode.SelectNodes("altura"))
                            {
                                int altura = int.Parse(alturaNode.Attributes["valor"].Value);
                                char letra = alturaNode.InnerText.Trim()[0];

                                sistema.Mapa[index, altura] = letra;
                            }
                        }

                        index++;
                    }

                    actual = actual.NextSibling;
                }

                Sistemas.Agregar(sistema);
            }
        }
        private void CargarMensajes(XmlNode nodoMensajes)
        {
            if (nodoMensajes == null) return;

            foreach (XmlNode mensajeNode in nodoMensajes.SelectNodes("Mensaje"))
            {
                string nombre = mensajeNode.Attributes["nombre"].Value;
                string sistema = mensajeNode["sistemaDrones"].InnerText.Trim();

                Mensaje mensaje = new Mensaje(nombre, sistema);

                XmlNode instrucciones = mensajeNode.SelectSingleNode("instrucciones");

                foreach (XmlNode instNode in instrucciones.SelectNodes("instruccion"))
                {
                    string dron = instNode.Attributes["dron"].Value;
                    int altura = int.Parse(instNode.InnerText);

                    mensaje.Instrucciones.Agregar(new Instruccion(dron, altura));
                }

                Mensajes.Agregar(mensaje);
            }
        }

        public SistemaDrones BuscarSistema(string nombre)
        {
            Nodo<SistemaDrones> actual = Sistemas.ObtenerCabeza();

            while (actual != null)
            {
                if (actual.Valor.Nombre == nombre)
                    return actual.Valor;

                actual = actual.Siguiente;
            }

            return null;
        }
    }
}
