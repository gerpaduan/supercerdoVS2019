namespace Entidades
{
    // Turno de trabajo: dos valores fijos (decision del usuario, 2026-09-29 -- ver docs/DECISIONS.md,
    // no se hizo catalogo configurable). Usado por EmpleadoTarifa, TarifaGeneral y RegistroJornada.
    public enum Turno
    {
        Manana,
        Tarde,
    }

    // Dia de la semana, para la tarifa opcional por dia (ej. sabado con valor distinto).
    public enum DiaSemana
    {
        Lunes,
        Martes,
        Miercoles,
        Jueves,
        Viernes,
        Sabado,
        Domingo,
    }
}
