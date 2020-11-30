using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppBirthday
{
    public partial class AppBirthday : ServiceBase
    {
        public AppBirthday()
        {
            InitializeComponent();
        }

        private static void ActualizarBDPorDia(List<string> usuarios, SqlConnection connection)
        {
            //string queryUpdate = "UPDATE PERSONA SET FecUltEnvioIndividual=GetDate() WHERE UserInstagram like '" + reader[3] + "'";
            SqlCommand cmd = connection.CreateCommand();
            for (int i = 0; i < usuarios.Count; i++)
            {
                string queryUpdate = "UPDATE PERSONA SET FecUltEnvioIndividual=GetDate() WHERE UserInstagram like '" + usuarios[i] + "'";
                cmd.CommandText = queryUpdate;
                cmd.ExecuteNonQuery();
            }
        }

        private static void ActualizarBDPorMes(List<string> usuarios, SqlConnection connection)
        {
            SqlCommand cmd = connection.CreateCommand();
            for (int i = 0; i < usuarios.Count; i++)
            {
                string queryUpdate = "UPDATE PERSONA SET FecUltEnvioGrupo=GetDate() WHERE UserInstagram like '" + usuarios[i] + "'";
                cmd.CommandText = queryUpdate;
                cmd.ExecuteNonQuery();
            }
        }

        private static List<string> CheckBDPorDia(SqlConnection connection)
        {
            SqlCommand cmd = connection.CreateCommand();
            string query = QueryObtenerPersonasPorDia();
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
            SqlDataReader reader = cmd.ExecuteReader();
            List<string> personas = new List<string>();
            List<string> usuarios = new List<string>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    // Console.WriteLine(reader);
                    personas.Add("Nombre: " + reader[1] + "\nApellido: " + reader[2] + "\nUserInstagram: @" + reader[3] + "\nFecha de Cumple: " + reader[4]);
                    //Console.WriteLine(reader[0] + ", " + reader[1] + ", " + reader[2] + ", " + reader[3]);
                    usuarios.Add((string)reader[3]);
                }
                reader.Close();
                ActualizarBDPorDia(usuarios, connection);
            }
            return personas;
        }

        private static List<string> CheckBDPorMes(SqlConnection connection)
        {
            SqlCommand cmd = connection.CreateCommand();
            string query = QueryObtenerPersonasPorMes();
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
            SqlDataReader reader = cmd.ExecuteReader();
            List<string> personas = new List<string>();
            List<string> usuarios = new List<string>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    personas.Add("Nombre: " + reader[1] + "\nApellido: " + reader[2] + "\nUserInstagram: @" + reader[3] + "\nFecha de Cumple: " + reader[4]);
                    usuarios.Add((string)reader[3]);
                }
                reader.Close();
                ActualizarBDPorMes(usuarios, connection);
            }
            return personas;
        }

        private static string QueryObtenerPersonasPorDia()
        {
            DateTime dateTime = DateTime.Now;
            string dia = dateTime.ToString("dd");
            String mes = dateTime.ToString("MM");
            String anio = dateTime.ToString("yyyy");
            string query = "SELECT * FROM PERSONA WHERE FecCumpleaños like '" + mes + "/"
                    + dia + "' AND (FecUltEnvioIndividual NOT LIKE '" + anio + "-%' OR FecUltEnvioIndividual is NULL)";
            return query;
        }

        private static string QueryObtenerPersonasPorMes()
        {
            DateTime dateTime = DateTime.Now;
            String mes = dateTime.ToString("MM");
            String anio = dateTime.ToString("yyyy");
            string query = "SELECT * FROM PERSONA WHERE FecCumpleaños like '" + mes + "/%"
                     + "' AND (FecUltEnvioGrupo NOT LIKE '" + anio + "-%' OR FecUltEnvioGrupo is NULL)";
            return query;
        }

        private static void EnviarCorreo(String mensaje, string asunto)
        {
            Console.WriteLine(DateTime.Now + " - Enviando correo...");
            var fromAddress = new MailAddress("aplicacionmailsroni@gmail.com", "AppBirthday");
            var toAddress = new MailAddress("juventud1@live.com.ar", "To Name");
            const string fromPassword = "khguadjewhsxottf";
            string subject = asunto;
            string body = mensaje;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
                Console.WriteLine(DateTime.Now + " - Correo enviado!");
            }
        }

        private static void GestionarCumplePorDia(SqlConnection connection)
        {
            Console.WriteLine("---\n" + DateTime.Now + " - Revisando si alguien cumple años hoy...");
            string mensaje = "--------------------------------";
            string subject = "ALGUIEN CUMPLE AÑOS HOY!!!!";
            List<string> personas = CheckBDPorDia(connection);
            if (personas.Count != 0)
            {
                if (personas.Count == 1)
                {
                    Console.WriteLine(DateTime.Now + " - 1 persona cumple años hoy!!");
                }
                else
                {
                    Console.WriteLine(DateTime.Now + " - " + personas.Count + " personas cumplen años hoy!!");
                }
                for (int i = 0; i < personas.Count; i++)
                {
                    //Console.WriteLine(personas[i]);
                    mensaje += "\n" + personas[i] + "\n--------------------------------";
                }
                EnviarCorreo(mensaje, subject);
            }
            else
            {
                Console.WriteLine(DateTime.Now + " - Nadie cumple hoy!!");
            }
        }

        private static void GestionarCumplePorMes(SqlConnection connection)
        {
            Console.WriteLine("---\n" + DateTime.Now + " - Revisando si alguien cumple años este mes...");
            string mensaje = "--------------------------------";
            string subject = "Cumpleaños de este mes!!!!";
            List<string> personas = CheckBDPorMes(connection);
            if (personas.Count != 0)
            {
                Console.WriteLine(DateTime.Now + " - Hay personas que cumplen años este mes!!");
                for (int i = 0; i < personas.Count; i++)
                {
                    //Console.WriteLine(personas[i]);
                    mensaje += "\n" + personas[i] + "\n--------------------------------";
                }
                EnviarCorreo(mensaje, subject);
            }
            else
            {
                Console.WriteLine(DateTime.Now + " - Nadie cumple este mes!!");
            }
        }

        protected override void OnStart(string[] args)
        {
            System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "AppBirthday-DEBUG.txt");
            string connectionString = "Data Source = localhost; Initial Catalog = proyectoCumpleanios; integrated security=False;User ID=sa;password=123;";
            Console.WriteLine("--------------------------------\n" + DateTime.Now + " - Iniciando Programa.");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    while (true)
                    {
                        connection.Close();
                        Console.WriteLine(DateTime.Now + " - Conectandose a la Base de Datos.");
                        connection.Open();
                        GestionarCumplePorMes(connection);
                        connection.Close();
                        connection.Open();
                        GestionarCumplePorDia(connection);
                        connection.Close();
                        // Console.WriteLine("--------------------------------");
                        Console.WriteLine("---\n" + DateTime.Now + " - Sleep por 1 hora.\n--------------------------------");
                        Thread.Sleep(3600000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        protected override void OnStop()
        {
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
