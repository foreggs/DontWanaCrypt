using System;
using System.Linq;

namespace DontWanaCrypt
{

    public class Validator
    {
        public string Label { get; set; }
        public Func<string, bool> IsValid { get; set; }

        public Validator(string label, Func<string, bool> isValid)
        {
            Label = label;
            IsValid = isValid;
        }
    }

    public enum InputType
    {
        Text,
        Password
    }
    public class InputConfiguration
    {

        public InputType Type { get; set; } = InputType.Text;
        public string Label { get; set; } = "Input";
        public Validator[] Validators { get; set; } = new Validator[] { };
        public bool ShowValidationErrors { get; set; } = true;
        public string Pre { get; set; } = "";
    }
    public static class OneInput
    {
        private static bool Render(InputConfiguration config, string output)
        {
            Console.Clear();

            Console.WriteLine(config.Pre);

            bool isValid = true;
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var validator in config.Validators)
            {
                if (!validator.IsValid(output))
                {
                    if (config.ShowValidationErrors)
                        Console.WriteLine($"{new String(' ', config.Label.Length)} - {validator.Label}");
                    isValid = false;
                }
            }
            Console.ResetColor();


            Console.Write($"{config.Label}: ");

            Console.ForegroundColor = isValid ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write(config.Type == InputType.Password ? new string(output.Select(c => '*').ToArray()) : output);

            Console.ResetColor();

            return isValid;
        }

        public static string Input(InputConfiguration config)
        {
            string output = "";

            while (true)
            {
                bool valid = Render(config, output);
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    if (valid)
                        break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (output.Length > 0)
                        output = output.Remove(output.Length - 1, 1);
                }
                else
                    output += i.KeyChar;
            }
            Console.Clear();
            return output;
        }
    }
}
