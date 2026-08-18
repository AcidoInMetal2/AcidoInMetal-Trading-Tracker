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
            ";
            comando.ExecuteNonQuery();
        }

        public int CrearSesion(DateTime fechaHora)
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            var comando = conexion.CreateCommand();
            comando.CommandText = @"
                INSERT INTO SesionesOperativas (FechaHora)
                VALUES ($fechaHora);
                SELECT last_insert_rowid();
            ";
            comando.Parameters.AddWithValue("$fechaHora", fechaHora.ToString("O"));

            var resultado = comando.ExecuteScalar();
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
    }
}