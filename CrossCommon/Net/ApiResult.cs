using System;
using System.Collections.Generic;
using System.Text;

namespace CrossCommon
{
    public class ApiResult<T>
    {
        public ApiResult(ApiResultStatus status) : this(status, default(T))
        {

        }

        public ApiResult(ApiResultStatus status, T item)
        {
            Status = status;
            Item = item;
        }

        public T Item { get; set; }

        public ApiResultStatus Status { get; set; }

        public bool IsSuccess => Status == ApiResultStatus.Success;
    }
}