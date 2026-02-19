using System.Text.RegularExpressions;

namespace AbaiLib
{
    public class TerminalEmulator
    {
        bool updated = false;
        bool mysqlInstalled = false;
        bool mysqlStarted = false;
        bool dbCreated = false;

        public string HandleCommand(string input)
        {
            if (input == "exit")
                return "";

            if (!IsSafe(input))
            {
                return "Forbidden command.";
            }

            return Process(input);
        }

        bool IsSafe(string input)
        {
            return !input.Contains(";") &&
                   !input.Contains("&&") &&
                   !input.Contains("|") &&
                   !input.Contains("rm");
        }

        string Process(string input)
        {
            switch (input)
            {
                case "sudo apt update":
                    updated = true;
                    return "Hit:1 http://archive.ubuntu.com\nReading package lists... Done";

                case "sudo apt install mysql-server -y":
                    if (!updated)
                        return "You must run 'sudo apt update' first.";

                    mysqlInstalled = true;
                    return "Installing mysql-server...\nDone.";

                case "sudo systemctl start mysql":
                    if (!mysqlInstalled)
                        return "MySQL is not installed.";

                    mysqlStarted = true;
                    return "MySQL service started.";

                case var s when Regex.IsMatch(s, @"^sudo mysql -e ""CREATE DATABASE [a-zA-Z0-9_]+;""$"):
                    if (!mysqlStarted)
                        return "MySQL service is not running.";

                    dbCreated = true;
                    return "Query OK, 1 row affected.";

                default:
                    return $"bash: {input}: command not found";
            }
        }
    }
}
