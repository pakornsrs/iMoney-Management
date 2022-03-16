using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.App.IData
{
    public interface ITypeCodeData : IDisposable
    {
        Task<ActionResult> GetTypeCodeData();
    }
}
