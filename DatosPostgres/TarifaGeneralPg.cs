using System;
using System.Collections.Generic;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ITarifaGeneralRepository. Solo plantilla, nunca se usa
    // para calcular una liquidacion (ver Entidades/TarifaGeneral.cs).
    public class TarifaGeneralPg : Contratos.ITarifaGeneralRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public TarifaGeneralPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private static Entidades.TarifaGeneral Mapear(System.Data.IDataRecord dr)
        {
            return new Entidades.TarifaGeneral
            {
                Id = Convert.ToInt32(dr["idtarifageneral"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                FormaLiquidacion = (Entidades.Empleado.formaLiquidacion)Enum.Parse(typeof(Entidades.Empleado.formaLiquidacion), Convert.ToString(dr["formaliquidacion"])),
                Turno = dr["turno"] == DBNull.Value ? (Entidades.Turno?)null : (Entidades.Turno)Enum.Parse(typeof(Entidades.Turno), Convert.ToString(dr["turno"])),
                DiaSemana = dr["diasemana"] == DBNull.Value ? (Entidades.DiaSemana?)null : (Entidades.DiaSemana)Enum.Parse(typeof(Entidades.DiaSemana), Convert.ToString(dr["diasemana"])),
                Valor = Convert.ToDecimal(dr["valor"]),
                VigenteDesde = Convert.ToDateTime(dr["vigentedesde"]),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                CreadoPor = dr["creadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["creadopor"])
            };
        }

        public List<Entidades.TarifaGeneral> Listar(int idEmpresa)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT idtarifageneral, idempresa, formaliquidacion, turno, diasemana, valor, vigentedesde, creado, creadopor
                  FROM tarifageneral WHERE idempresa = @idEmpresa ORDER BY formaliquidacion, turno, diasemana, vigentedesde DESC;",
                Mapear,
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public void Agregar(Entidades.TarifaGeneral tarifa)
        {
            const string sql = @"
                INSERT INTO tarifageneral (idempresa, formaliquidacion, turno, diasemana, valor, vigentedesde, creado, creadopor)
                VALUES (@idEmpresa, @forma, @turno, @diaSemana, @valor, @vigenteDesde, now(), @creadoPor);";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", tarifa.IdEmpresa);
                p.AddWithValue("forma", tarifa.FormaLiquidacion.ToString());
                p.AddWithValue("turno", tarifa.Turno.HasValue ? (object)tarifa.Turno.Value.ToString() : DBNull.Value);
                p.AddWithValue("diaSemana", tarifa.DiaSemana.HasValue ? (object)tarifa.DiaSemana.Value.ToString() : DBNull.Value);
                p.AddWithValue("valor", tarifa.Valor);
                p.AddWithValue("vigenteDesde", tarifa.VigenteDesde);
                p.AddWithValue("creadoPor", (object)tarifa.CreadoPor ?? DBNull.Value);
            });
        }

        // Una fila vigente a HOY por Turno/DiaSemana (para prellenar la grilla del boton "Copiar
        // tarifa estandar") -- DISTINCT ON toma la mas reciente de cada combinacion.
        public List<Entidades.TarifaGeneral> ListarVigentes(int idEmpresa, Entidades.Empleado.formaLiquidacion forma)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                @"SELECT DISTINCT ON (turno, diasemana)
                         idtarifageneral, idempresa, formaliquidacion, turno, diasemana, valor, vigentedesde, creado, creadopor
                  FROM tarifageneral
                  WHERE idempresa = @idEmpresa AND formaliquidacion = @forma AND vigentedesde <= now()::date
                  ORDER BY turno, diasemana, vigentedesde DESC;",
                Mapear,
                p => { p.AddWithValue("idEmpresa", idEmpresa); p.AddWithValue("forma", forma.ToString()); });
        }
    }
}
