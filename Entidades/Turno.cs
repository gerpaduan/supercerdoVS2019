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
    // Feriado (2026-09-30, ver docs/DECISIONS.md): un dia puntual marcado feriado en
    // RegistroJornada.EsFeriado resuelve primero contra una EmpleadoTarifa con DiaSemana=Feriado
    // antes de caer al dia real -- unifica "feriado" y "dia no laborable" en un solo valor
    // (decision deliberada, no se modela la distincion legal entre ambos).
    public enum DiaSemana
    {
        Lunes,
        Martes,
        Miercoles,
        Jueves,
        Viernes,
        Sabado,
        Domingo,
        Feriado,
    }
}
