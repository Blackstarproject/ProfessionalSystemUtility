using System.Security.Cryptography;
using System.Text;

namespace PasswordGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Password Credential Generator ---");
            Console.Write("Enter the new password you want to use: ");

            // Read password securely from console
            string newPassword = ReadPassword();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                Console.WriteLine("\nPassword cannot be empty. Exiting.");
                return;
            }

            // Generate a new random salt
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            // Hash the new password with the new salt
            string hash;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(newPassword, saltBytes, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = rfc2898DeriveBytes.GetBytes(256 / 8);
                hash = Convert.ToBase64String(hashBytes);
            }

            // Create the final App.config XML block
            StringBuilder configBlock = new();
            configBlock.AppendLine();
            configBlock.AppendLine("<!-- Copy the entire <appSettings> block below and paste it into your App.config file -->");
            configBlock.AppendLine("<appSettings>");
            configBlock.AppendLine($"    <add key=\"PasswordHash\" value=\"{hash}\" />");
            configBlock.AppendLine($"    <add key=\"Salt\" value=\"{salt}\" />");
            configBlock.AppendLine("</appSettings>");

            Console.WriteLine("\n--- GENERATION COMPLETE ---");
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(configBlock.ToString());
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        /// <summary>
        /// Reads a password from the console without displaying the characters.
        /// </summary>
        public static string ReadPassword()
        {
            var pass = new StringBuilder();
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (!char.IsControl(key.KeyChar))
                {
                    pass.Append(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass.Remove(pass.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);
            return pass.ToString();
        }
    }
}