using GestionCaja.Application.Common.Exceptions;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionCaja.Application.Contracts;

public interface IContractService
{
    Task<List<ContractResponse>> ListAsync(int? clientId, string? statusId, CancellationToken cancellationToken = default);
    Task<ContractResponse> GetAsync(int contractId, CancellationToken cancellationToken = default);
    Task<ContractResponse> CreateAsync(ContractCreateRequest request, CancellationToken cancellationToken = default);
    Task<ContractResponse> ChangeServiceAsync(int contractId, int serviceId, CancellationToken cancellationToken = default);
    Task<ContractResponse> ChangePaymentMethodAsync(int contractId, int methodPaymentId, CancellationToken cancellationToken = default);
    Task<ContractResponse> CancelAsync(int contractId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int contractId, CancellationToken cancellationToken = default);

    Task<List<PaymentResponse>> ListPaymentsAsync(int? clientId, int? contractId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> PayAsync(PaymentRequest request, CancellationToken cancellationToken = default);
}

public class ContractService : IContractService
{
    private static readonly string[] ActiveStatuses = [ContractStatuses.Vigente, ContractStatuses.RenovacionServicio];

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ContractService> _logger;

    public ContractService(IApplicationDbContext db, ICurrentUser currentUser, ILogger<ContractService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<ContractResponse>> ListAsync(int? clientId, string? statusId, CancellationToken cancellationToken = default)
    {
        var query = WithRelations().AsNoTracking();

        if (clientId.HasValue)
        {
            query = query.Where(c => c.ClientId == clientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusId))
        {
            query = query.Where(c => c.StatusContractId == statusId);
        }

        var contracts = await query.OrderByDescending(c => c.ContractId).ToListAsync(cancellationToken);
        return contracts.Select(ToResponse).ToList();
    }

    public async Task<ContractResponse> GetAsync(int contractId, CancellationToken cancellationToken = default)
    {
        return ToResponse(await FindOrThrowAsync(contractId, cancellationToken));
    }

    public async Task<ContractResponse> CreateAsync(ContractCreateRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == request.ClientId, cancellationToken)
            ?? throw new AppException("El cliente no existe", 404);

        if (!client.Active)
        {
            throw new AppException("El cliente está inactivo");
        }

        await EnsureServiceIsAvailableAsync(request.ServiceId, cancellationToken);
        await EnsureMethodPaymentExistsAsync(request.MethodPaymentId, cancellationToken);

        var hasActiveContract = await _db.Contracts.AnyAsync(c =>
            c.ClientId == request.ClientId && ActiveStatuses.Contains(c.StatusContractId), cancellationToken);

        if (hasActiveContract)
        {
            throw new AppException("El cliente ya tiene un contrato en curso. Use el cambio de servicio", 409);
        }

        var now = DateTime.UtcNow;
        var contract = new Contract
        {
            ClientId = request.ClientId,
            ServiceId = request.ServiceId,
            MethodPaymentId = request.MethodPaymentId,
            StatusContractId = ContractStatuses.Vigente,
            StartDate = now,
            EndDate = now.AddMonths(request.Months)
        };

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de contrato {ContractId} para el cliente {Identification} por {CurrentUser}",
            contract.ContractId, client.Identification, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(contract.ContractId, cancellationToken));
    }

