using System;
using System.Collections.Generic;

namespace BudgetBadger.Logic.Models
{
    public record Response
    {
        public StatusCode StatusCode { get; init; }
        public bool Success => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);
        public bool Failure => !Success;
        public string Message { get; init; }

        public Response(StatusCode statusCode = StatusCode.OK, string message = null)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public static implicit operator bool(Response result) => result.Success;
        public static Response OK() => new();
        public static Response NoContent() => new();
        public static Response InternalError(string message) => new(StatusCode.InternalError, message);
        public static Response BadRequest(string message) => new(StatusCode.BadRequest, message);
        public static Response Conflict(string message) => new(StatusCode.Conflict, message);
        public static Response NotFound(string message) => new(StatusCode.NotFound, message);
        public static Response Gone(string message) => new(StatusCode.Gone, message);
        public static Response Forbidden(string message) => new(StatusCode.Forbidden, message);
        public static Response NotImplemented(string message) => new(StatusCode.NotImplemented, message);
    }

    public record DataResponse<T> : Response
    {
        public T Data { get; init; }

        public DataResponse(StatusCode statusCode = StatusCode.OK, string message = null, T data = default)
            : base(statusCode, message)
        {
            Data = data;
        }
    }

    public class DataResponse
    {
        public static DataResponse<T> OK<T>(T value) => new(data: value);
        public static DataResponse<T> NoContent<T>(T _) => new();
        public static DataResponse<T> InternalError<T>(string message) => new(StatusCode.InternalError, message);
        public static DataResponse<T> BadRequest<T>(string message) => new(StatusCode.BadRequest, message);
        public static DataResponse<T> Conflict<T>(string message) => new(StatusCode.Conflict, message);
        public static DataResponse<T> NotFound<T>(string message) => new(StatusCode.NotFound, message);
        public static DataResponse<T> Gone<T>(string message) => new(StatusCode.Gone, message);
        public static DataResponse<T> Forbidden<T>(string message) => new(StatusCode.Forbidden, message);
        public static DataResponse<T> NotImplemented<T>(string message) => new(StatusCode.NotImplemented, message);
    }

    public record ItemsResponse<T> : Response
    {
        public IReadOnlyList<T> Items { get; init; }
        public int TotalItemsCount { get; init; }

        public ItemsResponse(StatusCode statusCode = StatusCode.OK,
            string message = null,
            IReadOnlyList<T> items = default,
            int totalItemsCount = default)
            : base(statusCode, message)
        {
            Items = items;
            TotalItemsCount = totalItemsCount;
        }
    }

    public static class ItemsResponse
    {
        public static ItemsResponse<T> OK<T>(IReadOnlyList<T> values, int totalValuesCount) => new(items: values, totalItemsCount: totalValuesCount);
        public static ItemsResponse<T> NoContent<T>(T _) => new();
        public static ItemsResponse<T> InternalError<T>(string message) => new(StatusCode.InternalError, message);
        public static ItemsResponse<T> BadRequest<T>(string message) => new(StatusCode.BadRequest, message);
        public static ItemsResponse<T> Conflict<T>(string message) => new(StatusCode.Conflict, message);
        public static ItemsResponse<T> NotFound<T>(string message) => new(StatusCode.NotFound, message);
        public static ItemsResponse<T> Gone<T>(string message) => new(StatusCode.Gone, message);
        public static ItemsResponse<T> Forbidden<T>(string message) => new(StatusCode.Forbidden, message);
        public static ItemsResponse<T> NotImplemented<T>(string message) => new(StatusCode.NotImplemented, message);
    }
}

