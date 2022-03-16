using Data.App.IData;
using iMoney_API;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.App.Data
{
    public class TypeCodeData : ITypeCodeData
    {
        private AppDbContext _context;

        public TypeCodeData(AppDbContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<ActionResult> GetTypeCodeData()
        {
            try
            {
                var Result = await _context.AppConfigCodeData.FromSqlRaw(@"SELECT * FROM public.""CONFIG_TYPE""");
                var CodeList = Result.ToList();

                if(CodeList.Count() > 0)
                {
                    return CodeList;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
