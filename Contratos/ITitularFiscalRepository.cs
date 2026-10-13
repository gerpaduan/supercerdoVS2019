using System;
using System.Collections.Generic;

namespace Contratos
{
    // Titulares fiscales por empresa y por sucursal (2026-10-09, ver docs/DECISIONS.md). Solo Postgres: en SQL Server
    // NegocioFactory.CrearTitularFiscal devuelve null y quien lo usa factura con los datos de la empresa como siempre.
    public interface ITitularFiscalRepository
    {
        // ---- Titulares ----
        // Todos los titulares de la empresa (el principal primero). Asegura que exista el principal.
        List<Entidades.TitularFiscal> Listar(int idEmpresa, bool soloHabilitados);
        Entidades.TitularFiscal Obtener(int idTitular);
        // Crea el principal si falta (idempotente) y lo devuelve.
        Entidades.TitularFiscal ObtenerPrincipal(int idEmpresa);
        // Alta (IdTitular = 0) o edicion de un titular NO principal; del principal solo se guardan topes y ventana anual.
        int Guardar(Entidades.TitularFiscal titular);
        void SetHabilitado(int idTitular, bool habilitado);
        // true si la PROPIA empresa ya tiene a ese CUIT (su CUIT o otro titular, incluso revocado), salvo el titular
        // excluido. Un CUIT SI puede ser titular en varias empresas (2026-10-12): lo de otras empresas no cuenta.
        bool ExisteCuit(long cuit, int idTitularExcluir);
        // ---- Titular compartido entre empresas (consultas que cruzan empresas: solo existencia / nombres) ----
        // Otras empresas (distintas de idEmpresaPropia) que tienen ese CUIT como titular no revocado.
        List<Entidades.EmpresaConCuit> OtrasEmpresasConCuit(long cuit, int idEmpresaPropia);
        // Empresa que administra el certificado de ese CUIT (pedido, instalacion): aquella donde es el titular
        // principal (su CUIT) o, si en ninguna lo es, la del titular mas antiguo. 0 si nadie lo tiene.
        int EmpresaAdministradoraDeCuit(long cuit);
        // Otras empresas que usan el mismo punto de venta de ese CUIT (principal: el de sus sucursales; resto: su lista).
        List<Entidades.EmpresaConCuit> OtrasEmpresasConPuntoVenta(long cuit, int codPuntoVenta, int idEmpresaPropia);
        // ---- Revocacion (super-admin) ----
        // true si existe alguna fila de factura electronica (incluso con error o de prueba) con ese titular.
        bool TieneFacturas(int idTitular);
        // Borra el titular y sus datos (puntos de venta, asignaciones) si borrar = true; si no, lo marca revocado. En
        // ambos casos las sucursales que lo usaban vuelven al principal. Una sola transaccion.
        void Revocar(int idTitular, bool borrar, int idUsuario);
        // Quita la marca de revocado (reponer la titularidad). El titular queda deshabilitado hasta que la empresa lo habilite.
        void Reponer(int idTitular);
        // Nombres de las sucursales cuyo titular es este (una asignacion NULL cuenta como el principal).
        List<string> SucursalesAsignadas(int idTitular);

        // ---- Sucursales ----
        // Titular asignado a la sucursal; null = sin asignacion explicita (se interpreta como el principal).
        int? ObtenerIdTitularDeSucursal(int idSucursal);
        // Asigna el titular a la sucursal y, si corresponde (titular no principal), el punto de venta que usa ahi.
        void AsignarTitularSucursal(int idSucursal, int idTitular, int? codPuntoVenta);
        // Punto de venta asignado a (titular, sucursal), o null.
        int? ObtenerPuntoVentaAsignado(int idTitular, int idSucursal);
        List<Entidades.TitularSucursalPuntoVenta> ListarPuntosVentaAsignados(int idTitular);
        // Guarda (o borra, con codPuntoVenta null) el punto de venta de un titular en una sucursal sin cambiar la
        // sucursal asignada.
        void GuardarPuntoVentaSucursal(int idTitular, int idSucursal, int? codPuntoVenta);

        // ---- Puntos de venta de ARCA de un titular no principal ----
        List<Entidades.TitularPuntoVenta> ListarPuntosVenta(int idTitular);
        void GuardarPuntoVenta(int idEmpresa, Entidades.TitularPuntoVenta puntoVenta);
        void EliminarPuntoVenta(int idTitular, int codPuntoVenta);

        // ---- Facturado (solo produccion, con CAE; las notas de credito restan) ----
        decimal Facturado(int idTitular, DateTime desde, DateTime hasta);
        // Lo facturado por un CUIT en CADA empresa donde figura como titular (la propia incluida; titulares cancelados tambien
        // cuentan: la persona facturo igual) y por su CUIT como principal (comprobantes viejos sin titular). Solo lectura,
        // consulta cruzada con el rol de super-admin: devuelve un total por empresa, nunca datos de los comprobantes.
        List<Entidades.FacturadoEmpresa> FacturadoCuitPorEmpresa(long cuit, DateTime desde, DateTime hasta);

        // ---- Certificado por CUIT (tabla certificados_arca_cuit, compartida entre las empresas con ese CUIT) ----
        // Clave cifrada del .pfx de ese CUIT y entorno, o null si no hay (pfx historico sin clave o sin certificado).
        string ObtenerClaveCuit(long cuit, bool homologacion);
        // Inserta o actualiza la clave cifrada del CUIT y entorno. El nombre del archivo solo se escribe al crear la fila
        // (no pisa el nombre historico, p. ej. certif-prod.pfx). Con esto el titular queda "con certificado".
        void GuardarClaveCuit(long cuit, bool homologacion, string nombreCertificado, string claveProtegida);
    }
}
