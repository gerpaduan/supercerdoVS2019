-- Etapa: Empresa.cs (tercero de los modulos chicos 0%-migrados). 2 metodos: findById,
-- ActualizarDatosBasicos (pantalla "Mi Empresa").
--
-- "empresas" ya existia en Postgres desde la Etapa 3 (creada junto con "sucursal" para el
-- piloto de findEmpresaById), pero le faltaban las 4 columnas de horario laboral que se
-- agregaron en SQL Server el 2026-08-14 (feature de restricciones de login por horario/
-- ubicacion, ver docs/DECISIONS.md esa fecha) -- drift de schema entre las dos bases,
-- detectado recien ahora. Mismos defaults que el original (00:00:00 / 23:59:59 = sin
-- restriccion real hasta que el admin las acote).
ALTER TABLE empresas
    ADD COLUMN horariodiurnodesde time NOT NULL DEFAULT '00:00:00',
    ADD COLUMN horariodiurnohasta time NOT NULL DEFAULT '23:59:59',
    ADD COLUMN horariotardedesde time NOT NULL DEFAULT '00:00:00',
    ADD COLUMN horariotardehasta time NOT NULL DEFAULT '23:59:59';
