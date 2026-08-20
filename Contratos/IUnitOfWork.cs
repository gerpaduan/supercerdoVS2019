using System;

namespace Contratos
{
    // Reemplazo explicito de System.Transactions.TransactionScope para el camino Postgres.
    // TransactionScope (pensado para SQL Server, que auto-enlista conexiones ADO.NET sin
    // problemas) no es compatible con el patron "una conexion nueva por metodo" que usa toda
    // la capa Postgres -- cada AbrirConTenant abre su propia transaccion explicita, y eso choca
    // con el auto-enlistment de Npgsql en una transaccion ambiente ("A transaction is already
    // in progress"). Encontrado y diagnosticado en el testeo profundo de escritura real,
    // 2026-08-20 (ver docs/DECISIONS.md).
    //
    // IUnitOfWork representa una unica conexion+transaccion explicita, compartida a mano a
    // traves de varias llamadas a distintos repositorios Postgres dentro de una misma
    // operacion de negocio (ej. Negocio.Venta.agregarVenta toca VentaPg + CierreCajaPg +
    // CuentaCorrientePg). Vive en Contratos (no en DatosPostgres) para que Negocio/*.cs pueda
    // orquestarla sin depender de Npgsql -- misma regla de capas que el resto de la migracion.
    //
    // Implementacion SQL Server: no aplica (los metodos que la reciben como parametro opcional
    // simplemente la ignoran, TransactionScope sigue manejando la atomicidad como siempre).
    // Implementacion Postgres real: DatosPostgres.UnitOfWorkPg.
    public interface IUnitOfWork : IDisposable
    {
        void Completar();
    }
}
