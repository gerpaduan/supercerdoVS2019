namespace Carnisys.Balanza.Agent
{
    // Clave PUBLICA con la que el agente verifica las actualizaciones (RSA-2048, SHA-256). La privada NO esta en el repo: vive en
    // ~/hosts/carnisys-balanza-firma.env y la usa scripts/publicar-agente-balanza.ps1 para firmar cada .exe publicado.
    // Rotar la clave = generar un par nuevo, cambiar esta constante, publicar un agente nuevo firmado con la clave VIEJA (que lo
    // instala) y recien despues firmar con la nueva. Ver docs/06-datos-e-integraciones/balanzas-e-impresoras.md.
    internal static class ActualizacionClave
    {
        public const string PublicaXml = "<RSAKeyValue><Modulus>re15+9i1BQmV0VovYWaOkIHhbyllEvf+E9NJjsNMv9KcEA0XeByk+MYTwL1n4pgXka/HSKSHezC+KPcECX1y1yEwCtcnN+vIB7SnN+ADXXWsl3fFSJ/kqtFSVtXAh8hbFHrIixiYOQXczgCoa05Yc37Gj6W7PtZS1KhfjpsqLkhZydR8oqA+5hEFKvPBeO//7Bpn2LFBVXlj1kFWJb5zd62kmpKFyyhcl+9nzDvL1ya99cQH3crwx4Qgi63WfD92ABGemo02vOPbBuGo/bOYBJEC2jJscgo5fAEKTQo4BZmWFB3Ar0I4Lb5gbtCX7SgDXuhBpLd0vJypZBBYdTsVHQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    }
}
