using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HooplaNewReleaseCheck
{
    public interface IHooplaResponse
    {
        Task<List<DigitalBook>> Run();
    }
}
