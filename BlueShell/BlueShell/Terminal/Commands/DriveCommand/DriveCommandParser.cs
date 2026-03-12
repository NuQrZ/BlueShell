using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public static partial class DriveCommandParser
    {
        private static readonly Regex Full = FullRegex();
        private static readonly Regex FullQuoted = FullQuotedRegex();
        private static readonly Regex QuotedArray = QuotedArrayRegex();
        private static readonly Regex IndexExact = IndexExactRegex();
        private static readonly Regex EmptyIndex = EmptyIndexRegex();

        public static bool TryParse(
            string commandLine,
            out List<string> driveTarget,
            out string operation,
            out string extraOperation,
            out List<Tuple<string, TerminalMessageKind>> errorMessages)
        {
            driveTarget = [];
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
            extraOperation = NormalizeOperation(extraOperationText);

            var (messages, ok, drives) = GenerateOutput(targetText, operation, extraOperation, rest);

            errorMessages = messages;
            driveTarget = drives;
            return ok;
        }

        private static (List<Tuple<string, TerminalMessageKind>> Messages, bool Ok, List<string> Drives)
            GenerateOutput(string driveTarget, string operation, string extraOperation, string rest)
        {
            List<Tuple<string, TerminalMessageKind>> errorMessages = [];
            List<string> drives = [];

            bool returnValue = true;

            if (string.IsNullOrEmpty(operation))
            {
                errorMessages.Add(Tuple.Create("\n>> Operation is required.\n\nAllowed operations are: \n-GetDrives\n-Open\n-Properties\n-Advanced\n", TerminalMessageKind.Error));
                returnValue = false;
            }
            else
            {
                bool isKnownOperation = IsKnownOperation(operation);
                bool isKnownExtraOperation = IsKnownExtraOperation(extraOperation);

                if (!isKnownOperation)
                {
                    errorMessages.Add(Tuple.Create($"\n>> Unknown drive operation: \"{operation}\".\nAllowed operations are: \n-GetDrives\n-Open\n-Properties\n-Advanced\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }

                if (!isKnownExtraOperation)
                {
                    errorMessages.Add(Tuple.Create($"\n>> Unknown extra operation: \"{extraOperation}\".\nAllowed extra operations are: \n-Print\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }
            }

            if (operation.Equals("GetDrives", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(driveTarget))
                {
                    errorMessages.Add(Tuple.Create("\n>> This operation does not take any drive target!\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }

                if (!string.IsNullOrWhiteSpace(extraOperation) &&
                    !extraOperation.Equals("Print", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessages.Add(Tuple.Create($"\n>> Unknown option: \"{extraOperation}\".\nAllowed options are:\n-Print\n", TerminalMessageKind.Error));
                    returnValue = false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(driveTarget))
                {
                    errorMessages.Add(Tuple.Create("\n>> Missing drive target! Use \"C:\\\" or an index like [0].\n", TerminalMessageKind.Error));
                    returnValue = false;
                }
                else if (EmptyIndex.IsMatch(driveTarget))
                {
                    errorMessages.Add(Tuple.Create("\n>> Index cannot be empty! Use [0], [1], ...\n", TerminalMessageKind.Warning));
                    returnValue = false;
                }
                else
                {

                    Match match = IndexExact.Match(driveTarget);
                    if (match.Success)
                    {
                        int index = int.Parse(match.Groups["n"].Value);

                        if (index < 0 || index >= DriveInfo.GetDrives().Length)
                        {
                            errorMessages.Add(Tuple.Create($"\n>> Drive with index: {index} does not exist!\n", TerminalMessageKind.Error));
                            returnValue = false;
                        }
                        else
                        {
                            drives.Add(DriveInfo.GetDrives()[index].RootDirectory.FullName);
                        }
                    }

                    else if (driveTarget.StartsWith('[') && driveTarget.EndsWith(']'))
                    {
                        if (!CheckDriveInputInBrackets(driveTarget, drives, errorMessages))
                        {
                            returnValue = false;
                        }
                    }

                    else if (driveTarget is ['"', _, ..] && driveTarget[^1] == '"')
                    {
                        string path = driveTarget[1..^1].Trim();
                        DriveInfo? driveInfo = GetDriveByName(path);

                        if (driveTarget == "\"C:\\Windows\\WinSxS\"")
                        {
                            drives.Add(driveTarget[1..^1].Trim());
                        }
                        else if (driveInfo == null)
                        {
                            errorMessages.Add(Tuple.Create($"\n>> Drive: \"{path}\" does not exist!\n", TerminalMessageKind.Error));
                            returnValue = false;
                        }
                        else
                        {
                            drives.Add(driveInfo.RootDirectory.FullName);
                        }
                    }
                    else
                    {
                        errorMessages.Add(Tuple.Create("\n>> Drive target must be quoted. Use \"C:\\\" or an index like [0].\n", TerminalMessageKind.Warning));
                        returnValue = false;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(rest))
            {
                return (errorMessages, returnValue, drives);
            }

            errorMessages.Add(Tuple.Create($"\n>> Unexpected argument: \"{rest}\".\n", TerminalMessageKind.Error));
            returnValue = false;

            return (errorMessages, returnValue, drives);
        }

        private static DriveInfo? GetDriveByName(string filePath)
        {
            return DriveInfo.GetDrives().FirstOrDefault(driveInfo => driveInfo.RootDirectory.FullName == filePath);
        }

        private static bool IsIntArray(string input)
        {
            string[] parts = input.Split(',');
            return parts.All(x => int.TryParse(x.Trim(), out int _));
        }

        private static bool CheckDriveInputInBrackets(
            string driveInput,
            List<string> drivesList,
            List<Tuple<string, TerminalMessageKind>> errorMessages)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            string inner = driveInput[1..^1].Trim();

            if (inner == "\"All\"")
            {
                drivesList.AddRange(drives.Select(driveInfo => driveInfo.RootDirectory.FullName));
                return true;
            }

            if (!IsIntArray(inner) && !QuotedArray.IsMatch(driveInput))
            {
                errorMessages.Add(Tuple.Create("\n>> Drive array is not properly formatted!\n", TerminalMessageKind.Error));
                return false;
            }

            inner = inner.Replace("\"", "").Trim();
            string[] items = inner.Split(',');

            if (items.Length == 0 || items.Any(string.IsNullOrWhiteSpace))
            {
                errorMessages.Add(Tuple.Create("\n>> Drive array contains empty elements or trailing comma!\n", TerminalMessageKind.Error));
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                items[i] = items[i].Trim();
            }

            if (items.All(s => int.TryParse(s, out _)))
            {
                foreach (string s in items)
                {
                    int index = int.Parse(s);

                    if (index < 0 || index >= drives.Length)
                    {
                        errorMessages.Add(Tuple.Create($"\n>> Drive with index: {index} does not exist!\n", TerminalMessageKind.Error));
                        return false;
                    }

                    drivesList.Add(drives[index].RootDirectory.FullName);
                }

                return true;
            }

            foreach (string path in items)
            {
                DriveInfo? driveInfo = GetDriveByName(path);

                if (driveInfo == null)
                {
                    errorMessages.Add(Tuple.Create($"\n>> Drive: \"{path}\" does not exist!\n", TerminalMessageKind.Error));
                    return false;
                }

                drivesList.Add(driveInfo.RootDirectory.FullName);
            }

            return true;
        }

        private static string NormalizeOperation(string operation)
        {
            string normalized = operation.StartsWith('-') ? operation[1..] : operation;
            return normalized.Trim();
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

        [GeneratedRegex("""^\s*Drive(?:\s+(?<Target>("[^"]*"|\[[^\]]*\]))\s*)?(?:\s+(?<Operation>-\S+))?(?:\s+(?<Extra>-\S+))?(?<Rest>(?:\s+.+)?)\s*$""", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex FullRegex();

        [GeneratedRegex("^\"[^\"]*\"$")]
        private static partial Regex FullQuotedRegex();

        [GeneratedRegex(
            """^\[\s*"[^"]*"\s*(?:,\s*"[^"]*"\s*)*\]$""",
            RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex QuotedArrayRegex();

        [GeneratedRegex(@"^\[(?<n>\d+)\]$", RegexOptions.Compiled)]
        private static partial Regex IndexExactRegex();

        [GeneratedRegex(@"^\[\s*\]$", RegexOptions.Compiled)]
        private static partial Regex EmptyIndexRegex();
    }
}