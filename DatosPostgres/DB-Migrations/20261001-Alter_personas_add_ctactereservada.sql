-- Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01): una persona reservada solo
-- tiene sus movimientos visibles para admin / permiso formCtasCtes; el resto de los
-- usuarios ve unicamente lo que cargo desde la apertura de su caja. Reemplaza el uso de
-- personas.ctacte, que queda en la base sin efecto funcional (el switch se oculta).
-- Aditiva e idempotente. Correr conectado a 'carnisys' con el rol carnisys_admin ANTES de
-- desplegar el codigo.

DO $$
BEGIN
    -- Solo la primera vez (cuando la columna todavia no existe) se marcan los empleados
    -- ya cargados: los empleados nacen reservados. Si el admin destilda el flag despues,
    -- una re-ejecucion de este script no lo vuelve a pisar.
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'personas' AND column_name = 'ctactereservada'
    ) THEN
        ALTER TABLE personas ADD COLUMN ctactereservada boolean NOT NULL DEFAULT false;

        UPDATE personas p
           SET ctactereservada = true
         WHERE EXISTS (SELECT 1 FROM empleado e WHERE e.idpersona = p.idpersona);
    END IF;
END $$;
