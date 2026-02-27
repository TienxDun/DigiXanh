using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Helpers;
using DigiXanh.API.Patterns.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly OrderProcessingFacade _orderFacade;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        OrderProcessingFacade orderFacade,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _orderFacade = orderFacade;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("vnpay-return")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VNPayReturnResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayReturn()
    {
        // QUAN TRỌNG: Lấy dữ liệu gốc (raw) từ QueryString vì ASP.NET Core tự động decode
        // khi dùng Request.Query. VNPay signature được tính trên dữ liệu URL-encoded.
        var vnpayData = QueryStringHelper.ParseRaw(Request.QueryString.Value);

        _logger.LogInformation(
            "[VNPay-Return] Callback received. RawQueryString={QueryString}",
            Request.QueryString);
        
        _logger.LogInformation(
            "[VNPay-Return] Parsed data - TxnRef={TxnRef}, ResponseCode={ResponseCode}, TransactionStatus={TransactionStatus}, TransactionNo={TransactionNo}, Amount={Amount}",
            vnpayData.GetValueOrDefault("vnp_TxnRef"),
            vnpayData.GetValueOrDefault("vnp_ResponseCode"),
            vnpayData.GetValueOrDefault("vnp_TransactionStatus"),
            vnpayData.GetValueOrDefault("vnp_TransactionNo"),
            vnpayData.GetValueOrDefault("vnp_Amount"));

        var result = await _orderFacade.ProcessVNPayReturnAsync(vnpayData);

        _logger.LogInformation(
            "[VNPay-Return] Processed. OrderId={OrderId}, Success={Success}, Status={Status}, Message={Message}",
            result.OrderId,
            result.Success,
            result.Status,
            result.Message);

        if (WantsJsonResponse())
        {
            _logger.LogInformation("[VNPay-Return] Returning JSON response for FE verification");
            return Ok(result);
        }

        var frontendReturnUrl = _configuration["VNPay:FrontendReturnUrl"]
                                ?? "http://localhost:4200/payment-return";

        // Dùng Request.Query (đã decode) để truyền về FE cho dễ đọc
        var redirectQuery = Request.Query.ToDictionary(x => x.Key, x => (string?)x.Value.ToString());
        redirectQuery["paymentStatus"] = ResolvePaymentStatus(result);
        redirectQuery["message"] = result.Message;
        redirectQuery["orderId"] = result.OrderId > 0 ? result.OrderId.ToString() : null;
        redirectQuery["transactionId"] = result.TransactionId;

        var redirectUrl = QueryHelpers.AddQueryString(frontendReturnUrl, redirectQuery);
        _logger.LogInformation("[VNPay-Return] Redirecting to FE URL: {RedirectUrl}", redirectUrl);
        return Redirect(redirectUrl);
    }

    [HttpGet("vnpay-ipn")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VNPayIpnResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayIpnGet()
    {
        var vnpayData = QueryStringHelper.ParseRaw(Request.QueryString.Value);

        _logger.LogInformation(
            "[VNPay-IPN][GET] Received. TxnRef={TxnRef}, ResponseCode={ResponseCode}, TransactionStatus={TransactionStatus}",
            vnpayData.GetValueOrDefault("vnp_TxnRef"),
            vnpayData.GetValueOrDefault("vnp_ResponseCode"),
            vnpayData.GetValueOrDefault("vnp_TransactionStatus"));

        var result = await _orderFacade.ProcessVNPayIpnAsync(vnpayData);
        _logger.LogInformation("[VNPay-IPN][GET] Processed. RspCode={RspCode}, Message={Message}", result.RspCode, result.Message);
        return Ok(result);
    }

    [HttpPost("vnpay-ipn")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded", "application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VNPayIpnResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayIpnPost()
    {
        var vnpayData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync();
            foreach (var item in form)
            {
                vnpayData[item.Key] = item.Value.ToString();
            }
        }
        else
        {
            var payload = await Request.ReadFromJsonAsync<Dictionary<string, string>>();
            if (payload != null)
            {
                foreach (var item in payload)
                {
                    vnpayData[item.Key] = item.Value;
                }
            }
        }

        _logger.LogInformation(
            "[VNPay-IPN][POST] Received. TxnRef={TxnRef}, ResponseCode={ResponseCode}, TransactionStatus={TransactionStatus}",
            vnpayData.GetValueOrDefault("vnp_TxnRef"),
            vnpayData.GetValueOrDefault("vnp_ResponseCode"),
            vnpayData.GetValueOrDefault("vnp_TransactionStatus"));

        var result = await _orderFacade.ProcessVNPayIpnAsync(vnpayData);
        _logger.LogInformation("[VNPay-IPN][POST] Processed. RspCode={RspCode}, Message={Message}", result.RspCode, result.Message);
        return Ok(result);
    }

    private bool WantsJsonResponse()
    {
        if (!Request.Query.TryGetValue("format", out var formatValues))
        {
            return false;
        }

        return formatValues.Any(value => string.Equals(value, "json", StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolvePaymentStatus(VNPayReturnResponse response)
    {
        if (response.Success)
        {
            return "success";
        }

        return string.Equals(response.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            ? "cancelled"
            : "failed";
    }
}
