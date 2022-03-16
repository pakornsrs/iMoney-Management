using iMoney_API.Entities;
using iMoney_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

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

        [HttpGet("GetTypeCodeList")]
        public ActionResult GetTypeCodeController()
        {
            try
            {
                var Result = _context.AppConfigCodeData.FromSqlRaw(@"SELECT * FROM public.""CONFIG_TYPE""");
                var CodeList = Result.ToList();

                StringBuilder sb = new StringBuilder();
                string resultString = "";
                int i = 1;

                foreach (var item in CodeList)
                {
                    if (i < CodeList.Count)
                    {
                        sb.Append(item.CONFIG_CODE + " : " + item.CONFIG_KEYWORD);
                        sb.Append(Environment.NewLine);
                    }
                    else
                    {
                        sb.Append(item.CONFIG_CODE + " : " + item.CONFIG_KEYWORD);
                    }

                    i = i + 1;

                    resultString = sb.ToString();
                }


                ControllerReturnObject returnObject = new ControllerReturnObject()
                {
                    StatusCode = "200",
                    ErrorMessage = "ขอรายการสำเร็จ",
                    ReturnObject = resultString
                };

                return Ok(returnObject);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("AddTypeCode")]
        public ActionResult AddTypeCodeController([FromBody] ConfigCodeModel request)
        {
            ControllerReturnObject result = new ControllerReturnObject();
            List<String> err_message = new List<String>();

            try
            {
                var Previous_Record = _context.AppConfigCodeData.FromSqlRaw(@"SELECT * FROM public.""CONFIG_TYPE""");
                var Config_Code_List = Previous_Record.ToList();

                if (string.IsNullOrEmpty(request.ConfigCode) || string.IsNullOrWhiteSpace(request.ConfigCode))
                {
                    err_message.Add("กรุณากำหนด Code ของรายการที่จะสร้างใหม่");
                }
                else
                {
                    // Check format

                    var InputCode = request.ConfigCode.Substring(0, 2);

                    if (InputCode.Equals("IC") || InputCode.Equals("SP"))
                    {
                        // Check duplicate
                        var Result = _context.AppConfigCodeData.FromSqlRaw(@"SELECT * FROM public.""CONFIG_TYPE""");
                        var CodeList = Result.ToList();

                        bool IsCorrectCode = false;

                        foreach (var item in CodeList)
                        {
                            if (item.CONFIG_CODE == request.ConfigCode)
                            {
                                IsCorrectCode = true;
                            }
                        }

                        if (IsCorrectCode == true)
                        {
                            err_message.Add("Code ที่กำหนดถูกใช้แล้ว กรุณาเปลี่ยนเป็น Code อื่น");
                        }
                    }
                    else
                    {
                        err_message.Add("รูปแบบ Code ไม่ถูกต้อง โดย Code ต้องขึ้นต้นด้วย SP สำหรับรายการรายจ่าย หรือ IC สำหรับรายการรายรับ เท่านั้น");
                    }
                }

                if (string.IsNullOrEmpty(request.Keyword) || string.IsNullOrWhiteSpace(request.Keyword))
                {
                    err_message.Add("กรุณาใส่ชื่อเรียกรายการ");
                }

                if (err_message.Count > 0)
                {
                    string err = "";

                    if(err_message.Count > 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        int i = 1;

                        foreach (var item in err_message)
                        {
                            if (i < err_message.Count)
                            {
                                sb.Append(item + ", ");
                            }
                            else
                            {
                                sb.Append(item);
                            }

                            i = i + 1;

                            err = sb.ToString();
                        }
                    }
                    else
                    {
                        err = err_message[0];
                    }

                    result.StatusCode = "400";
                    result.ErrorMessage = "ข้อมูลไม่ถูกต้อง";
                    result.ErrorDetail = err;

                    return Ok(result);
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
            catch (Exception)
            {
                result.StatusCode = "500";
                result.ErrorMessage = "เกิดข้อผิดพลาดภายในระบบ";
                result.ErrorDetail = "เกิดข้อผิดพลาดภายในระบบ";

                return Ok(result);
            }
        }

        [HttpPost("AddTransaction")]
        public ActionResult AddTransactionRecordController([FromBody] TransactionRecordModel request)
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
                    err_message.Add("กรุณาใส่ Code ของรายการ");
                }
                else
                {
                    var Result = _context.AppConfigCodeData.FromSqlRaw(@"SELECT * FROM public.""CONFIG_TYPE""");
                    var CodeList = Result.ToList();

                    bool IsCorrectCode = false;

                    foreach (var item in CodeList)
                    {
                        if(item.CONFIG_CODE == request.TransType)
                        {
                            IsCorrectCode = true;
                        }
                    }

                    if (IsCorrectCode == false)
                    {
                        err_message.Add("Code ที่ใส่ไม่ถูกต้อง กรุณาลองอีกครั้ง");
                    }
                }

                // Convert amount 

                decimal amount = 0;

                try
                {
                    amount = Convert.ToDecimal(request.TransAmount);
                }
                catch (Exception)
                {
                    err_message.Add("จำนวนเงินที่กรอกต้องเป็นตัวเลขเท่านั้น !");
                }

                if(amount <= 0)
                {
                    err_message.Add("จำนวนเงินที่จ่ายต้องมีค่ามากกว่า 0");
                }

                if(err_message.Count > 0)
                {
                    string err = "";
                    if (err_message.Count > 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        int i = 1;

                        foreach (var item in err_message)
                        {
                            if(i < err_message.Count)
                            {
                                sb.Append(item + ", ");
                            }
                            else
                            {
                                sb.Append(item);
                            }

                            i = i+1;
                            
                           err = sb.ToString();
                        }
                    }
                    else
                    {
                        err = err_message[0];
                    }

                    result.StatusCode = "400";
                    result.ErrorMessage = "ข้อมูลไม่ถูกต้อง";
                    result.ErrorDetail = err;

                    return Ok(result);
                }

                var Previous_Record = _context.AppTransactionRecord.FromSqlRaw(@"SELECT * FROM public.""TRANSACTION_RECORD""");
                var Config_Code_List = Previous_Record.ToList();

                var CurrentTime = DateTime.Now.TimeOfDay.ToString();
                var CurrentDay = DateTime.UtcNow;

                if (request.TransType.Substring(0, 2).Equals("SP"))
                {
                    amount = (-1) * amount;
                }

                var record = new TransactionRecordEntity()
                {
                    TRANS_ID_KEY = Config_Code_List.Count() + 1,
                    TRANS_NAME = request.TransName,
                    TRANS_TYPE = request.TransType,
                    TRANS_TIME = CurrentTime,
                    TRANS_DATE = CurrentDay,
                    TRANS_NOTE = request.TransNote,
                    TRANS_AMOUNT = amount

                };

                _context.Add(record);
                _context.SaveChanges();

                result.StatusCode = "200";
                result.ErrorMessage = "บันทึกรายการสำเร็จ";

                return Ok(result);
            }
            catch (Exception)
            {

                result.StatusCode = "500";
                result.ErrorMessage = "เกิดข้อผิดพลาดภายในระบบ";
                result.ErrorDetail = "เกิดข้อผิดพลาดภายในระบบ";

                return Ok(result);
            }

        }
    }
}
