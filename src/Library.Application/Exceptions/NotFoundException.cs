using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Application.Exceptions
{
    // : extends Exception
   public class NotFoundException : Exception
    {
        // : is super(message)
        public NotFoundException(string messsage) : base(messsage)
        {
        }
    }
}
