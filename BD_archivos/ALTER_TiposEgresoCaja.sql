USE [SuperCerdo]

alter table [dbo].[TiposEgresoCaja] add esCompra tinyint null;

declare
 @id int = (select MAX(id) from TiposEgresoCaja) + 1;

insert into TiposEgresoCaja (id, tipoEgresoCaja, orden, esCompra)
	values (@id,'Compra', 1, 1);