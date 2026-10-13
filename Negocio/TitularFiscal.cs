using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Titulares fiscales por empresa y por sucursal (2026-10-09, ver docs/DECISIONS.md "Titulares fiscales por empresa y
    // por sucursal"). Solo Postgres (en SQL Server NegocioFactory.CrearTitularFiscal devuelve null y se factura con los
    // datos de la empresa como siempre). Reglas puras (resolver emisor y punto de venta, validar altas, ventanas de
    // tiempo) sobre Contratos.ITitularFiscalRepository, para poder probarlas sin base de datos.
    // Codigo C# 7.3: Negocio tambien compila para net472.
    public class TitularFiscal
    {
        private readonly Contratos.ITitularFiscalRepository oTitularD;

        public const int IdIvaRRII = 2;
        public const int IdIvaMonotributo = 3;

        public TitularFiscal(Contratos.ITitularFiscalRepository repositorio)
        {
            oTitularD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        // ------------------------------------------------------------------ consultas simples

        public List<Entidades.TitularFiscal> Listar(int idEmpresa, bool soloHabilitados)
        {
            return oTitularD.Listar(idEmpresa, soloHabilitados);
        }

        public Entidades.TitularFiscal Obtener(int idTitular)
        {
            return idTitular <= 0 ? null : oTitularD.Obtener(idTitular);
        }

        public Entidades.TitularFiscal ObtenerPrincipal(int idEmpresa)
        {
            return oTitularD.ObtenerPrincipal(idEmpresa);
        }

        public int? ObtenerIdTitularDeSucursal(int idSucursal)
        {
            return oTitularD.ObtenerIdTitularDeSucursal(idSucursal);
        }

        public List<Entidades.TitularPuntoVenta> ListarPuntosVenta(int idTitular)
        {
            return oTitularD.ListarPuntosVenta(idTitular);
        }

        public List<Entidades.TitularSucursalPuntoVenta> ListarPuntosVentaAsignados(int idTitular)
        {
            return oTitularD.ListarPuntosVentaAsignados(idTitular);
        }

        public decimal Facturado(int idTitular, DateTime desde, DateTime hasta)
        {
            return oTitularD.Facturado(idTitular, desde, hasta);
        }

        // ------------------------------------------------------------------ certificado por CUIT (compartido entre empresas)

        // Clave cifrada del .pfx de un CUIT y entorno (tabla certificados_arca_cuit), o null.
        public string ObtenerClaveCuit(long cuit, bool homologacion)
        {
            return oTitularD.ObtenerClaveCuit(cuit, homologacion);
        }

        // Guarda la clave cifrada del CUIT y deja registrado el certificado (el titular pasa a "tener certificado").
        public void GuardarClaveCuit(long cuit, bool homologacion, string nombreCertificado, string claveProtegida)
        {
            oTitularD.GuardarClaveCuit(cuit, homologacion, nombreCertificado, claveProtegida);
        }

        // ------------------------------------------------------------------ titular compartido entre empresas (2026-10-12)

        // Otras empresas que ya tienen ese CUIT como titular (sin revocar). Solo ids y nombres.
        public List<Entidades.EmpresaConCuit> OtrasEmpresasConCuit(long cuit, int idEmpresaPropia)
        {
            return oTitularD.OtrasEmpresasConCuit(cuit, idEmpresaPropia);
        }

        // Empresa que administra el certificado de ese CUIT (la que lo tiene como principal o, si ninguna, la del titular
        // mas antiguo). 0 = nadie lo tiene.
        public int EmpresaAdministradoraDeCuit(long cuit)
        {
            return oTitularD.EmpresaAdministradoraDeCuit(cuit);
        }

        // true si la empresa puede pedir, instalar o descartar el certificado de ese CUIT. Si todavia nadie lo administra
        // (0) se deja pasar: es el primer alta del CUIT.
        public bool PuedeAdministrarCertificado(long cuit, int idEmpresa)
        {
            int administradora = oTitularD.EmpresaAdministradoraDeCuit(cuit);
            return administradora == 0 || administradora == idEmpresa;
        }

        public List<Entidades.EmpresaConCuit> OtrasEmpresasConPuntoVenta(long cuit, int codPuntoVenta, int idEmpresaPropia)
        {
            return oTitularD.OtrasEmpresasConPuntoVenta(cuit, codPuntoVenta, idEmpresaPropia);
        }

        // Para el alta de un titular cuyo CUIT ya esta en otras empresas: el super-admin tiene que confirmar de forma
        // explicita que esa empresa usara el mismo certificado. Devuelve el mensaje de error o null si se puede seguir.
        public string ErrorSiFaltaConfirmarCompartido(long cuit, int idEmpresa, bool confirmado)
        {
            var otras = oTitularD.OtrasEmpresasConCuit(cuit, idEmpresa);
            if (otras.Count == 0 || confirmado) return null;
            return "Ese CUIT ya es titular en: " + string.Join(", ", otras.Select(e => e.Nombre)) + ". Esta empresa usaría el mismo certificado de ARCA; confirmalo para continuar.";
        }

        // Cancela la titularidad (solo el super-admin; lo verifica el controlador). Si el titular nunca facturo en la empresa
        // se borra (puede haber sido un error de asignacion y la empresa no ve nada); si ya facturo queda revocado, de solo
        // lectura. En ambos casos sus sucursales vuelven al titular principal y no se puede emitir factura ni nota de credito.
        public Entidades.ResultadoRevocacion Revocar(int idEmpresa, int idTitular, int idUsuario)
        {
            var titular = oTitularD.Obtener(idTitular);
            if (titular == null || titular.IdEmpresa != idEmpresa)
                return new Entidades.ResultadoRevocacion { Ok = false, Mensaje = "El titular no existe." };
            if (titular.EsPredeterminado)
                return new Entidades.ResultadoRevocacion { Ok = false, Mensaje = "El titular principal (la propia empresa) no se puede cancelar." };
            if (titular.Revocado)
                return new Entidades.ResultadoRevocacion { Ok = false, Mensaje = "La titularidad ya estaba cancelada." };

            bool facturo = oTitularD.TieneFacturas(idTitular);
            oTitularD.Revocar(idTitular, !facturo, idUsuario);
            return new Entidades.ResultadoRevocacion
            {
                Ok = true,
                Borrado = !facturo,
                Mensaje = facturo
                    ? "Titularidad cancelada. Como ya facturó en esta empresa queda visible, de solo lectura."
                    : "Titularidad cancelada. Como nunca facturó en esta empresa se eliminó y la empresa ya no lo ve."
            };
        }

        // Repone una titularidad cancelada (queda deshabilitado: la empresa decide cuando habilitarlo).
        public string Reponer(int idEmpresa, int idTitular)
        {
            var titular = oTitularD.Obtener(idTitular);
            if (titular == null || titular.IdEmpresa != idEmpresa) return "El titular no existe.";
            if (!titular.Revocado) return "La titularidad no está cancelada.";
            oTitularD.Reponer(idTitular);
            return null;
        }

        // ------------------------------------------------------------------ resolver emisor

        // Titular con el que se factura una venta de esa sucursal: el pedido (elegido en el modal, vale solo para esa
        // factura), o el asignado a la sucursal, o el principal. Debe ser de la empresa y estar habilitado, y tener un
        // punto de venta resoluble. NO exige certificado (si falta, la emision lo informa con un mensaje claro).
        public Entidades.ResultadoEmisor ResolverEmisor(Entidades.Empresa empresa, Entidades.Sucursal sucursal, int? idTitularSolicitado)
        {
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));
            if (sucursal == null) throw new ArgumentNullException(nameof(sucursal));

            Entidades.TitularFiscal titular = null;
            if (idTitularSolicitado.HasValue && idTitularSolicitado.Value > 0)
            {
                titular = oTitularD.Obtener(idTitularSolicitado.Value);
            }
            else
            {
                // La asignacion se lee por idSucursal (no de venta.Sucursal: la columna nueva no viaja ahi).
                int? asignado = oTitularD.ObtenerIdTitularDeSucursal(sucursal.IdSucursal);
                titular = asignado.HasValue ? oTitularD.Obtener(asignado.Value) : null;
                if (titular == null) titular = oTitularD.ObtenerPrincipal(empresa.IdEmpresa);
            }

            if (titular == null || titular.IdEmpresa != empresa.IdEmpresa)
                return Entidades.ResultadoEmisor.Error("TITULAR_NO_DISPONIBLE", "El titular elegido no existe o no pertenece a esta empresa.");

            if (titular.Revocado)
                return Entidades.ResultadoEmisor.Error("TITULAR_NO_DISPONIBLE", "La titularidad de " + titular.NombreCorto + " fue cancelada por el administrador del sistema: no se puede facturar con ese titular.");

            if (!titular.Habilitado)
                return Entidades.ResultadoEmisor.Error("TITULAR_NO_DISPONIBLE", "El titular " + titular.NombreCorto + " no está habilitado para facturar.");

            string motivo;
            int puntoVenta = ResolverPuntoVenta(titular, sucursal, out motivo);
            if (puntoVenta <= 0 && !titular.EsPredeterminado)
                return Entidades.ResultadoEmisor.Error("SIN_PUNTO_VENTA", motivo);

            return Entidades.ResultadoEmisor.Correcto(Entidades.EmisorFiscal.DesdeTitular(titular, empresa, sucursal, puntoVenta));
        }

        // Punto de venta del titular en la sucursal. Principal: el de la sucursal (como hoy, aunque sea 0). No principal:
        // el asignado a (titular, sucursal) o, si tiene un unico punto de venta habilitado, ese; si no, 0 (no disponible
        // en esa sucursal): el numero de punto de venta es por CUIT, usar el de la sucursal seria adivinar.
        public int ResolverPuntoVenta(Entidades.TitularFiscal titular, Entidades.Sucursal sucursal, out string motivo)
        {
            motivo = null;
            if (titular.EsPredeterminado)
                return sucursal.CodPuntoVentaAfip;

            int? asignado = oTitularD.ObtenerPuntoVentaAsignado(titular.IdTitular, sucursal.IdSucursal);
            if (asignado.HasValue && asignado.Value > 0)
                return asignado.Value;

            var habilitados = oTitularD.ListarPuntosVenta(titular.IdTitular).Where(p => p.Habilitado).ToList();
            if (habilitados.Count == 1)
                return habilitados[0].CodPuntoVentaAfip;

            motivo = habilitados.Count == 0
                ? "El titular " + titular.NombreCorto + " no tiene puntos de venta cargados."
                : "El titular " + titular.NombreCorto + " tiene varios puntos de venta y ninguno asignado a esta sucursal.";
            return 0;
        }

        // Titulares habilitados de la empresa con su punto de venta en esta sucursal, para el selector del modal.
        // Los que no se pueden usar aca van con Disponible = false y el motivo (sin punto de venta, sin certificado).
        public List<Entidades.TitularOpcion> OpcionesParaSucursal(Entidades.Empresa empresa, Entidades.Sucursal sucursal)
        {
            var opciones = new List<Entidades.TitularOpcion>();
            foreach (var titular in oTitularD.Listar(empresa.IdEmpresa, true))
            {
                string motivo;
                int puntoVenta = ResolverPuntoVenta(titular, sucursal, out motivo);
                bool disponible = true;
                if (puntoVenta <= 0 && !titular.EsPredeterminado) disponible = false;
                else if (!titular.TieneCertificado) { disponible = false; motivo = "Sin certificado de ARCA cargado."; }

                opciones.Add(new Entidades.TitularOpcion
                {
                    IdTitular = titular.IdTitular,
                    Nombre = titular.RazonSocial,
                    Cuit = titular.Cuit,
                    EsRRII = titular.EsRRII,
                    EsPredeterminado = titular.EsPredeterminado,
                    PuntoVenta = puntoVenta,
                    TieneCertificado = titular.TieneCertificado,
                    Disponible = disponible,
                    Motivo = disponible ? null : motivo
                });
            }
            return opciones;
        }

        // Sugerencia "facturar con un titular RI": solo si el emisor actual NO es RI y el cliente SI lo es (un monotributo
        // con otro monotributo factura C y no hay nada que sugerir). Devuelve los titulares RI disponibles en la sucursal.
        public List<Entidades.TitularOpcion> TitularesRIParaCliente(Entidades.Empresa empresa, Entidades.Sucursal sucursal,
            Entidades.EmisorFiscal emisorActual, int idIvaCliente)
        {
            if (emisorActual == null || emisorActual.EsRRII) return new List<Entidades.TitularOpcion>();
            if (idIvaCliente != IdIvaRRII) return new List<Entidades.TitularOpcion>();

            return OpcionesParaSucursal(empresa, sucursal)
                .Where(o => o.EsRRII && o.Disponible && o.IdTitular != (emisorActual.IdTitular ?? 0))
                .ToList();
        }

        // Emisor de una factura ya emitida (reimpresion, edicion, nota de credito): los datos guardados en el comprobante.
        // Para la NC hace falta ademas el nombre del certificado del titular de origen; se completa con su fila.
        // NO exige que el titular siga habilitado (la NC de una factura vieja se puede emitir igual), PERO si el super-admin
        // le cancelo la titularidad (Revocado) la NC se bloquea: la empresa ya no puede usar ese certificado.
        public Entidades.EmisorFiscal EmisorDeFactura(Entidades.FacturaElectronica factura, Entidades.Empresa empresa, Entidades.Sucursal sucursal)
        {
            var emisor = Entidades.EmisorFiscal.DesdeFactura(factura, empresa, sucursal);

            if (factura != null && factura.IdTitular.HasValue)
            {
                var titular = oTitularD.Obtener(factura.IdTitular.Value);
                if (titular != null)
                {
                    emisor.NombreCertificadoPfx = titular.NombreCertificadoPfx;
                    emisor.EsPredeterminado = titular.EsPredeterminado;
                    emisor.Revocado = titular.Revocado;
                }
            }
            else if (empresa != null)
            {
                // Comprobante anterior a esta funcion (o del principal sin id): su certificado es el de la empresa.
                emisor.NombreCertificadoPfx = empresa.NombreCertificado_pfx;
                emisor.EsPredeterminado = true;
            }
            return emisor;
        }

        // ------------------------------------------------------------------ altas y edicion

        // Valida y guarda un titular. Devuelve null si salio bien (con el id en idGuardado) o el mensaje de error.
        // Titular no principal: los datos de la persona (razon social, CUIT, condicion IVA) ya vienen copiados en el
        // objeto por el controlador. El principal solo guarda topes y ventana anual.
        public string Guardar(Entidades.TitularFiscal titular, out int idGuardado)
        {
            idGuardado = titular == null ? 0 : titular.IdTitular;
            if (titular == null) return "Datos de titular inválidos.";

            string errorTopes = ValidarTopes(titular);
            if (errorTopes != null) return errorTopes;

            if (titular.IdTitular > 0)
            {
                var existente = oTitularD.Obtener(titular.IdTitular);
                if (existente == null || existente.IdEmpresa != titular.IdEmpresa) return "El titular no existe.";
                if (existente.Revocado) return "La titularidad de este titular está cancelada: repónela para poder editarlo.";
                if (existente.EsPredeterminado)
                {
                    // Principal: sus datos fiscales son los de la empresa; solo topes y ventana.
                    existente.LimiteMensual = titular.LimiteMensual;
                    existente.LimiteAnual = titular.LimiteAnual;
                    existente.VentanaAnual = titular.VentanaAnual;
                    idGuardado = oTitularD.Guardar(existente);
                    return null;
                }
                // El CUIT de un titular no principal es inmutable: es la carpeta de su certificado.
                if (existente.Cuit != titular.Cuit)
                    return "El CUIT de un titular no se puede cambiar (es la carpeta de su certificado). Dalo de baja y creá otro.";
            }

            string errorDatos = ValidarDatosFiscales(titular);
            if (errorDatos != null) return errorDatos;

            if (oTitularD.ExisteCuit(titular.Cuit, titular.IdTitular))
                return "Esta empresa ya tiene a ese CUIT como titular (o es el CUIT de la propia empresa). Si la titularidad fue cancelada, hay que reponerla.";

            idGuardado = oTitularD.Guardar(titular);
            return null;
        }

        // Datos obligatorios para poder facturar con un titular no principal.
        public static string ValidarDatosFiscales(Entidades.TitularFiscal titular)
        {
            if (!titular.IdPersona.HasValue || titular.IdPersona.Value <= 0) return "Elegí la persona titular.";
            if (!Entidades.CuitHelper.EsValido(titular.Cuit.ToString())) return "El CUIT de la persona no es válido (11 dígitos con dígito verificador).";
            if (string.IsNullOrWhiteSpace(titular.RazonSocial)) return "La persona no tiene razón social cargada.";
            if (titular.IdIva != IdIvaRRII && titular.IdIva != IdIvaMonotributo)
                return "El titular debe ser Responsable Inscripto o Monotributista (corregí la condición IVA de la persona).";
            if (!titular.InicioActividad.HasValue) return "Cargá la fecha de inicio de actividades.";
            if (titular.Iibb <= 0) return "Cargá el número de Ingresos Brutos (IIBB).";
            return null;
        }

        public static string ValidarTopes(Entidades.TitularFiscal titular)
        {
            if (titular.LimiteMensual.HasValue && titular.LimiteMensual.Value < 0m) return "El tope mensual no puede ser negativo.";
            if (titular.LimiteAnual.HasValue && titular.LimiteAnual.Value < 0m) return "El tope anual no puede ser negativo.";
            if (titular.VentanaAnual != Entidades.TitularFiscal.VentanaMovil12Meses && titular.VentanaAnual != Entidades.TitularFiscal.VentanaCalendario)
                return "La ventana del tope anual es inválida.";
            return null;
        }

        // Habilitar / deshabilitar. No se deshabilita un titular que sea el de alguna sucursal.
        public string SetHabilitado(int idEmpresa, int idTitular, bool habilitado)
        {
            var titular = oTitularD.Obtener(idTitular);
            if (titular == null || titular.IdEmpresa != idEmpresa) return "El titular no existe.";
            if (titular.Revocado) return "La titularidad fue cancelada por el administrador del sistema: no se puede habilitar.";
            if (!habilitado)
            {
                var sucursales = oTitularD.SucursalesAsignadas(idTitular);
                if (sucursales.Count > 0)
                    return "No se puede deshabilitar: es el titular de " + string.Join(", ", sucursales) + ". Asigná otro titular a esas sucursales primero.";
            }
            oTitularD.SetHabilitado(idTitular, habilitado);
            return null;
        }

        // ------------------------------------------------------------------ sucursal y puntos de venta

        // Asigna el titular (y su punto de venta si no es el principal) a la sucursal. Devuelve null o el error.
        public string AsignarTitularSucursal(Entidades.Sucursal sucursal, int idEmpresa, int idTitular, int? codPuntoVenta)
        {
            if (sucursal == null || sucursal.IdEmpresa != idEmpresa) return "La sucursal no existe.";
            var titular = oTitularD.Obtener(idTitular);
            if (titular == null || titular.IdEmpresa != idEmpresa) return "El titular no existe.";
            if (titular.Revocado) return "La titularidad fue cancelada por el administrador del sistema.";
            if (!titular.Habilitado) return "El titular no está habilitado.";
            if (!titular.TieneCertificado) return "El titular todavía no tiene certificado de ARCA cargado.";

            if (titular.EsPredeterminado)
            {
                oTitularD.AsignarTitularSucursal(sucursal.IdSucursal, idTitular, null);
                return null;
            }

            var habilitados = oTitularD.ListarPuntosVenta(idTitular).Where(p => p.Habilitado).ToList();
            int elegido = codPuntoVenta ?? 0;
            if (elegido <= 0)
            {
                // Sin eleccion: sirve el ya asignado o el unico punto de venta del titular.
                int? previo = oTitularD.ObtenerPuntoVentaAsignado(idTitular, sucursal.IdSucursal);
                elegido = previo ?? (habilitados.Count == 1 ? habilitados[0].CodPuntoVentaAfip : 0);
            }
            if (elegido <= 0) return "Elegí el punto de venta que usa el titular en esta sucursal.";
            if (!habilitados.Any(p => p.CodPuntoVentaAfip == elegido))
                return "El punto de venta " + elegido + " no está en la lista (habilitada) del titular.";

            oTitularD.AsignarTitularSucursal(sucursal.IdSucursal, idTitular, elegido);
            return null;
        }

        // Punto de venta de un titular no principal en una sucursal donde NO es el asignado (se usa cuando se lo elige
        // para una sola factura). null o 0 lo borra.
        public string GuardarPuntoVentaSucursal(int idEmpresa, int idTitular, int idSucursal, int? codPuntoVenta)
        {
            var titular = oTitularD.Obtener(idTitular);
            if (titular == null || titular.IdEmpresa != idEmpresa) return "El titular no existe.";
            if (titular.EsPredeterminado) return "El titular principal usa el punto de venta de la sucursal.";
            if (codPuntoVenta.HasValue && codPuntoVenta.Value > 0)
            {
                if (!oTitularD.ListarPuntosVenta(idTitular).Any(p => p.Habilitado && p.CodPuntoVentaAfip == codPuntoVenta.Value))
                    return "El punto de venta " + codPuntoVenta.Value + " no está en la lista (habilitada) del titular.";
            }
            oTitularD.GuardarPuntoVentaSucursal(idTitular, idSucursal, codPuntoVenta.HasValue && codPuntoVenta.Value > 0 ? codPuntoVenta : null);
            return null;
        }

        public string GuardarPuntoVenta(int idEmpresa, Entidades.TitularPuntoVenta puntoVenta)
        {
            var titular = oTitularD.Obtener(puntoVenta.IdTitular);
            if (titular == null || titular.IdEmpresa != idEmpresa) return "El titular no existe.";
            if (titular.EsPredeterminado) return "El titular principal usa el punto de venta de la sucursal.";
            if (puntoVenta.CodPuntoVentaAfip <= 0 || puntoVenta.CodPuntoVentaAfip > 99999) return "El punto de venta debe ser un número entre 1 y 99999.";
            if (!puntoVenta.Habilitado)
            {
                // No se deshabilita un punto de venta asignado a una sucursal.
                if (oTitularD.ListarPuntosVentaAsignados(puntoVenta.IdTitular).Any(a => a.CodPuntoVentaAfip == puntoVenta.CodPuntoVentaAfip))
                    return "El punto de venta está asignado a una sucursal: cambiá esa asignación primero.";
            }
            if (puntoVenta.Origen != "ARCA") puntoVenta.Origen = "MANUAL";
            oTitularD.GuardarPuntoVenta(idEmpresa, puntoVenta);
            return null;
        }

        public string EliminarPuntoVenta(int idEmpresa, int idTitular, int codPuntoVenta)
        {
            var titular = oTitularD.Obtener(idTitular);
            if (titular == null || titular.IdEmpresa != idEmpresa) return "El titular no existe.";
            if (oTitularD.ListarPuntosVentaAsignados(idTitular).Any(a => a.CodPuntoVentaAfip == codPuntoVenta))
                return "El punto de venta está asignado a una sucursal: cambiá esa asignación primero.";
            oTitularD.EliminarPuntoVenta(idTitular, codPuntoVenta);
            return null;
        }

        // ------------------------------------------------------------------ facturado vs topes (solo informativo)

        // Rango del mes (primer y ultimo dia).
        public static void RangoMensual(int anio, int mes, out DateTime desde, out DateTime hasta)
        {
            desde = new DateTime(anio, mes, 1);
            hasta = desde.AddMonths(1).AddDays(-1);
        }

        // Rango del tope anual de un titular, referido al fin del mes elegido: 12 meses moviles = los 12 meses que
        // terminan en ese mes; calendario = del 1 de enero al fin de ese mes.
        public static void RangoAnual(string ventana, int anio, int mes, out DateTime desde, out DateTime hasta)
        {
            DateTime finMes;
            DateTime inicioMes;
            RangoMensual(anio, mes, out inicioMes, out finMes);
            hasta = finMes;
            desde = ventana == Entidades.TitularFiscal.VentanaCalendario
                ? new DateTime(anio, 1, 1)
                : inicioMes.AddMonths(-11);
        }

        public List<Entidades.ConsumoTitular> ConsumoDeTitulares(int idEmpresa, int anio, int mes)
        {
            var lista = new List<Entidades.ConsumoTitular>();
            foreach (var titular in oTitularD.Listar(idEmpresa, false))
            {
                var consumo = new Entidades.ConsumoTitular { Titular = titular };
                DateTime d, h;
                RangoMensual(anio, mes, out d, out h);
                consumo.MesDesde = d;
                consumo.MesHasta = h;
                consumo.FacturadoMes = oTitularD.Facturado(titular.IdTitular, d, h);

                RangoAnual(titular.VentanaAnual, anio, mes, out d, out h);
                consumo.AnualDesde = d;
                consumo.AnualHasta = h;
                consumo.FacturadoAnual = oTitularD.Facturado(titular.IdTitular, d, h);

                // Titular compartido entre empresas (2026-10-12): el tope real es de la persona, o sea del total que su CUIT
                // factura en TODAS las empresas donde figura (tambien las que le cancelaron la titularidad: facturo igual). Si el
                // CUIT esta en una sola empresa no se suma nada y el tope se compara con lo facturado en esta, como siempre.
                // Se consulta primero el periodo anual (una consulta) y el mensual solo si de verdad se comparte.
                if (titular.Cuit > 0 && !titular.Revocado)
                {
                    var anual = oTitularD.FacturadoCuitPorEmpresa(titular.Cuit, consumo.AnualDesde, consumo.AnualHasta);
                    if (anual.Count > 1)
                    {
                        consumo.PorEmpresaAnual = anual;
                        consumo.TotalCuitAnual = anual.Sum(x => x.Importe);
                        consumo.PorEmpresaMes = oTitularD.FacturadoCuitPorEmpresa(titular.Cuit, consumo.MesDesde, consumo.MesHasta);
                        consumo.TotalCuitMes = consumo.PorEmpresaMes.Sum(x => x.Importe);
                    }
                }
                lista.Add(consumo);
            }
            return lista;
        }
    }
}