    public async Task<ContractResponse> ChangeServiceAsync(int contractId, int serviceId, CancellationToken cancellationToken = default)
    {
        var current = await FindOrThrowAsync(contractId, cancellationToken);
        EnsureActive(current);

        if (current.ServiceId == serviceId)
        {
            throw new AppException("El contrato ya tiene ese servicio");
        }

        await EnsureServiceIsAvailableAsync(serviceId, cancellationToken);

        // Regla del enunciado: el contrato actual pasa a sustituido y se crea un registro nuevo
        // en estado de renovación de servicio, conservando la misma fecha final.
        current.StatusContractId = ContractStatuses.Sustituido;

        var renewed = new Contract
        {
            ClientId = current.ClientId,
            ServiceId = serviceId,
            MethodPaymentId = current.MethodPaymentId,
            StatusContractId = ContractStatuses.RenovacionServicio,
            StartDate = DateTime.UtcNow,
            EndDate = current.EndDate,
            PreviousContract = current
        };

        _db.Contracts.Add(renewed);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de contrato {ContractId}: sustituido por el contrato {NewContractId} (cambio de servicio) por {CurrentUser}",
            current.ContractId, renewed.ContractId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(renewed.ContractId, cancellationToken));
    }

    public async Task<ContractResponse> ChangePaymentMethodAsync(int contractId, int methodPaymentId, CancellationToken cancellationToken = default)
    {
        var contract = await FindOrThrowAsync(contractId, cancellationToken);
        EnsureActive(contract);

        if (contract.MethodPaymentId == methodPaymentId)
        {
            throw new AppException("El contrato ya tiene esa forma de pago");
        }

        await EnsureMethodPaymentExistsAsync(methodPaymentId, cancellationToken);

        contract.MethodPaymentId = methodPaymentId;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de contrato {ContractId}: nueva forma de pago {MethodPaymentId} por {CurrentUser}",
            contract.ContractId, methodPaymentId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(contractId, cancellationToken));
    }

    public async Task<ContractResponse> CancelAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await FindOrThrowAsync(contractId, cancellationToken);
        EnsureActive(contract);

        // Regla del enunciado: se actualiza el estado y la fecha fin
        contract.StatusContractId = ContractStatuses.Cancelado;
        contract.EndDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de contrato {ContractId}: cancelado por {CurrentUser}", contract.ContractId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(contractId, cancellationToken));
    }

    public async Task DeleteAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await FindOrThrowAsync(contractId, cancellationToken);

        var hasPayments = await _db.Payments.AnyAsync(p => p.ContractId == contractId, cancellationToken);
        if (hasPayments)
        {
            throw new AppException("No se puede eliminar un contrato con pagos registrados. Use la cancelación");
        }

        _db.Contracts.Remove(contract);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica del contrato {ContractId} por {CurrentUser}", contract.ContractId, _currentUser.UserName);
    }

    public async Task<List<PaymentResponse>> ListPaymentsAsync(int? clientId, int? contractId, CancellationToken cancellationToken = default)
    {
        var query = _db.Payments.AsNoTracking();

        if (clientId.HasValue)
        {
            query = query.Where(p => p.ClientId == clientId.Value);
        }

        if (contractId.HasValue)
        {
            query = query.Where(p => p.ContractId == contractId.Value);
        }

        return await query
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentResponse
            {
                PaymentId = p.PaymentId,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                ContractId = p.ContractId,
                ServiceName = p.Contract.Service.ServiceName,
                ClientId = p.ClientId,
                ClientName = p.Client.Name + " " + p.Client.LastName,
                MethodPaymentId = p.MethodPaymentId,
                MethodPaymentDescription = p.MethodPayment.Description,
                CashierUserName = p.UserCashier.UserName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentResponse> PayAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await FindOrThrowAsync(request.ContractId, cancellationToken);
        EnsureActive(contract);

        var methodPaymentId = request.MethodPaymentId ?? contract.MethodPaymentId;
        await EnsureMethodPaymentExistsAsync(methodPaymentId, cancellationToken);

        var payment = new Payment
        {
            ContractId = contract.ContractId,
            ClientId = contract.ClientId,
            Amount = request.Amount ?? contract.Service.Price,
            MethodPaymentId = methodPaymentId,
            UserCashierId = _currentUser.UserId,
            PaymentDate = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de pago {PaymentId} por {Amount} del contrato {ContractId} por {CurrentUser}",
            payment.PaymentId, payment.Amount, contract.ContractId, _currentUser.UserName);

        var payments = await ListPaymentsAsync(null, contract.ContractId, cancellationToken);
        return payments.First(p => p.PaymentId == payment.PaymentId);
    }

    private IQueryable<Contract> WithRelations()
    {
        return _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Service)
            .Include(c => c.StatusContract)
            .Include(c => c.MethodPayment);
    }

    private async Task<Contract> FindOrThrowAsync(int contractId, CancellationToken cancellationToken)
    {
        var contract = await WithRelations().FirstOrDefaultAsync(c => c.ContractId == contractId, cancellationToken);
        return contract ?? throw new AppException("El contrato no existe", 404);
    }

    private static void EnsureActive(Contract contract)
    {
        if (!ActiveStatuses.Contains(contract.StatusContractId))
        {
            throw new AppException($"El contrato no está en curso (estado actual: {contract.StatusContract.Description})");
        }
    }

    private async Task EnsureServiceIsAvailableAsync(int serviceId, CancellationToken cancellationToken)
    {
        var exists = await _db.Services.AnyAsync(s => s.ServiceId == serviceId && s.Active, cancellationToken);
        if (!exists)
        {
            throw new AppException("El servicio no existe o está inactivo");
        }
    }

    private async Task EnsureMethodPaymentExistsAsync(int methodPaymentId, CancellationToken cancellationToken)
    {
        var exists = await _db.MethodPayments.AnyAsync(m => m.MethodPaymentId == methodPaymentId, cancellationToken);
        if (!exists)
        {
            throw new AppException("La forma de pago no existe");
        }
    }

    private static ContractResponse ToResponse(Contract contract) => new()
    {
        ContractId = contract.ContractId,
        StartDate = contract.StartDate,
        EndDate = contract.EndDate,
        ClientId = contract.ClientId,
        ClientName = $"{contract.Client.Name} {contract.Client.LastName}",
        ClientIdentification = contract.Client.Identification,
        ServiceId = contract.ServiceId,
        ServiceName = contract.Service.ServiceName,
        SpeedMbps = contract.Service.SpeedMbps,
        Price = contract.Service.Price,
        StatusId = contract.StatusContractId,
        StatusDescription = contract.StatusContract.Description,
        MethodPaymentId = contract.MethodPaymentId,
        MethodPaymentDescription = contract.MethodPayment.Description,
        PreviousContractId = contract.PreviousContractId
    };
}
