using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HooplaNewReleaseCheck
{
    public interface IEmail
    {
        Task SendEmailAsync(List<DigitalBook> newBooksToRead);
    }
}
