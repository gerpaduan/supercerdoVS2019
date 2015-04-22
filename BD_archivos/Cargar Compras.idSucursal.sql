use SuperCerdo
DECLARE @idSucursal INT, @idCompra INT, @lastId int, @tipoCompra varchar(50)
SET @idCompra = 1
SET @lastId = (select top 1 Compras.idCompra from Compras order by Compras.idCompra desc)
set @idSucursal = 0		
set @tipoCompra = 'Media Res'
	
WHILE @idCompra <= @lastId
BEGIN
 --   update Compras set idSucursal = (select top 1 idSucursal 
	--			from CortePorCompra where CortePorCompra.idCompra = Compras.idCompra)
	--where Compras.idCompra = @idCompra	
	set @tipoCompra = (select Compras.tipoCompra from Compras where Compras.idCompra = @idCompra)
	
	IF @tipoCompra = 'Media Res'
		BEGIN	
			set @idSucursal = (select top 1 MediaRes.idSucursal 
					from MediaRes where MediaRes.idCompra = @idCompra)
					
			update Compras set idSucursal = @idSucursal
			where Compras.idCompra = @idCompra
		END	
	ELSE 
		BEGIN		
			
			set @idSucursal = (select top 1 CortePorCompra.idSucursal 
					from CortePorCompra where CortePorCompra.idCompra = @idCompra)
					
			update Compras set idSucursal = @idSucursal
			where Compras.idCompra = @idCompra
		END
			
    SET @idCompra = @idCompra + 1
    set @idSucursal = 0    
END

--se actualizan los tipoCompra
update Compras set tipoCompra = 'Cierre Stock' where idProveedor = 13

update Compras set tipoCompra = 'Egreso Stock' where idProveedor = 28