// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorMpdMessage.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    internal class ErrorMpdMessage<T> : IMpdMessage<T>
    {
        public ErrorMpdMessage(IMpcCommand<T> command, ErrorMpdResponse<T> errorResponse)
        {
            this.Request = new MpdRequest<T>(command);
            this.Response = errorResponse;
        }

        public IMpdRequest<T> Request { get; }

        public IMpdResponse<T> Response { get; }

        public bool IsResponseValid => false;

        public override string ToString() => Response.Result.ErrorMessage;
    }
}