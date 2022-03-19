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


                APIReturnObject returnObject = new APIReturnObject()
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
            APIReturnObject result = new APIReturnObject();
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
                    if (err_message.Count > 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        int i = 1;

                        foreach (var item in err_message)
                        {
                            if (i < err_message.Count)
                            {
                                sb.Append(i.ToString() + " " + item);
                                sb.Append(Environment.NewLine);
                            }
                            else
                            {
                                sb.Append(i.ToString() + " " + item);
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
                    result.ErrorMessage = "ทำรายการไม่สำเร็จ";
                    result.ErrorDetail = err;

                    return Ok(result);
                }

                var record = new ConfigCodeEntity()
                {
                    ID_KEY = Config_Code_List.Count() + 1,
                    CONFIG_CODE = request.ConfigCode,
                    MAIN_CONFIG_CODE = request.ConfigCode.Substring(0,2),
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
            APIReturnObject result = new APIReturnObject();
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
                                sb.Append(i.ToString() + " " + item);
                                sb.Append(Environment.NewLine);
                            }
                            else
                            {
                                sb.Append(i.ToString() + " " + item);
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
                    result.ErrorMessage = "ทำรายการไม่สำเร็จ";
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
                    TRANS_MAIN_TYPE = request.TransType.Substring(0,2),
                    TRANS_TIME = CurrentTime,
                    TRANS_DATE = CurrentDay,
                    TRANS_NOTE = request.TransNote == "ไม่เพิ่ม" ? DBNull.Value.ToString() : request.TransNote,
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

        [HttpGet("GetReport")]
        public ActionResult GetReportController()
        {
            APIReturnObject result = new APIReturnObject();

            try
            {
                int QueryParameter1;
                int QueryParameter2;

                if (DateTime.UtcNow.Day >= 20)
                {
                    QueryParameter1 = DateTime.UtcNow.Month;

                    if(DateTime.UtcNow.Month == 12)
                    {
                        QueryParameter2 = 1;
                    }
                    else
                    {
                        QueryParameter2 = DateTime.UtcNow.Month + 1;
                    } 
                }
                else
                {
                    if(DateTime.UtcNow.Month == 1)
                    {
                        QueryParameter1 = 12;
                    }
                    else
                    {
                        QueryParameter1 = DateTime.UtcNow.Month - 1;
                    }

                    QueryParameter2 = DateTime.UtcNow.Month;
                }
                var CurrentMonth = DateTime.UtcNow.Month;

                var Record = _context.AppTransactionRecord.FromSqlRaw
                    (
                        $@"SELECT * FROM public.""TRANSACTION_RECORD"" 
                            WHERE(EXTRACT(DAY FROM ""TRANS_DATE"") > 19)
                                AND(EXTRACT(MONTH FROM ""TRANS_DATE"") = {QueryParameter1})         
                        UNION SELECT * FROM public.""TRANSACTION_RECORD"" 
	                        WHERE(EXTRACT(DAY FROM ""TRANS_DATE"") < 20) 
		                        AND(EXTRACT(MONTH FROM ""TRANS_DATE"") = {QueryParameter2})"
                    );

                if(Record != null)
                {
                    var RecordList = Record.ToList();

                    decimal Income = 0;
                    decimal Saving = 0;
                    decimal Investment = 0;
                    decimal Expense = 0;
                    decimal Unclassify = 0;

                    foreach(var item in RecordList)
                    {
                        if(item.TRANS_TYPE.Substring(0,2) == "IC")
                        {
                            Income = Income + item.TRANS_AMOUNT;
                        }
                        else if(item.TRANS_TYPE.Substring(0, 2) == "IV")
                        {
                            Investment = Investment + item.TRANS_AMOUNT;
                        }
                        else if (item.TRANS_TYPE.Substring(0, 2) == "SV")
                        {
                            Saving = Saving + item.TRANS_AMOUNT;
                        }
                        else if (item.TRANS_TYPE.Substring(0, 2) == "SP")
                        {
                            Expense = Expense + item.TRANS_AMOUNT;
                        }
                        else
                        {
                            Unclassify = Unclassify + item.TRANS_AMOUNT;
                        }
                    }

                    GetReportModel returnObject = new GetReportModel()
                    {
                        Income = Income,
                        Saving = Saving,
                        Investment = Investment,
                        Expense = Expense,
                        Unclassify = Unclassify,
                        Balanced = Income + Saving + Investment + Expense + Unclassify
                    };

                    var sb = new StringBuilder();

                    sb.Append("รายงานบันทึกรายรับรายจ่าย");
                    sb.Append(Environment.NewLine);
                    sb.Append($"รอบ 20/{QueryParameter1}/{DateTime.Now.Year} ถึง 19/{QueryParameter2}/{DateTime.Now.Year}");
                    sb.Append(Environment.NewLine);
                    sb.Append($"ข้อมูล ณ วันที่ {DateTime.Now.Day}/{DateTime.Now.Month}/{DateTime.Now.Year}");
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                    sb.Append("รายรับรวม : " + Income.ToString("N2") + " บาท");
                    sb.Append(Environment.NewLine);
                    sb.Append("เงินเก็บรวม : " + Saving.ToString("N2") + " บาท");
                    sb.Append(Environment.NewLine);
                    sb.Append("เงินลงทุนรวม : " + Investment.ToString("N2") + " บาท");
                    sb.Append(Environment.NewLine);
                    sb.Append("รายจ่ายรวม : " + Expense.ToString("N2") + " บาท");
                    sb.Append(Environment.NewLine);

                    if (Unclassify > 0)
                    {
                        sb.Append("รายการที่ระบุไม่ได้ :" + Unclassify.ToString("N2") + " บาท");
                        sb.Append(Environment.NewLine);
                    }

                    if(returnObject.Balanced > 0)
                    {
                        sb.Append("ดุลบัญชี :" + "+" + returnObject.Balanced.ToString("N2") + " บาท");
                    }
                    else
                    {
                        sb.Append("ดุลบัญชี :" + returnObject.Balanced.ToString("N2") + " บาท");
                    }
                    

                    var report = sb.ToString();

                    result.StatusCode = "200";
                    result.ErrorMessage = "รายการร้องขอสำเร็จ";
                    result.ReturnObject = report;

                    return Ok(result);
                }

                result.StatusCode = "404";
                result.ErrorMessage = "ไม่พบข้อมูล";

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
