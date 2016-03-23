use SuperCerdo

alter table Embutidos 
	add	creado	datetime,	
		creadoPor int,	
	 	actualizado	datetime,	
	 	actualizadoPor int;
	 	
update Embutidos set creado = fechaEmbutido;