using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain
{
    public class Result<T> 
    {
        public bool IsSuccess { get; set; }
        public bool IsError { get { return !IsSuccess; } }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; } = 400;
    }
}
