using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Adapter;

public interface IPaymentAdapterFactory
{
    IPaymentAdapter GetAdapter(PaymentMethod method);
}

public class PaymentAdapterFactory : IPaymentAdapterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentAdapterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentAdapter GetAdapter(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => _serviceProvider.GetRequiredService<CashPaymentAdapter>(),
            PaymentMethod.VNPay => _serviceProvider.GetRequiredService<VNPayPaymentAdapter>(),
            _ => throw new NotSupportedException($"Payment method {method} is not supported.")
        };
    }
}
