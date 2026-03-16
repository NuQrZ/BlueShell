using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BlueShell.Terminal.Commands.FolderCommand
{
    public static partial class FolderCommandParser
    {
        private static readonly Regex BasicCommandRegex = BasicGeneratedRegex();
        private static readonly Regex ComplexCommandRegex = ComplexGeneratedRegex();
        private static readonly Regex FullQuoted = FullQuotedRegex();

        public static bool TryParse(
            string commandLine,
            out List<string> filePaths,
            out string operation,
            out string destination,
            out string extraOperation,
            out List<Tuple<string, TerminalMessageKind>> errorMessages)
        {
            filePaths = [];
            operation = "";
            destination = "";
            extraOperation = "";
            errorMessages = [];

            Match basicMatch = BasicCommandRegex.Match(commandLine);
            Match complexMatch = ComplexCommandRegex.Match(commandLine);

            string sourceGroup;
            string operationGroup;
            string extraOperationGroup;
            string restGroup;

            if (basicMatch.Success)
            {
                sourceGroup = basicMatch.Groups["Source"].Success ? basicMatch.Groups["Source"].Value : "";
                operationGroup = basicMatch.Groups["Operation"].Success ? basicMatch.Groups["Operation"].Value : "";
                extraOperationGroup = basicMatch.Groups["Extra"].Success ? basicMatch.Groups["Extra"].Value : "";
                restGroup = basicMatch.Groups["Rest"].Success ? basicMatch.Groups["Rest"].Value : "";
            }
            else if (complexMatch.Success)
            {
                sourceGroup = complexMatch.Groups["Source"].Success ? complexMatch.Groups["Source"].Value : "";
                operationGroup = complexMatch.Groups["Operation"].Success ? complexMatch.Groups["Operation"].Value : "";
                destination = complexMatch.Groups["Destination"].Success
                    ? complexMatch.Groups["Destination"].Value
                    : "";
                extraOperationGroup = complexMatch.Groups["Overwrite"].Success
                    ? complexMatch.Groups["Overwrite"].Value
                    : "";
                restGroup = complexMatch.Groups["Rest"].Success ? complexMatch.Groups["Rest"].Value : "";
            }
            else
            {
                errorMessages.Add(Tuple.Create("\n>> Invalid syntax!\n", TerminalMessageKind.Error));
                return false;
            }

            operation = NormalizeOperation(operationGroup);
            extraOperation = NormalizeOperation(extraOperationGroup);

            var (messages, ok, parsedFilePaths) =
                GenerateOutput(sourceGroup, operation, destination, extraOperation, restGroup);

            filePaths = parsedFilePaths;
            errorMessages = messages;

            return ok;
        }

        private static (List<Tuple<string, TerminalMessageKind>>, bool returnValue, List<string> parsedFilePaths)
            GenerateOutput(
                string filePathInput,
                string operation,
                string destination,
                string extraOperation,
                string rest)
        {
            List<Tuple<string, TerminalMessageKind>> errorMessages = [];
            List<string> parsedFilePaths = [];

            bool returnValue = true;

            if (string.IsNullOrEmpty(operation))
            {
                errorMessages.Add(Tuple.Create(
                    "\n>> Operation is required.\n\nAllowed operations are: \n-Open\n-Delete\n-Rename\n-Move\n-Copy\n-Properties\n-Advanced\n",
                    TerminalMessageKind.Error));
                returnValue = false;
            }
            else
            {
                bool isKnownOperation = IsKnownOperation(operation);
                bool isKnownExtraOperation = IsKnownExtraOperation(extraOperation);

                if (!isKnownOperation)
                {
                    errorMessages.Add(Tuple.Create(
                        $"\n>> Unknown folder operation: \"{operation}\".\nAllowed operations are: \n-Open\n-Delete\n-Rename\n-Move\n-Copy\n-Properties\n-Advanced\n",
                        TerminalMessageKind.Warning));
                    returnValue = false;
                }

                if (!isKnownExtraOperation)
                {
                    errorMessages.Add(Tuple.Create(
                        $"\n>> Unknown extra operation: \"{extraOperation}\".\nAllowed extra operations are: \n-Print\n",
                        TerminalMessageKind.Warning));
                    returnValue = false;
                }
            }

            if (destination == "")
            {
                // Everything else
                if (filePathInput.StartsWith('[') && filePathInput.EndsWith(']'))
                {
                    returnValue = CheckFilePathInput(filePathInput, parsedFilePaths, errorMessages);
                }
                else
                {
                    if (!FullQuoted.IsMatch(filePathInput))
                    {
                        errorMessages.Add(Tuple.Create("\n>> Invalid file path list input!\n",
                            TerminalMessageKind.Error));
                        returnValue = false;
                    }

                    string cleanedPath = filePathInput[1..^1].Replace("\"", "").Trim();

                    if (!Directory.Exists(cleanedPath))
                    {
                        errorMessages.Add(Tuple.Create(
                            $">> Folder with file path: \"{filePathInput.Trim()[1..^1].Trim()}\" does not exist or cannot be opened!",
                            TerminalMessageKind.Warning));
                        returnValue = false;
                    }

                    parsedFilePaths.Add(cleanedPath);
                }
            }
            else
            {
                // Rename, Copy, Move
            }

            if (string.IsNullOrWhiteSpace(rest))
            {
                return (errorMessages, returnValue, parsedFilePaths);
            }

            errorMessages.Add(Tuple.Create($"\n>> Unexpected argument: \"{rest}\".\n", TerminalMessageKind.Error));
            returnValue = false;

            return (errorMessages, returnValue, parsedFilePaths);
        }

        private static bool IsKnownOperation(string operation)
        {
            return operation.Equals("Open", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Delete", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Rename", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Copy", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Move", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Properties", StringComparison.OrdinalIgnoreCase)
                   || operation.Equals("Advanced", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsKnownExtraOperation(string extraOperation)
        {
            return string.IsNullOrEmpty(extraOperation)
                   || extraOperation.Equals("Print", StringComparison.OrdinalIgnoreCase);
        }

        private static bool CheckFilePathInput(
            string filePathInput,
            List<string> parsedFilePaths,
            List<Tuple<string, TerminalMessageKind>> errorMessages)
        {
            bool returnValue = true;

            if (filePathInput == "")
            {
                errorMessages.Add(Tuple.Create("\n>> File path list is empty!\n", TerminalMessageKind.Warning));
                return false;
            }

            if (!QuotedArrayRegex().IsMatch(filePathInput))
            {
                errorMessages.Add(Tuple.Create("\n>> File Path array is not properly formatted!\n",
                    TerminalMessageKind.Error));
                return false;
            }

            filePathInput = filePathInput.Replace("[", "").Replace("]", "").Trim();

            string[] filePaths = filePathInput.Trim().Split(',');

            foreach (string filePath in filePaths)
            {
                string cleanedFilePath = filePath[1..^1].Replace("\"", "").Trim();
                // Folder ["C:\", "C:\Windows", "C:\Windows\System32"] -Open
                if (!Directory.Exists(cleanedFilePath))
                {
                    errorMessages.Add(Tuple.Create($"\n>> Folder with file path: {filePath} does not exist!",
                        TerminalMessageKind.Warning));
                    returnValue = false;
                }
                else
                {
                    parsedFilePaths.Add(cleanedFilePath);
                }
            }

            return returnValue;
        }

        private static string NormalizeOperation(string operation)
        {
            string normalized = operation.StartsWith('-') ? operation[1..] : operation;
            return normalized.Trim();
        }

        [GeneratedRegex(
            """^\s*Folder\s+(?<Source>("[^"]*"|\[[^\]]*\]))\s+(?<Operation>-(?:Open|Delete|Properties|Advanced))(?:\s+(?<Extra>-(?:Print)))?(?<Rest>(?:\s+.+)?)\s*$""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex BasicGeneratedRegex();

        [GeneratedRegex(
            """^\s*Folder\s+(?<Source>"[^"]*")\s+(?<Operation>-(?:Move|Copy|Rename))\s+(?<Destination>"[^"]*")(?:\s+(?<Overwrite>-(?:True|False)))?(?<Rest>(?:\s+.+)?)\s*$""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex ComplexGeneratedRegex();

        [GeneratedRegex("^\"[^\"]*\"$")]
        private static partial Regex FullQuotedRegex();

        [GeneratedRegex(
            """^\[\s*"[^"]*"\s*(?:,\s*"[^"]*"\s*)*\]$""",
            RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex QuotedArrayRegex();
    }
}