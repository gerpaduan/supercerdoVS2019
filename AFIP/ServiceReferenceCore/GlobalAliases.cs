// Compila SOLO en net10.0 (ver AFIP.csproj). GenerarFacturaService.cs referencia estos tipos sin
// calificar (FEAuthRequest, FECAERequest, etc., resueltos en net472 por el `using AFIP.WSFEHOMO;`
// que trae el proxy ASMX viejo) -- aca se declaran alias globales de compilacion hacia los tipos
// reales generados en AFIP.WSFECore (ver WSFEServiceReference.cs) para que el mismo codigo fuente
// compile sin cambios en los dos TFM. No se puede resolver esto con un alias de NAMESPACE
// (`using AFIP.WSFEHOMO = AFIP.WSFECore;` no es sintaxis valida en C#, el lado izquierdo de un
// using-alias-directive tiene que ser un identificador simple, no un nombre calificado) -- por eso
// son alias de TIPO, uno por cada tipo que GenerarFacturaService.cs usa sin calificar.
global using FEAuthRequest = AFIP.WSFECore.FEAuthRequest;
global using FECAERequest = AFIP.WSFECore.FECAERequest;
global using FECAECabRequest = AFIP.WSFECore.FECAECabRequest;
global using FECAEDetRequest = AFIP.WSFECore.FECAEDetRequest;
global using FECAEResponse = AFIP.WSFECore.FECAEResponse;
global using AlicIva = AFIP.WSFECore.AlicIva;
global using CbteAsoc = AFIP.WSFECore.CbteAsoc;
