namespace AROCONSTRUCCIONES.Services.Helpers
{
    public static class NumeroALetras
    {
        public static string Convertir(decimal numero, string moneda = "PEN")
        {
            string monedaNombre = (moneda == "PEN" || moneda == "SOLES") ? "SOLES" : "DÓLARES AMERICANOS";

            int entero = (int)Math.Truncate(numero);
            int decimales = (int)((numero - entero) * 100);

            string letras = ConvertirEntero(entero);
            return $"{letras} CON {decimales:00}/100 {monedaNombre}";
        }

        private static string ConvertirEntero(int n)
        {
            if (n == 0) return "CERO";
            if (n < 0) return "MENOS " + ConvertirEntero(Math.Abs(n));
            if (n < 10) return Unidades[n];
            if (n < 20) return Especiales[n - 10];
            if (n < 100) return Centenas(n);
            if (n < 1000) return ConvertirCientos(n);
            if (n < 1000000) return ConvertirMiles(n);
            return "NÚMERO MUY GRANDE";
        }

        private static string[] Unidades = { "CERO", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
        private static string[] Especiales = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };

        private static string Centenas(int n)
        {
            int unidad = n % 10;
            int decena = n / 10;
            string[] DecenasDiez = { "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
            if (n < 30) return (n == 20) ? "VEINTE" : "VEINTI" + Unidades[unidad].ToLower();
            return DecenasDiez[decena] + (unidad > 0 ? " Y " + Unidades[unidad] : "");
        }

        private static string ConvertirCientos(int n)
        {
            if (n == 100) return "CIEN";
            int centena = n / 100;
            int resto = n % 100;
            string txtCentena = centena switch { 1 => "CIENTO", 5 => "QUINIENTOS", 7 => "SETECIENTOS", 9 => "NOVECIENTOS", _ => Unidades[centena] + "CIENTOS" };
            return txtCentena + (resto > 0 ? " " + ConvertirEntero(resto) : "");
        }

        private static string ConvertirMiles(int n)
        {
            int miles = n / 1000;
            int resto = n % 1000;
            string txtMiles = (miles == 1) ? "MIL" : ConvertirEntero(miles) + " MIL";
            return txtMiles + (resto > 0 ? " " + ConvertirEntero(resto) : "");
        }
    }
}