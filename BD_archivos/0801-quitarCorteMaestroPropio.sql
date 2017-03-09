use SuperCerdo

--quitar el corte maestro cuando es el mismo
update Corte set idCorteMaestro = 0 where idCorte = idCorteMaestro

--Aca deberia quitarse los maestro con codigo < 0
update Corte set idCorteMaestro = 0 WHERE     (idCorteMaestro IN
                          (SELECT     idCorte
                            FROM          dbo.Corte AS Corte_1
                            WHERE      (codigo < 0)))
