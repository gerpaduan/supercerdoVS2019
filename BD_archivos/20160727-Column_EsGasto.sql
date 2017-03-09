use SuperCerdo

ALTER TABLE dbo.TiposEgresoCaja ADD esGasto tinyint NULL;
ALTER TABLE dbo.EgresosCaja ADD esGasto tinyint NULL;

update dbo.TiposEgresoCaja set esGasto = 1;
update dbo.EgresosCaja set esGasto = 1;


--update dbo.EgresosCaja set esGasto = 0 where dbo.EgresosCaja.idTipoEgresoCaja = id;