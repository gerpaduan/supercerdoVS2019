USE [SuperCerdo]

ALTER TABLE [dbo].[Personas] ADD ctaCte tinyint null;
ALTER TABLE [dbo].[Personas] ADD bonificacion float null;

UPDATE dbo.Personas set ctaCte = 0;
UPDATE dbo.Personas set bonificacion = 0;


