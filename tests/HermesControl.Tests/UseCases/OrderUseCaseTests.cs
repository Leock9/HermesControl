using Bogus;
using HermesControl.Api.Domain.Gateways;
using HermesControl.Api.Domain.UseCases.Requests;
using HermesControl.Api.Domain.UseCases;
using HermesControl.Api.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Bogus.Extensions.Brazil;
using FluentAssertions;
using HermesControl.Api.Infrastructure.CerberusGateway;
using HermesControl.Api.Infrastructure.SoulMenuGateway;

namespace HermesControl.Tests.UseCases;

public class OrderUseCaseTests
{
    private readonly ILogger<OrderUseCase> _logger;
    private readonly IOrderGateway _orderRepository;
    private readonly IPaymentGateway _paymentService;
    private readonly IOrderQueue _queue;
    private readonly ICerberusGateway _cerberusGateway;
    private readonly ISoulMenuGateway _soulMenuGateway;

    private readonly OrderUseCase _orderService;

    public OrderUseCaseTests()
    {
        _logger = new Mock<ILogger<OrderUseCase>>().Object;
        _orderRepository = new Mock<IOrderGateway>().Object;
        _paymentService = new Mock<IPaymentGateway>().Object;
        _queue = new Mock<IOrderQueue>().Object;
        _cerberusGateway = new Mock<ICerberusGateway>().Object;
        _soulMenuGateway = new Mock<ISoulMenuGateway>().Object;

        _orderService = new OrderUseCase
            (
              _logger,
              _orderRepository,
              _paymentService,
              _queue,
              _cerberusGateway,
              _soulMenuGateway
            );
    }

    [Fact]
    public void CreateOrderAsyncWhenOrderIsValidPayedSuccesShouldReturnOrder()
    {
        var faker = new Faker("pt_BR");

        var request = new BaseOrderRequest
        (
           faker.Random.Decimal(1, 100),
           faker.Person.Cpf(),
           faker.Make(10, () => Guid.NewGuid().ToString())
        );

        var payment = new Payment(request.TotalOrder)
        {
            IsAproved = true
        };

        var getByDocument = new GetByDocumentResponse
        (
            Guid.NewGuid().ToString(),
            faker.Person.FullName,
            faker.Person.Cpf(),
            faker.Person.Email
        );

        var getById = new GetByIdResponse
        (
            Guid.NewGuid(),
            true
        );

        Mock.Get(_cerberusGateway).Setup(x => x.GetByDocumentAsync(It.IsAny<string>())).ReturnsAsync(getByDocument);
        Mock.Get(_soulMenuGateway).Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(getById);
        Mock.Get(_paymentService).Setup(x => x.PayAsync(It.IsAny<Payment>())).Returns(payment);
        Mock.Get(_orderRepository).Setup(x => x.Create(It.IsAny<Order>()));

        var result = _orderService.CreateOrder(request);

        Mock.Get(_orderRepository).Verify(x => x.Create(It.Is<Order>(o => o.Document == request.Document &&
                                                                                            o.TotalOrder == request.TotalOrder &&
                                                                                            o.ItemMenuIds == request.ItemMenuIds &&
                                                                                            o.Status == Api.Domain.ValueObjects.Status.Received)), Times.Once);
        Mock.Get(_queue).Verify(x => x.Publish(It.IsAny<Order>()), Times.Once);

        result.Should()
              .NotBeEmpty();
    }

    [Fact]
    public void CreateOrderAsyncWhenOrderIsValidButNotPayedShouldReturnOrder()
    {
        var faker = new Faker("pt_BR");

        var request = new BaseOrderRequest
        (
           faker.Random.Decimal(1, 100),
           faker.Person.Cpf(),
           faker.Make(10, () => Guid.NewGuid().ToString())
        );

        var payment = new Payment(request.TotalOrder)
        {
            IsAproved = false
        };

        var getByDocument = new GetByDocumentResponse
        (
            Guid.NewGuid().ToString(),
            faker.Person.FullName,
            faker.Person.Cpf(),
            faker.Person.Email
        );

        var getById = new GetByIdResponse
        (
            Guid.NewGuid(),
            true
        );

        Mock.Get(_cerberusGateway).Setup(x => x.GetByDocumentAsync(It.IsAny<string>())).ReturnsAsync(getByDocument);
        Mock.Get(_soulMenuGateway).Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(getById);
        Mock.Get(_paymentService).Setup(x => x.PayAsync(It.IsAny<Payment>())).Returns(payment);
        Mock.Get(_orderRepository).Setup(x => x.Create(It.IsAny<Order>()));

        var result = _orderService.CreateOrder(request);

        Mock.Get(_orderRepository).Verify(x => x.Create(It.Is<Order>(o => o.Document == request.Document &&
                                                                                            o.TotalOrder == request.TotalOrder &&
                                                                                            o.ItemMenuIds == request.ItemMenuIds &&
                                                                                            o.Status == Api.Domain.ValueObjects.Status.PaymentPending)), Times.Once);
        Mock.Get(_queue).Verify(x => x.Publish(It.IsAny<Order>()), Times.Never);


        result.Should()
              .NotBeEmpty();
    }

