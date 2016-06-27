use SuperCerdo

update Corte set mayorista = 0, enCierreStock = 0;

update Corte set mayorista = 1 where codigo > 5000;

update Corte set enCierreStock = 1 where independiente = 1 and (codigo > 0 and codigo < 150);
