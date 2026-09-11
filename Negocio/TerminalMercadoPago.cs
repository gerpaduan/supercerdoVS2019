using System;
using System.Collections.Generic;

namespace Negocio
{
    // Terminales fisicas Mercado Pago Point dadas de alta por sucursal. Sin precedente en SQL
    // Server -- nace directo sobre Postgres (Contratos.ITerminalMercadoPagoRepository).
    public class TerminalMercadoPago
    {
        private readonly Contratos.ITerminalMercadoPagoRepository oTerminalD;

        public TerminalMercadoPago(Contratos.ITerminalMercadoPagoRepository repositorio)
        {
            oTerminalD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public List<Entidades.TerminalMercadoPago> Listar(int idEmpresa)
        {
            return oTerminalD.Listar(idEmpresa);
        }

        public List<Entidades.TerminalMercadoPago> ListarPorSucursal(int idSucursal)
        {
            return oTerminalD.ListarPorSucursal(idSucursal);
        }

        // Se llama justo despues de crear la Caja/POS en Mercado Pago (Fase 3) -- todavia no
        // hay terminal fisica vinculada en ese momento (eso es un paso manual posterior, ver
        // VincularTerminalFisica), asi que nace inactiva y sin TerminalIdMp.
        public void Agregar(Entidades.TerminalMercadoPago terminal)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            if (string.IsNullOrWhiteSpace(terminal.PosId))
                throw new ArgumentException("El pos_id de Mercado Pago es obligatorio.", nameof(terminal));

            terminal.TerminalIdMp = null;
            terminal.FechaAltaUtc = DateTime.UtcNow;
            terminal.Activo = false;
            oTerminalD.Agregar(terminal);
        }

        public void Actualizar(Entidades.TerminalMercadoPago terminal)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            oTerminalD.Actualizar(terminal);
        }

        // Se llama despues de que el admin empareja la terminal fisica con la app de Mercado
        // Pago (paso manual, fuera de CarniSys) y CarniSys verifica el resultado consultando
        // GET /terminals/v1/list (Negocio.MercadoPagoPointClient.BuscarTerminalVinculada).
        public void VincularTerminalFisica(Entidades.TerminalMercadoPago terminal, string terminalIdMp)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            if (string.IsNullOrWhiteSpace(terminalIdMp))
                throw new ArgumentException("El terminal_id de Mercado Pago es obligatorio.", nameof(terminalIdMp));

            terminal.TerminalIdMp = terminalIdMp.Trim();
            terminal.Activo = true;
            oTerminalD.Actualizar(terminal);
        }

        public void Eliminar(int id, int idEmpresa)
        {
            oTerminalD.Eliminar(id, idEmpresa);
        }
    }
}
