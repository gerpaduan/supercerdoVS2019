use SuperCerdo

update Corte set creado = DATEADD(day, -5, SYSDATETIME()) where creado is null;

alter table Embutidos 
	add	creado	datetime,	
		creadoPor int,	
	 	actualizado	datetime,	
	 	actualizadoPor int;
	 	
update Embutidos set creado = fechaEmbutido;
