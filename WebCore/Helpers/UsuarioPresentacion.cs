// Detalles de presentacion compartidos por las pantallas de Usuarios (listado y ficha, rediseno 2026-11-10): iniciales y color
// del avatar. El color sale del id para que un mismo usuario siempre se vea igual.
namespace WebCore.Helpers
{
    public static class UsuarioPresentacion
    {
        private static readonly string[] ColoresAvatar = { "#7c5cd6", "#2f9e74", "#d9822b", "#5b6b8c", "#c2417a", "#3a86c8", "#b5651d", "#8a4fb3" };

        // "Lucia Ferreyra" -> "LF"; un solo nombre -> su inicial; vacio -> "?".
        public static string Iniciales(string? nombre)
        {
            var partes = (nombre ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "?";
            return partes.Length == 1
                ? partes[0].Substring(0, 1).ToUpperInvariant()
                : (partes[0].Substring(0, 1) + partes[1].Substring(0, 1)).ToUpperInvariant();
        }

        public static string ColorAvatar(int id) => ColoresAvatar[Math.Abs(id) % ColoresAvatar.Length];
    }
}
