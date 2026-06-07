namespace FranchAdm
{
    public static class UserSession
    {
        public static int UserId { get; set; }
        public static string FullName { get; set; } = string.Empty;
        public static string RoleName { get; set; } = string.Empty;

        // Быстрая проверка прав
        public static bool IsAdmin => RoleName == "Администратор";
        public static bool IsManager => RoleName == "Менеджер";

        public static void Clear()
        {
            UserId = 0;
            FullName = string.Empty;
            RoleName = string.Empty;
        }
    }
}