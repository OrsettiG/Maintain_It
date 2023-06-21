using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Services
{
    public interface IDatabaseItemManager
    {
        event EventHandler RequestReceived;
        void Initialize();
        void IndexItems();
    }
}
