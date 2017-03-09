use SuperCerdo

ALTER TABLE dbo.Corte ADD promedio float NULL;
ALTER TABLE dbo.ActualizacionCorte ADD promedio float NULL;

UPDATE dbo.Corte set promedio = 1 where tipo = 'Unidad';
UPDATE dbo.Corte set promedio = 0 where tipo <> 'Unidad';

UPDATE dbo.ActualizacionCorte set promedio = 1 where tipo = 'Unidad';
UPDATE dbo.ActualizacionCorte set promedio = 0 where tipo <> 'Unidad';