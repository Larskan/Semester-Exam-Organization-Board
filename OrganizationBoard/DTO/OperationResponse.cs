using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class OperationResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }

        // Constructors
        public OperationResponse()
        {
            IsSuccess = true; // Default to success
            Message = "Operation successful";
            StatusCode = 200; // Default to OK
        }

        public OperationResponse(string message, bool isSuccess = false, int statusCode = 500)
        {
            IsSuccess = isSuccess;
            Message = message;
            StatusCode = statusCode;
        }

        public OperationResponse(T data) : this()
        {
            Data = data;
        }

        public OperationResponse(T data, string message) : this(data)
        {
            Message = message;
        }
    }
}