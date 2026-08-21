using Microsoft.Data.Sqlite;
using System;
using System.IO;
using Acidoinmetal_Trading_Tracker.Models;

namespace Acidoinmetal_Trading_Tracker.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            string carpetaDatos = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AcidoInMetalTradingTracker"
            );

            Directory.CreateDirectory(carpetaDatos);

            string rutaDb = Path.Combine(carpetaDatos, "TradingTracker.db");
            _connectionString = $"Data Source={rutaDb}";

            InicializarBaseDeDatos();
        }

        private void InicializarBaseDeDatos()
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            var comando = conexion.CreateCommand();
            comando.CommandText = @"
                CREATE TABLE IF NOT EXISTS SesionesOperativas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FechaHora TEXT NOT NULL,
                    Descanso INTEGER NOT NULL DEFAULT 1,
                    EstadoAnimico INTEGER NOT NULL DEFAULT 1,
                    NivelStress INTEGER NOT NULL DEFAULT 1,
                    NivelAnsiedad INTEGER NOT NULL DEFAULT 1,
                    CabinaEsteril INTEGER NOT NULL DEFAULT 1,
                    TraderStatusConfirmado INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS AnalisisPar (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SesionId INTEGER NOT NULL,
                    Par TEXT NOT NULL,
                    Tipo TEXT NOT NULL,
                    Marco TEXT NOT NULL DEFAULT '',
                    Link TEXT NOT NULL DEFAULT '',
                    RangoOperativo TEXT,
                    EstadoRango TEXT,
                    Direccion TEXT NOT NULL DEFAULT 'SIN DEFINIR',
                    Comentarios TEXT NOT NULL DEFAULT '',
                    UNIQUE(SesionId, Par, Tipo),
                    FOREIGN KEY (SesionId) REFERENCES SesionesOperativas(Id)
                );
            ";
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Busca la sesión de la fecha dada (comparando solo la parte de fecha,
        /// sin hora) y la reutiliza si existe. Si no existe, crea una nueva.
        /// Esto evita crear una sesión duplicada cada vez que se abre la app
        /// el mismo día.
        /// </summary>
        public int ObtenerOCrearSesionPorFecha(DateTime fecha)
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            string fechaSolo = fecha.ToString("yyyy-MM-dd");

            var comandoBuscar = conexion.CreateCommand();
            comandoBuscar.CommandText = @"
                SELECT Id FROM SesionesOperativas
                WHERE date(FechaHora) = date($fechaSolo)
                LIMIT 1;
            ";
            comandoBuscar.Parameters.AddWithValue("$fechaSolo", fechaSolo);

            var idExistente = comandoBuscar.ExecuteScalar();
            if (idExistente != null)
                return Convert.ToInt32(idExistente);

            var comandoCrear = conexion.CreateCommand();
            comandoCrear.CommandText = @"
                INSERT INTO SesionesOperativas (FechaHora)
                VALUES ($fechaHora);
                SELECT last_insert_rowid();
            ";
            comandoCrear.Parameters.AddWithValue("$fechaHora", fecha.ToString("O"));

            var resultado = comandoCrear.ExecuteScalar();
            return Convert.ToInt32(resultado);
        }

        public void GuardarTraderStatus(int sesionId, SesionOperativa datos)
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            var comando = conexion.CreateCommand();
            comando.CommandText = @"
                UPDATE SesionesOperativas
                SET Descanso = $descanso,
                    EstadoAnimico = $estadoAnimico,
                    NivelStress = $nivelStress,
                    NivelAnsiedad = $nivelAnsiedad,
                    CabinaEsteril = $cabinaEsteril,
                    TraderStatusConfirmado = 1
                WHERE Id = $id;
            ";
            comando.Parameters.AddWithValue("$descanso", datos.Descanso);
            comando.Parameters.AddWithValue("$estadoAnimico", datos.EstadoAnimico);
            comando.Parameters.AddWithValue("$nivelStress", datos.NivelStress);
            comando.Parameters.AddWithValue("$nivelAnsiedad", datos.NivelAnsiedad);
            comando.Parameters.AddWithValue("$cabinaEsteril", datos.CabinaEsteril);
            comando.Parameters.AddWithValue("$id", sesionId);

            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Guarda (inserta o actualiza) los datos de Análisis Macro/Micro para
        /// un Par dentro de una sesión. Gracias al UNIQUE(SesionId, Par, Tipo),
        /// esto nunca duplica filas: si ya existe el registro para esa
        /// combinación, lo actualiza.
        /// </summary>
        public void GuardarAnalisisPar(int sesionId, string par, string tipo, AnalisisPar datos)
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            var comando = conexion.CreateCommand();
            comando.CommandText = @"
                INSERT INTO AnalisisPar
                    (SesionId, Par, Tipo, Marco, Link, RangoOperativo, EstadoRango, Direccion, Comentarios)
                VALUES
                    ($sesionId, $par, $tipo, $marco, $link, $rango, $estadoRango, $direccion, $comentarios)
                ON CONFLICT(SesionId, Par, Tipo) DO UPDATE SET
                    Marco = excluded.Marco,
                    Link = excluded.Link,
                    RangoOperativo = excluded.RangoOperativo,
                    EstadoRango = excluded.EstadoRango,
                    Direccion = excluded.Direccion,
                    Comentarios = excluded.Comentarios;
            ";
            comando.Parameters.AddWithValue("$sesionId", sesionId);
            comando.Parameters.AddWithValue("$par", par);
            comando.Parameters.AddWithValue("$tipo", tipo);
            comando.Parameters.AddWithValue("$marco", datos.Marco ?? string.Empty);
            comando.Parameters.AddWithValue("$link", datos.Link ?? string.Empty);
            comando.Parameters.AddWithValue("$rango", (object?)datos.RangoOperativo ?? DBNull.Value);
            comando.Parameters.AddWithValue("$estadoRango", (object?)datos.EstadoRango ?? DBNull.Value);
            comando.Parameters.AddWithValue("$direccion", datos.Direccion ?? "SIN DEFINIR");
            comando.Parameters.AddWithValue("$comentarios", datos.Comentarios ?? string.Empty);

            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Devuelve los datos guardados de Análisis Macro/Micro para un Par
        /// dentro de una sesión, o null si todavía no se cargó nada.
        /// </summary>
        public AnalisisPar? ObtenerAnalisisPar(int sesionId, string par, string tipo)
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            var comando = conexion.CreateCommand();
            comando.CommandText = @"
                SELECT Marco, Link, RangoOperativo, EstadoRango, Direccion, Comentarios
                FROM AnalisisPar
                WHERE SesionId = $sesionId AND Par = $par AND Tipo = $tipo
                LIMIT 1;
            ";
            comando.Parameters.AddWithValue("$sesionId", sesionId);
            comando.Parameters.AddWithValue("$par", par);
            comando.Parameters.AddWithValue("$tipo", tipo);

            using var lector = comando.ExecuteReader();
            if (!lector.Read())
                return null;

            return new AnalisisPar
            {
                SesionId = sesionId,
                Par = par,
                Tipo = tipo,
                Marco = lector.GetString(0),
                Link = lector.GetString(1),
                RangoOperativo = lector.IsDBNull(2) ? null : lector.GetString(2),
                EstadoRango = lector.IsDBNull(3) ? null : lector.GetString(3),
                Direccion = lector.GetString(4),
                Comentarios = lector.GetString(5)
            };
        }
    }
}