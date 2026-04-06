using System;
using System.IO;
using System.Xml;

namespace ProyectoDrones.Services
{
    using ProyectoDrones.Domain;

    public class OutputService
    {
        public void GenerarXML(
            string rutaSalida,
            string nombreMensaje,
            string nombreSistema,
            string mensajeRecibido,
            Lista<TiempoAccion> timeline)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(rutaSalida, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("respuesta");

                writer.WriteStartElement("listaMensajes");

                writer.WriteStartElement("mensaje");
                writer.WriteAttributeString("nombre", nombreMensaje);

                writer.WriteElementString("sistemaDrones", nombreSistema);

                int tiempoOptimo = CalcularTiempoTotal(timeline);
                writer.WriteElementString("tiempoOptimo", tiempoOptimo.ToString());

                writer.WriteElementString("mensajeRecibido", mensajeRecibido);

                writer.WriteStartElement("instrucciones");

                Nodo<TiempoAccion> nodoTiempo = timeline.ObtenerCabeza();

                while (nodoTiempo != null)
                {
                    TiempoAccion ta = nodoTiempo.Valor;

                    writer.WriteStartElement("tiempo");
                    writer.WriteAttributeString("valor", ta.Tiempo.ToString());

                    writer.WriteStartElement("acciones");

                    Nodo<Accion> nodoAccion = ta.Acciones.ObtenerCabeza();

                    while (nodoAccion != null)
                    {
                        Accion acc = nodoAccion.Valor;

                        writer.WriteStartElement("dron");
                        writer.WriteAttributeString("nombre", acc.Dron);
                        writer.WriteString(acc.Tipo);
                        writer.WriteEndElement();

                        nodoAccion = nodoAccion.Siguiente;
                    }

                    writer.WriteEndElement(); // acciones
                    writer.WriteEndElement(); // tiempo

                    nodoTiempo = nodoTiempo.Siguiente;
                }

                writer.WriteEndElement(); // instrucciones
                writer.WriteEndElement(); // mensaje
                writer.WriteEndElement(); // listaMensajes
                writer.WriteEndElement(); // respuesta

                writer.WriteEndDocument();
            }
        }

        // funciiona para calcular el tiempo total
        private int CalcularTiempoTotal(Lista<TiempoAccion> timeline)
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
    }
}
