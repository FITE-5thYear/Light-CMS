using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components
{
    public interface IComponent
    {
        void Bootstrap(string connectionString);
    }
}
