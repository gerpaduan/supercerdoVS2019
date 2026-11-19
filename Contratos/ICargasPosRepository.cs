using System;
using System.Data;

namespace Contratos
{
    // Lista de lo que un operador cargo desde cierto momento (normalmente, desde que abrio su caja) en Stock, Movimientos y
    // Elaborados (2026-11-18, ver docs/DECISIONS.md "Cargas desde el POS"). La usa el menu "Cargas" del POS para que el cajero
    // verifique lo que ya cargo y no lo cargue dos veces. Solo lectura. Solo Postgres: en SQL Server la fabrica devuelve null y el
    // POS muestra el menu sin la lista.
    public interface ICargasPosRepository
    {
        // Cargas del operador con creado >= desde (la fecha y hora de ALTA, no la fecha del registro, que el usuario puede cambiar),
        // sin anuladas, mas nuevas primero, hasta limitePorTipo de cada origen (stock, movimiento, elaborado).
        // Columnas: origen ('stock'|'movimiento'|'elaborado'), subtipo (stock: el tipocompra; elaborado: 'elaborado'|'rapido'|'desarme';
        // movimiento: ''), id, creado, lineas (cantidad), kg (suma en valor absoluto), detalle (movimiento: sucursal destino;
        // elaborado: producto elaborado; stock: '').
        DataTable ListarCargasDesde(int idOperador, DateTime desde, int limitePorTipo);
    }
}