    [Fact]
    public void GetAllAsyncWhenHasOrdersShouldReturnOrdersOrderBy()
    {
        var faker = new Faker("pt_BR");

        var orders = faker.Make(10, () => new Order
               (
                faker.Random.Decimal(1, 100),
                faker.Person.Cpf(),
                faker.Make(10, () => Guid.NewGuid().ToString()),
                new Payment(faker.Random.Decimal(1, 100))
                ));

        Mock.Get(_orderRepository).Setup(x => x.GetAll()).ReturnsAsync(orders);

        var result = _orderService.GetAll().Result;

        result.Should()
              .NotBeEmpty()
              .And
              .HaveCount(10);
    }

    [Fact]
    public async Task UpdateStatusOrderAsyncWhenOrderIsValidShouldReturnOrder()
    {
        var faker = new Faker("pt_BR");
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), (int)Api.Domain.ValueObjects.Status.Preparation);

        var order = new Order
            (
             faker.Random.Decimal(1, 100),
             faker.Person.Cpf(),
             faker.Make(10, () => Guid.NewGuid().ToString()),
             new Payment(faker.Random.Decimal(1, 100))
            );

        order.Status = (Api.Domain.ValueObjects.Status.Received);
        order.Payment.IsAproved = true;

        Mock.Get(_orderRepository).Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(order);
        Mock.Get(_orderRepository).Setup(x => x.UpdateAsync(It.IsAny<Order>()));

        await _orderService.UpdateStatusOrderAsync(request);

        Mock.Get(_orderRepository).Verify(x => x.UpdateAsync(It.Is<Order>(o => ((int)o.Status) == request.Status)), Times.Once);

        Mock.Get(_queue).Verify(x => x.Publish(It.Is<Order>(o => ((int)o.Status) == request.Status)), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusOrderAsyncWhenOrderIsCanceled()
    {
        var faker = new Faker("pt_BR");
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), (int)Api.Domain.ValueObjects.Status.Canceled);

        var order = new Order
            (
             faker.Random.Decimal(1, 100),
             faker.Person.Cpf(),
             faker.Make(10, () => Guid.NewGuid().ToString()),
             new Payment(faker.Random.Decimal(1, 100))
            );

        order.Status = (Api.Domain.ValueObjects.Status.Received);
        order.Payment.IsAproved = true;

        Mock.Get(_orderRepository).Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(order);
        Mock.Get(_orderRepository).Setup(x => x.UpdateAsync(It.IsAny<Order>()));

        await _orderService.UpdateStatusOrderAsync(request);

        Mock.Get(_orderRepository).Verify(x => x.UpdateAsync(It.Is<Order>(o => ((int)o.Status) == request.Status)), Times.Once);

        Mock.Get(_queue).Verify(x => x.Publish(It.Is<Order>(o => ((int)o.Status) == request.Status)), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusOrderAsyncWhenOrderPaymentoNotAproved()
    {
        var faker = new Faker("pt_BR");
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), (int)Api.Domain.ValueObjects.Status.Preparation);

        var order = new Order
            (
             faker.Random.Decimal(1, 100),
             faker.Person.Cpf(),
             faker.Make(10, () => Guid.NewGuid().ToString()),
             new Payment(faker.Random.Decimal(1, 100))
            );

        order.Status = (Api.Domain.ValueObjects.Status.Received);
        order.Payment.IsAproved = false;

        Mock.Get(_orderRepository).Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(order);
        Mock.Get(_orderRepository).Setup(x => x.UpdateAsync(It.IsAny<Order>()));

        await _orderService.UpdateStatusOrderAsync(request);

        Mock.Get(_orderRepository).Verify(x => x.UpdateAsync(It.Is<Order>(o => (o.Status) == Api.Domain.ValueObjects.Status.PaymentPending)), Times.Once);

        Mock.Get(_queue).Verify(x => x.Publish(It.Is<Order>(o => (o.Status) == Api.Domain.ValueObjects.Status.PaymentPending)), Times.Once);
    }
}
