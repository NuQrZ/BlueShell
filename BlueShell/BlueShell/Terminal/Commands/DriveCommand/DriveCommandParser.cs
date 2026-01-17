using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public static class DriveCommandParser
    {
        private static readonly Regex Full =
            new(@"^\s*Drive(?:\s+(?<Target>(""[^""]*""|\[[^\]]*\]))\s*)?(?:\s+(?<Operation>-\S+))?(?:\s+(?<Extra>-\S+))?(?<Rest>(?:\s+.+)?)\s*$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex IndexExact = new(@"^\[(?<n>\d+)\]$", RegexOptions.Compiled);

        private static readonly Regex EmptyIndex = new(@"^\[\s*\]$", RegexOptions.Compiled);

        public static bool TryParse(
            string commandLine,
            out string driveTarget,
            out string operation,
            out string extraOperation,
            out List<Tuple<string, TerminalMessageKind>> errorMessages)
        {
            driveTarget = "";
            operation = "";
            extraOperation = "";
            errorMessages = [];

            Match match = Full.Match(commandLine);
            if (!match.Success)
            {
                errorMessages.Add(Tuple.Create("\n>> Invalid syntax!\n", TerminalMessageKind.Error));
                return false;
            }

            string targetText = match.Groups["Target"].Success ? match.Groups["Target"].Value.Trim() : "";
            string operationText = match.Groups["Operation"].Success ? match.Groups["Operation"].Value.Trim() : "";
            string extraOperationText = match.Groups["Extra"].Success ? match.Groups["Extra"].Value.Trim() : "";
            string rest = match.Groups["Rest"].Success ? match.Groups["Rest"].Value.Trim() : "";

            operation = NormalizeOperation(operationText);
            extraOperation = NormalizeExtraOperation(extraOperationText);

            (List<Tuple<string, TerminalMessageKind>>, bool, string) output = GenerateOutput(
                targetText,
                operation,
                extraOperation,
                rest);

            errorMessages = output.Item1;
            driveTarget = output.Item3;

            return output.Item2;
        }

        private static (List<Tuple<string, TerminalMessageKind>>, bool, string) GenerateOutput(string driveTarget, string operation, string extraOperation, string rest)
        {
            List<Tuple<string, TerminalMessageKind>> errorMessages = [];
            bool returnValue = false;
            string drive = "";

            if (operation == "GetDrives")
            {
                if (!string.IsNullOrWhiteSpace(driveTarget))
                {
                    errorMessages.Add(Tuple.Create(">> This operation does not take any drive target!\n", TerminalMessageKind.Warning));

                    returnValue = false;
                }
                else if (extraOperation != "Print" && extraOperation != "")
                {
                    errorMessages.Add(Tuple.Create($">> Unknown option: \"{extraOperation}\".\nAllowed options are:\n-Print\n", TerminalMessageKind.Error));

                    returnValue = false;
                }
                else
                {
                    driveTarget = "";
                    returnValue = true;
                }
            }

            bool isKnownOperation = IsKnownOperation(operation);
            bool isKnownExtraOperation = IsKnownExtraOperation(extraOperation);

            if (operation != "GetDrives")
            {
                if (string.IsNullOrWhiteSpace(driveTarget))
                {
                    errorMessages.Add(Tuple.Create(">> Missing drive target! Use \"C:\\\" ...\n", TerminalMessageKind.Error));
                    returnValue = false;
                }
                else if (EmptyIndex.IsMatch(driveTarget))
                {
                    errorMessages.Add(Tuple.Create(">> Index cannot be empty! Use [0], [1], ...\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }
                else if (IndexExact.IsMatch(driveTarget))
                {
                    drive = driveTarget;
                    returnValue = true;
                }
                else if (driveTarget.StartsWith('[') && driveTarget.EndsWith(']'))
                {
                    errorMessages.Add(Tuple.Create(">> Index must be a number like [0].\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }
                else if (driveTarget is ['"', _, ..] && driveTarget[^1] == '"')
                {
                    drive = driveTarget[1..^1];
                    returnValue = true;
                }
                else
                {
                    errorMessages.Add(Tuple.Create(">> Drive target must be quoted. Use \"C:\\\" or an index like [0].\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }
            }

            if (string.IsNullOrEmpty(operation))
            {
                errorMessages.Add(Tuple.Create(">> Operation is required.\nAllowed operations are: \n-GetDrives\n-Open\n-Properties\n-Advanced\n", TerminalMessageKind.Error));
                returnValue = false;
            }
            else
            {
                if (!isKnownOperation)
                {
                    errorMessages.Add(Tuple.Create($">> Unknown drive operation: \"{operation}\".\nAllowed operations are: \n-GetDrives\n-Open\n-Properties\n-Advanced\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }

                if (!isKnownExtraOperation)
                {
                    errorMessages.Add(Tuple.Create($">> Unknown extra operation: \"{extraOperation}\".\nAllowed extra operations are: \n-Print", TerminalMessageKind.Warning));
                    returnValue = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(rest))
            {
                errorMessages.Add(Tuple.Create($">> Unexpected argument: \"{rest}\".\n", TerminalMessageKind.Error));
                returnValue = false;
            }


            return (errorMessages, returnValue, drive);
        }

        private static string NormalizeOperation(string operation)
        {
            string normalizedOperation = operation.StartsWith('-') ? operation[1..] : operation;
            return normalizedOperation.Trim();
        }

        private static string NormalizeExtraOperation(string extraOperation)
        {
            string normalizedExtraOperation = extraOperation.StartsWith('-') ? extraOperation[1..] : extraOperation;
            return normalizedExtraOperation.Trim();
        }

        private static bool IsKnownOperation(string operation)
        {
            return operation.Equals("GetDrives", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Open", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Properties", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Advanced", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsKnownExtraOperation(string extraOperation)
        {
            return string.IsNullOrEmpty(extraOperation)
                   || extraOperation.Equals("Print", StringComparison.OrdinalIgnoreCase);
        }

    }
}
