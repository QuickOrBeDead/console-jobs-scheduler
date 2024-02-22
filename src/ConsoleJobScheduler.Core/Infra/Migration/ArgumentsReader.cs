namespace ConsoleJobScheduler.Core.Infra.Migration
{
    public sealed class ArgumentsReader
    {
        private readonly IDictionary<string, string> _arguments;

        public ArgumentsReader(string[] args)
        {
            _arguments = new Dictionary<string, string>();

            for (var i = 0; i < args.Length; i++)
            {
                var argumentName = args[i];
                if (string.IsNullOrWhiteSpace(argumentName))
                {
                    continue;
                }

                if (argumentName.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                {
                    argumentName = argumentName[2..];
                    if (string.IsNullOrWhiteSpace(argumentName))
                    {
                        continue;
                    }

                    if (i < args.Length - 1)
                    {
                        _arguments.Add(argumentName, args[i + 1]);
                        i++;
                    }
                }
            }
        }

        public string? GetValue(string key)
        {
            if (_arguments.TryGetValue(key, out var value))
            {
                return value;
            }

            /*
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"{key} argument not found!");

            Environment.Exit(-1);
            */
            return null;
        }
    }
}