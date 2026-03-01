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
        private static readonly Regex Full =
            FullRegex();

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
            extraOperation = NormalizeExtraOperation(extraOperationText);

            var (messages, ok, drives) = GenerateOutput(targetText, operation, extraOperation, rest);

            errorMessages = messages;
            driveTarget = drives;
            return ok;
        }

        private static (List<Tuple<string, TerminalMessageKind>> Messages, bool Ok, List<string> Drives)
            GenerateOutput(string driveTarget, string operation, string extraOperation, string rest)
        {
            List<Tuple<string, TerminalMessageKind>> messages = [];
            List<string> drives = [];

            bool returnValue = true;

            bool isKnownOperation = IsKnownOperation(operation);
            bool isKnownExtraOperation = IsKnownExtraOperation(extraOperation);

            if (string.IsNullOrEmpty(operation))
            {
                messages.Add(Tuple.Create(
                    "\n>> Operation is required.\n\nAllowed operations are: \n-GetDrives\n-Open\n-Properties\n-Advanced\n",
                    TerminalMessageKind.Error));
                returnValue = false;
            }
            else
            {
                if (!isKnownOperation)
                {
                    messages.Add(Tuple.Create(
                        $"\n>> Unknown drive operation: \"{operation}\".\nAllowed operations are: \n-GetDrives\n-Open\n-Properties\n-Advanced\n",
                        TerminalMessageKind.Warning));
                    returnValue = false;
                }

                if (!isKnownExtraOperation)
                {
                    messages.Add(Tuple.Create(
                        $"\n>> Unknown extra operation: \"{extraOperation}\".\nAllowed extra operations are: \n-Print\n",
                        TerminalMessageKind.Warning));
                    returnValue = false;
                }
            }

            if (operation.Equals("GetDrives", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(driveTarget))
                {
                    messages.Add(Tuple.Create(
                        "\n>> This operation does not take any drive target!\n",
                        TerminalMessageKind.Warning));
                    returnValue = false;
                }

                if (!string.IsNullOrWhiteSpace(extraOperation) &&
                    !extraOperation.Equals("Print", StringComparison.OrdinalIgnoreCase))
                {
                    messages.Add(Tuple.Create(
                        $"\n>> Unknown option: \"{extraOperation}\".\nAllowed options are:\n-Print\n",
                        TerminalMessageKind.Error));
                    returnValue = false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(driveTarget))
                {
                    messages.Add(Tuple.Create(
                        "\n>> Missing drive target! Use \"C:\\\" or an index like [0].\n",
                        TerminalMessageKind.Error));
                    returnValue = false;
                }
                else if (EmptyIndex.IsMatch(driveTarget))
                {
                    messages.Add(Tuple.Create(
                        "\n>> Index cannot be empty! Use [0], [1], ...\n",
                        TerminalMessageKind.Warning));
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
                            messages.Add(Tuple.Create(
                                $"\n>> Drive with index: {index} does not exist!\n",
                                TerminalMessageKind.Error));
                            returnValue = false;
                        }
                        else
                        {
                            drives.Add(DriveInfo.GetDrives()[index].RootDirectory.FullName);
                        }
                    }

                    else if (driveTarget.StartsWith('[') && driveTarget.EndsWith(']'))
                    {
                        if (!CheckDriveInputInBrackets(driveTarget, DriveInfo.GetDrives(), drives, messages))
                            returnValue = false;
                    }

                    else if (driveTarget is ['"', _, ..] && driveTarget[^1] == '"')
                    {
                        string path = driveTarget[1..^1].Trim();
                        DriveInfo? driveInfo = GetDriveByName(DriveInfo.GetDrives(), path);
                        if (driveInfo == null)
                        {
                            messages.Add(Tuple.Create(
                                $"\n>> Drive: \"{path}\" does not exist!\n",
                                TerminalMessageKind.Error));
                            returnValue = false;
                        }
                        else
                        {
                            drives.Add(driveInfo.RootDirectory.FullName);
                        }
                    }
                    else
                    {
                        messages.Add(Tuple.Create(
                            "\n>> Drive target must be quoted. Use \"C:\\\" or an index like [0].\n",
                            TerminalMessageKind.Warning));
                        returnValue = false;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(rest))
            {
                return (messages, returnValue, drives);
            }

            messages.Add(Tuple.Create(
                $"\n>> Unexpected argument: \"{rest}\".\n",
                TerminalMessageKind.Error));
            returnValue = false;

            return (messages, returnValue, drives);
        }

        private static DriveInfo? GetDriveByName(DriveInfo[] driveInfos, string filePath)
        {
            foreach (DriveInfo driveInfo in driveInfos)
            {
                if (driveInfo.RootDirectory.FullName == filePath)
                {
                    return driveInfo;
                }
            }
            return null;
        }

        private static bool CheckDriveInputInBrackets(
            string driveInput,
            DriveInfo[] driveInfos,
            List<string> drives,
            List<Tuple<string, TerminalMessageKind>> messages)
        {
            string inner = driveInput[1..^1].Trim();

            if (inner == "\"All\"")
            {
                drives.AddRange(driveInfos.Select(driveInfo => driveInfo.RootDirectory.FullName));
                return true;
            }

            inner = inner.Replace("\"", "").Trim();

            string[] items = inner.Split(',');

            if (items.Length == 0 || items.Any(string.IsNullOrWhiteSpace))
            {
                messages.Add(Tuple.Create(
                    "\n>> Drive array contains empty elements or trailing comma!\n",
                    TerminalMessageKind.Error));
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

                    if (index < 0 || index >= driveInfos.Length)
                    {
                        messages.Add(Tuple.Create(
                            $"\n>> Drive with index: {index} does not exist!\n",
                            TerminalMessageKind.Error));
                        return false;
                    }

                    drives.Add(driveInfos[index].RootDirectory.FullName);
                }

                return true;
            }

            if (items.Any(s => int.TryParse(s, out _)))
            {
                messages.Add(Tuple.Create(
                    "\n>> Drive array must be only of one type, drive paths or drive indexes!\n",
                    TerminalMessageKind.Warning));
                return false;
            }

            foreach (string path in items)
            {
                DriveInfo? di = GetDriveByName(driveInfos, path);
                if (di == null)
                {
                    messages.Add(Tuple.Create(
                        $"\n>> Drive: \"{path}\" does not exist!\n",
                        TerminalMessageKind.Error));
                    return false;
                }

                drives.Add(di.RootDirectory.FullName);
            }

            return true;
        }

        private static string NormalizeOperation(string operation)
        {
            string normalized = operation.StartsWith('-') ? operation[1..] : operation;
            return normalized.Trim();
        }

        private static string NormalizeExtraOperation(string extraOperation)
        {
            string normalized = extraOperation.StartsWith('-') ? extraOperation[1..] : extraOperation;
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
        [GeneratedRegex(@"^\[(?<n>\d+)\]$", RegexOptions.Compiled)]
        private static partial Regex IndexExactRegex();
        [GeneratedRegex(@"^\[\s*\]$", RegexOptions.Compiled)]
        private static partial Regex EmptyIndexRegex();
    }
}