// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdResponseResult.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    using System;
    using System.Text.RegularExpressions;

    internal class MpdResponseResult : IMpdResponseResult
    {
        private static readonly Regex ErrorPattern = new Regex("^ACK \\[(?<code>[0-9]*)@(?<nr>[0-9]*)] \\{(?<command>[a-z]*)} (?<message>.*)$");

        private readonly string endLine;

        public MpdResponseResult(string endLine, bool connected, Exception exception)
        {
            this.endLine = endLine;
            this.Connected = connected;
            this.Exception = exception;
            if (this.Exception != null)
            {
                this.Status = "EXCEPTION";
                this.ErrorMessage = Exception.ToString();
            }

            if (!string.IsNullOrEmpty(this.endLine))
            {
                if (this.endLine.Equals(Constants.Ok))
                {
                    this.Status = this.endLine;
                    this.Error = false;
                }
                else
                {
                    this.ParseErrorResponse();
                }
            }
        }

        public bool Connected { get; } = false;

        public bool Error { get; } = true;

        public string Status { get; private set; } = "UNKNOWN";

        public string ErrorMessage { get; private set; } = string.Empty;

        public string MpdError { get; private set; } = string.Empty;

        public Exception Exception { get; }

        private void ParseErrorResponse()
        {
            this.Status = "ERROR";
            this.MpdError = this.endLine;

            var match = ErrorPattern.Match(this.endLine);

            if (match.Groups.Count != 5)
            {
                this.ErrorMessage = $"Unexpected response from server: {MpdError}";
            }
            else
            {
                var errorCode = match.Result("${code}");
                var commandListItem = match.Result("${nr}");
                var commandFailed = match.Result("${command}");
                var errorMessage = match.Result("${message}");
                this.ErrorMessage = $"ErrorCode: {errorCode}, CommandListItem: {commandListItem}, CommandFailed: {commandFailed}, ErrorMessage: {errorMessage}";
            }
        }
    }
}