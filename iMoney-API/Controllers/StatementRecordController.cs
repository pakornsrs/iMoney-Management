using iMoney_API.Entities;
using iMoney_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace iMoney_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatementRecordController : ControllerBase
    {
        private readonly ILogger<StatementRecordController> _logger;
        private AppDbContext _context;

        public StatementRecordController(ILogger<StatementRecordController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public ActionResult GetConfigCode()
        {
            try
            {
                var Result = _context.AppConfigCodeData.FromSqlRaw(@"SELECT * FROM public.""CONFIG_TYPE""");

                return Ok(Result.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult InsertConfigCode([FromBody] ConfigCodeModel request)
        {
            ControllerReturnObject result = new ControllerReturnObject();
            List<String> err_message = new List<String>();

            try
            {
                var Previous_Record = _context.AppConfigCodeData.FromSqlRaw(@"SELECT * FROM public.""CONFIG_TYPE""");
                var Config_Code_List = Previous_Record.ToList();

                if (string.IsNullOrEmpty(request.ConfigCode) || string.IsNullOrWhiteSpace(request.ConfigCode))
                {
                    err_message.Add("กรุณาใส่ Type Code ของรายการ");
                }

                if (string.IsNullOrEmpty(request.Keyword) || string.IsNullOrWhiteSpace(request.Keyword))
                {
                    err_message.Add("กรุณาใส่ชื่อรายการ");
                }

                var IsDuplicateCode = Config_Code_List.FirstOrDefault(item => item.CONFIG_CODE == request.ConfigCode);

                if (IsDuplicateCode != null)
                {
                    err_message.Add("Type Code ของรายการซ้ำซ้อน กรุณาใช้ Type Code อื่น");
                }

                if (err_message.Count > 0)
                {
                    string err = "";
                    foreach (var item in err_message)
                    {
                        err = err + ", " + item;
                    }

                    result.StatusCode = "400";
                    result.ErrorMessage = "ข้อมูลไม่ถูกต้อง";
                    result.ErrorDetail = err;

                    return BadRequest(result);
                }

                var record = new ConfigCodeEntity()
                {
                    ID_KEY = Config_Code_List.Count() + 1,
                    CONFIG_CODE = request.ConfigCode,
                    CONFIG_KEYWORD = request.Keyword,
                };

                _context.Add(record);
                _context.SaveChanges();

                result.StatusCode = "200";
                result.ErrorMessage = "บันทึกรายการสำเร็จ";
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("AddTransaction")]
        public ActionResult TransactionRecord([FromBody] TransactionRecordModel request)
        {
            ControllerReturnObject result = new ControllerReturnObject();
            List<String> err_message = new List<String>();

            try
            {
                if(string.IsNullOrEmpty(request.TransName) || string.IsNullOrWhiteSpace(request.TransName))
                {
                    err_message.Add("กรุณาใส่คำอธิบายรายรับ/จ่าย");
                }

                if (string.IsNullOrEmpty(request.TransType) || string.IsNullOrWhiteSpace(request.TransType))
                {
                    err_message.Add("กรุณาใส่ Type Code");
                }
                else
                {
                    var code = _context.AppConfigCodeData.FromSqlRaw(@"SELECT ""CONFIG_CODE"" FROM public.""CONFIG_TYPE""").ToList();
                    var IsCorrectCode = code.FirstOrDefault(item => item.CONFIG_CODE == request.TransType);

                    if(IsCorrectCode == null)
                    {
                        err_message.Add("Type Code ที่ใส่ไม่ถูกต้อง กรุณาลองอีกครั้ง");
                    }
                }

                if(request.TransAmount <= 0)
                {
                    err_message.Add("จำนวนเงินที่จ่ายต้องมีค่ามากกว่า 0");
                }

                var Previous_Record = _context.AppTransactionRecord.FromSqlRaw(@"SELECT * FROM public.""TRANSACTION_RECORD""");
                var Config_Code_List = Previous_Record.ToList();

                var CurrentTime = DateTime.Now.TimeOfDay.ToString();
                var CurrentDay = DateTime.UtcNow;


                var record = new TransactionRecordEntity()
                {
                    TRANS_ID_KEY = Config_Code_List.Count() + 1,
                    TRANS_NAME = request.TransName,
                    TRANS_TYPE = request.TransType,
                    TRANS_TIME = CurrentTime,
                    TRANS_DATE = CurrentDay,
                    TRANS_NOTE = request.TransNote,
                    TRANS_AMOUNT = request.TransAmount

                };

                _context.Add(record);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
