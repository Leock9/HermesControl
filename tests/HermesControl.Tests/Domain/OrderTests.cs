using Bogus;
using FluentAssertions;
using HermesControl.Api.Domain.Base;
using HermesControl.Api.Domain.ValueObjects;
using HermesControl.Api.Domain;
using Bogus.Extensions.Brazil;

namespace HermesControl.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void CreateOrder()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = faker.Random.Decimal(1, 100);
        var itemMenuIds = faker.Make(10, () => Guid.NewGuid().ToString());
        var payment = new Payment(totalOrder) { IsAproved = true };
        var order = new Order(totalOrder, document, itemMenuIds, payment);

        order.Should()
             .Match<Order>(o => o.TotalOrder == totalOrder)
             .And.Match<Order>(o => o.Document == document)
             .And.Match<Order>(o => o.Status == Status.Received)
             .And.NotBeNull();
    }

    [Fact]
    public void CreateOrderWhenTotalOrderIsZero()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = 0;
        var itemMenuIds = faker.Make(10, () => Guid.NewGuid().ToString());
        var payment = new Payment(totalOrder) { IsAproved = true };

        Action action = () =>
        {
            new Order(totalOrder, document, itemMenuIds, payment);
        };

        action.Should()
              .Throw<DomainException>()
              .WithMessage("Total order is required");
    }

    [Fact]
    public void ChangeOrderStatusToPreparing()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = faker.Random.Decimal(1, 100);
        var itemMenuIds = faker.Make(10, () => Guid.NewGuid().ToString());
        var payment = new Payment(totalOrder) { IsAproved = true };

        var order = new Order(totalOrder, document, itemMenuIds, payment);

        order = order.ChangeStatus(Status.Preparation);

        order.Should()
             .Match<Order>(o => o.Status == Status.Preparation)
             .And.NotBeNull();
    }

    [Fact]
    public void ChangeOrderStatusToInDelivery()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = faker.Random.Decimal(1, 100);
        var itemMenuIds = faker.Make(10, () => Guid.NewGuid().ToString());
        var payment = new Payment(totalOrder) { IsAproved = true };
        var order = new Order(totalOrder, document, itemMenuIds, payment);

        order = order.ChangeStatus(Status.Preparation);
        order = order.ChangeStatus(Status.Ready);

        order.Should()
             .Match<Order>(o => o.Status == Status.Ready)
             .And.NotBeNull();
    }

    [Fact]
    public void ChangeOrderStatusToDelivered()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = faker.Random.Decimal(1, 100);
        var itemMenuIds = faker.Make(10, () => Guid.NewGuid().ToString());
        var payment = new Payment(totalOrder) { IsAproved = true };
        var order = new Order(totalOrder, document, itemMenuIds, payment);

        order = order.ChangeStatus(Status.Preparation);
        order = order.ChangeStatus(Status.Ready);
        order = order.ChangeStatus(Status.Finished);

        order.Should()
             .Match<Order>(o => o.Status == Status.Finished)
             .And.NotBeNull();
    }

    [Fact]
    public void ChangeOrderStatusToReceived()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = faker.Random.Decimal(1, 100);
        var itemMenuIds = faker.Make(10, () => Guid.NewGuid().ToString());
        var payment = new Payment(totalOrder) { IsAproved = true };
        var order = new Order(totalOrder, document, itemMenuIds, payment);

        Action action = () =>
        {
            order = order.ChangeStatus(Status.Received);
        };

        action.Should()
              .Throw<DomainException>()
              .WithMessage("Status cannot be changed to received");
    }

    [Fact]
    public void CreateOrderWhenItemMenuIsEmpty()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = faker.Random.Decimal(1, 100);
        var payment = new Payment(totalOrder) { IsAproved = true };
        var itemMenuIds = new List<string>();

        Action action = () =>
        {
            new Order(totalOrder, document, itemMenuIds, payment);
        };

        action.Should()
              .Throw<DomainException>()
              .WithMessage("Item Menu is required");
    }

    [Fact]
    public void CreateOrderWhenPaymentIsNotAproved()
    {
        var faker = new Faker("pt_BR");
        var document = faker.Person.Cpf();
        var totalOrder = faker.Random.Decimal(1, 100);
        var itemMenuIds = faker.Make(10, () => Guid.NewGuid().ToString());
        var payment = new Payment(totalOrder);
        var order = new Order(totalOrder, document, itemMenuIds, payment);

        order.Should()
             .Match<Order>(o => o.Status == Status.PaymentPending)
             .And.NotBeNull();
    }
}

