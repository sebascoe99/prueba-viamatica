using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class StatusContractConfiguration : IEntityTypeConfiguration<StatusContract>
{
    public void Configure(EntityTypeBuilder<StatusContract> builder)
    {
        builder.ToTable("statuscontract");
        builder.HasKey(s => s.StatusId);

        builder.Property(s => s.StatusId).HasMaxLength(3);
        builder.Property(s => s.Description).HasMaxLength(50).IsRequired();

        builder.HasData(
            new StatusContract { StatusId = ContractStatuses.Vigente, Description = "Contrato vigente" },
            new StatusContract { StatusId = ContractStatuses.Sustituido, Description = "Contrato sustituido" },
            new StatusContract { StatusId = ContractStatuses.RenovacionServicio, Description = "Renovación de servicio" },
            new StatusContract { StatusId = ContractStatuses.Cancelado, Description = "Contrato cancelado" });
    }
}

public class MethodPaymentConfiguration : IEntityTypeConfiguration<MethodPayment>
{
    public void Configure(EntityTypeBuilder<MethodPayment> builder)
    {
        builder.ToTable("methodpayment");
        builder.HasKey(m => m.MethodPaymentId);

        builder.Property(m => m.MethodPaymentId).ValueGeneratedNever();
        builder.Property(m => m.Description).HasMaxLength(50).IsRequired();

        builder.HasData(
            new MethodPayment { MethodPaymentId = 1, Description = "Efectivo" },
            new MethodPayment { MethodPaymentId = 2, Description = "Tarjeta de crédito" },
            new MethodPayment { MethodPaymentId = 3, Description = "Tarjeta de débito" },
            new MethodPayment { MethodPaymentId = 4, Description = "Transferencia bancaria" });
    }
}

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contract", t =>
            t.HasCheckConstraint("ck_contract_dates", "end_date >= start_date"));

        builder.HasKey(c => c.ContractId);

        builder.Property(c => c.StatusContractId).HasMaxLength(3).IsRequired();

        builder.HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Service)
            .WithMany()
            .HasForeignKey(c => c.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.StatusContract)
            .WithMany()
            .HasForeignKey(c => c.StatusContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.MethodPayment)
            .WithMany()
            .HasForeignKey(c => c.MethodPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PreviousContract)
            .WithMany()
            .HasForeignKey(c => c.PreviousContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ClientId, c.StatusContractId });
        builder.HasIndex(c => c.ServiceId);
        builder.HasIndex(c => c.PreviousContractId);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", t =>
            t.HasCheckConstraint("ck_payments_amount", "amount > 0"));

        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.Amount).HasPrecision(10, 2);

        builder.HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Contract)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.MethodPayment)
            .WithMany()
            .HasForeignKey(p => p.MethodPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UserCashier)
            .WithMany()
            .HasForeignKey(p => p.UserCashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.ClientId, p.PaymentDate });
        builder.HasIndex(p => p.ContractId);
        builder.HasIndex(p => new { p.UserCashierId, p.PaymentDate });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
